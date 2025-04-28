using System;
using System.Collections.Generic;
using System.Linq;
using Snowflake.Data.Client;

namespace Snowflake.EntityFrameworkCore.Storage.Internal;

/// <summary>
/// Provides extension methods for class <see cref="SnowflakeDbConnectionStringBuilder"/>.
/// </summary>
public static class SnowflakeConnectionStringBuilderExtensions
{
    /// <summary>
    /// Gets a new connection string with no schema and the default database in case the database in the given connection string
    /// doesn't exist. This in order to avoid errors when Snowflake connector tries to open a new connection
    /// with a non-existent database.
    /// </summary>
    /// <param name="builder"></param>
    /// <returns>A new connection string based in the original one, with no schema and the default database.</returns>
    internal static string GetMasterConnectionString(this SnowflakeDbConnectionStringBuilder builder)
    {
        var keys = builder.Keys ?? Array.Empty<string>();
        var connectionString = new List<string>();
        connectionString.AddRange(from object key in keys
            where IsValidKey(key?.ToString())
            select $"{key}={builder[key.ToString()!]}");
        connectionString.Add("db=SNOWFLAKE");
        return string.Join(';', connectionString);
    }

    /// <summary>
    /// Gets the database from the connection string.
    /// </summary>
    /// <param name="builder">
    /// The connection string builder which is a key-value structure
    /// with all the connection string properties.
    /// </param>
    /// <returns>The database in the connection string.</returns>
    internal static string GetDatabase(this SnowflakeDbConnectionStringBuilder builder)
        => builder.TryGetValue("db", out var database) ? database.ToString() : string.Empty;

    /// <summary>
    /// Gets the schema from the connection string.
    /// </summary>
    /// <param name="builder">
    /// The connection string builder which is a key-value structure
    /// with all the connection string properties.
    /// </param>
    /// <returns>The schema in the connection string.</returns>
    internal static string GetSchema(this SnowflakeDbConnectionStringBuilder builder)
        => builder.TryGetValue("schema", out var schema) ? schema.ToString() : "PUBLIC";

    private static bool IsValidKey(string key)
    {
        return key != null && !key.Equals("db") && !key.Equals("schema");
    }
}