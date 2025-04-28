using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Snowflake.EntityFrameworkCore.Metadata;
using Snowflake.EntityFrameworkCore.Metadata.Internal;

namespace Snowflake.EntityFrameworkCore.Design.Internal;

public class SnowflakeAnnotationCodeGenerator : AnnotationCodeGenerator
{
    #region MethodInfos

    private static readonly MethodInfo ModelUseIdentityColumnsMethodInfo
        = typeof(SnowflakeModelBuilderExtensions).GetRuntimeMethod(
            nameof(SnowflakeModelBuilderExtensions.UseIdentityColumns), [typeof(ModelBuilder), typeof(long), typeof(int)])!;

    private static readonly MethodInfo ModelUseHiLoMethodInfo
        = typeof(SnowflakeModelBuilderExtensions).GetRuntimeMethod(
            nameof(SnowflakeModelBuilderExtensions.UseHiLo), [typeof(ModelBuilder), typeof(string), typeof(string)])!;

    private static readonly MethodInfo ModelUseKeySequencesMethodInfo
        = typeof(SnowflakeModelBuilderExtensions).GetRuntimeMethod(
            nameof(SnowflakeModelBuilderExtensions.UseKeySequences), [typeof(ModelBuilder), typeof(string), typeof(string)])!;

    private static readonly MethodInfo ModelHasAnnotationMethodInfo
        = typeof(ModelBuilder).GetRuntimeMethod(
            nameof(ModelBuilder.HasAnnotation), [typeof(string), typeof(object)])!;

    private static readonly MethodInfo EntityTypeToTableMethodInfo
        = typeof(RelationalEntityTypeBuilderExtensions).GetRuntimeMethod(
            nameof(RelationalEntityTypeBuilderExtensions.ToTable), [typeof(EntityTypeBuilder), typeof(string)])!;

    private static readonly MethodInfo PropertyUseIdentityColumnsMethodInfo
        = typeof(SnowflakePropertyBuilderExtensions).GetRuntimeMethod(
            nameof(SnowflakePropertyBuilderExtensions.UseIdentityColumn), [typeof(PropertyBuilder), typeof(long), typeof(int)])!;

    private static readonly MethodInfo PropertyUseHiLoMethodInfo
        = typeof(SnowflakePropertyBuilderExtensions).GetRuntimeMethod(
            nameof(SnowflakePropertyBuilderExtensions.UseHiLo), [typeof(PropertyBuilder), typeof(string), typeof(string)])!;

    private static readonly MethodInfo PropertyUseSequenceMethodInfo
        = typeof(SnowflakePropertyBuilderExtensions).GetRuntimeMethod(
            nameof(SnowflakePropertyBuilderExtensions.UseSequence), [typeof(PropertyBuilder), typeof(string), typeof(string)])!;

    private static readonly MethodInfo IndexIncludePropertiesMethodInfo
        = typeof(SnowflakeIndexBuilderExtensions).GetRuntimeMethod(
            nameof(SnowflakeIndexBuilderExtensions.IncludeProperties), [typeof(IndexBuilder), typeof(string[])])!;

    private static readonly MethodInfo TableIsTemporalMethodInfo
        = typeof(SnowflakeTableBuilderExtensions).GetRuntimeMethod(
            nameof(SnowflakeTableBuilderExtensions.IsTemporal), [typeof(TableBuilder), typeof(bool)])!;
    
    private static readonly MethodInfo HybridTableMethodInfo
        = typeof(SnowflakeTableBuilderExtensions).GetRuntimeMethod(
            nameof(SnowflakeTableBuilderExtensions.IsHybridTable), [typeof(TableBuilder), typeof(bool)])!;

    #endregion MethodInfos

    
    public SnowflakeAnnotationCodeGenerator(AnnotationCodeGeneratorDependencies dependencies)
        : base(dependencies)
    {
    }

