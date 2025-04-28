namespace Snowflake.EntityFrameworkCore.Update.Internal;

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Update;
using Storage.Internal;

public class SnowflakeModificationCommandBatch : AffectedCountModificationCommandBatch
{
    private const int DefaultNetworkPacketSizeBytes = 4096;
    private const int MaxScriptLength = 65536 * DefaultNetworkPacketSizeBytes / 2;

    /// <summary>
    ///     The Snowflake limit on parameters, including two extra parameters to sp_executesql (@stmt and @params).
    /// </summary>
    private const int MaxParameterCount = 2100 - 2;

    private readonly List<IReadOnlyModificationCommand> _pendingBulkInsertCommands = new();

    public  SnowflakeModificationCommandBatch(
        ModificationCommandBatchFactoryDependencies dependencies,
        int maxBatchSize)
        : base(dependencies, maxBatchSize)
    {
    }

    protected  new virtual ISnowflakeUpdateSqlGenerator UpdateSqlGenerator
        => (ISnowflakeUpdateSqlGenerator)base.UpdateSqlGenerator;

    /// <inheritdoc />
    protected override  void RollbackLastCommand(IReadOnlyModificationCommand modificationCommand)
    {
        if (_pendingBulkInsertCommands.Count > 0)
        {
            _pendingBulkInsertCommands.RemoveAt(_pendingBulkInsertCommands.Count - 1);
        }

        base.RollbackLastCommand(modificationCommand);
    }

    /// <inheritdoc />
    protected override  bool IsValid()
    {
        if (ParameterValues.Count > MaxParameterCount)
        {
            return false;
        }

        var sqlLength = SqlBuilder.Length;

        if (_pendingBulkInsertCommands.Count > 0)
        {
            // Conservative heuristic for the length of the pending bulk insert commands.
            // See EXEC sp_server_info.
            var numColumns = _pendingBulkInsertCommands[0].ColumnModifications.Count;

            sqlLength +=
                numColumns * 128 // column name lengths
                + 128 // schema name length
                + 128 // table name length
                + _pendingBulkInsertCommands.Count * numColumns * 6 // column parameter placeholders
                + 300; // some extra fixed overhead
        }

        return sqlLength < MaxScriptLength;
    }

    private void ApplyPendingBulkInsertCommands()
    {
        if (_pendingBulkInsertCommands.Count == 0)
        {
            return;
        }

        var commandPosition = ResultSetMappings.Count;

        var wasCachedCommandTextEmpty = IsCommandTextEmpty;

        var resultSetMappingOperations = UpdateSqlGenerator.AppendBulkInsertOperation(
            SqlBuilder, _pendingBulkInsertCommands, commandPosition, out var requiresTransaction);

        SetRequiresTransaction(!wasCachedCommandTextEmpty || requiresTransaction);

        
        // for (var i = 0; i < _pendingBulkInsertCommands.Count; i++)
        // {
        //     resultSetMapping.ForEach(r =>ResultSetMappings.Add(resultSetMapping));
        // }

        foreach (var resultSetMapping in resultSetMappingOperations)
        {
            ResultSetMappings.Add(resultSetMapping);
            // All result mappings are marked as "not last", mark the last one as "last".
            if (resultSetMapping.HasFlag(ResultSetMapping.HasResultRow))
            {
                ResultSetMappings[^1] &= ~ResultSetMapping.NotLastInResultSet;
                ResultSetMappings[^1] |= ResultSetMapping.LastInResultSet;
            }
        }
    }

    /// <inheritdoc />
    public override  bool TryAddCommand(IReadOnlyModificationCommand modificationCommand)
    {
        // If there are any pending bulk insert commands and the new command is incompatible with them (not an insert, insert into a
        // separate table..), apply the pending commands.
        if (_pendingBulkInsertCommands.Count > 0
            && (modificationCommand.EntityState == EntityState.Added
                || modificationCommand.StoreStoredProcedure is not null
                || !CanBeInsertedInSameStatement(_pendingBulkInsertCommands[0], modificationCommand)))
        {
            ApplyPendingBulkInsertCommands();
            _pendingBulkInsertCommands.Clear();
            return false;
        }

        return base.TryAddCommand(modificationCommand);
    }

    /// <inheritdoc />
    protected override  void AddCommand(IReadOnlyModificationCommand modificationCommand)
    {
        // TryAddCommand above already applied any pending commands if the new command is incompatible with them.
        // So if the new command is an insert, just append it to pending, otherwise do the regular add logic.
        if (modificationCommand is { EntityState: EntityState.Added, StoreStoredProcedure: null })
        {
            _pendingBulkInsertCommands.Add(modificationCommand);
            AddParameters(modificationCommand);
        }
        else
        {
            base.AddCommand(modificationCommand);
        }
    }

