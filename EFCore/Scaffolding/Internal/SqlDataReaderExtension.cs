using System.Data.Common;

namespace Snowflake.EntityFrameworkCore.Scaffolding.Internal;

/// <summary>
///   A <see cref="ProviderCodeGenerator" /> for Snowflake.
/// </summary>
public static class SqlDataReaderExtension
{
    /// <summary>
    /// Get the value or default of a DbDataReader column.
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="name"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T? GetValueOrDefault<T>(this DbDataReader reader, string name)
    {
        var idx = reader.GetOrdinal(name);
        return reader.IsDBNull(idx)
            ? default
            : reader.GetFieldValue<T>(idx);
    }

    /// <summary>
    /// Get the informational schema identifier.
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public static string? GetInformationalSchemaIdentifier(this DbDataReader reader, string name)
    {
        return reader.GetValueOrDefault<string>(name);
    }

    /// <summary>
    /// Get the value or default of a DbDataRecord column.
    /// </summary>
    /// <param name="record"></param>
    /// <param name="name"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T? GetValueOrDefault<T>(this DbDataRecord record, string name)
    {
        var idx = record.GetOrdinal(name);
        return record.IsDBNull(idx)
            ? default
            : (T)record.GetValue(idx);
    }

    /// <summary>
    /// Get the informational schema identifier.
    /// </summary>
    /// <param name="record"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public static string GetInformationalSchemaIdentifier(this DbDataRecord record, string name)
    {
        return record.GetValueOrDefault<string>(name);
    }

    /// <summary>
    /// Get the value of a DbDataRecord column.
    /// </summary>
    /// <param name="record"></param>
    /// <param name="name"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T GetFieldValue<T>(this DbDataRecord record, string name)
        => (T)record.GetValue(record.GetOrdinal(name));
}