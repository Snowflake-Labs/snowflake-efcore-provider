using System;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Snowflake.EntityFrameworkCore;
using Snowflake.EntityFrameworkCore.Metadata.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore;


/// <summary>
///     Snowflake specific extension methods for <see cref="EntityTypeBuilder" />.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
///     for more information and examples.
/// </remarks>
public static class SnowflakeEntityTypeBuilderExtensions
{
    /// <summary>
    ///     Configures the table as temporal.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-memory-optimized">Using Snowflake memory-optimized tables with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="entityTypeBuilder">The builder for the entity being configured.</param>
    /// <param name="temporal">A value indicating whether the table is temporal.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder instance if the configuration was applied,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    public static IConventionEntityTypeBuilder? IsTemporal(
        this IConventionEntityTypeBuilder entityTypeBuilder,
        bool temporal = true,
        bool fromDataAnnotation = false)
    {
        if (entityTypeBuilder.CanSetIsTemporal(temporal, fromDataAnnotation))
        {
            entityTypeBuilder.Metadata.SetIsTemporal(temporal, fromDataAnnotation);

            return entityTypeBuilder;
        }

        return null;
    }

    /// <summary>
    ///     Returns a value indicating whether the mapped table can be configured as temporal.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-memory-optimized">Using Snowflake memory-optimized tables with EF Core</see>
    ///     for more information and examples.
    /// </remarks>
    /// <param name="entityTypeBuilder">The builder for the entity type being configured.</param>
    /// <param name="temporal">A value indicating whether the table is temporal.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the mapped table can be configured as temporal.</returns>
    public static bool CanSetIsTemporal(
        this IConventionEntityTypeBuilder entityTypeBuilder,
        bool temporal = true,
        bool fromDataAnnotation = false)
        => entityTypeBuilder.CanSetAnnotation(SnowflakeAnnotationNames.IsTemporal, temporal, fromDataAnnotation);
    
    /// <summary>
    ///     Configures the table as hybrid type.
    /// </summary>
    /// <param name="entityTypeBuilder">The builder for the entity being configured.</param>
    /// <param name="isHybrid">A value indicating whether the table is hybrid table.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>
    ///     The same builder instance if the configuration was applied,
    ///     <see langword="null" /> otherwise.
    /// </returns>
    public static IConventionEntityTypeBuilder? IsHybridTable(
        this IConventionEntityTypeBuilder entityTypeBuilder,
        bool isHybrid = true,
        bool fromDataAnnotation = false)
    {
        if (entityTypeBuilder.CanSetHybridTable(isHybrid, fromDataAnnotation))
        {
            entityTypeBuilder.Metadata.SetIsHybridTable(isHybrid, fromDataAnnotation);

            return entityTypeBuilder;
        }

        return null;
    }

    /// <summary>
    ///     Returns a value indicating whether the mapped table can be configured as hybrid table.
    /// </summary>
    /// <param name="entityTypeBuilder">The builder for the entity type being configured.</param>
    /// <param name="hybrid">A value indicating whether the table is hybrid.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns><see langword="true" /> if the mapped table can be configured as hybrid.</returns>
    public static bool CanSetHybridTable(
        this IConventionEntityTypeBuilder entityTypeBuilder,
        bool hybrid = true,
        bool fromDataAnnotation = false)
        => entityTypeBuilder.CanSetAnnotation(SnowflakeAnnotationNames.HybridTable, hybrid, fromDataAnnotation);
}