    /// <inheritdoc />
    public override IReadOnlyList<MethodCallCodeFragment> GenerateFluentApiCalls(
        IModel model,
        IDictionary<string, IAnnotation> annotations)
    {
        var fragments = new List<MethodCallCodeFragment>(base.GenerateFluentApiCalls(model, annotations));

        if (GenerateValueGenerationStrategy(annotations, model, onModel: true) is MethodCallCodeFragment valueGenerationStrategy)
        {
            fragments.Add(valueGenerationStrategy);
        }

        return fragments;
    }

    /// <inheritdoc />
    public override IReadOnlyList<MethodCallCodeFragment> GenerateFluentApiCalls(
        IProperty property,
        IDictionary<string, IAnnotation> annotations)
    {
        var fragments = new List<MethodCallCodeFragment>(base.GenerateFluentApiCalls(property, annotations));

        if (GenerateValueGenerationStrategy(annotations, property.DeclaringType.Model, onModel: false) is MethodCallCodeFragment
            valueGenerationStrategy)
        {
            fragments.Add(valueGenerationStrategy);
        }

        return fragments;
    }

    /// <inheritdoc />
    public override IReadOnlyList<MethodCallCodeFragment> GenerateFluentApiCalls(
        IEntityType entityType,
        IDictionary<string, IAnnotation> annotations)
    {
        var fragments = new List<MethodCallCodeFragment>(base.GenerateFluentApiCalls(entityType, annotations));

        if (annotations.TryGetValue(SnowflakeAnnotationNames.IsTemporal, out var isTemporalAnnotation)
            && isTemporalAnnotation.Value as bool? == true)
        {
            var toTemporalTableCall = new MethodCallCodeFragment(
                EntityTypeToTableMethodInfo,
                new NestedClosureCodeFragment(
                    "tb",
                    new MethodCallCodeFragment(
                        TableIsTemporalMethodInfo)));

            fragments.Add(toTemporalTableCall);

            annotations.Remove(SnowflakeAnnotationNames.IsTemporal);
        }
        

        return fragments;
    }
    

    /// <inheritdoc />
    protected override bool IsHandledByConvention(IModel model, IAnnotation annotation)
    {
        if (annotation.Name == RelationalAnnotationNames.DefaultSchema)
        {
            return (string?)annotation.Value == "dbo";
        }

        return annotation.Name == SnowflakeAnnotationNames.ValueGenerationStrategy
            && (SnowflakeValueGenerationStrategy)annotation.Value! == SnowflakeValueGenerationStrategy.IdentityColumn;
    }

    /// <inheritdoc />
    protected override bool IsHandledByConvention(IProperty property, IAnnotation annotation)
    {
        if (annotation.Name == SnowflakeAnnotationNames.ValueGenerationStrategy)
        {
            return (SnowflakeValueGenerationStrategy)annotation.Value! == property.DeclaringType.Model.GetValueGenerationStrategy();
        }

        return base.IsHandledByConvention(property, annotation);
    }

    /// <inheritdoc />
    protected override MethodCallCodeFragment? GenerateFluentApi(IKey key, IAnnotation annotation)
        => null;

    /// <inheritdoc />
    protected override MethodCallCodeFragment? GenerateFluentApi(IIndex index, IAnnotation annotation)
        => annotation.Name switch
        {
            SnowflakeAnnotationNames.Include => new MethodCallCodeFragment(IndexIncludePropertiesMethodInfo, annotation.Value),
            _ => null
        };

