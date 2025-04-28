using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Globalization;
using System.Text;
using Microsoft.Extensions.Primitives;
using Snowflake.EntityFrameworkCore.Metadata;
using Snowflake.EntityFrameworkCore.Storage.Internal;

namespace Snowflake.EntityFrameworkCore.Update.Internal;

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Update;
using Snowflake.EntityFrameworkCore.Metadata;
using Snowflake.EntityFrameworkCore.Utilities;

/// <summary>
/// Represents a Snowflake-specific update SQL generator.
/// </summary>
public class SnowflakeUpdateSqlGenerator : UpdateAndSelectSqlGenerator, ISnowflakeUpdateSqlGenerator
{
    /// <summary>
    /// Creates a new instance of <see cref="SnowflakeUpdateSqlGenerator" />
    /// </summary>
    /// <param name="dependencies"></param>
    public SnowflakeUpdateSqlGenerator(
        UpdateSqlGeneratorDependencies dependencies)
        : base(dependencies)
    {
    }

    /// <inheritdoc />
    public override void AppendObtainNextSequenceValueOperation(
        StringBuilder commandStringBuilder,
        string name,
        string? schema
        )
    {
        SqlGenerationHelper.DelimitIdentifier(commandStringBuilder, name, schema);
        commandStringBuilder.Append(".NEXTVAL");
    }

    /// <inheritdoc />
    public override ResultSetMapping AppendInsertOperation(
        StringBuilder commandStringBuilder,
        IReadOnlyModificationCommand command,
        int commandPosition,
        out bool requiresTransaction)
    {
        // If no database-generated columns need to be read back, just do a simple INSERT (default behavior).
        // If there are generated columns but we can use OUTPUT without INTO (i.e. no triggers), we can do a simple INSERT ... OUTPUT,
        // which is also the default behavior, doesn't require a transaction and is the most efficient.
        if (command.ColumnModifications.All(o => !o.IsRead))
        {
            return AppendInsertReturningOperation(commandStringBuilder, command, commandPosition,
                out requiresTransaction);
        }

        return AppendInsertAndSelectOperation(commandStringBuilder, command, commandPosition, out requiresTransaction);
    }

    /// <summary>
    /// Creates a new instance of <see cref="SnowflakeUpdateSqlGenerator" />
    /// </summary>
    /// <param name="commandStringBuilder"></param>
    /// <param name="name"></param>
    /// <param name="schema"></param>
    /// <param name="writeOperations"></param>
    /// <param name="readOperations"></param>
    protected override void AppendInsertCommand(
        StringBuilder commandStringBuilder,
        string name,
        string? schema,
        IReadOnlyList<IColumnModification> writeOperations,
        IReadOnlyList<IColumnModification> readOperations)
    {
        // In Snowflake the OUTPUT clause is placed differently (before the VALUES instead of at the end)
        AppendInsertCommandHeader(commandStringBuilder, name, schema, writeOperations);
        AppendOutputClause(commandStringBuilder, readOperations);
        AppendValuesHeader(commandStringBuilder, writeOperations);
        AppendValues(commandStringBuilder, name, schema, writeOperations);
        commandStringBuilder.AppendLine(SqlGenerationHelper.StatementTerminator);
    }

    /// <inheritdoc />
    public override ResultSetMapping AppendUpdateOperation(
            StringBuilder commandStringBuilder,
            IReadOnlyModificationCommand command,
            int commandPosition,
            out bool requiresTransaction)
        // Snowflake does not support OUTPUT CLAUSE.
        => AppendUpdateAndSelectOperation(commandStringBuilder, command, commandPosition, out requiresTransaction);

    /// <inheritdoc />
    protected override void AppendUpdateCommand(
        StringBuilder commandStringBuilder,
        string name,
        string? schema,
        IReadOnlyList<IColumnModification> writeOperations,
        IReadOnlyList<IColumnModification> readOperations,
        IReadOnlyList<IColumnModification> conditionOperations,
        bool appendReturningOneClause = false)
    {
        // In Snowflake the OUTPUT clause is placed differently (before the WHERE instead of at the end)
        AppendUpdateCommandHeader(commandStringBuilder, name, schema, writeOperations);
        AppendOutputClause(commandStringBuilder, readOperations, appendReturningOneClause ? "1" : null);
        AppendWhereClause(commandStringBuilder, conditionOperations);
        commandStringBuilder.AppendLine(SqlGenerationHelper.StatementTerminator);
    }

