using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Snowflake.EntityFrameworkCore.Metadata.Internal;

// ReSharper disable once CheckNamespace
namespace Snowflake.EntityFrameworkCore;

/// <summary>
///     Index extension methods for Snowflake-specific metadata.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-modeling">Modeling entity types and relationships</see>, and
///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
///     for more information and examples.
/// </remarks>
public static class SnowflakeIndexExtensions
{
    /// <summary>
    ///     Returns included property names, or <see langword="null" /> if they have not been specified.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <returns>The included property names, or <see langword="null" /> if they have not been specified.</returns>
    public static IReadOnlyList<string>? GetIncludeProperties(this IReadOnlyIndex index)
        => (index is RuntimeIndex)
            ? throw new InvalidOperationException(CoreStrings.RuntimeModelMissingData)
            : (string[]?)index[SnowflakeAnnotationNames.Include];

    /// <summary>
    ///     Returns included property names, or <see langword="null" /> if they have not been specified.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="storeObject">The identifier of the store object.</param>
    /// <returns>The included property names, or <see langword="null" /> if they have not been specified.</returns>
    public static IReadOnlyList<string>? GetIncludeProperties(this IReadOnlyIndex index, in StoreObjectIdentifier storeObject)
    {
        if (index is RuntimeIndex)
        {
            throw new InvalidOperationException(CoreStrings.RuntimeModelMissingData);
        }

        var annotation = index.FindAnnotation(SnowflakeAnnotationNames.Include);
        if (annotation != null)
        {
            return (IReadOnlyList<string>?)annotation.Value;
        }

        var sharedTableRootIndex = index.FindSharedObjectRootIndex(storeObject);
        return sharedTableRootIndex?.GetIncludeProperties(storeObject);
    }

    /// <summary>
    ///     Sets included property names.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="properties">The value to set.</param>
    public static void SetIncludeProperties(this IMutableIndex index, IReadOnlyList<string> properties)
        => index.SetAnnotation(
            SnowflakeAnnotationNames.Include,
            properties);

    /// <summary>
    ///     Sets included property names.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <param name="fromDataAnnotation">Indicates whether the configuration was specified using a data annotation.</param>
    /// <param name="properties">The value to set.</param>
    /// <returns>The configured property names.</returns>
    public static IReadOnlyList<string>? SetIncludeProperties(
        this IConventionIndex index,
        IReadOnlyList<string>? properties,
        bool fromDataAnnotation = false)
        => (IReadOnlyList<string>?)index.SetAnnotation(
            SnowflakeAnnotationNames.Include,
            properties,
            fromDataAnnotation)?.Value;

    /// <summary>
    ///     Returns the <see cref="ConfigurationSource" /> for the included property names.
    /// </summary>
    /// <param name="index">The index.</param>
    /// <returns>The <see cref="ConfigurationSource" /> for the included property names.</returns>
    public static ConfigurationSource? GetIncludePropertiesConfigurationSource(this IConventionIndex index)
        => index.FindAnnotation(SnowflakeAnnotationNames.Include)?.GetConfigurationSource();
}
