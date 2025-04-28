using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;

namespace Snowflake.EntityFrameworkCore.Metadata.Conventions;

/// <inheritdoc />
public class SnowflakeForeignKeyIndexConvention : ForeignKeyIndexConvention
{
    /// <inheritdoc />
    public SnowflakeForeignKeyIndexConvention(ProviderConventionSetBuilderDependencies dependencies) : base(
        dependencies)
    {
    }

    /// <inheritdoc />
    protected override IConventionIndex CreateIndex(
        IReadOnlyList<IConventionProperty> properties,
        bool unique,
        IConventionEntityTypeBuilder entityTypeBuilder)
    {
        return entityTypeBuilder.Metadata.IsHybridTable()
            ? base.CreateIndex(properties, unique, entityTypeBuilder)
            : null;
    }
}