    /// <inheritdoc />
    protected override void AppendUpdateColumnValue(
        ISqlGenerationHelper updateSqlGeneratorHelper,
        IColumnModification columnModification,
        StringBuilder stringBuilder,
        string name,
        string? schema)
    {
        if (columnModification.JsonPath is not (null or "$"))
        {
            stringBuilder.Append("JSON_MODIFY(");
            updateSqlGeneratorHelper.DelimitIdentifier(stringBuilder, columnModification.ColumnName);

            // using strict so that we don't remove json elements when they are assigned NULL value
            stringBuilder.Append(", 'strict ");
            stringBuilder.Append(columnModification.JsonPath);
            stringBuilder.Append("', ");

            if (columnModification.Property is { IsPrimitiveCollection: false })
            {
                base.AppendUpdateColumnValue(updateSqlGeneratorHelper, columnModification, stringBuilder, name, schema);
            }
            else
            {
                stringBuilder.Append("JSON_QUERY(");
                base.AppendUpdateColumnValue(updateSqlGeneratorHelper, columnModification, stringBuilder, name, schema);
                stringBuilder.Append(")");
            }

            stringBuilder.Append(")");
        }
        else
        {
            if (columnModification.TypeMapping is ISnowflakeStringLiteralRequired)
            {
                AppendSqlLiteral(stringBuilder, columnModification, null,null);
            }
            else if ((columnModification.UseCurrentValue && columnModification.Value == null) || (columnModification.UseOriginalValue && columnModification.OriginalValue == null))
            {
                stringBuilder.Append("NULL");
            }
            else
            {
                base.AppendUpdateColumnValue(updateSqlGeneratorHelper, columnModification, stringBuilder, name, schema);
            }
        }
    }
    
    /// <inheritdoc />
    protected override void AppendValues(
        StringBuilder commandStringBuilder,
        string name,
        string? schema,
        IReadOnlyList<IColumnModification> operations)
    {
        if (operations.Count > 0)
        {
            commandStringBuilder
                .Append('(')
                .AppendJoin(
                    operations,
                    (this, name, schema),
                    (sb, o, p) =>
                    {
                        if (o.IsWrite)
                        {
                            var (g, n, s) = p;
                            if (!o.UseCurrentValueParameter || o.TypeMapping is ISnowflakeStringLiteralRequired)
                            {
                                AppendSqlLiteral(sb, o, n, s);
                            }
                            else if((o.UseCurrentValue && o.Value == null) || (o.UseOriginalValue && o.OriginalValue == null))
                            {
                                sb.Append("NULL");
                            }
                            else
                            {
                                g.SqlGenerationHelper.GenerateParameterNamePlaceholder(sb, o.ParameterName);
                            }
                        }
                        else
                        {
                            sb.Append("DEFAULT");
                        }
                    })
                .Append(')');
        }
    }
    
    /// <inheritdoc />
    protected override void AppendWhereCondition(
        StringBuilder commandStringBuilder,
        IColumnModification columnModification,
        bool useOriginalValue)
    {
        SqlGenerationHelper.DelimitIdentifier(commandStringBuilder, columnModification.ColumnName);

        var parameterValue = useOriginalValue
            ? columnModification.OriginalValue
            : columnModification.Value;

        if (parameterValue == null)
        {
            commandStringBuilder.Append(" IS NULL");
        }
        else
        {
            commandStringBuilder.Append(" = ");
            if (!columnModification.UseParameter)
            {
                AppendSqlLiteral(commandStringBuilder, columnModification, null, null);
            }
            else if (columnModification.UseOriginalValue && columnModification.TypeMapping is ISnowflakeStringLiteralRequired)
            {
                commandStringBuilder.Append(columnModification.TypeMapping!.GenerateProviderValueSqlLiteral(columnModification.OriginalValue));
            }
            else
            {
                if ((columnModification.UseCurrentValue && columnModification.Value == null) || (columnModification.UseOriginalValue && columnModification.OriginalValue == null))
                {
                    commandStringBuilder.Append("NULL");
                }
                else
                {
                    SqlGenerationHelper.GenerateParameterNamePlaceholder(
                        commandStringBuilder, useOriginalValue
                            ? columnModification.OriginalParameterName!
                            : columnModification.ParameterName!);
                }
            }
        }
    }

