using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Storage;
using Snowflake.Data.Client;
using Snowflake.EntityFrameworkCore.Internal;
using Snowflake.EntityFrameworkCore.Migrations.Operations;

namespace Snowflake.EntityFrameworkCore.Storage.Internal;

/// <summary>
/// Represents a Snowflake-specific database creator.
/// </summary>
public class SnowflakeDatabaseCreator : RelationalDatabaseCreator
{
    private readonly ISnowflakeConnection _connection;
    private readonly IRawSqlCommandBuilder _rawSqlCommandBuilder;

    /// <summary>
    /// Creates a new instance of <see cref="SnowflakeDatabaseCreator" />
    /// </summary>
    /// <param name="dependencies"></param>
    /// <param name="connection"></param>
    /// <param name="rawSqlCommandBuilder"></param>
    public SnowflakeDatabaseCreator(
        RelationalDatabaseCreatorDependencies dependencies,
        ISnowflakeConnection connection,
        IRawSqlCommandBuilder rawSqlCommandBuilder)
        : base(dependencies)
    {
        _connection = connection;
        _rawSqlCommandBuilder = rawSqlCommandBuilder;
    }

    /// <inheritdoc />
    public override void Create()
    {
        using var masterConnection = _connection.CreateMasterConnection();
        Dependencies.MigrationCommandExecutor
            .ExecuteNonQuery(CreateCreateOperations(), masterConnection);

        ClearPool();
    }

    /// <inheritdoc />
    public override bool Exists()
    {
        using var masterConnection = _connection.CreateMasterConnection();
        return Dependencies.ExecutionStrategy.Execute(
            masterConnection,
            connection => (bool)DatabaseExistsCommand(this._connection.Database)
                .ExecuteScalar(
                    new RelationalCommandParameterObject(
                        connection,
                        null,
                        null,
                        Dependencies.CurrentContext.Context,
                        Dependencies.CommandLogger, CommandSource.Migrations))!,
            null);
    }

    /// <inheritdoc />
    public override async Task CreateAsync(CancellationToken cancellationToken = default)
    {
        var masterConnection = _connection.CreateMasterConnection();
        await using (masterConnection.ConfigureAwait(false))
        {
            await Dependencies.MigrationCommandExecutor
                .ExecuteNonQueryAsync(CreateCreateOperations(), masterConnection, cancellationToken)
                .ConfigureAwait(false);

            ClearPool();
        }
    }

    /// <inheritdoc />
    public override async Task<bool> ExistsAsync(CancellationToken cancellationToken = default)
    {
        var masterConnection = _connection.CreateMasterConnection();
        Task<bool> result;
        await using (masterConnection.ConfigureAwait(false))
        {
            result = (Task<bool>)await Dependencies.ExecutionStrategy.ExecuteAsync(
                masterConnection,
                (connection, ct) => DatabaseExistsCommand(this._connection.Database)
                    .ExecuteScalarAsync(
                        new RelationalCommandParameterObject(
                            connection,
                            null,
                            null,
                            Dependencies.CurrentContext.Context,
                            Dependencies.CommandLogger, CommandSource.Migrations),
                        cancellationToken: ct),
                null,
                cancellationToken).ConfigureAwait(false);
        }

        return result.Result;
    }

    /// <inheritdoc />
    public override bool HasTables()
        => Dependencies.ExecutionStrategy.Execute(
            _connection,
            connection => (long)CreateHasTablesCommand()
                              .ExecuteScalar(
                                  new RelationalCommandParameterObject(
                                      connection,
                                      null,
                                      null,
                                      Dependencies.CurrentContext.Context,
                                      Dependencies.CommandLogger, CommandSource.Migrations))!
                          != 0,
            null);

    /// <inheritdoc />
    public override async Task<bool> HasTablesAsync(CancellationToken cancellationToken = default)
        => (int)(await Dependencies.ExecutionStrategy.ExecuteAsync(
               _connection,
               (connection, ct) => CreateHasTablesCommand()
                   .ExecuteScalarAsync(
                       new RelationalCommandParameterObject(
                           connection,
                           null,
                           null,
                           Dependencies.CurrentContext.Context,
                           Dependencies.CommandLogger, CommandSource.Migrations),
                       cancellationToken: ct),
               null,
               cancellationToken).ConfigureAwait(false))!
           != 0;

    private IRelationalCommand CreateHasTablesCommand()
        => _rawSqlCommandBuilder
            .Build(
                @"SELECT DECODE(COUNT(*), 0, 0, 1) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA=CURRENT_SCHEMA()");

    private IRelationalCommand DatabaseExistsCommand(string databaseName)
        => _rawSqlCommandBuilder
            .Build(
                $"""
                 SELECT 
                    TO_BOOLEAN(COUNT(1)) FROM INFORMATION_SCHEMA.DATABASES
                 WHERE 
                    DATABASE_NAME = '{databaseName.ToUpper()}';
                 """);


    private IReadOnlyList<MigrationCommand> CreateCreateOperations()
    {
        return Dependencies.MigrationsSqlGenerator.Generate(
        [
            new SnowflakeCreateDatabaseOperation()
            {
                Name = this._connection.Database,
                Collation = Dependencies.CurrentContext.Context.GetService<IDesignTimeModel>()
                    .Model.GetRelationalModel().Collation
            },
            new SnowflakeCreateSchemaOperation
            {
                Name = this._connection.Schema,
                DatabaseName = this._connection.Database,
            }
        ]);
    }

    /// <inheritdoc />
    public override void Delete()
    {
        ClearAllPools();

        using var masterConnection = _connection.CreateMasterConnection();
        Dependencies.MigrationCommandExecutor
            .ExecuteNonQuery(CreateDropCommands(), masterConnection);
    }

    /// <inheritdoc />
    public override async Task DeleteAsync(CancellationToken cancellationToken = default)
    {
        ClearAllPools();

        var masterConnection = _connection.CreateMasterConnection();
        await using var _ = masterConnection.ConfigureAwait(false);
        await Dependencies.MigrationCommandExecutor
            .ExecuteNonQueryAsync(CreateDropCommands(), masterConnection, cancellationToken)
            .ConfigureAwait(false);
    }

    private IReadOnlyList<MigrationCommand> CreateDropCommands()
    {
        var databaseName = _connection.Database;

        if (string.IsNullOrEmpty(databaseName))
        {
            throw new InvalidOperationException(SnowflakeStrings.NoInitialCatalog);
        }

        var operations = new MigrationOperation[] { new SnowflakeDropDatabaseOperation { Name = databaseName } };

        return Dependencies.MigrationsSqlGenerator.Generate(operations);
    }


    /// <summary>
    /// Clear connection pools in case there are active connections that are pooled
    /// </summary>
    private static void ClearAllPools()
        => SnowflakeDbConnectionPool.ClearAllPools();

    /// <summary>
    /// Clear connection pool for the database connection since after the 'create database' call, a previously
    /// invalid connection may now be valid.
    /// </summary>
    private void ClearPool()
        => SnowflakeDbConnectionPool.ClearAllPools();
}