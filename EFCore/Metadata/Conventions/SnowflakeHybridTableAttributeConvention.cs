using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using System;
using System.Linq;
using Snowflake.EntityFrameworkCore.Internal;

namespace Snowflake.EntityFrameworkCore.Metadata.Conventions;

/// <summary>
///    A convention that configures a model as a hybrid table if it has the <see cref="HybridTableAttribute" /> applied.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see> for more information and examples.
/// </remarks>
public class SnowflakeHybridTableAttributeConvention : TypeAttributeConventionBase<HybridTableAttribute>, IModelFinalizingConvention
{
    /// <summary>
    ///     Creates a new instance of <see cref="SnowflakeHybridTableAttributeConvention" />.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this convention.</param>
    public SnowflakeHybridTableAttributeConvention(ProviderConventionSetBuilderDependencies dependencies)
        : base(dependencies)
    {
    }

    /// <summary>
    ///     Called after an entity type is added to the model if it has an attribute.
    /// </summary>
    /// <param name="entityTypeBuilder">The builder for the entity type.</param>
    /// <param name="attribute">The attribute.</param>
    /// <param name="context">Additional information associated with convention execution.</param>
    protected override void ProcessEntityTypeAdded(
        IConventionEntityTypeBuilder entityTypeBuilder,
        HybridTableAttribute attribute,
        IConventionContext<IConventionEntityTypeBuilder> context)
        => entityTypeBuilder.IsHybridTable(fromDataAnnotation: true);


    /// <inheritdoc />
    /// Validates that all entity types marked as HYBRID TABLE have a primary key defined.
    public void ProcessModelFinalizing(IConventionModelBuilder modelBuilder, IConventionContext<IConventionModelBuilder> context)
    {
        foreach (var entityType in modelBuilder.Metadata.GetEntityTypes())
        {
            if (entityType.IsHybridTable() && !entityType.GetKeys().Any(k => k.IsPrimaryKey()))
            {
                throw new InvalidOperationException(SnowflakeStrings.InvalidHybridTableWithoutPrimaryKey(entityType.Name));
            }
        }
    }
}
