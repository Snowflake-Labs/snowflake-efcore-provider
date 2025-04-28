using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Snowflake.Data.Client;
using Snowflake.EntityFrameworkCore.Constants;
using Snowflake.EntityFrameworkCore.Internal;

namespace Snowflake.EntityFrameworkCore.Migrations.Internal;

public class SnowflakeMigrationCommandExecutor : MigrationCommandExecutor
{
    private static Dictionary<int, string> CustomErrorMapping = new()
    {
        { SnowflakeErrorCodes.StatementCountNotMatch, SnowflakeStrings.MultipleStatementsInSqlOperationNotSupported() }
    };
    
    /// <inheritdoc />
    public override async Task ExecuteNonQueryAsync(
        IEnumerable<MigrationCommand> migrationCommands,
        IRelationalConnection connection,
        CancellationToken cancellationToken = new ())
    {
        try
        {
            await base.ExecuteNonQueryAsync(migrationCommands, connection, cancellationToken);
        }
        catch (SnowflakeDbException e)
        {
            HandleExecutionError(e);
            throw;
        }
    }

    /// <inheritdoc />
    public override void ExecuteNonQuery(
        IEnumerable<MigrationCommand> migrationCommands,
        IRelationalConnection connection)
    {
        try
        {
            base.ExecuteNonQuery(migrationCommands, connection);
        }
        catch (SnowflakeDbException e)
        {
            HandleExecutionError(e);
            throw;
        }
    }

    private static void HandleExecutionError(SnowflakeDbException e)
    {
        if (CustomErrorMapping.TryGetValue(e.ErrorCode, out var customError))
        {
            throw new InvalidOperationException(customError, e);
        }
    }
}