    /// <inheritdoc />
    public override ResultSetMapping AppendDeleteOperation(
            StringBuilder commandStringBuilder,
            IReadOnlyModificationCommand command,
            int commandPosition,
            out bool requiresTransaction)
        // We normally do a simple DELETE, with an OUTPUT clause emitting "1" for concurrency checking.
        // However, if OUTPUT (without INTO) isn't supported (e.g. there are triggers), we do DELETE+SELECT.
        => AppendDeleteAndSelectOperation(commandStringBuilder, command, commandPosition, out requiresTransaction);

    /// <inheritdoc />
    protected override void AppendDeleteCommand(
        StringBuilder commandStringBuilder,
        string name,
        string? schema,
        IReadOnlyList<IColumnModification> readOperations,
        IReadOnlyList<IColumnModification> conditionOperations,
        bool appendReturningOneClause = false)
    {
        // In Snowflake the OUTPUT clause is placed differently (before the WHERE instead of at the end)
        AppendDeleteCommandHeader(commandStringBuilder, name, schema);
        AppendOutputClause(commandStringBuilder, readOperations, appendReturningOneClause ? "1" : null);
        AppendWhereClause(commandStringBuilder, conditionOperations);
        commandStringBuilder.AppendLine(SqlGenerationHelper.StatementTerminator);
    }

    /// <inheritdoc />
    public virtual List<ResultSetMapping> AppendBulkInsertOperation(
        StringBuilder commandStringBuilder,
        IReadOnlyList<IReadOnlyModificationCommand> modificationCommands,
        int commandPosition,
        out bool requiresTransaction)
    {
        var firstCommand = modificationCommands[0];
        List<ResultSetMapping> results = new List<ResultSetMapping>();

        if (modificationCommands.Count == 1)
        {
            return
                [AppendInsertOperation(commandStringBuilder, firstCommand, commandPosition, out requiresTransaction)];
        }

        var table = StoreObjectIdentifier.Table(firstCommand.TableName, modificationCommands[0].Schema);

        var readOperations = firstCommand.ColumnModifications.Where(o => o.IsRead).ToList();
        var writeOperations = firstCommand.ColumnModifications.Where(o => o.IsWrite).ToList();
        var keyOperations = firstCommand.ColumnModifications.Where(o => o.IsKey).ToList();

        var writableOperations = modificationCommands[0].ColumnModifications
            .Where(
                o =>
                    o.Property?.GetValueGenerationStrategy(table) != SnowflakeValueGenerationStrategy.IdentityColumn
                    && o.Property?.GetComputedColumnSql() is null
                    && o.Property?.GetColumnType() is not "rowversion" and not "timestamp")
            .ToList();


        if (readOperations.Count == 0)
        {
            // We have no values to read, just use a plain old multi-row INSERT.
            AppendInsertMultipleRows(commandStringBuilder, modificationCommands, writeOperations,
                out requiresTransaction);
            return modificationCommands.Select(_ => ResultSetMapping.NoResults).ToList();
        }

        foreach (var command in modificationCommands)
        {
            results.Add(AppendInsertOperation(commandStringBuilder, modificationCommands[0], commandPosition++));
        }

        requiresTransaction = true;
        return results;
    }

    private ResultSetMapping AppendInsertMultipleRows(
        StringBuilder commandStringBuilder,
        IReadOnlyList<IReadOnlyModificationCommand> modificationCommands,
        List<IColumnModification> writeOperations,
        out bool requiresTransaction)
    {
        Check.DebugAssert(writeOperations.Count > 0, $"writeOperations.Count is {writeOperations.Count}");

        var name = modificationCommands[0].TableName;
        var schema = modificationCommands[0].Schema;

        AppendInsertCommandHeader(commandStringBuilder, name, schema, writeOperations);
        AppendValuesHeader(commandStringBuilder, writeOperations);
        AppendValues(commandStringBuilder, name, schema, writeOperations);
        for (var i = 1; i < modificationCommands.Count; i++)
        {
            commandStringBuilder.AppendLine(",");
            AppendValues(commandStringBuilder, name, schema,
                modificationCommands[i].ColumnModifications.Where(o => o.IsWrite).ToList());
        }

        commandStringBuilder.AppendLine(SqlGenerationHelper.StatementTerminator);

        requiresTransaction = false;

        return ResultSetMapping.NoResults;
    }