    private static bool CanBeInsertedInSameStatement(
        IReadOnlyModificationCommand firstCommand,
        IReadOnlyModificationCommand secondCommand)
        => firstCommand.TableName == secondCommand.TableName
            && firstCommand.Schema == secondCommand.Schema
            && firstCommand.ColumnModifications.Where(o => o.IsWrite).Select(o => o.ColumnName).SequenceEqual(
                secondCommand.ColumnModifications.Where(o => o.IsWrite).Select(o => o.ColumnName))
            && firstCommand.ColumnModifications.Where(o => o.IsRead).Select(o => o.ColumnName).SequenceEqual(
                secondCommand.ColumnModifications.Where(o => o.IsRead).Select(o => o.ColumnName));

    /// <inheritdoc />
    public override  void Complete(bool moreBatchesExpected)
    {
        ApplyPendingBulkInsertCommands();
        
        if (this.StoreCommand != null)
            throw new InvalidOperationException(RelationalStrings.ModificationCommandBatchAlreadyComplete);
        if (!this.RequiresTransaction)
            this.UpdateSqlGenerator.PrependEnsureAutocommit(this.SqlBuilder);
        this.RelationalCommandBuilder.Append(this.SqlBuilder.ToString());
        CleanupCommandBuilderParameters();

        var parameterValues = new Dictionary<string, object?>(this.ParameterValues.Count);
        var paramIndex = 1;
        foreach (var command in ModificationCommands)
        {
            var parametersData = GetFilteredParametersData(command);
            
            foreach (var parameter in parametersData)
            {
                if(HandleTypedParameterAsLiteralValue(parameter.TypeMapping))
                    continue;
                
                RelationalCommandBuilder.AddParameter(
                    parameter.Item1,
                    $"{paramIndex}",
                    parameter.TypeMapping!,
                    parameter.IsNullable);

                parameterValues.Add(parameter.Item1, parameter.Item4);

                paramIndex++;
            }

            paramIndex = AddKeyColumnsParameters(command, paramIndex, parameterValues);
        }
            
        RelationalCommandBuilder.AddParameter(
            "MULTI_STATEMENT_COUNT",
            "MULTI_STATEMENT_COUNT",
             new SnowflakeDecimalTypeMapping("int"),
            false
        );
        parameterValues.Add("MULTI_STATEMENT_COUNT", 0);
        
        this.StoreCommand = new RawSqlCommand(this.RelationalCommandBuilder.Build(), parameterValues);

    }

    private int AddKeyColumnsParameters(IReadOnlyModificationCommand command, int paramIndex, Dictionary<string, object> parameterValues)
    {
        if (command.EntityState != EntityState.Added && command.EntityState != EntityState.Modified)
            return paramIndex;
            
        var keyConditionsParameters = command.ColumnModifications.Where(o => o.IsKey && !o.IsRead).ToList();
        var hasReadColumns = command.ColumnModifications.Any(o => o.IsRead);
        if(keyConditionsParameters.Count == 0 || !hasReadColumns)
            return paramIndex;
            
        foreach (var keyParameter in keyConditionsParameters)
        {
            var invariantName = $"kp{paramIndex}";
            RelationalCommandBuilder.AddParameter(
                invariantName,
                $"{paramIndex}",
                keyParameter.TypeMapping!,
                keyParameter.IsNullable);

            parameterValues.Add(invariantName, keyParameter.Value);

            paramIndex++;
        }

        return paramIndex;
    }

    private void CleanupCommandBuilderParameters()
    {
        var parametersCount = this.RelationalCommandBuilder.Parameters.Count();
        while(parametersCount >= 1)
        {
            this.RelationalCommandBuilder.RemoveParameterAt(--parametersCount);
        }
    }

    private static bool HandleTypedParameterAsLiteralValue(RelationalTypeMapping typeMapping)
    {
        return typeMapping is ISnowflakeStringLiteralRequired;
    }

    private IEnumerable<(string Name, RelationalTypeMapping TypeMapping, bool? IsNullable, object Value)> GetFilteredParametersData(IReadOnlyModificationCommand command)
    {
        var filteredColumns = new List<(string Name, RelationalTypeMapping TypeMapping, bool? IsNullable, object Value)>();

        foreach (var column in command.ColumnModifications)
        {
            if(column.IsRead)
                continue;

            if (!string.IsNullOrEmpty(column.ParameterName) && column.UseCurrentValueParameter && column.Value != null)
            {
                filteredColumns.Add((column.ParameterName, column.TypeMapping!, column.IsNullable, column.Value));
            }
            
            if (!string.IsNullOrEmpty(column.OriginalParameterName) && column.UseOriginalValueParameter && column.OriginalValue != null)
            {
                filteredColumns.Add((column.OriginalParameterName, column.TypeMapping!, column.IsNullable, column.OriginalValue));
            }
        }
        return filteredColumns.OrderBy(t => int.Parse(Dependencies.SqlGenerationHelper.GenerateParameterName(t.Item1)));
    }

