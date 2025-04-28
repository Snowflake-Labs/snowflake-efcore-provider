using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Snowflake.EntityFrameworkCore.Design.Internal;
using Snowflake.EntityFrameworkCore.Metadata.Internal;

namespace Snowflake.EntityFrameworkCore.TestUtilities;

public class SnowflakeDatabaseCleaner : RelationalDatabaseCleaner
{
    protected override IDatabaseModelFactory CreateDatabaseModelFactory(ILoggerFactory loggerFactory)
    {
        var services = new ServiceCollection();
        services.AddEntityFrameworkSnowflake();

        new SnowflakeDesignTimeServices().ConfigureDesignTimeServices(services);

        return services
            .BuildServiceProvider() // No scope validation; cleaner violates scopes, but only resolve services once.
            .GetRequiredService<IDatabaseModelFactory>();
    }

    protected override bool AcceptTable(DatabaseTable table)
        => table is not DatabaseView;

    protected override bool AcceptIndex(DatabaseIndex index)
        => false;

    private readonly string _dropViewsSql = @"
DECLARE
  c1 CURSOR FOR
    SELECT table_name
    FROM information_schema.views
    WHERE table_schema NOT IN ('INFORMATION_SCHEMA', 'PUBLIC');

BEGIN
  FOR rec IN c1 DO
    EXECUTE IMMEDIATE 'DROP VIEW ""' || rec.table_name || '"" CASCADE';
  END FOR;
END;";

    protected override void OpenConnection(IRelationalConnection connection)
    {
        base.OpenConnection(connection);
        // TODO REVIEW IF WE COULD SEND IT IN EACH Statement as parameter.
        var cmd = connection.DbConnection.CreateCommand();
        cmd.CommandText = "ALTER SESSION SET MULTI_STATEMENT_COUNT = 0";
        cmd.ExecuteNonQuery();
    }

    protected override string BuildCustomSql(DatabaseModel databaseModel)
        => _dropViewsSql;

    protected override string BuildCustomEndingSql(DatabaseModel databaseModel)
        => _dropViewsSql
            + @"
DECLARE
  c1 CURSOR FOR
    SELECT schema_name
    FROM information_schema.schemata
    WHERE schema_name NOT IN ('INFORMATION_SCHEMA', 'PUBLIC');

BEGIN
  FOR rec IN c1 DO
    EXECUTE IMMEDIATE 'DROP SCHEMA ""' || rec.schema_name || '"" CASCADE';
  END FOR;
  RETURN 'Success';
END;";

    protected override MigrationOperation Drop(DatabaseTable table)
        => AddSqlServerSpecificAnnotations(base.Drop(table), table);

    protected override MigrationOperation Drop(DatabaseForeignKey foreignKey)
        => AddSqlServerSpecificAnnotations(base.Drop(foreignKey), foreignKey.Table);

    protected override MigrationOperation Drop(DatabaseIndex index)
        => AddSqlServerSpecificAnnotations(base.Drop(index), index.Table);

    private static TOperation AddSqlServerSpecificAnnotations<TOperation>(TOperation operation, DatabaseTable table)
        where TOperation : MigrationOperation
    {

        if (table[SnowflakeAnnotationNames.IsTemporal] != null)
        {
            operation[SnowflakeAnnotationNames.IsTemporal]
                = table[SnowflakeAnnotationNames.IsTemporal];
        }

        return operation;
    }
}
