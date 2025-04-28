using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Snowflake.Data.Client;
using Snowflake.EntityFrameworkCore.Extensions.Internal;
using Snowflake.EntityFrameworkCore.Internal;
using Snowflake.EntityFrameworkCore.Metadata.Internal;
using Snowflake.EntityFrameworkCore.Scaffolding.Metadata;
using Snowflake.EntityFrameworkCore.Utilities;

namespace Snowflake.EntityFrameworkCore.Scaffolding.Internal;

/// <summary>
/// A <see cref="DatabaseModelFactory" /> for Snowflake.
/// </summary>
public class SnowflakeDatabaseModelFactory : DatabaseModelFactory
{
    private readonly IDiagnosticsLogger<DbLoggerCategory.Scaffolding> _logger;
    private readonly IRelationalTypeMappingSource _typeMappingSource;

    private static readonly ISet<string> DateTimePrecisionTypes = new HashSet<string>
    {
        "datetime",
        "time",
        "timestamp_ntz",
        "timestamp_ltz",
        "timestamp_tz"
    };

    private static readonly ISet<string> MaxLengthRequiredTypes
        = new HashSet<string>
        {
            "binary",
            "varbinary",
            "char",
            "varchar",
            "nchar",
            "nvarchar",
            "TEXT"
        };


