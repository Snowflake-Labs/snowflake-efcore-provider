using Microsoft.EntityFrameworkCore.Storage;

namespace Snowflake.EntityFrameworkCore.Storage.Internal;

/// <summary>
/// Snowflake specific implementation of <see cref="IRelationalConnection" />
/// </summary>
public interface ISnowflakeConnection : IRelationalConnection
{
    /// <summary>
    /// Gets the database from the connection string.
    /// </summary>
    string Database { get; }

    /// <summary>
    /// Gets the schema from the connection string.
    /// </summary>
    string Schema { get; }

    /// <summary>
    /// Creates a new connection to the database.
    /// </summary>
    /// <returns></returns>
    ISnowflakeConnection CreateMasterConnection();

    /// <summary>
    /// Indicates whether multiple active result sets are enabled.
    /// </summary>
    bool IsMultipleActiveResultSetsEnabled { get; }
}