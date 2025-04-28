namespace Snowflake.EntityFrameworkCore.Migrations.Internal;

using System;
using System.Text;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;

public class SnowflakeHistoryRepository : HistoryRepository
{
    public  SnowflakeHistoryRepository(HistoryRepositoryDependencies dependencies)
        : base(dependencies)
    {
    }

    /// <inheritdoc />
    protected override  string ExistsSql =>
        $"""
         SELECT 
             TO_BOOLEAN(COUNT(1)) FROM INFORMATION_SCHEMA.TABLES
         WHERE 
             TABLE_NAME = '{TableName}'{SqlGenerationHelper.StatementTerminator}
         """;

    /// <inheritdoc />
    protected override  bool InterpretExistsResult(object? value)
        => bool.TryParse(value?.ToString(), out var result) && result;

    /// <inheritdoc />
    public override  string GetCreateIfNotExistsScript()
    {
        var script = this.GetCreateScript();
        return script.Insert(script.IndexOf("CREATE TABLE", StringComparison.Ordinal) + 12, " IF NOT EXISTS");
    }

    /// <inheritdoc />
    public override  string GetBeginIfNotExistsScript(string migrationId)
    {
        var stringTypeMapping = Dependencies.TypeMappingSource.GetMapping(typeof(string));

        return new StringBuilder()
            .AppendLine("EXECUTE IMMEDIATE $$")
            .AppendLine("   BEGIN")
            .AppendLine("       IF ( NOT EXISTS (")
            .Append("           SELECT * FROM ")
            .AppendLine(SqlGenerationHelper.DelimitIdentifier(TableName, TableSchema))
            .Append("           WHERE ")
            .Append(SqlGenerationHelper.DelimitIdentifier(MigrationIdColumnName))
            .Append(" = ")
            .AppendLine(stringTypeMapping.GenerateSqlLiteral(migrationId))
            .Append("       )")
            .AppendLine(")")
            .AppendLine("       THEN")
            .Append("           BEGIN")
            .ToString();
    }

    /// <inheritdoc />
    public override  string GetBeginIfExistsScript(string migrationId)
    {
        var stringTypeMapping = Dependencies.TypeMappingSource.GetMapping(typeof(string));

        return new StringBuilder()
            .AppendLine("EXECUTE IMMEDIATE $$")
            .AppendLine("   BEGIN")
            .AppendLine("       IF ( EXISTS (")
            .Append("           SELECT * FROM ")
            .AppendLine(SqlGenerationHelper.DelimitIdentifier(TableName, TableSchema))
            .Append("           WHERE ")
            .Append(SqlGenerationHelper.DelimitIdentifier(MigrationIdColumnName))
            .Append(" = ")
            .AppendLine(stringTypeMapping.GenerateSqlLiteral(migrationId))
            .Append("       )")
            .AppendLine(")")
            .AppendLine("       THEN")
            .Append("           BEGIN")
            .ToString();
    }

    /// <inheritdoc />
    public override  string GetEndIfScript()
        => new StringBuilder()
            .AppendLine("           END")
            .Append("       END IF")
            .AppendLine(SqlGenerationHelper.StatementTerminator)
            .AppendLine("   END")
            .Append("$$")
            .Append(SqlGenerationHelper.StatementTerminator)
            .ToString();
}
