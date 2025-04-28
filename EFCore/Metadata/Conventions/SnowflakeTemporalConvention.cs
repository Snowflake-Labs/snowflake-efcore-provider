using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Snowflake.EntityFrameworkCore.Metadata.Internal;

namespace Snowflake.EntityFrameworkCore.Metadata.Conventions;

/// <summary>
///     A convention that manipulates temporal settings for an entity mapped to a temporal table.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see>, and
///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
///     for more information and examples.
/// </remarks>
public class SnowflakeTemporalConvention : IEntityTypeAnnotationChangedConvention,
    ISkipNavigationForeignKeyChangedConvention,
    IModelFinalizingConvention
{
    private const string DefaultPeriodStartName = "PeriodStart";
    private const string DefaultPeriodEndName = "PeriodEnd";

    /// <summary>
    ///     Creates a new instance of <see cref="SnowflakeTemporalConvention" />.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this convention.</param>
    /// <param name="relationalDependencies">Parameter object containing relational dependencies for this convention.</param>
    public SnowflakeTemporalConvention(
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

    /// <inheritdoc />
    public virtual void ProcessEntityTypeAnnotationChanged(
        IConventionEntityTypeBuilder entityTypeBuilder,
        string name,
        IConventionAnnotation? annotation,
        IConventionAnnotation? oldAnnotation,
        IConventionContext<IConventionAnnotation> context)
    {
        if (name == SnowflakeAnnotationNames.IsTemporal)
        {
            if (annotation?.Value as bool? == true)
            {
                foreach (var skipLevelNavigation in entityTypeBuilder.Metadata.GetSkipNavigations())
                {
                    if (skipLevelNavigation.DeclaringEntityType.IsTemporal()
                        && skipLevelNavigation.Inverse is IConventionSkipNavigation inverse
                        && inverse.DeclaringEntityType.IsTemporal()
                        && skipLevelNavigation.JoinEntityType is { HasSharedClrType: true } joinEntityType
                        && !joinEntityType.IsTemporal()
                        && joinEntityType.GetConfigurationSource() == ConfigurationSource.Convention)
                    {
                        joinEntityType.SetIsTemporal(true);
                    }
                }
            }
        }
    }

    /// <inheritdoc />
    public virtual void ProcessSkipNavigationForeignKeyChanged(
        IConventionSkipNavigationBuilder skipNavigationBuilder,
        IConventionForeignKey? foreignKey,
        IConventionForeignKey? oldForeignKey,
        IConventionContext<IConventionForeignKey> context)
    {
        if (skipNavigationBuilder.Metadata.JoinEntityType is { HasSharedClrType: true } joinEntityType
            && !joinEntityType.IsTemporal()
            && joinEntityType.GetConfigurationSource() == ConfigurationSource.Convention
            && skipNavigationBuilder.Metadata.DeclaringEntityType.IsTemporal()
            && skipNavigationBuilder.Metadata.Inverse is IConventionSkipNavigation inverse
            && inverse.DeclaringEntityType.IsTemporal())
        {
            joinEntityType.SetIsTemporal(true);
        }
    }

    /// <inheritdoc />
    public virtual void ProcessModelFinalizing(
        IConventionModelBuilder modelBuilder,
        IConventionContext<IConventionModelBuilder> context)
    {
    }
}