    /// <summary>
    ///     Appends a <c>WHERE</c> clause involving rows affected.
    /// </summary>
    /// <param name="commandStringBuilder">The builder to which the SQL should be appended.</param>
    /// <param name="operations">The operations from which to build the conditions.</param>
    protected override void AppendWhereAffectedClause(
        StringBuilder commandStringBuilder,
        IReadOnlyList<IColumnModification> operations)
    {
        Check.NotNull(commandStringBuilder, nameof(commandStringBuilder));
        Check.NotNull(operations, nameof(operations));

        var identityModification = operations.Where(IsIdentityOperation).ToList();
        var whereModifications = operations.Where(m => m.IsKey && !m.IsRead).ToList();

        if (whereModifications.Count > 0)
        {
            commandStringBuilder
                .AppendLine()
                .Append("WHERE ")
                .AppendJoin(
                    whereModifications, (sb, v) =>
                    {
                        AppendWhereCondition(sb, v, v.UseOriginalValueParameter);
                        return true;
                    }, " AND ");
        }

        if (identityModification.Count > 0)
        {
            commandStringBuilder
                .AppendLine()
                .Append("ORDER BY ")
                .AppendJoin(
                    identityModification, (sb, v) =>
                    {
                        SqlGenerationHelper.DelimitIdentifier(sb, v.ColumnName);
                        sb.Append(" DESC");
                        return true;
                    }, ", ")
                .Append(" LIMIT 1");
        }
    }

    private const string InsertedTableBaseName = "@inserted";
    private const string ToInsertTableAlias = "i";
    private const string PositionColumnName = "_Position";
    private const string PositionColumnDeclaration = "[" + PositionColumnName + "] [int]";
    private const string FullPositionColumnName = ToInsertTableAlias + "." + PositionColumnName;

    private ResultSetMapping AppendMergeWithOutput(
        StringBuilder commandStringBuilder,
        IReadOnlyList<IReadOnlyModificationCommand> modificationCommands,
        List<IColumnModification> writeOperations,
        List<IColumnModification> readOperations,
        out bool requiresTransaction)
    {
        var name = modificationCommands[0].TableName;
        var schema = modificationCommands[0].Schema;

        AppendMergeCommandHeader(
            commandStringBuilder,
            name,
            schema,
            ToInsertTableAlias,
            modificationCommands,
            writeOperations,
            PositionColumnName);
        AppendOutputClause(
            commandStringBuilder,
            readOperations,
            FullPositionColumnName);
        commandStringBuilder.AppendLine(SqlGenerationHelper.StatementTerminator);

        requiresTransaction = false;

        return ResultSetMapping.NotLastInResultSet | ResultSetMapping.IsPositionalResultMappingEnabled;
    }

    private ResultSetMapping AppendInsertMultipleDefaultRows(
        StringBuilder commandStringBuilder,
        IReadOnlyList<IReadOnlyModificationCommand> modificationCommands,
        List<IColumnModification> writeableOperations,
        out bool requiresTransaction)
    {
        Check.DebugAssert(writeableOperations.Count > 0, $"writeableOperations.Count is {writeableOperations.Count}");

        var name = modificationCommands[0].TableName;
        var schema = modificationCommands[0].Schema;

        AppendInsertCommandHeader(commandStringBuilder, name, schema, writeableOperations);
        AppendValuesHeader(commandStringBuilder, writeableOperations);
        AppendValues(commandStringBuilder, name, schema, writeableOperations);
        for (var i = 1; i < modificationCommands.Count; i++)
        {
            commandStringBuilder.AppendLine(",");
            AppendValues(commandStringBuilder, name, schema, writeableOperations);
        }

        commandStringBuilder.AppendLine(SqlGenerationHelper.StatementTerminator);

        requiresTransaction = false;

        return ResultSetMapping.NoResults;
    }

