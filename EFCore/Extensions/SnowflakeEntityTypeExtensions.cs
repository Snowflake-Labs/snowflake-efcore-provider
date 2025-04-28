using Microsoft.EntityFrameworkCore.Metadata;
using Snowflake.EntityFrameworkCore.Metadata.Internal;

// ReSharper disable once CheckNamespace
namespace Snowflake.EntityFrameworkCore;


/// <summary>
///     Entity type extension methods for Snowflake-specific metadata.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
///     for more information and examples.
/// </remarks>
public static class SnowflakeEntityTypeExtensions
{
    #region Temporal table

    /// <summary>
    ///     Returns a value indicating whether the entity type is mapped to a temporal table.
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <returns><see langword="true" /> if the entity type is mapped to a temporal table.</returns>
    public static bool IsTemporal(this IReadOnlyEntityType entityType)
        => entityType[SnowflakeAnnotationNames.IsTemporal] as bool? ?? false;

    /// <summary>
    ///     Sets a value indicating whether the entity type is mapped to a temporal table.
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <param name="temporal">The value to set.</param>
    public static void SetIsTemporal(this IMutableEntityType entityType, bool temporal)
        => entityType.SetOrRemoveAnnotation(SnowflakeAnnotationNames.IsTemporal, temporal);

    /// <summary>
    ///     Sets a value indicating whether the entity type is mapped to a temporal table.
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <param name="temporal">The value to set.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The configured value.</returns>
    public static bool? SetIsTemporal(
        this IConventionEntityType entityType,
        bool? temporal,
        bool fromDataAnnotation = false)
        => (bool?)entityType.SetOrRemoveAnnotation(
            SnowflakeAnnotationNames.IsTemporal,
            temporal,
            fromDataAnnotation)?.Value;

    /// <summary>
    ///     Gets the configuration source for the temporal table setting.
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <returns>The configuration source for the temporal table setting.</returns>
    public static ConfigurationSource? GetIsTemporalConfigurationSource(this IConventionEntityType entityType)
        => entityType.FindAnnotation(SnowflakeAnnotationNames.IsTemporal)?.GetConfigurationSource();


    #endregion Temporal table
    
    #region Hybrid table
    
    /// <summary>
    ///     Returns a value indicating whether the entity type is hybrid table
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <returns><see langword="true" /> if the entity type is hybrid table.</returns>
    public static bool IsHybridTable(this IReadOnlyEntityType entityType)
        => entityType[SnowflakeAnnotationNames.HybridTable] as bool? ?? false;
    
    /// <summary>
    ///     Sets a value indicating whether the entity type is mapped to a hybrid table.
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <param name="hybrid">The value to set.</param>
    public static void SetIsHybridTable(this IMutableEntityType entityType, bool hybrid)
        => entityType.SetOrRemoveAnnotation(SnowflakeAnnotationNames.HybridTable, hybrid);

    /// <summary>
    ///     Sets a value indicating whether the entity type is mapped to a hybrid table.
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <param name="hybrid">The value to set.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <returns>The configured value.</returns>
    public static bool? SetIsHybridTable(
        this IConventionEntityType entityType,
        bool? hybrid,
        bool fromDataAnnotation = false)
        => (bool?)entityType.SetOrRemoveAnnotation(
            SnowflakeAnnotationNames.HybridTable,
            hybrid,
            fromDataAnnotation)?.Value;

    /// <summary>
    ///     Gets the configuration source for the hybrid table setting.
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <returns>The configuration source for the hybrid table setting.</returns>
    public static ConfigurationSource? GetHybridTableConfigurationSource(this IConventionEntityType entityType)
        => entityType.FindAnnotation(SnowflakeAnnotationNames.HybridTable)?.GetConfigurationSource();

    
    #endregion
}
