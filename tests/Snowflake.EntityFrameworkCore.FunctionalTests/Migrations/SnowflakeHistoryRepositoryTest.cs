using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Snowflake.Data.Client;
using Snowflake.EntityFrameworkCore.TestUtilities;

namespace Snowflake.EntityFrameworkCore.FunctionalTests.Migrations;

public class SnowflakeHistoryRepositoryTest
{
    [ConditionalFact]
    public void GetCreateScript_works()
    {
        var sql = CreateHistoryRepository().GetCreateScript();

        Assert.Equal(
            """
            CREATE TABLE "__EFMigrationsHistory" (
                "MigrationId" nvarchar(150) NOT NULL,
                "ProductVersion" nvarchar(32) NOT NULL,
                CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId"));

            """, sql, ignoreLineEndingDifferences: true);
    }

    [ConditionalFact]
    public void GetCreateScript_works_with_schema()
    {
        var sql = CreateHistoryRepository("my").GetCreateScript();

        Assert.Equal(
            """
            CREATE SCHEMA IF NOT EXISTS "my";
            CREATE TABLE "my"."__EFMigrationsHistory" (
                "MigrationId" nvarchar(150) NOT NULL,
                "ProductVersion" nvarchar(32) NOT NULL,
                CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId"));

            """, sql, ignoreLineEndingDifferences: true);
    }

    [ConditionalFact]
    public void GetCreateIfNotExistsScript_works()
    {
        var sql = CreateHistoryRepository().GetCreateIfNotExistsScript();

        Assert.Equal(
            """
            CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
                "MigrationId" nvarchar(150) NOT NULL,
                "ProductVersion" nvarchar(32) NOT NULL,
                CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId"));

            """, sql, ignoreLineEndingDifferences: true);
    }

    [ConditionalFact]
    public void GetCreateIfNotExistsScript_works_with_schema()
    {
        var sql = CreateHistoryRepository("my").GetCreateIfNotExistsScript();

        Assert.Equal(
            """
            CREATE SCHEMA IF NOT EXISTS "my";
            CREATE TABLE IF NOT EXISTS "my"."__EFMigrationsHistory" (
                "MigrationId" nvarchar(150) NOT NULL,
                "ProductVersion" nvarchar(32) NOT NULL,
                CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId"));

            """, sql, ignoreLineEndingDifferences: true);
    }

    [ConditionalFact]
    public void GetDeleteScript_works()
    {
        var sql = CreateHistoryRepository().GetDeleteScript("Migration1");

        Assert.Equal(
            """
            DELETE FROM "__EFMigrationsHistory"
            WHERE "MigrationId" = 'Migration1';

            """, sql, ignoreLineEndingDifferences: true);
    }

    [ConditionalFact]
    public void GetInsertScript_works()
    {
        var sql = CreateHistoryRepository().GetInsertScript(
            new HistoryRow("Migration1", "7.0.0"));

        Assert.Equal(
            """
            INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
            VALUES ('Migration1', '7.0.0');

            """, sql, ignoreLineEndingDifferences: true);
    }

    [ConditionalFact]
    public void GetBeginIfNotExistsScript_works()
    {
        var sql = CreateHistoryRepository().GetBeginIfNotExistsScript("Migration1");

        Assert.Equal(
            """
            EXECUTE IMMEDIATE $$
               BEGIN
                   IF ( NOT EXISTS (
                       SELECT * FROM "__EFMigrationsHistory"
                       WHERE "MigrationId" = 'Migration1'
                   ))
                   THEN
                       BEGIN
            """, sql, ignoreLineEndingDifferences: true);
    }

    [ConditionalFact]
    public void GetBeginIfExistsScript_works()
    {
        var sql = CreateHistoryRepository().GetBeginIfExistsScript("Migration1");

        Assert.Equal(
            """
            EXECUTE IMMEDIATE $$
               BEGIN
                   IF ( EXISTS (
                       SELECT * FROM "__EFMigrationsHistory"
                       WHERE "MigrationId" = 'Migration1'
                   ))
                   THEN
                       BEGIN
            """, sql, ignoreLineEndingDifferences: true);
    }

    [ConditionalFact]
    public void GetEndIfScript_works()
    {
        var sql = CreateHistoryRepository().GetEndIfScript();

        Assert.Equal(
            """
                       END
                   END IF;
               END
            $$;
            """, sql, ignoreLineEndingDifferences: true);
    }

    private static IHistoryRepository CreateHistoryRepository(string schema = null)
        => new DbContext(
                new DbContextOptionsBuilder()
                    .UseInternalServiceProvider(SnowflakeTestHelpers.Instance.CreateServiceProvider())
                    .UseSnowflake(
                        new SnowflakeDbConnection("Database=DummyDatabase"),
                        b => b.MigrationsHistoryTable(HistoryRepository.DefaultTableName, schema))
                    .Options)
            .GetService<IHistoryRepository>();
}