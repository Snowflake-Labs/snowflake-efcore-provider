using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace Snowflake.EntityFrameworkCore.Metadata.Conventions;

/// <summary>
///     A convention that configures the filter for unique non-clustered indexes with nullable columns
///     to filter out null values.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-conventions">Model building conventions</see>, and
///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
///     for more information and examples.
/// </remarks>
public class SnowflakeIndexConvention :
    IEntityTypeBaseTypeChangedConvention,
    IIndexAddedConvention,
    IIndexUniquenessChangedConvention,
    IIndexAnnotationChangedConvention,
    IPropertyNullabilityChangedConvention,
    IPropertyAnnotationChangedConvention
{
    private readonly ISqlGenerationHelper _sqlGenerationHelper;

    /// <summary>
    ///     Creates a new instance of <see cref="SnowflakeIndexConvention" />.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this convention.</param>
    /// <param name="relationalDependencies"> Parameter object containing relational dependencies for this convention.</param>
    /// <param name="sqlGenerationHelper">SQL command generation helper service.</param>
    public SnowflakeIndexConvention(
        ProviderConventionSetBuilderDependencies dependencies,
        RelationalConventionSetBuilderDependencies relationalDependencies,
        ISqlGenerationHelper sqlGenerationHelper)
    {
        _sqlGenerationHelper = sqlGenerationHelper;

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
    ///     Called after the base type of an entity type changes.
    /// </summary>
    /// <param name="entityTypeBuilder">The builder for the entity type.</param>
    /// <param name="newBaseType">The new base entity type.</param>
    /// <param name="oldBaseType">The old base entity type.</param>
    /// <param name="context">Additional information associated with convention execution.</param>
    public virtual void ProcessEntityTypeBaseTypeChanged(
        IConventionEntityTypeBuilder entityTypeBuilder,
        IConventionEntityType? newBaseType,
        IConventionEntityType? oldBaseType,
        IConventionContext<IConventionEntityType> context)
    {
        if (oldBaseType == null
            || newBaseType == null)
        {
            foreach (var index in entityTypeBuilder.Metadata.GetDeclaredIndexes())
            {
                SetIndexFilter(index.Builder);
            }
        }
    }

    /// <summary>
    ///     Called after an index is added to the entity type.
    /// </summary>
    /// <param name="indexBuilder">The builder for the index.</param>
    /// <param name="context">Additional information associated with convention execution.</param>
    public virtual void ProcessIndexAdded(
        IConventionIndexBuilder indexBuilder,
        IConventionContext<IConventionIndexBuilder> context)
        => SetIndexFilter(indexBuilder);

    /// <summary>
    ///     Called after the uniqueness for an index is changed.
    /// </summary>
    /// <param name="indexBuilder">The builder for the index.</param>
    /// <param name="context">Additional information associated with convention execution.</param>
    public virtual void ProcessIndexUniquenessChanged(
        IConventionIndexBuilder indexBuilder,
        IConventionContext<bool?> context)
        => SetIndexFilter(indexBuilder);

    /// <summary>
    ///     Called after the nullability for a property is changed.
    /// </summary>
    /// <param name="propertyBuilder">The builder for the property.</param>
    /// <param name="context">Additional information associated with convention execution.</param>
    public virtual void ProcessPropertyNullabilityChanged(
        IConventionPropertyBuilder propertyBuilder,
        IConventionContext<bool?> context)
    {
        foreach (var index in propertyBuilder.Metadata.GetContainingIndexes())
        {
            SetIndexFilter(index.Builder);
        }
    }

    /// <summary>
    ///     Called after an annotation is changed on an index.
    /// </summary>
    /// <param name="indexBuilder">The builder for the index.</param>
    /// <param name="name">The annotation name.</param>
    /// <param name="annotation">The new annotation.</param>
    /// <param name="oldAnnotation">The old annotation.</param>
    /// <param name="context">Additional information associated with convention execution.</param>
    public virtual void ProcessIndexAnnotationChanged(
        IConventionIndexBuilder indexBuilder,
        string name,
        IConventionAnnotation? annotation,
        IConventionAnnotation? oldAnnotation,
        IConventionContext<IConventionAnnotation> context)
    {
    }

    /// <summary>
    ///     Called after an annotation is changed on a property.
    /// </summary>
    /// <param name="propertyBuilder">The builder for the property.</param>
    /// <param name="name">The annotation name.</param>
    /// <param name="annotation">The new annotation.</param>
    /// <param name="oldAnnotation">The old annotation.</param>
    /// <param name="context">Additional information associated with convention execution.</param>
    public virtual void ProcessPropertyAnnotationChanged(
        IConventionPropertyBuilder propertyBuilder,
        string name,
        IConventionAnnotation? annotation,
        IConventionAnnotation? oldAnnotation,
        IConventionContext<IConventionAnnotation> context)
    {
        if (name == RelationalAnnotationNames.ColumnName)
        {
            foreach (var index in propertyBuilder.Metadata.GetContainingIndexes())
            {
                SetIndexFilter(index.Builder, columnNameChanged: true);
            }
        }
    }

    /// <summary>
    ///    Sets the filter for the index.
    /// </summary>
    /// <param name="indexBuilder"></param>
    /// <param name="columnNameChanged"></param>
    private void SetIndexFilter(IConventionIndexBuilder indexBuilder, bool columnNameChanged = false)
    {
        var index = indexBuilder.Metadata;
        if (index.IsUnique
            && GetNullableColumns(index) is { Count: > 0 } nullableColumns)
        {
            if (columnNameChanged
                || index.GetFilter() == null)
            {
                indexBuilder.HasFilter(CreateIndexFilter(nullableColumns));
            }
        }
        else
        {
            if (index.GetFilter() != null)
            {
                indexBuilder.HasFilter(null);
            }
        }
    }

    /// <summary>
    ///   Creates the filter expression for the index.
    /// </summary>
    /// <param name="nullableColumns"></param>
    /// <returns></returns>
    private string CreateIndexFilter(List<string> nullableColumns)
    {
        var builder = new StringBuilder();
        for (var i = 0; i < nullableColumns.Count; i++)
        {
            if (i != 0)
            {
                builder.Append(" AND ");
            }

            builder
                .Append(_sqlGenerationHelper.DelimitIdentifier(nullableColumns[i]))
                .Append(" IS NOT NULL");
        }

        return builder.ToString();
    }

    
    /// <summary>
    ///    Returns the nullable columns of the index.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    private static List<string>? GetNullableColumns(IReadOnlyIndex index)
    {
        var tableName = index.DeclaringEntityType.GetTableName();
        if (tableName == null)
        {
            return null;
        }

        var nullableColumns = new List<string>();
        var table = StoreObjectIdentifier.Table(tableName, index.DeclaringEntityType.GetSchema());
        foreach (var property in index.Properties)
        {
            var columnName = property.GetColumnName(table);
            if (columnName == null)
            {
                return null;
            }

            if (!property.IsColumnNullable(table))
            {
                continue;
            }

            nullableColumns.Add(columnName);
        }

        return nullableColumns;
    }
}