    private void AppendMergeCommandHeader(
        StringBuilder commandStringBuilder,
        string name,
        string? schema,
        string toInsertTableAlias,
        IReadOnlyList<IReadOnlyModificationCommand> modificationCommands,
        IReadOnlyList<IColumnModification> writeOperations,
        string? additionalColumns = null)
    {
        commandStringBuilder.Append("MERGE ");
        SqlGenerationHelper.DelimitIdentifier(commandStringBuilder, name, schema);

        commandStringBuilder
            .Append(" USING (");

        AppendValuesHeader(commandStringBuilder, writeOperations);
        AppendValues(commandStringBuilder, writeOperations, "0");
        for (var i = 1; i < modificationCommands.Count; i++)
        {
            commandStringBuilder.AppendLine(",");
            AppendValues(
                commandStringBuilder,
                modificationCommands[i].ColumnModifications.Where(o => o.IsWrite).ToList(),
                i.ToString(CultureInfo.InvariantCulture));
        }

        commandStringBuilder
            .Append(") AS ").Append(toInsertTableAlias)
            .Append(" (")
            .AppendJoin(
                writeOperations,
                SqlGenerationHelper,
                (sb, o, helper) => helper.DelimitIdentifier(sb, o.ColumnName));
        if (additionalColumns != null)
        {
            commandStringBuilder
                .Append(", ")
                .Append(additionalColumns);
        }

        commandStringBuilder
            .Append(')')
            .AppendLine(" ON 1=0")
            .AppendLine("WHEN NOT MATCHED THEN")
            .Append("INSERT ")
            .Append('(')
            .AppendJoin(
                writeOperations,
                SqlGenerationHelper,
                (sb, o, helper) => helper.DelimitIdentifier(sb, o.ColumnName))
            .Append(')');

        AppendValuesHeader(commandStringBuilder, writeOperations);
        commandStringBuilder
            .Append('(')
            .AppendJoin(
                writeOperations,
                (toInsertTableAlias, SqlGenerationHelper),
                static (sb, o, state) =>
                {
                    var (alias, helper) = state;
                    sb.Append(alias).Append('.');
                    helper.DelimitIdentifier(sb, o.ColumnName);
                })
            .Append(')');
    }

    /// <inheritdoc />
    public override ResultSetMapping AppendStoredProcedureCall(
        StringBuilder commandStringBuilder,
        IReadOnlyModificationCommand command,
        int commandPosition,
        out bool requiresTransaction)
    {
        Check.DebugAssert(command.StoreStoredProcedure is not null, "command.StoredProcedure is not null");

        var storedProcedure = command.StoreStoredProcedure;

        var resultSetMapping = ResultSetMapping.NoResults;

        foreach (var resultColumn in storedProcedure.ResultColumns)
        {
            resultSetMapping = ResultSetMapping.LastInResultSet;

            if (resultColumn == command.RowsAffectedColumn)
            {
                resultSetMapping |= ResultSetMapping.ResultSetWithRowsAffectedOnly;
            }
            else
            {
                resultSetMapping = ResultSetMapping.LastInResultSet;
                break;
            }
        }

        Check.DebugAssert(
            storedProcedure.Parameters.Any() || storedProcedure.ResultColumns.Any(),
            "Stored procedure call with neither parameters nor result columns");

        commandStringBuilder.Append("EXEC ");

        if (storedProcedure.ReturnValue is not null)
        {
            var returnValueModification =
                command.ColumnModifications.First(c => c.Column is IStoreStoredProcedureReturnValue);

            Check.DebugAssert(returnValueModification.UseCurrentValueParameter,
                "returnValueModification.UseCurrentValueParameter");
            Check.DebugAssert(!returnValueModification.UseOriginalValueParameter,
                "!returnValueModification.UseOriginalValueParameter");

            SqlGenerationHelper.GenerateParameterNamePlaceholder(commandStringBuilder,
                returnValueModification.ParameterName!);

            commandStringBuilder.Append(" = ");

            resultSetMapping |= ResultSetMapping.HasOutputParameters;
        }

        SqlGenerationHelper.DelimitIdentifier(commandStringBuilder, storedProcedure.Name, storedProcedure.Schema);

        if (storedProcedure.Parameters.Any())
        {
            commandStringBuilder.Append(' ');

            var first = true;

            // Only positional parameter style supported for now, see #28439

            // Note: the column modifications are already ordered according to the sproc parameter ordering
            // (see ModificationCommand.GenerateColumnModifications)
            for (var i = 0; i < command.ColumnModifications.Count; i++)
            {
                var columnModification = command.ColumnModifications[i];

                if (columnModification.Column is not IStoreStoredProcedureParameter parameter)
                {
                    continue;
                }

                if (first)
                {
                    first = false;
                }
                else
                {
                    commandStringBuilder.Append(", ");
                }

                Check.DebugAssert(columnModification.UseParameter,
                    "Column modification matched a parameter, but UseParameter is false");

                SqlGenerationHelper.GenerateParameterNamePlaceholder(
                    commandStringBuilder, columnModification.UseOriginalValueParameter
                        ? columnModification.OriginalParameterName!
                        : columnModification.ParameterName!);

                // Note that in/out parameters also get suffixed with OUTPUT in Snowflake
                if (parameter.Direction.HasFlag(ParameterDirection.Output))
                {
                    commandStringBuilder.Append(" OUTPUT");
                    resultSetMapping |= ResultSetMapping.HasOutputParameters;
                }
            }
        }

        commandStringBuilder.AppendLine(SqlGenerationHelper.StatementTerminator);

        requiresTransaction = true;

        return resultSetMapping;
    }

