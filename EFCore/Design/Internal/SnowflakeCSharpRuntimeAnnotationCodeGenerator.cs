using Microsoft.EntityFrameworkCore.Design.Internal;
using Snowflake.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Snowflake.EntityFrameworkCore.Design.Internal;

#pragma warning disable EF1001 // Internal EF Core API usage.
public class SnowflakeCSharpRuntimeAnnotationCodeGenerator : RelationalCSharpRuntimeAnnotationCodeGenerator
{
    
    public SnowflakeCSharpRuntimeAnnotationCodeGenerator(
        CSharpRuntimeAnnotationCodeGeneratorDependencies dependencies,
        RelationalCSharpRuntimeAnnotationCodeGeneratorDependencies relationalDependencies)
        : base(dependencies, relationalDependencies)
    {
    }

    /// <inheritdoc />
    public override void Generate(IModel model, CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
    {
        if (!parameters.IsRuntime)
        {
            var annotations = parameters.Annotations;
            annotations.Remove(SnowflakeAnnotationNames.IdentityIncrement);
            annotations.Remove(SnowflakeAnnotationNames.IdentitySeed);
        }

        base.Generate(model, parameters);
    }

    /// <inheritdoc />
    public override void Generate(IProperty property, CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
    {
        if (!parameters.IsRuntime)
        {
            var annotations = parameters.Annotations;
            annotations.Remove(SnowflakeAnnotationNames.IdentityIncrement);
            annotations.Remove(SnowflakeAnnotationNames.IdentitySeed);

            if (!annotations.ContainsKey(SnowflakeAnnotationNames.ValueGenerationStrategy))
            {
                annotations[SnowflakeAnnotationNames.ValueGenerationStrategy] = property.GetValueGenerationStrategy();
            }
        }

        base.Generate(property, parameters);
    }

    /// <inheritdoc />
    public override void Generate(IColumn column, CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
    {
        if (!parameters.IsRuntime)
        {
            var annotations = parameters.Annotations;
            annotations.Remove(SnowflakeAnnotationNames.Identity);
            annotations.Remove(SnowflakeAnnotationNames.IsTemporal);
        }

        base.Generate(column, parameters);
    }

    /// <inheritdoc />
    public override void Generate(IIndex index, CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
    {
        if (!parameters.IsRuntime)
        {
            var annotations = parameters.Annotations;
            annotations.Remove(SnowflakeAnnotationNames.Include);
        }

        base.Generate(index, parameters);
    }

    /// <inheritdoc />
    public override void Generate(ITableIndex index, CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
    {
        if (!parameters.IsRuntime)
        {
            var annotations = parameters.Annotations;
            annotations.Remove(SnowflakeAnnotationNames.Include);
        }

        base.Generate(index, parameters);
    }

    /// <inheritdoc />
    public override void Generate(IKey key, CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
    {
        if (!parameters.IsRuntime)
        {
            var annotations = parameters.Annotations;
        }

        base.Generate(key, parameters);
    }

    /// <inheritdoc />
    public override void Generate(IUniqueConstraint uniqueConstraint, CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
    {
        if (!parameters.IsRuntime)
        {
            var annotations = parameters.Annotations;
        }

        base.Generate(uniqueConstraint, parameters);
    }

    /// <inheritdoc />
    public override void Generate(IEntityType entityType, CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
    {
        if (!parameters.IsRuntime)
        {
            var annotations = parameters.Annotations;
        }

        base.Generate(entityType, parameters);
    }

    /// <inheritdoc />
    public override void Generate(IRelationalPropertyOverrides overrides, CSharpRuntimeAnnotationCodeGeneratorParameters parameters)
    {
        if (!parameters.IsRuntime)
        {
            var annotations = parameters.Annotations;
            annotations.Remove(SnowflakeAnnotationNames.IdentityIncrement);
            annotations.Remove(SnowflakeAnnotationNames.IdentitySeed);
        }

        base.Generate(overrides, parameters);
    }
}
