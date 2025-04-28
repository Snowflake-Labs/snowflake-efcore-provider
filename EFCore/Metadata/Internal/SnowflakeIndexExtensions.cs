using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Snowflake.EntityFrameworkCore.Internal;

namespace Snowflake.EntityFrameworkCore.Metadata.Internal;

/// <summary>
/// Provides extension methods for IReadOnlyIndex to determine compatibility of indexes for Snowflake and to format included properties.
/// </summary>
public static class SnowflakeIndexExtensions
{
    /// <summary>
    /// Extension method for the IReadOnlyIndex interface. It checks if two indexes are compatible for Snowflake by comparing their included properties.
    /// If the indexes are not compatible and the shouldThrow parameter is set to true, it throws an InvalidOperationException.
    /// </summary>
    /// <param name="index"></param>
    /// <param name="duplicateIndex"></param>
    /// <param name="storeObject"></param>
    /// <param name="shouldThrow"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static bool AreCompatibleForSnowflake(
        this IReadOnlyIndex index,
        IReadOnlyIndex duplicateIndex,
        in StoreObjectIdentifier storeObject,
        bool shouldThrow)
    {
        if (index.GetIncludeProperties() != duplicateIndex.GetIncludeProperties())
        {
            if (index.GetIncludeProperties() == null
                || duplicateIndex.GetIncludeProperties() == null
                || !SameColumnNames(index, duplicateIndex, storeObject))
            {
                if (shouldThrow)
                {
                    throw new InvalidOperationException(
                        SnowflakeStrings.DuplicateIndexIncludedMismatch(
                            index.DisplayName(),
                            index.DeclaringEntityType.DisplayName(),
                            duplicateIndex.DisplayName(),
                            duplicateIndex.DeclaringEntityType.DisplayName(),
                            index.DeclaringEntityType.GetSchemaQualifiedTableName(),
                            index.GetDatabaseName(storeObject),
                            FormatInclude(index, storeObject),
                            FormatInclude(duplicateIndex, storeObject)));
                }

                return false;
            }
        }

        return true;

        static bool SameColumnNames(
            IReadOnlyIndex index,
            IReadOnlyIndex duplicateIndex,
            StoreObjectIdentifier storeObject
        )
            => index.GetIncludeProperties()!.Select(
                    p => index.DeclaringEntityType.FindProperty(p)!.GetColumnName(storeObject))
                .SequenceEqual(
                    duplicateIndex.GetIncludeProperties()!.Select(
                        p => duplicateIndex.DeclaringEntityType.FindProperty(p)!.GetColumnName(storeObject)));
    }

    /// <summary>
    /// Formats the included properties of an index as a string.
    /// </summary>
    /// <param name="index"></param>
    /// <param name="storeObject"></param>
    /// <returns></returns>
    private static string FormatInclude(IReadOnlyIndex index, StoreObjectIdentifier storeObject)
        => index.GetIncludeProperties() == null
            ? "{}"
            : "{'"
              + string.Join(
                  "', '",
                  index.GetIncludeProperties()!.Select(
                      p => index.DeclaringEntityType.FindProperty(p)
                          ?.GetColumnName(storeObject)))
              + "'}";
}