    private void AppendValues(
        StringBuilder commandStringBuilder,
        IReadOnlyList<IColumnModification> operations,
        string additionalLiteral)
    {
        if (operations.Count > 0)
        {
            commandStringBuilder
                .Append('(')
                .AppendJoin(
                    operations,
                    SqlGenerationHelper,
                    (sb, o, helper) =>
                    {
                        if (o.IsWrite)
                        {
                            helper.GenerateParameterName(sb, o.ParameterName!);
                        }
                        else
                        {
                            sb.Append("DEFAULT");
                        }
                    })
                .Append(", ")
                .Append(additionalLiteral)
                .Append(')');
        }
    }

    private void AppendDeclareTable(
        StringBuilder commandStringBuilder,
        string name,
        int index,
        IReadOnlyList<IColumnModification> operations,
        string? additionalColumns = null)
    {
        commandStringBuilder
            .Append("DECLARE ")
            .Append(name)
            .Append(index)
            .Append(" TABLE (")
            .AppendJoin(
                operations,
                this,
                (sb, o, generator) =>
                {
                    generator.SqlGenerationHelper.DelimitIdentifier(sb, o.ColumnName);
                    sb.Append(' ').Append(GetTypeNameForCopy(o.Property!));
                });

        if (additionalColumns != null)
        {
            commandStringBuilder
                .Append(", ")
                .Append(additionalColumns);
        }

        commandStringBuilder
            .Append(')')
            .AppendLine(SqlGenerationHelper.StatementTerminator);
    }

    private static string GetTypeNameForCopy(IProperty property)
    {
        var typeName = property.GetColumnType();

        return property.ClrType == typeof(byte[])
               && (typeName.Equals("rowversion", StringComparison.OrdinalIgnoreCase)
                   || typeName.Equals("timestamp", StringComparison.OrdinalIgnoreCase))
            ? property.IsNullable ? "varbinary(8)" : "binary(8)"
            : typeName;
    }

    /// <inheritdoc />
    protected override void AppendReturningClause(
        StringBuilder commandStringBuilder,
        IReadOnlyList<IColumnModification> operations,
        string? additionalValues = null)
        => AppendOutputClause(commandStringBuilder, operations, additionalValues);

    private void AppendOutputClause(
        StringBuilder commandStringBuilder,
        IReadOnlyList<IColumnModification> operations,
        string? additionalReadValues = null)
    {
        if (operations.Count > 0 || additionalReadValues is not null)
        {
            commandStringBuilder
                .AppendLine()
                .Append("OUTPUT ")
                .AppendJoin(
                    operations,
                    SqlGenerationHelper,
                    (sb, o, helper) =>
                    {
                        sb.Append("INSERTED.");
                        helper.DelimitIdentifier(sb, o.ColumnName);
                    });

            if (additionalReadValues is not null)
            {
                if (operations.Count > 0)
                {
                    commandStringBuilder.Append(", ");
                }

                commandStringBuilder.Append(additionalReadValues);
            }
        }
    }

    private void AppendOutputIntoClause(
        StringBuilder commandStringBuilder,
        IReadOnlyList<IColumnModification> operations,
        string tableName,
        int tableIndex,
        string? additionalColumns = null)
    {
        if (operations.Count > 0 || additionalColumns is not null)
        {
            AppendOutputClause(commandStringBuilder, operations, additionalColumns);

            commandStringBuilder.AppendLine()
                .Append("INTO ").Append(tableName).Append(tableIndex);
        }
    }