    /// <summary>
    ///     Consumes the data reader created by <see cref="M:Microsoft.EntityFrameworkCore.Update.ReaderModificationCommandBatch.Execute(Microsoft.EntityFrameworkCore.Storage.IRelationalConnection)" />.
    /// </summary>
    /// <param name="reader">The data reader.</param>
    protected override void Consume(RelationalDataReader reader)
    {
        int num1 = 0;
        try
        {
            bool? nullable1 = new bool?();
            bool flag1 = false;
            while (num1 < this.ResultSetMappings.Count)
            {
                ResultSetMapping resultSetMapping = this.ResultSetMappings[num1];
                if (resultSetMapping.HasFlag((Enum)ResultSetMapping.HasResultRow))
                {
                    nullable1 = reader.DbDataReader.NextResult();
                    bool? nullable2 = nullable1;
                    bool flag2 = false;
                    if (nullable2.GetValueOrDefault() == flag2 & nullable2.HasValue)
                        throw new InvalidOperationException(RelationalStrings.MissingResultSetWhenSaving);
                    num1 = (resultSetMapping.HasFlag((Enum)ResultSetMapping.ResultSetWithRowsAffectedOnly)
                        ? this.ConsumeResultSetWithRowsAffectedOnly(num1, reader)
                        : this.ConsumeResultSet(num1, reader)) + 1;
                    
                    nullable1 = reader.DbDataReader.NextResult();
                    
                }
                else
                    ++num1;

                if (resultSetMapping.HasFlag((Enum)ResultSetMapping.HasOutputParameters))
                    flag1 = true;
            }

            bool? nullable3 = nullable1;
            bool flag3 = true;
            if (nullable3.GetValueOrDefault() == flag3 & nullable3.HasValue)
                this.Dependencies.UpdateLogger.UnexpectedTrailingResultSetWhenSaving();
            reader.Close();
            if (!flag1)
                return;
            int baseParameterIndex = 0;
            num1 = 0;
            while (num1 < this.ResultSetMappings.Count)
            {
                IReadOnlyModificationCommand modificationCommand = this.ModificationCommands[num1];
                if (this.ResultSetMappings[num1].HasFlag((Enum)ResultSetMapping.HasOutputParameters))
                {
                    DbParameter dbParameter =
                        modificationCommand.RowsAffectedColumn is IStoreStoredProcedureParameter rowsAffectedColumn
                            ? reader.DbCommand.Parameters[baseParameterIndex + rowsAffectedColumn.Position]
                            : (modificationCommand.StoreStoredProcedure.ReturnValue != null
                                ? reader.DbCommand.Parameters[baseParameterIndex++]
                                : (DbParameter)null);
                    if (dbParameter != null)
                    {
                        if (!(dbParameter.Value is int num2))
                            throw new InvalidOperationException(
                                RelationalStrings.StoredProcedureRowsAffectedNotPopulated(
                                    (object)modificationCommand.StoreStoredProcedure.SchemaQualifiedName));
                        if (num2 != 1)
                            this.ThrowAggregateUpdateConcurrencyException(reader, num1 + 1, 1, 0);
                    }

                    modificationCommand.PropagateOutputParameters(reader.DbCommand.Parameters, baseParameterIndex);
                }

                ++num1;
                baseParameterIndex += ParameterCount(modificationCommand);
            }
        }
        catch (Exception ex) when (!(ex is DbUpdateException) && !(ex is OperationCanceledException))
        {
            throw new DbUpdateException(RelationalStrings.UpdateStoreException, ex,
                this.ModificationCommands[
                    num1 < this.ModificationCommands.Count ? num1 : this.ModificationCommands.Count - 1].Entries);
        }
    }

    private static int ParameterCount(IReadOnlyModificationCommand command)
    {
        IStoreStoredProcedure storeStoredProcedure = command.StoreStoredProcedure;
        if (storeStoredProcedure != null)
            return storeStoredProcedure.Parameters.Count;
        int num = 0;
        for (int index = 0; index < command.ColumnModifications.Count; ++index)
        {
            IColumnModification columnModification = command.ColumnModifications[index];
            if (columnModification.UseCurrentValueParameter)
                ++num;
            if (columnModification.UseOriginalValueParameter)
                ++num;
        }

        return num;
    }

