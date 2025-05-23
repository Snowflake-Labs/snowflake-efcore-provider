using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace Snowflake.EntityFrameworkCore.Metadata.Conventions;

/// <summary>
///     A convention that configures the default model <see cref="SnowflakeValueGenerationStrategy" /> as
///     <see cref="SnowflakeValueGenerationStrategy.IdentityColumn" />.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see>, and
///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
///     for more information and examples.
/// </remarks>
public class SnowflakeValueGenerationStrategyConvention : IModelInitializedConvention, IModelFinalizingConvention
{
    /// <summary>
    ///     Creates a new instance of <see cref="SnowflakeValueGenerationStrategyConvention" />.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this convention.</param>
    /// <param name="relationalDependencies"> Parameter object containing relational dependencies for this convention.</param>
    public SnowflakeValueGenerationStrategyConvention(
        ProviderConventionSetBuilderDependencies dependencies,
        RelationalConventionSetBuilderDependencies relationalDependencies)
    {
        Dependencies = dependencies;
        RelationalDependencies = relationalDependencies;
    }

    /// <summary>
    ///     Dependencies for this service.
    /// </summary>
    protected virtual ProviderConventionSetBuilderDependencies Dependencies { get; }

    /// <summary>
    ///     Relational provider-specific dependencies for this service.
    /// </summary>
    protected virtual RelationalConventionSetBuilderDependencies RelationalDependencies { get; }

    /// <summary>
    ///     Called after a model is initialized.
    /// </summary>
    /// <param name="modelBuilder">The builder for the model.</param>
    /// <param name="context">Additional information associated with convention execution.</param>
    public virtual void ProcessModelInitialized(
        IConventionModelBuilder modelBuilder,
        IConventionContext<IConventionModelBuilder> context)
        => modelBuilder.HasValueGenerationStrategy(SnowflakeValueGenerationStrategy.IdentityColumn);

    /// <inheritdoc />
    public virtual void ProcessModelFinalizing(
        IConventionModelBuilder modelBuilder,
        IConventionContext<IConventionModelBuilder> context)
    {
        foreach (var entityType in modelBuilder.Metadata.GetEntityTypes())
        {
            foreach (var property in entityType.GetDeclaredProperties())
            {
                SnowflakeValueGenerationStrategy? strategy = null;
                var declaringTable = property.GetMappedStoreObjects(StoreObjectType.Table).FirstOrDefault();
                if (declaringTable.Name != null!)
                {
                    strategy = property.GetValueGenerationStrategy(declaringTable, Dependencies.TypeMappingSource);
                    if (strategy == SnowflakeValueGenerationStrategy.None
                        && !IsStrategyNoneNeeded(property, declaringTable))
                    {
                        strategy = null;
                    }
                }
                else
                {
                    var declaringView = property.GetMappedStoreObjects(StoreObjectType.View).FirstOrDefault();
                    if (declaringView.Name != null!)
                    {
                        strategy = property.GetValueGenerationStrategy(declaringView, Dependencies.TypeMappingSource);
                        if (strategy == SnowflakeValueGenerationStrategy.None
                            && !IsStrategyNoneNeeded(property, declaringView))
                        {
                            strategy = null;
                        }
                    }
                }

                // Needed for the annotation to show up in the model snapshot
                if (strategy != null
                    && declaringTable.Name != null)
                {
                    property.Builder.HasValueGenerationStrategy(strategy);

                    if (strategy == SnowflakeValueGenerationStrategy.Sequence)
                    {
                        var sequence = modelBuilder.HasSequence(
                            property.GetSequenceName(declaringTable)
                            ?? entityType.GetRootType().ShortName() + modelBuilder.Metadata.GetSequenceNameSuffix(),
                            property.GetSequenceSchema(declaringTable)
                            ?? modelBuilder.Metadata.GetSequenceSchema()).Metadata;

                        property.Builder.HasDefaultValueSql(
                            RelationalDependencies.UpdateSqlGenerator.GenerateObtainNextSequenceValueOperation(
                                sequence.Name, sequence.Schema));
                    }
                }
            }
        }

        bool IsStrategyNoneNeeded(IReadOnlyProperty property, StoreObjectIdentifier storeObject)
        {
            if (property.ValueGenerated == ValueGenerated.OnAdd
                && !property.TryGetDefaultValue(storeObject, out _)
                && property.GetDefaultValueSql(storeObject) == null
                && property.GetComputedColumnSql(storeObject) == null
                && property.DeclaringType.Model.GetValueGenerationStrategy() == SnowflakeValueGenerationStrategy.IdentityColumn)
            {
                var providerClrType = (property.GetValueConverter()
                        ?? (property.FindRelationalTypeMapping(storeObject)
                            ?? Dependencies.TypeMappingSource.FindMapping((IProperty)property))?.Converter)
                    ?.ProviderClrType.UnwrapNullableType();

                return providerClrType != null
                    && (providerClrType.IsInteger() || providerClrType == typeof(decimal));
            }

            return false;
        }
    }
}