    private ResultSetMapping AppendInsertSingleRowWithOutputInto(
        StringBuilder commandStringBuilder,
        IReadOnlyModificationCommand command,
        IReadOnlyList<IColumnModification> keyOperations,
        IReadOnlyList<IColumnModification> readOperations,
        int commandPosition,
        out bool requiresTransaction)
    {
        var name = command.TableName;
        var schema = command.Schema;
        var operations = command.ColumnModifications;

        var writeOperations = operations.Where(o => o.IsWrite).ToList();

        AppendDeclareTable(commandStringBuilder, InsertedTableBaseName, commandPosition, keyOperations);

        AppendInsertCommandHeader(commandStringBuilder, name, schema, writeOperations);
        AppendOutputIntoClause(commandStringBuilder, keyOperations, InsertedTableBaseName, commandPosition);
        AppendValuesHeader(commandStringBuilder, writeOperations);
        AppendValues(commandStringBuilder, name, schema, writeOperations);
        commandStringBuilder.Append(SqlGenerationHelper.StatementTerminator);

        requiresTransaction = true;

        return AppendSelectCommand(
            commandStringBuilder, readOperations, keyOperations, InsertedTableBaseName, commandPosition, name, schema);
    }

    private ResultSetMapping AppendSelectCommand(
        StringBuilder commandStringBuilder,
        IReadOnlyList<IColumnModification> readOperations,
        IReadOnlyList<IColumnModification> keyOperations,
        string insertedTableName,
        int insertedTableIndex,
        string tableName,
        string? schema,
        string? orderColumn = null)
    {
        if (readOperations.SequenceEqual(keyOperations))
        {
            commandStringBuilder
                .AppendLine()
                .Append("SELECT ")
                .AppendJoin(
                    readOperations,
                    SqlGenerationHelper,
                    (sb, o, helper) => helper.DelimitIdentifier(sb, o.ColumnName, "i"))
                .Append(" FROM ")
                .Append(insertedTableName).Append(insertedTableIndex).Append(" i");
        }
        else
        {
            commandStringBuilder
                .AppendLine()
                .Append("SELECT ")
                .AppendJoin(
                    readOperations,
                    SqlGenerationHelper,
                    (sb, o, helper) => helper.DelimitIdentifier(sb, o.ColumnName, "t"))
                .Append(" FROM ");
            SqlGenerationHelper.DelimitIdentifier(commandStringBuilder, tableName, schema);
            commandStringBuilder
                .AppendLine(" t")
                .Append("INNER JOIN ")
                .Append(insertedTableName).Append(insertedTableIndex)
                .Append(" i")
                .Append(" ON ")
                .AppendJoin(
                    keyOperations, (sb, c) =>
                    {
                        sb.Append('(');
                        SqlGenerationHelper.DelimitIdentifier(sb, c.ColumnName, "t");
                        sb.Append(" = ");
                        SqlGenerationHelper.DelimitIdentifier(sb, c.ColumnName, "i");
                        sb.Append(')');
                    }, " AND ");
        }

        if (orderColumn != null)
        {
            commandStringBuilder
                .AppendLine()
                .Append("ORDER BY ");
            SqlGenerationHelper.DelimitIdentifier(commandStringBuilder, orderColumn, "i");
        }

        commandStringBuilder
            .AppendLine(SqlGenerationHelper.StatementTerminator)
            .AppendLine();

        return ResultSetMapping.LastInResultSet;
    }

    /// <inheritdoc />
    protected override ResultSetMapping AppendSelectAffectedCountCommand(
        StringBuilder commandStringBuilder,
        string name,
        string? schema,
        int commandPosition)
    {
        return ResultSetMapping.NoResults;
    }

    /// <inheritdoc />
    public override void AppendBatchHeader(StringBuilder commandStringBuilder)
    {
    }

    /// <inheritdoc />
    public override void PrependEnsureAutocommit(StringBuilder commandStringBuilder)
    {
    }

    /// <inheritdoc />
    protected override void AppendIdentityWhereCondition(StringBuilder commandStringBuilder,
        IColumnModification columnModification)
    {
    }

    /// <inheritdoc />
    protected override void AppendRowsAffectedWhereCondition(StringBuilder commandStringBuilder,
        int expectedRowsAffected)
    {
    }
}