    private const string NamePartRegex
        = @"(?:(?:(?<part{0}>\""(?:(?:\""\"")|[^\""])+\""))|(?<part{0}>[^\.\""\""]+))";

    private static readonly Dictionary<Type, Func<string, object>> ParserFunctionsDictionary = new()
    {
        { typeof(string), val => val },
        { typeof(bool), EvaluateBoolean },
        { typeof(Guid), val => Guid.TryParse(val, out var guid) ? guid : null },
        { typeof(DateTime), val => DateTime.TryParse(val, out var dateTime) ? dateTime : null },
        { typeof(DateOnly), val => DateOnly.TryParse(val, out var dateOnly) ? dateOnly : null },
        { typeof(TimeOnly), val => TimeOnly.TryParse(val, out var timeOnly) ? timeOnly : null },
        { typeof(DateTimeOffset), val => DateTimeOffset.TryParse(val, out var dateTimeOffset) ? dateTimeOffset : null }
    };

    private static readonly Regex PartExtractor
        = new(
            string.Format(
                CultureInfo.InvariantCulture,
                @"^{0}(?:\.{1})?$",
                string.Format(CultureInfo.InvariantCulture, NamePartRegex, 1),
                string.Format(CultureInfo.InvariantCulture, NamePartRegex, 2)),
            RegexOptions.Compiled,
            TimeSpan.FromMilliseconds(1000));

    private bool _supportHybridTables = false;

    /// <summary>
    /// Creates a new instance of <see cref="SnowflakeDatabaseModelFactory" />
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="typeMappingSource"></param>
    public SnowflakeDatabaseModelFactory(
        IDiagnosticsLogger<DbLoggerCategory.Scaffolding> logger,
        IRelationalTypeMappingSource typeMappingSource)
    {
        _logger = logger;
        _typeMappingSource = typeMappingSource;
    }

    /// <inheritdoc />
    public override DatabaseModel Create(string connectionString, DatabaseModelFactoryOptions options)
    {
        using var connection = new SnowflakeDbConnection(connectionString);
        return Create(connection, options);
    }

    /// <inheritdoc />
    public override DatabaseModel Create(DbConnection connection, DatabaseModelFactoryOptions options)
    {
        var databaseModel = new DatabaseModel();

        var connectionStartedOpen = connection.State == ConnectionState.Open;
        if (!connectionStartedOpen)
        {
            connection.Open();
        }

        try
        {
            _supportHybridTables = CheckHybridTablesSupport(connection);
            databaseModel.DatabaseName = connection.Database;
            databaseModel.DefaultSchema = GetDefaultSchema(connection);

            var accountCollation = GetParameterCollation(connection, "FOR ACCOUNT");
            var databaseCollation = GetParameterCollation(connection, $"FOR DATABASE {connection.Database}");
            if (!string.IsNullOrEmpty(databaseCollation) && databaseCollation != accountCollation)
            {
                databaseModel.Collation = databaseCollation;
            }

            var schemaList = options.Schemas.ToList();
            var schemaFilter = GenerateSchemaFilter(schemaList);
            var tableList = options.Tables.ToList();
            var tableFilter = GenerateTableFilter(tableList.Select(Parse).ToList(), schemaFilter);

            GetSequences(connection, databaseModel, schemaFilter);


            GetTables(connection, databaseModel, tableFilter, databaseCollation);

            foreach (var schema in schemaList
                         .Except(
                             databaseModel.Sequences.Select(s => s.Schema)
                                 .Concat(databaseModel.Tables.Select(t => t.Schema))))
            {
                _logger.MissingSchemaWarning(schema);
            }

            foreach (var table in tableList)
            {
                var (parsedSchema, parsedTableName) = Parse(table);
                if (!databaseModel.Tables.Any(
                        t => !string.IsNullOrEmpty(parsedSchema)
                             && t.Schema == parsedSchema
                             || t.Name == parsedTableName))
                {
                    _logger.MissingTableWarning(table);
                }
            }

            return databaseModel;
        }
        finally
        {
            if (!connectionStartedOpen)
            {
                connection.Close();
            }
        }

        static string GetParameterCollation(DbConnection connection, string forClause)
        {
            using var command = connection.CreateCommand();
            command.CommandText = $"SHOW PARAMETERS LIKE \'DEFAULT_DDL_COLLATION\' {forClause}";
            using var reader = command.ExecuteReader();
            return reader.Read() ? reader.GetString(1) : string.Empty;
        }
    }

    private string? GetDefaultSchema(DbConnection connection)
    {
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT CURRENT_SCHEMA();";

        if (command.ExecuteScalar() is string schema)
        {
            _logger.DefaultSchemaFound(schema);

            return schema;
        }

        return null;
    }

    private static Func<string, string>? GenerateSchemaFilter(IReadOnlyList<string> schemas)
        => schemas.Count > 0
            ? (s =>
            {
                var schemaFilterBuilder = new StringBuilder();
                schemaFilterBuilder.Append(s);
                schemaFilterBuilder.Append(" IN (");
                schemaFilterBuilder.AppendJoin(", ", schemas.Select(EscapeLiteral));
                schemaFilterBuilder.Append(')');
                return schemaFilterBuilder.ToString();
            })
            : null;

    private static (string? Schema, string Table) Parse(string table)
    {
        var match = PartExtractor.Match(table.Trim());

        if (!match.Success)
        {
            throw new InvalidOperationException(SnowflakeStrings.InvalidTableToIncludeInScaffolding(table));
        }

        var part1 = match.Groups["part1"].Value.Replace(@"""""", @"""");
        var part2 = match.Groups["part2"].Value.Replace(@"""""", @"""");

        return string.IsNullOrEmpty(part2) ? (null, part1) : (part1, part2);
    }

    private static Func<string, string, string>? GenerateTableFilter(
        IReadOnlyList<(string? Schema, string Table)> tables,
        Func<string, string>? schemaFilter)
        => schemaFilter != null
           || tables.Count > 0
            ? ((s, t) =>
            {
                var tableFilterBuilder = new StringBuilder();

                var openBracket = false;
                if (schemaFilter != null)
                {
                    tableFilterBuilder
                        .Append('(')
                        .Append(schemaFilter(s));
                    openBracket = true;
                }

                if (tables.Count > 0)
                {
                    if (openBracket)
                    {
                        tableFilterBuilder
                            .AppendLine()
                            .Append("OR ");
                    }
                    else
                    {
                        tableFilterBuilder.Append('(');
                        openBracket = true;
                    }

                    var tablesWithoutSchema = tables.Where(e => string.IsNullOrEmpty(e.Schema)).ToList();
                    if (tablesWithoutSchema.Count > 0)
                    {
                        tableFilterBuilder.Append(t);
                        tableFilterBuilder.Append(" IN (");
                        tableFilterBuilder.AppendJoin(", ", tablesWithoutSchema.Select(e => EscapeLiteral(e.Table)));
                        tableFilterBuilder.Append(')');
                    }

                    var tablesWithSchema = tables.Where(e => !string.IsNullOrEmpty(e.Schema)).ToList();
                    if (tablesWithSchema.Count > 0)
                    {
                        if (tablesWithoutSchema.Count > 0)
                        {
                            tableFilterBuilder.Append(" OR ");
                        }

                        tableFilterBuilder.Append(t);
                        tableFilterBuilder.Append(" IN (");
                        tableFilterBuilder.AppendJoin(", ", tablesWithSchema.Select(e => EscapeLiteral(e.Table)));
                        tableFilterBuilder.Append(") AND CONCAT(");
                        tableFilterBuilder.Append(s);
                        tableFilterBuilder.Append(", '.', ");
                        tableFilterBuilder.Append(t);
                        tableFilterBuilder.Append(") IN (");
                        tableFilterBuilder.AppendJoin(
                            ", ",
                            tablesWithSchema.Select(e =>
                                $"'{ProcessObjectIdentifier(e.Schema)}.{ProcessObjectIdentifier(e.Table)}'"));
                        tableFilterBuilder.Append(')');
                    }
                }

                if (openBracket)
                {
                    tableFilterBuilder.Append(')');
                }

                return tableFilterBuilder.ToString();
            })
            : null;

    private static string EscapeLiteral(string s)
    {
        return $"'{ProcessObjectIdentifier(s)}'";
    }

    private static string ProcessObjectIdentifier(string s)
    {
        if (s.StartsWith("\"") && s.EndsWith("\""))
        {
            return $"{s.Substring(1).Substring(0, s.Length - 2)}";
        }

        return s.ToUpper();
    }

    private void GetSequences(
        DbConnection connection,
        DatabaseModel databaseModel,
        Func<string, string>? schemaFilter)
    {
        using var command = connection.CreateCommand();
        command.CommandText =
            $@"
SELECT SEQUENCE_SCHEMA as schema_name,
SEQUENCE_NAME as name,
DATA_TYPE as type_name,
NUMERIC_PRECISION as precision,
NUMERIC_SCALE as scale,
""INCREMENT""::number as ""INCREMENT"",
(CASE
    WHEN start_value >  9223372036854775807 THEN  9223372036854775807
    WHEN start_value < -9223372036854775808 THEN -9223372036854775808
    ELSE start_value
    END)::number AS start_value,
(CASE
    WHEN minimum_value >  9223372036854775807 THEN  9223372036854775807
    WHEN minimum_value < -9223372036854775808 THEN -9223372036854775808
    ELSE minimum_value
    END)::number AS minimum_value,
(CASE
    WHEN maximum_value >  9223372036854775807 THEN  9223372036854775807
    WHEN maximum_value < -9223372036854775808 THEN -9223372036854775808
    ELSE maximum_value
    END)::number AS maximum_value
FROM {connection.Database}.INFORMATION_SCHEMA.SEQUENCES
";

        if (schemaFilter != null)
        {
            command.CommandText += @"WHERE " + schemaFilter("SEQUENCE_SCHEMA");
        }

        command.CommandText += ';';

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var schema = reader.GetInformationalSchemaIdentifier("SCHEMA_NAME");
            var name = reader.GetInformationalSchemaIdentifier("NAME");
            var storeType = reader.GetString("TYPE_NAME");
            var precision = reader.GetValueOrDefault<long>("PRECISION");
            var scale = reader.GetValueOrDefault<long>("SCALE");
            var incrementBy = (int)reader.GetValueOrDefault<long>("INCREMENT");
            var startValue = reader.GetValueOrDefault<long>("START_VALUE");
            var minValue = reader.GetValueOrDefault<long>("MINIMUM_VALUE");
            var maxValue = reader.GetValueOrDefault<long>("MAXIMUM_VALUE");

            storeType = GetStoreType(storeType, maxLength: 0, precision: precision, scale: scale);

            _logger.SequenceFound(DisplayName(schema, name), storeType, incrementBy, startValue, minValue, maxValue);

            var sequence = new DatabaseSequence
            {
                Database = databaseModel,
                Name = name,
                Schema = schema,
                StoreType = storeType,
                IncrementBy = incrementBy,
                StartValue = startValue,
                MinValue = minValue,
                MaxValue = maxValue
            };

            databaseModel.Sequences.Add(sequence);
        }
    }

    private void GetTables(
        DbConnection connection,
        DatabaseModel databaseModel,
        Func<string, string, string>? tableFilter,
        string? databaseCollation)
    {
        using var command = connection.CreateCommand();
        var tables = new List<DatabaseTable>();

        _logger.Logger.LogDebug("Inside GetTables method");
        var builder = new StringBuilder(
            $@"
SELECT 
    TABLE_SCHEMA as schema,
    TABLE_NAME as name,
    comment,
    CASE ");

        if (SupportsHybridTable())
        {
            builder.Append("WHEN table_type = 'BASE TABLE' AND IS_HYBRID = 'YES' THEN 'HYBRID TABLE'::varchar");
        }

        builder.Append($@"
        WHEN table_type = 'BASE TABLE' AND IS_DYNAMIC = 'YES' THEN 'DYNAMIC TABLE'::varchar
        WHEN table_type = 'BASE TABLE' AND IS_ICEBERG = 'YES' THEN 'ICEBERG TABLE'::varchar
        WHEN table_type = 'BASE TABLE' THEN 'TABLE'::varchar
        ELSE table_type::varchar
    END as type,
    CASE 
        WHEN IS_TRANSIENT = 'YES' THEN 'TRANSIENT'::varchar
        ELSE 'PERMANENT'::varchar
    END as temporal_type
    FROM {connection.Database}.INFORMATION_SCHEMA.TABLES");

        var tableFilterSqlBuilder = new StringBuilder("""
                                                      TABLE_SCHEMA <> 'INFORMATION_SCHEMA' AND TABLE_TYPE <> 'VIEW'
                                                      """);

        if (tableFilter != null)
        {
            tableFilterSqlBuilder
                .AppendLine()
                .Append("AND ")
                .Append(tableFilter("TABLE_SCHEMA", "TABLE_NAME"));
        }

        var tableFilterSql = tableFilterSqlBuilder.ToString();
        builder.AppendLine().Append("WHERE ").Append(tableFilterSql);

        // If views are supported, scaffold them too.
        string? viewFilter = null;

        builder.AppendLine().Append(
            $"""
             UNION
             SELECT  
             TABLE_SCHEMA as schema,
             TABLE_NAME as name,
             comment,
             'VIEW'::varchar as type,
             'PERMANENT'::varchar as temporal_type
             FROM {connection.Database}.INFORMATION_SCHEMA.VIEWS
             """);

        var viewFilterBuilder = new StringBuilder("""
                                                  TABLE_SCHEMA <> 'INFORMATION_SCHEMA'
                                                  """);
        if (tableFilter != null)
        {
            viewFilterBuilder
                .AppendLine()
                .Append("AND ")
                .Append(tableFilter("TABLE_SCHEMA", "TABLE_NAME"));
        }

        viewFilter = viewFilterBuilder.ToString();
        builder.AppendLine().Append("WHERE ").Append(viewFilter);

        builder.Append(";");
        command.CommandText = builder.ToString();

        using (var reader = command.ExecuteReader())
        {
            while (reader.Read())
            {
                var schema = reader.GetValueOrDefault<string>("SCHEMA");
                var name = reader.GetValueOrDefault<string>("NAME");
                var comment = reader.GetValueOrDefault<string>("COMMENT");

                _logger.TableFound(DisplayName(schema, name));
                var table = GetTableByType(databaseModel, reader, name);

                table.Schema = schema;
                table.Comment = comment;

                if (reader.GetValueOrDefault<string>("TEMPORAL_TYPE") == "YES")
                {
                    table[SnowflakeAnnotationNames.IsTemporal] = true;
                }

                tables.Add(table);
            }
        }

        // This is done separately due to MARS property may be turned off
        GetColumns(connection, tables, tableFilterSql, viewFilter, databaseCollation);

        _logger.Logger.LogDebug("Inside GetTables method - after GetColumns");
        GetIndexes(connection, tables, tableFilter);
        GetStandardTablestPrimaryKeys(connection, tables, tableFilter);
        GetTablesForeignKeys(connection, tables, tableFilter);

        foreach (var table in tables)
        {
            databaseModel.Tables.Add(table);
        }
    }

    private static DatabaseTable GetTableByType(DatabaseModel databaseModel, DbDataReader reader, string name)
    {
        var type = reader.GetString("TYPE");
        DatabaseTable table;
        switch (type)
        {
            case "VIEW":
                table = new DatabaseView() { Database = databaseModel, Name = name };
                break;
            case "DYNAMIC TABLE":
                table = new DatabaseDynamicTable() { Database = databaseModel, Name = name };
                break;
            case "EXTERNAL TABLE":
                table = new DatabaseExternalTable() { Database = databaseModel, Name = name };
                break;
            case "HYBRID TABLE":
                table = new DatabaseHybridTable() { Database = databaseModel, Name = name };
                table[SnowflakeAnnotationNames.HybridTable] = true;
                break;
            default:
                table = new DatabaseTable() { Database = databaseModel, Name = name };
                break;
        }

        return table;
    }

    private void GetColumns(
        DbConnection connection,
        IReadOnlyList<DatabaseTable> tables,
        string tableFilter,
        string? viewFilter,
        string? databaseCollation)
    {
        using var command = connection.CreateCommand();
        Check.DebugAssert(viewFilter is not null, "viewFilter is not null");
        var builder = new StringBuilder(
            $"""
             SELECT 
                 TABLE_SCHEMA as table_schema,
                 TABLE_NAME,
                 COLUMN_NAME,
                 ORDINAL_POSITION as ordinal,
                 DATA_TYPE as type_name,
                 CHARACTER_MAXIMUM_LENGTH as max_length,
                 NUMERIC_PRECISION as precision,
                 NUMERIC_SCALE as scale,
                 IS_NULLABLE,
                 IS_IDENTITY,
                 COLUMN_DEFAULT as default_sql,
                 COMMENT,
                 COLLATION_NAME
             FROM
             (
                 SELECT 
                 TABLE_SCHEMA as schema,
                 TABLE_NAME as name,
                 FROM {connection.Database}.INFORMATION_SCHEMA.TABLES WHERE {tableFilter}
             UNION
                 SELECT TABLE_SCHEMA as schema,
                 TABLE_NAME as name
                 FROM {connection.Database}.INFORMATION_SCHEMA.VIEWS WHERE {viewFilter}
             ) o 
             JOIN 
             {connection.Database}.INFORMATION_SCHEMA.COLUMNS c ON o.name = c.table_name WHERE o.schema = c.table_schema
             ORDER BY TABLE_SCHEMA, TABLE_NAME, ORDINAL_POSITION;
             """);


        builder.AppendLine().Append("");

        command.CommandText = builder.ToString();

        using var reader = command.ExecuteReader();
        var tableColumnGroups = reader.Cast<DbDataRecord>()
            .GroupBy(
                ddr => (tableSchema: ddr.GetValueOrDefault<string>("TABLE_SCHEMA"),
                    tableName: ddr.GetFieldValue<string>("TABLE_NAME")));

        foreach (var tableColumnGroup in tableColumnGroups)
        {
            var tableSchema = tableColumnGroup.Key.tableSchema;
            var tableName = tableColumnGroup.Key.tableName;

            var table = tables.Single(t => t.Schema == tableSchema && t.Name == tableName);

            foreach (var dataRecord in tableColumnGroup)
            {
                var columnName = dataRecord.GetInformationalSchemaIdentifier("COLUMN_NAME");
                var ordinal = dataRecord.GetFieldValue<long>("ORDINAL");
                var dataTypeName = dataRecord.GetValueOrDefault<string>("TYPE_NAME");
                var maxLength = dataRecord.GetValueOrDefault<long>("MAX_LENGTH");
                var precision = dataRecord.GetValueOrDefault<long>("PRECISION");
                var scale = dataRecord.GetValueOrDefault<long>("SCALE");
                var nullable = dataRecord.GetValueOrDefault<string>("IS_NULLABLE") == "YES";
                var isIdentity = dataRecord.GetValueOrDefault<string>("IS_IDENTITY") == "YES";
                var defaultValueSql = dataRecord.GetValueOrDefault<string>("DEFAULT_SQL");
                var comment = dataRecord.GetValueOrDefault<string>("COMMENT");
                var collation = dataRecord.GetValueOrDefault<string>("COLLATION_NAME");

                if (dataTypeName is null)
                {
                    _logger.ColumnWithoutTypeWarning(DisplayName(tableSchema, tableName), columnName);
                    continue;
                }

                _logger.ColumnFound(
                    DisplayName(tableSchema, tableName),
                    columnName,
                    ordinal,
                    DisplayName(string.Empty, dataTypeName),
                    maxLength,
                    precision,
                    scale,
                    nullable,
                    isIdentity,
                    defaultValueSql);

                string storeType = GetStoreType(dataTypeName, maxLength, precision, scale);
                string systemTypeName = dataTypeName;


                var column = new DatabaseColumn
                {
                    Table = table,
                    Name = columnName,
                    StoreType = storeType,
                    IsNullable = nullable,
                    DefaultValue = TryParseClrDefault(systemTypeName, defaultValueSql),
                    DefaultValueSql = defaultValueSql,
                    Comment = comment,
                    Collation = collation,
                    ValueGenerated = isIdentity
                        ? ValueGenerated.OnAdd
                        : default(ValueGenerated?)
                };

                table.Columns.Add(column);
            }
        }
    }

    private object? TryParseClrDefault(string dataTypeName, string? defaultValueSql)
    {
        defaultValueSql = defaultValueSql?.Trim();
        if (string.IsNullOrEmpty(defaultValueSql))
        {
            return null;
        }

        var mapping = _typeMappingSource.FindMapping(dataTypeName);
        if (mapping == null)
        {
            return null;
        }

        Unwrap();

        if (defaultValueSql.Equals("NULL", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var type = mapping.ClrType;

        // Regex to match CAST(value AS type(number, number))
        const string pattern = @"CAST\((?:.+?)\s+AS\s+(?:[A-Z_]+\s*(\(\d+(,\d+)?\))?)\)";
        if (Regex.IsMatch(defaultValueSql, pattern))
        {
            // Get the inner value, without the CAST function.
            var innerValue = defaultValueSql.Split("CAST(")[1].Split("AS ")[0];
            // Clean up the inner value.
            innerValue = innerValue.Replace("'", "").Trim();

            return EvaluateType(type, innerValue);
        }

        if (type.IsNumeric())
        {
            return EvaluateNumeric(defaultValueSql, type);
        }

        if ((defaultValueSql.StartsWith('\'') || defaultValueSql.StartsWith("N'", StringComparison.OrdinalIgnoreCase))
            && defaultValueSql.EndsWith('\''))
        {
            var startIndex = defaultValueSql.IndexOf('\'');
            defaultValueSql = defaultValueSql.Substring(startIndex + 1, defaultValueSql.Length - (startIndex + 2));

            return EvaluateType(type, defaultValueSql);
        }

        return null;

        void Unwrap()
        {
            while (defaultValueSql.StartsWith('(') && defaultValueSql.EndsWith(')'))
            {
                defaultValueSql = (defaultValueSql.Substring(1, defaultValueSql.Length - 2)).Trim();
            }
        }
    }

    private static object EvaluateBoolean(string value)
    {
        // Clean up the value.
        var cleanValue = value.Replace("'", "").Trim();

        return cleanValue switch
        {
            "0 <> 0" => false,
            "1 <> 0" => true,
            _ => (bool.TryParse(cleanValue, out var boolValue) ? boolValue : null)
        };
    }

    private static object EvaluateNumeric(object value, Type type)
    {
        try
        {
            if (type.IsInteger())
            {
                // In order to avoid exceptions, we need to convert to double first, if the value is decimal.
                var decimalValue = Convert.ToDouble(value);
                return Convert.ToInt32(decimalValue);
            }

            return Convert.ChangeType(value, type);
        }
        catch
        {
            return null;
        }
    }

    private static object EvaluateType(Type type, string value)
    {
        if (type.IsNumeric())
        {
            return EvaluateNumeric(value, type);
        }

        return ParserFunctionsDictionary.TryGetValue(type, out var parseFunc)
            ? parseFunc(value)
            : null;
    }

    private static string GetStoreType(string dataTypeName, long maxLength, long precision, long scale)
    {
        if (dataTypeName is "decimal" or "numeric" or "NUMBER")
        {
            return $"{dataTypeName}({precision}, {scale})";
        }

        if (DateTimePrecisionTypes.Contains(dataTypeName)
            && scale != 7)
        {
            return $"{dataTypeName}({scale})";
        }

        if (MaxLengthRequiredTypes.Contains(dataTypeName))
        {
            if (maxLength == -1 || maxLength == null)
            {
                return dataTypeName;
            }

            if (dataTypeName is "nvarchar" or "nchar")
            {
                maxLength /= 2;
            }

            return $"{dataTypeName}({maxLength})";
        }

        return dataTypeName;
    }

    private void GetStandardTablestPrimaryKeys(DbConnection connection, IReadOnlyList<DatabaseTable> tables,
        Func<string, string, string>? tableFilter)
    {
        using var commandGetPK = connection.CreateCommand();
        var parameter = (SnowflakeDbParameter)commandGetPK.CreateParameter();
        parameter.ParameterName = "MULTI_STATEMENT_COUNT";
        parameter.DbType = DbType.Int32;
        parameter.Value = 2;
        commandGetPK.Parameters.Add(parameter);
        var commandPKBuilder = new StringBuilder(@$"SHOW PRIMARY KEYS IN DATABASE {connection.Database};
SELECT l.* FROM (SELECT * FROM TABLE(RESULT_SCAN(LAST_QUERY_ID()))) l join {connection.Database}.INFORMATION_SCHEMA.TABLES t ON ""table_name"" = t.TABLE_NAME WHERE 
""database_name"" = t.TABLE_CATALOG and ""schema_name"" = t.TABLE_SCHEMA");
        if (SupportsHybridTable())
        {
            commandPKBuilder.Append(" and t.IS_HYBRID != 'YES'");
        }

        if (tableFilter != null)
        {
            var filter = tableFilter("\"schema_name\"", "\"table_name\"");
            commandPKBuilder.AppendLine($" AND {filter};");
        }

        var commandTextGetPrimaryKeys = commandPKBuilder.ToString();
        commandGetPK.CommandText = commandTextGetPrimaryKeys;
        _logger.Logger.LogDebug($"COMMAND {commandTextGetPrimaryKeys}");
        using var readerPK = commandGetPK.ExecuteReader();
        readerPK.NextResult();

        var tablePks = readerPK.Cast<DbDataRecord>().GroupBy(
            ddr => (tableSchema: ddr.GetValueOrDefault<string>("schema_name"),
                tableName: ddr.GetFieldValue<string>("table_name"))).ToDictionary(t => t.Key, t => t.ToList());

        foreach (var tablePk in tablePks)
        {
            var tableSchema = tablePk.Key.tableSchema;
            var tableName = tablePk.Key.tableName;
            var table = tables.Single(t => t.Schema == tableSchema && t.Name == tableName);
            if (table.PrimaryKey != null)
            {
                continue;
            }

            var primaryKeyGroups = tablePk.Value
                .GroupBy(
                    ddr =>
                        ddr.GetFieldValue<string>("constraint_name"))
                .ToArray();
            Check.DebugAssert(primaryKeyGroups.Length is 0 or 1, "Multiple primary keys found");
            if (primaryKeyGroups.Length == 1)
            {
                if (TryGetPrimaryKey(primaryKeyGroups[0], table, out var primaryKey))
                {
                    _logger.StandardTableFoundPrimaryKey(DisplayName(tableSchema, tableName), primaryKey.Name!);
                    table.PrimaryKey = primaryKey;
                }
            }
        }
    }

    private void GetIndexes(DbConnection connection, IReadOnlyList<DatabaseTable> tables,
        Func<string, string, string>? tableFilter)
    {
        _logger.Logger.LogDebug("Inside Get Indexes");
        using var command = connection.CreateCommand();

        var commandText = $@"
SELECT 
i.TABLE_SCHEMA,
i.TABLE_NAME,
i.NAME as INDEX_NAME,
IFF(tc.CONSTRAINT_TYPE = 'PRIMARY KEY', TRUE, FALSE) AS IS_PRIMARY_KEY,
IFF(tc.CONSTRAINT_TYPE = 'UNIQUE', TRUE, FALSE) AS IS_UNIQUE_CONSTRAINT,
IFF(i.IS_UNIQUE = 'YES', TRUE, FALSE) AS IS_UNIQUE,
ic.NAME as COLUMN_NAME
FROM {connection.Database}.INFORMATION_SCHEMA.INDEXES i
JOIN {connection.Database}.INFORMATION_SCHEMA.INDEX_COLUMNS ic ON i.NAME = ic.INDEX_NAME AND i.TABLE_NAME = ic.TABLE_NAME 
LEFT OUTER JOIN {connection.Database}.INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc ON i.CONSTRAINT_NAME = tc.CONSTRAINT_NAME
WHERE i.TABLE_SCHEMA = ic.TABLE_SCHEMA";
        if (tableFilter != null)
        {
            commandText += @$"
 AND {tableFilter("i.TABLE_SCHEMA", "i.TABLE_NAME")}";
        }

        commandText += @"
ORDER BY [table_schema], [table_name], [index_name];";

        _logger.Logger.LogDebug(commandText);

        command.CommandText = commandText;

        using var reader = command.ExecuteReader();
        var tableIndexGroups = reader.Cast<DbDataRecord>()
            .GroupBy(
                ddr => (tableSchema: ddr.GetValueOrDefault<string>("TABLE_SCHEMA"),
                    tableName: ddr.GetFieldValue<string>("TABLE_NAME")));

        foreach (var tableIndexGroup in tableIndexGroups)
        {
            var tableSchema = tableIndexGroup.Key.tableSchema;
            var tableName = tableIndexGroup.Key.tableName;

            var table = tables.Single(t => t.Schema == tableSchema && t.Name == tableName);

            var primaryKeyGroups = tableIndexGroup.Where(
                ddr =>
                    ddr.GetValueOrDefault<bool>("is_primary_key") ||
                    (
                        ddr.GetValueOrDefault<string>("index_name").EndsWith("_PRIMARY") &&
                        ddr.GetValueOrDefault<bool>("is_unique")
                    )
            ).GroupBy(ddr => ddr.GetFieldValue<string>("index_name")).ToArray();
            
            Check.DebugAssert(primaryKeyGroups.Length is 0 or 1, "Multiple primary keys found");

            if (primaryKeyGroups.Length == 1)
            {
                if (TryGetPrimaryKey(primaryKeyGroups[0], table, out var primaryKey))
                {
                    _logger.PrimaryKeyFound(DisplayName(tableSchema, tableName), primaryKey.Name!);
                    table.PrimaryKey = primaryKey;
                }
            }

            var uniqueConstraintGroups = tableIndexGroup
                .Where(ddr => ddr.GetValueOrDefault<bool>("is_unique_constraint"))
                .GroupBy(
                    ddr =>
                        ddr.GetInformationalSchemaIdentifier("index_name"))
                .ToArray();

            foreach (var uniqueConstraintGroup in uniqueConstraintGroups)
            {
                if (TryGetUniqueConstraint(uniqueConstraintGroup, out var uniqueConstraint))
                {
                    _logger.UniqueConstraintFound(uniqueConstraintGroup.Key!, DisplayName(tableSchema, tableName));
                    table.UniqueConstraints.Add(uniqueConstraint);
                }
            }

            var indexGroups = tableIndexGroup
                .Where(
                    ddr => !ddr.GetValueOrDefault<bool>("IS_PRIMARY_KEY")
                           && !ddr.GetValueOrDefault<bool>("is_unique_constraint"))
                .GroupBy(
                    ddr =>
                        (Name: ddr.GetInformationalSchemaIdentifier("INDEX_NAME"),
                            IsUnique: ddr.GetValueOrDefault<bool>("IS_UNIQUE")))
                .ToArray();

            foreach (var indexGroup in indexGroups)
            {
                if (TryGetIndex(indexGroup, out var index))
                {
                    _logger.IndexFound(indexGroup.Key.Name!, DisplayName(tableSchema, tableName),
                        indexGroup.Key.IsUnique);
                    table.Indexes.Add(index);
                }
            }


            bool TryGetUniqueConstraint(
                IGrouping<string, DbDataRecord> uniqueConstraintGroup,
                [NotNullWhen(true)] out DatabaseUniqueConstraint? uniqueConstraint)
            {
                uniqueConstraint = new DatabaseUniqueConstraint { Table = table, Name = uniqueConstraintGroup.Key };

                foreach (var dataRecord in uniqueConstraintGroup)
                {
                    var columnName = dataRecord.GetInformationalSchemaIdentifier("COLUMN_NAME");
                    var column = table.Columns.FirstOrDefault(c => c.Name == columnName)
                                 ?? table.Columns.FirstOrDefault(
                                     c => c.Name.Equals(columnName, StringComparison.OrdinalIgnoreCase));

                    if (column is null)
                    {
                        return false;
                    }

                    uniqueConstraint.Columns.Add(column);
                }

                return true;
            }

            bool TryGetIndex(
                IGrouping<(string? Name, bool IsUnique),
                    DbDataRecord> indexGroup,
                [NotNullWhen(true)] out DatabaseIndex? index)
            {
                index = new DatabaseIndex
                {
                    Table = table,
                    Name = indexGroup.Key.Name,
                    IsUnique = indexGroup.Key.IsUnique,
                };

                foreach (var dataRecord in indexGroup)
                {
                    var columnName = dataRecord.GetInformationalSchemaIdentifier("COLUMN_NAME");
                    var column = table.Columns.FirstOrDefault(c => c.Name == columnName)
                                 ?? table.Columns.FirstOrDefault(
                                     c => c.Name.Equals(columnName, StringComparison.OrdinalIgnoreCase));

                    if (column is null)
                    {
                        return false;
                    }

                    index.Columns.Add(column);
                }

                return index.Columns.Count > 0;
            }
        }
    }

    private bool TryGetPrimaryKey(
        IGrouping<string, DbDataRecord> primaryKeyGroup,
        DatabaseTable? table,
        [NotNullWhen(true)] out DatabasePrimaryKey? primaryKey)
    {
        primaryKey = new DatabasePrimaryKey { Table = table, Name = primaryKeyGroup.Key };

        foreach (var dataRecord in primaryKeyGroup)
        {
            var columnName = dataRecord.GetInformationalSchemaIdentifier("COLUMN_NAME");
            var column = table.Columns.FirstOrDefault(c => c.Name == columnName)
                         ?? table.Columns.FirstOrDefault(
                             c => c.Name.Equals(columnName, StringComparison.OrdinalIgnoreCase));

            if (column is null)
            {
                return false;
            }

            primaryKey.Columns.Add(column);
        }

        return true;
    }

    private void GetTablesForeignKeys(DbConnection connection, IReadOnlyList<DatabaseTable> tables,
        Func<string, string, string>? tableFilter)
    {
        _logger.Logger.LogDebug("Inside Foreign keys");
        using var command = connection.CreateCommand();

        var parameter = (SnowflakeDbParameter)command.CreateParameter();
        parameter.ParameterName = "MULTI_STATEMENT_COUNT";
        parameter.DbType = DbType.Int32;
        parameter.Value = 2;
        command.Parameters.Add(parameter);
        var commandTextBuilder = new StringBuilder(@$"
SHOW IMPORTED KEYS in DATABASE {connection.Database};
SELECT
    RESULT.*, ");
        commandTextBuilder.Append(SupportsHybridTable() ? "T.IS_HYBRID" : "'NO' AS IS_HYBRID");
        commandTextBuilder.Append(@$"
FROM
    (
        SELECT
            *
        FROM
            TABLE(RESULT_SCAN(LAST_QUERY_ID()))
    ) RESULT
    JOIN {connection.Database}.INFORMATION_SCHEMA.TABLES T ON ""pk_table_name"" = T.TABLE_NAME
WHERE
    ""pk_database_name"" = T.TABLE_CATALOG
    AND ""pk_schema_name"" = T.TABLE_SCHEMA
");

        if (tableFilter != null)
        {
            commandTextBuilder.AppendLine($" AND {tableFilter("\"fk_schema_name\"", "\"fk_table_name\"")}");
        }

        command.CommandText = commandTextBuilder.ToString();

        using var reader = command.ExecuteReader();
        reader.NextResult();
        var tableForeignKeyGroups = reader.Cast<DbDataRecord>()
            .GroupBy(
                ddr => (tableSchema: ddr.GetValueOrDefault<string>("fk_schema_name"),
                    tableName: ddr.GetFieldValue<string>("fk_table_name")));

        foreach (var tableForeignKeyGroup in tableForeignKeyGroups)
        {
            var tableSchema = tableForeignKeyGroup.Key.tableSchema;
            var tableName = tableForeignKeyGroup.Key.tableName;

            var table = tables.Single(t => t.Schema == tableSchema && t.Name == tableName);

            var foreignKeyGroups = tableForeignKeyGroup
                .GroupBy(
                    c => (Name: c.GetValueOrDefault<string>("fk_name"),
                        PrincipalTableSchema: c.GetValueOrDefault<string>("pk_schema_name"),
                        PrincipalTableName: c.GetValueOrDefault<string>("pk_table_name"),
                        OnDeleteAction: c.GetValueOrDefault<string>("delete_rule"),
                        IsHybrid: c.GetValueOrDefault<string>("IS_HYBRID")));

            foreach (var foreignKeyGroup in foreignKeyGroups)
            {
                var fkName = foreignKeyGroup.Key.Name;
                var principalTableSchema = foreignKeyGroup.Key.PrincipalTableSchema;
                var principalTableName = foreignKeyGroup.Key.PrincipalTableName;
                var onDeleteAction = foreignKeyGroup.Key.OnDeleteAction;
                var isHybrid = foreignKeyGroup.Key.IsHybrid;

                if (principalTableName == null)
                {
                    _logger.ForeignKeyReferencesUnknownPrincipalTableWarning(
                        fkName,
                        DisplayName(table.Schema, table.Name));

                    continue;
                }

                if (isHybrid == "YES")
                {
                    _logger.ForeignKeyFound(
                        fkName!,
                        DisplayName(table.Schema, table.Name),
                        DisplayName(principalTableSchema, principalTableName),
                        onDeleteAction!);
                }
                else
                {
                    _logger.StandardTableForeignKeyFound(
                        fkName!,
                        DisplayName(table.Schema, table.Name),
                        DisplayName(principalTableSchema, principalTableName));
                }

                var principalTable = tables.FirstOrDefault(
                                         t => t.Schema == principalTableSchema
                                              && t.Name == principalTableName)
                                     ?? tables.FirstOrDefault(
                                         t => t.Schema?.Equals(principalTableSchema,
                                                  StringComparison.OrdinalIgnoreCase) == true
                                              && t.Name.Equals(principalTableName, StringComparison.OrdinalIgnoreCase));

                if (principalTable == null)
                {
                    _logger.ForeignKeyReferencesMissingPrincipalTableWarning(
                        fkName,
                        DisplayName(table.Schema, table.Name),
                        DisplayName(principalTableSchema, principalTableName));

                    continue;
                }

                var foreignKey = new DatabaseForeignKey
                {
                    Table = table,
                    Name = fkName,
                    PrincipalTable = principalTable,
                    OnDelete = ConvertToReferentialAction(onDeleteAction)
                };

                var invalid = false;

                foreach (var dataRecord in foreignKeyGroup)
                {
                    var columnName = dataRecord.GetInformationalSchemaIdentifier("fk_column_name");
                    var column = table.Columns.FirstOrDefault(c => c.Name == columnName)
                                 ?? table.Columns.FirstOrDefault(
                                     c => c.Name.Equals(columnName, StringComparison.OrdinalIgnoreCase));
                    Check.DebugAssert(column != null, "column is null.");

                    var principalColumnName = dataRecord.GetInformationalSchemaIdentifier("pk_column_name");
                    var principalColumn =
                        foreignKey.PrincipalTable.Columns.FirstOrDefault(c => c.Name == principalColumnName)
                        ?? foreignKey.PrincipalTable.Columns.FirstOrDefault(
                            c => c.Name.Equals(principalColumnName, StringComparison.OrdinalIgnoreCase));
                    if (principalColumn == null)
                    {
                        invalid = true;
                        _logger.ForeignKeyPrincipalColumnMissingWarning(
                            fkName!,
                            DisplayName(table.Schema, table.Name),
                            principalColumnName!,
                            DisplayName(principalTableSchema, principalTableName));
                        break;
                    }

                    foreignKey.Columns.Add(column);
                    foreignKey.PrincipalColumns.Add(principalColumn);
                }

                if (!invalid)
                {
                    if (foreignKey.Columns.SequenceEqual(foreignKey.PrincipalColumns))
                    {
                        _logger.ReflexiveConstraintIgnored(
                            foreignKey.Name!,
                            DisplayName(table.Schema, table.Name));
                    }
                    else
                    {
                        var duplicated = table.ForeignKeys
                            .FirstOrDefault(
                                k => k.Columns.SequenceEqual(foreignKey.Columns)
                                     && k.PrincipalColumns.SequenceEqual(foreignKey.PrincipalColumns)
                                     && k.PrincipalTable.Equals(foreignKey.PrincipalTable));
                        if (duplicated != null)
                        {
                            _logger.DuplicateForeignKeyConstraintIgnored(
                                foreignKey.Name!,
                                DisplayName(table.Schema, table.Name),
                                duplicated.Name!);
                            continue;
                        }

                        table.ForeignKeys.Add(foreignKey);
                    }
                }
            }
        }
    }

    private bool SupportsHybridTable() => _supportHybridTables;

    private static string DisplayName(string? schema, string name)
        => (!string.IsNullOrEmpty(schema) ? schema + "." : "") + name;

    private static ReferentialAction? ConvertToReferentialAction(string? onDeleteAction)
        => onDeleteAction switch
        {
            // Only supported RESTRICT and NO ACTION for Snowflake 
            "RESTRICT" => ReferentialAction.Restrict,
            "NO ACTION" => ReferentialAction.NoAction,
            _ => null
        };

    static bool CheckHybridTablesSupport(DbConnection connection)
    {
        using var command = connection.CreateCommand();
        command.CommandText =
            $"""
             SELECT COUNT(column_name) > 0
             FROM {connection.Database}.information_schema.columns
             WHERE table_schema = 'INFORMATION_SCHEMA'
               AND table_name = 'TABLES'
               AND column_name = 'IS_HYBRID';
             """;

        var result = command.ExecuteScalar();
        return result != null ? Convert.ToBoolean(result) : false;
    }
}