    private static MethodCallCodeFragment? GenerateValueGenerationStrategy(
        IDictionary<string, IAnnotation> annotations,
        IModel model,
        bool onModel)
    {
        SnowflakeValueGenerationStrategy strategy;
        if (annotations.TryGetValue(SnowflakeAnnotationNames.ValueGenerationStrategy, out var strategyAnnotation)
            && strategyAnnotation.Value != null)
        {
            annotations.Remove(SnowflakeAnnotationNames.ValueGenerationStrategy);
            strategy = (SnowflakeValueGenerationStrategy)strategyAnnotation.Value;
        }
        else
        {
            return null;
        }

        switch (strategy)
        {
            case SnowflakeValueGenerationStrategy.IdentityColumn:
                // Support pre-6.0 IdentitySeed annotations, which contained an int rather than a long
                if (annotations.TryGetValue(SnowflakeAnnotationNames.IdentitySeed, out var seedAnnotation)
                    && seedAnnotation.Value != null)
                {
                    annotations.Remove(SnowflakeAnnotationNames.IdentitySeed);
                }
                else
                {
                    seedAnnotation = model.FindAnnotation(SnowflakeAnnotationNames.IdentitySeed);
                }

                var seed = seedAnnotation is null
                    ? 1L
                    : seedAnnotation.Value is int intValue
                        ? intValue
                        : (long?)seedAnnotation.Value ?? 1L;

                var increment = GetAndRemove<int?>(annotations, SnowflakeAnnotationNames.IdentityIncrement)
                    ?? model.FindAnnotation(SnowflakeAnnotationNames.IdentityIncrement)?.Value as int?
                    ?? 1;
                return new MethodCallCodeFragment(
                    onModel ? ModelUseIdentityColumnsMethodInfo : PropertyUseIdentityColumnsMethodInfo,
                    (seed, increment) switch
                    {
                        (1L, 1) => Array.Empty<object>(),
                        (_, 1) => new object[] { seed },
                        _ => new object[] { seed, increment }
                    });

            case SnowflakeValueGenerationStrategy.SequenceHiLo:
            {
                var name = GetAndRemove<string>(annotations, SnowflakeAnnotationNames.HiLoSequenceName);
                var schema = GetAndRemove<string>(annotations, SnowflakeAnnotationNames.HiLoSequenceSchema);
                return new MethodCallCodeFragment(
                    onModel ? ModelUseHiLoMethodInfo : PropertyUseHiLoMethodInfo,
                    (name, schema) switch
                    {
                        (null, null) => Array.Empty<object>(),
                        (_, null) => new object[] { name },
                        _ => new object[] { name!, schema }
                    });
            }

            case SnowflakeValueGenerationStrategy.Sequence:
            {
                var nameOrSuffix = GetAndRemove<string>(
                    annotations,
                    onModel ? SnowflakeAnnotationNames.SequenceNameSuffix : SnowflakeAnnotationNames.SequenceName);

                var schema = GetAndRemove<string>(annotations, SnowflakeAnnotationNames.SequenceSchema);
                return new MethodCallCodeFragment(
                    onModel ? ModelUseKeySequencesMethodInfo : PropertyUseSequenceMethodInfo,
                    (name: nameOrSuffix, schema) switch
                    {
                        (null, null) => Array.Empty<object>(),
                        (_, null) => new object[] { nameOrSuffix },
                        _ => new object[] { nameOrSuffix!, schema }
                    });
            }

            case SnowflakeValueGenerationStrategy.None:
                return new MethodCallCodeFragment(
                    ModelHasAnnotationMethodInfo,
                    SnowflakeAnnotationNames.ValueGenerationStrategy,
                    SnowflakeValueGenerationStrategy.None);

            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private static T? GetAndRemove<T>(IDictionary<string, IAnnotation> annotations, string annotationName)
    {
        if (annotations.TryGetValue(annotationName, out var annotation)
            && annotation.Value != null)
        {
            annotations.Remove(annotationName);
            return (T)annotation.Value;
        }

        return default;
    }

    protected override AttributeCodeFragment GenerateDataAnnotation(IEntityType entityType, IAnnotation annotation)
        => annotation.Name switch
        {
            SnowflakeAnnotationNames.HybridTable => new AttributeCodeFragment(
                typeof(HybridTableAttribute)),
            _ => base.GenerateDataAnnotation(entityType, annotation)
        };

    protected override MethodCallCodeFragment GenerateFluentApi(IEntityType entityType, IAnnotation annotation)
        => annotation.Name switch
        {
            SnowflakeAnnotationNames.HybridTable => new MethodCallCodeFragment(
                EntityTypeToTableMethodInfo,
                new NestedClosureCodeFragment(
                    "tb",
                    new MethodCallCodeFragment(
                        HybridTableMethodInfo))),
            _ => base.GenerateFluentApi(entityType, annotation)
        };
}
