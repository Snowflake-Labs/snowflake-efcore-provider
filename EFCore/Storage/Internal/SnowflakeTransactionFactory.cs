using System;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage;

namespace Snowflake.EntityFrameworkCore.Storage.Internal;

/// <summary>
///    A factory for creating <see cref="SnowflakeTransaction" /> instances.
/// </summary>
public class SnowflakeTransactionFactory : IRelationalTransactionFactory
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="RelationalTransactionFactory" /> class.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this service.</param>
    public SnowflakeTransactionFactory(RelationalTransactionFactoryDependencies dependencies)
    {
        Dependencies = dependencies;
    }

    /// <summary>
    ///     Relational provider-specific dependencies for this service.
    /// </summary>
    protected virtual RelationalTransactionFactoryDependencies Dependencies { get; }

    /// <inheritdoc />
    public virtual RelationalTransaction Create(
        IRelationalConnection connection,
        DbTransaction transaction,
        Guid transactionId,
        IDiagnosticsLogger<DbLoggerCategory.Database.Transaction> logger,
        bool transactionOwned)
        => new SnowflakeTransaction(connection, transaction, transactionId, logger, transactionOwned,
            Dependencies.SqlGenerationHelper);
}