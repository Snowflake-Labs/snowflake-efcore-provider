using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Snowflake.EntityFrameworkCore.Metadata.Internal;

/// <inheritdoc />
public class SnowflakeAnnotationProvider : RelationalAnnotationProvider
{
    /// <summary>
    ///     Initializes a new instance of this class.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this service.</param>
    public SnowflakeAnnotationProvider(RelationalAnnotationProviderDependencies dependencies)
        : base(dependencies)
    {
    }

    /// <inheritdoc />
    public override IEnumerable<IAnnotation> For(ITable table, bool designTime)
    {
        if (!designTime)
        {
            yield break;
        }

        var entityType = (IEntityType)table.EntityTypeMappings.First().TypeBase;

        if (entityType.IsHybridTable())
        {
            yield return new Annotation(SnowflakeAnnotationNames.HybridTable, true);
        }

        if (entityType.IsTemporal() && designTime)
        {
            yield return new Annotation(SnowflakeAnnotationNames.IsTemporal, true);
        }
    }

    /// <inheritdoc />
    public override IEnumerable<IAnnotation> For(IUniqueConstraint constraint, bool designTime)
    {
        if (!designTime)
        {
            yield break;
        }
    }

    /// <inheritdoc />
    public override IEnumerable<IAnnotation> For(ITableIndex index, bool designTime)
    {
        if (!designTime)
        {
            yield break;
        }

        // Model validation ensures that these facets are the same on all mapped indexes
        var modelIndex = index.MappedIndexes.First();
        var table = StoreObjectIdentifier.Table(index.Table.Name, index.Table.Schema);

        if (modelIndex.GetIncludeProperties(table) is IReadOnlyList<string> includeProperties)
        {
            var includeColumns = includeProperties
                .Select(
                    p => modelIndex.DeclaringEntityType.FindProperty(p)!
                        .GetColumnName(StoreObjectIdentifier.Table(table.Name, table.Schema)))
                .ToArray();

            yield return new Annotation(
                SnowflakeAnnotationNames.Include,
                includeColumns);
        }
    }

    /// <inheritdoc />
    public override IEnumerable<IAnnotation> For(IColumn column, bool designTime)
    {
        if (!designTime)
        {
            yield break;
        }

        var table = StoreObjectIdentifier.Table(column.Table.Name, column.Table.Schema);
        var identityProperty = column.PropertyMappings
            .Select(m => m.Property)
            .FirstOrDefault(
                p => p.GetValueGenerationStrategy(table)
                    == SnowflakeValueGenerationStrategy.IdentityColumn);
        if (identityProperty != null)
        {
            var seed = identityProperty.GetIdentitySeed(table);
            var increment = identityProperty.GetIdentityIncrement(table);

            yield return new Annotation(
                SnowflakeAnnotationNames.Identity,
                string.Format(CultureInfo.InvariantCulture, "{0}, {1}", seed ?? 1, increment ?? 1));
        }

        var entityType = (IEntityType)column.Table.EntityTypeMappings.First().TypeBase;
        if (entityType.IsTemporal() && designTime)
        {
            // TODO: issue #27459 - we want to avoid having those annotations on every column
            yield return new Annotation(SnowflakeAnnotationNames.IsTemporal, true);
        }
    }
}