    /// <summary>
    ///     Consumes the data reader created by <see cref="M:Microsoft.EntityFrameworkCore.Update.ReaderModificationCommandBatch.ExecuteAsync(Microsoft.EntityFrameworkCore.Storage.IRelationalConnection,System.Threading.CancellationToken)" />.
    /// </summary>
    /// <param name="reader">The data reader.</param>
    /// <param name="cancellationToken">A <see cref="T:System.Threading.CancellationToken" /> to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <exception cref="T:System.OperationCanceledException">If the <see cref="T:System.Threading.CancellationToken" /> is canceled.</exception>
    protected override async Task ConsumeAsync(
        RelationalDataReader reader,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        int commandIndex = 0;
        try
        {
            bool? nullable1 = new bool?();
            bool hasOutputParameters = false;
            bool? nullable2;
            while (commandIndex < this.ResultSetMappings.Count)
            {
                ResultSetMapping resultSetMapping = this.ResultSetMappings[commandIndex];
                if (resultSetMapping.HasFlag((Enum)ResultSetMapping.HasResultRow))
                {
                    nullable1 = await reader.DbDataReader.NextResultAsync(cancellationToken).ConfigureAwait(false);
                    nullable2 = nullable1;
                    bool flag = false;
                    if (nullable2.GetValueOrDefault() == flag & nullable2.HasValue)
                        throw new InvalidOperationException(RelationalStrings.MissingResultSetWhenSaving);
                    ConfiguredTaskAwaitable<int> configuredTaskAwaitable;
                    int num;
                    if (resultSetMapping.HasFlag((Enum)ResultSetMapping.ResultSetWithRowsAffectedOnly))
                    {
                        configuredTaskAwaitable =
                            this.ConsumeResultSetWithRowsAffectedOnlyAsync(commandIndex, reader, cancellationToken)
                                .ConfigureAwait(false);
                        num = await configuredTaskAwaitable;
                    }
                    else
                    {
                        configuredTaskAwaitable = this.ConsumeResultSetAsync(commandIndex, reader, cancellationToken)
                            .ConfigureAwait(false);
                        num = await configuredTaskAwaitable;
                    }

                    commandIndex = num + 1;
                    nullable1 = await reader.DbDataReader.NextResultAsync(cancellationToken).ConfigureAwait(false);
                }
                else
                    ++commandIndex;

                if (resultSetMapping.HasFlag((Enum)ResultSetMapping.HasOutputParameters))
                    hasOutputParameters = true;
            }

            nullable2 = nullable1;
            bool flag1 = true;
            if (nullable2.GetValueOrDefault() == flag1 & nullable2.HasValue)
                this.Dependencies.UpdateLogger.UnexpectedTrailingResultSetWhenSaving();
            await reader.CloseAsync().ConfigureAwait(false);
            if (!hasOutputParameters)
                return;
            int parameterCounter = 0;
            commandIndex = 0;
            IReadOnlyModificationCommand command;
            while (commandIndex < this.ResultSetMappings.Count)
            {
                command = this.ModificationCommands[commandIndex];
                if (this.ResultSetMappings[commandIndex].HasFlag((Enum)ResultSetMapping.HasOutputParameters))
                {
                    DbParameter dbParameter =
                        command.RowsAffectedColumn is IStoreStoredProcedureParameter rowsAffectedColumn
                            ? reader.DbCommand.Parameters[parameterCounter + rowsAffectedColumn.Position]
                            : (command.StoreStoredProcedure.ReturnValue != null
                                ? reader.DbCommand.Parameters[parameterCounter++]
                                : (DbParameter)null);
                    if (dbParameter != null)
                    {
                        if (!(dbParameter.Value is int num))
                            throw new InvalidOperationException(
                                RelationalStrings.StoredProcedureRowsAffectedNotPopulated(
                                    (object)command.StoreStoredProcedure.SchemaQualifiedName));
                        if (num != 1)
                            await this.ThrowAggregateUpdateConcurrencyExceptionAsync(reader, commandIndex + 1, 1, 0,
                                cancellationToken).ConfigureAwait(false);
                    }

                    command.PropagateOutputParameters(reader.DbCommand.Parameters, parameterCounter);
                }

                ++commandIndex;
                parameterCounter += ParameterCount(command);
            }

            command = (IReadOnlyModificationCommand)null;
        }
        catch (Exception ex) when (!(ex is DbUpdateException) && !(ex is OperationCanceledException))
        {
            throw new DbUpdateException(RelationalStrings.UpdateStoreException, ex,
                this.ModificationCommands[
                        commandIndex < this.ModificationCommands.Count
                            ? commandIndex
                            : this.ModificationCommands.Count - 1]
                    .Entries);
        }
    }
}
