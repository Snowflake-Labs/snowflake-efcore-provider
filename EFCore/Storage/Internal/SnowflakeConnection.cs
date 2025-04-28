using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Collections.Concurrent;
using Snowflake.Data.Client;

namespace Snowflake.EntityFrameworkCore.Storage.Internal;

/// <summary>
///   Represents a Snowflake-specific connection.
/// </summary>
public class SnowflakeConnection : RelationalConnection, ISnowflakeConnection
{
    private SnowflakeDbConnectionStringBuilder _connectionStringBuilder;

    // Compensate for slow Snowflake database creation
    private const int DefaultMasterConnectionCommandTimeout = 60;

    private static readonly ConcurrentDictionary<string, bool> MultipleActiveResultSetsEnabledMap = new();

    /// <summary>
    ///    Creates a new instance of <see cref="SnowflakeConnection" />.
    /// </summary>
    /// <param name="dependencies"></param>
    public SnowflakeConnection(RelationalConnectionDependencies dependencies)
        : base(dependencies)
    {
        this._connectionStringBuilder = null;
    }

    /// <inheritdoc />
    protected override void OpenDbConnection(bool errorsExpected)
    {
        if (errorsExpected
            && DbConnection is SnowflakeDbConnection sqlConnection)
        {
            sqlConnection.Open();
        }
        else
        {
            DbConnection.Open();
        }
    }

    /// <inheritdoc />
    protected override DbConnection CreateDbConnection()
        => new SnowflakeDbConnection(GetValidatedConnectionString());

    /// <summary>
    ///    Creates a new connection for the current <see cref="DbContext" />.
    /// </summary>
    /// <returns></returns>
    public virtual ISnowflakeConnection CreateMasterConnection()
    {
        var masterConnectionString = this.ConnectionStringBuilder.GetMasterConnectionString();

        var contextOptions = new DbContextOptionsBuilder()
            .UseSnowflake(
                masterConnectionString,
                b => b.CommandTimeout(CommandTimeout ?? DefaultMasterConnectionCommandTimeout))
            .Options;

        return new SnowflakeConnection(Dependencies with { ContextOptions = contextOptions });
    }

    /// <inheritdoc />
    public string Database
        => !string.IsNullOrEmpty(this.DbConnection.Database)
            ? this.DbConnection.Database
            : this.ConnectionStringBuilder.GetDatabase();

    /// <inheritdoc />
    public string Schema => this.ConnectionStringBuilder.GetSchema();

    /// <inheritdoc />
    public virtual bool IsMultipleActiveResultSetsEnabled =>
        false;

    /// <inheritdoc />
    protected override bool SupportsAmbientTransactions
        => true;

    private SnowflakeDbConnectionStringBuilder ConnectionStringBuilder
        => this._connectionStringBuilder ??= new SnowflakeDbConnectionStringBuilder()
        {
            ConnectionString = this.ConnectionString ?? string.Empty,
        };
}