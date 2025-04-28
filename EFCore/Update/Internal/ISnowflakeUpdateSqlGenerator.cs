using System.Text;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Update;

namespace Snowflake.EntityFrameworkCore.Update.Internal;

/// <inheritdoc />
public interface ISnowflakeUpdateSqlGenerator : IUpdateSqlGenerator
{
    /// <summary>
    ///    Appends a SQL command to the given <see cref="StringBuilder" /> that represents an insert operation for a single row.
    /// </summary>
    /// <param name="commandStringBuilder"></param>
    /// <param name="modificationCommands"></param>
    /// <param name="commandPosition"></param>
    /// <param name="requiresTransaction"></param>
    /// <returns></returns>
    List<ResultSetMapping> AppendBulkInsertOperation(
        StringBuilder commandStringBuilder,
        IReadOnlyList<IReadOnlyModificationCommand> modificationCommands,
        int commandPosition,
        out bool requiresTransaction);


    /// <summary>
    ///   Appends a SQL command to the given <see cref="StringBuilder" /> that represents an insert operation for a single row.
    /// </summary>
    /// <param name="commandStringBuilder"></param>
    /// <param name="modificationCommands"></param>
    /// <param name="commandPosition"></param>
    /// <returns></returns>
    List<ResultSetMapping> AppendBulkInsertOperation(
        StringBuilder commandStringBuilder,
        IReadOnlyList<IReadOnlyModificationCommand> modificationCommands,
        int commandPosition)
        => AppendBulkInsertOperation(commandStringBuilder, modificationCommands, commandPosition, out _);
}