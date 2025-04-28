using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage;

namespace Snowflake.EntityFrameworkCore.Storage.Internal;

/// <summary>
///    Represents a Snowflake-specific transaction.
/// </summary>
public class SnowflakeTransaction : RelationalTransaction
{
    /// <summary>
    ///    Creates a new <see cref="SnowflakeTransaction" /> instance.
    /// </summary>
    /// <param name="connection"></param>
    /// <param name="transaction"></param>
    /// <param name="transactionId"></param>
    /// <param name="logger"></param>
    /// <param name="transactionOwned"></param>
    /// <param name="sqlGenerationHelper"></param>
    public SnowflakeTransaction(
        IRelationalConnection connection,
        DbTransaction transaction,
        Guid transactionId,
        IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> logger,
        bool transactionOwned,
        ISqlGenerationHelper sqlGenerationHelper)
        : base(connection, transaction, transactionId, logger, transactionOwned, sqlGenerationHelper)
    {
    }

    /// <inheritdoc />
    public override bool SupportsSavepoints => false;

    /// <inheritdoc />
    public override void ReleaseSavepoint(string name)
    {
    }

    /// <inheritdoc />
    public override Task ReleaseSavepointAsync(string name, CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}