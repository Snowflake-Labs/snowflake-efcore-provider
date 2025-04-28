using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Snowflake.EntityFrameworkCore.Diagnostics;
using Snowflake.EntityFrameworkCore.TestUtilities;

namespace Snowflake.EntityFrameworkCore.FunctionalTests.Migrations;

public class SnowflakeMigrationsInfrastructureTest : MigrationsInfrastructureTestBase<SnowflakeMigrationsInfrastructureTest.SnowflakeMigrationsInfrastructureFixture>
{
    public SnowflakeMigrationsInfrastructureTest(SnowflakeMigrationsInfrastructureFixture fixture)
        : base(fixture)
    {
    }

    #region DiffSnapshot NOT SUPPORTED

    [ConditionalFact(Skip = "DiffSnapshot NOT SUPPORTED - SNOW-1887705")]
    public override void Can_diff_against_2_2_model()
    {
        // TODO
    }

    [ConditionalFact(Skip = "DiffSnapshot NOT SUPPORTED - SNOW-1887705")]
    public override void Can_diff_against_3_0_ASP_NET_Identity_model()
    {
        // TODO
    }

    [ConditionalFact(Skip = "DiffSnapshot NOT SUPPORTED - SNOW-1887705")]
    public override void Can_diff_against_2_2_ASP_NET_Identity_model()
    {
        // TODO
    }

    [ConditionalFact(Skip = "DiffSnapshot NOT SUPPORTED - SNOW-1887705")]
    public override void Can_diff_against_2_1_ASP_NET_Identity_model()
    {
        // TODO
    }
    
    #endregion

    #region OBJECT_ID NOT SUPPORTED AND ENSUREDELETED INITIAL CATALOG NOT SPECIFIED

    [ConditionalFact(Skip = "OBJECT_ID NOT SUPPORTED - SNOW-1887705 AND EnsureDeleted Initial Catalog not specified - SNOW-1887709")]
    public override void Can_apply_all_migrations()
    {
        using var db = Fixture.CreateContext();
        // TODO SNOW-1887709
        // db.Database.EnsureDeleted();

        GiveMeSomeTime(db);

        db.Database.Migrate();

        var history = db.GetService<IHistoryRepository>();
        Assert.Collection(
            history.GetAppliedMigrations(),
            x => Assert.Equal("00000000000001_Migration1", x.MigrationId),
            x => Assert.Equal("00000000000002_Migration2", x.MigrationId),
            x => Assert.Equal("00000000000003_Migration3", x.MigrationId),
            x => Assert.Equal("00000000000004_Migration4", x.MigrationId),
            x => Assert.Equal("00000000000005_Migration5", x.MigrationId),
            x => Assert.Equal("00000000000006_Migration6", x.MigrationId),
            x => Assert.Equal("00000000000007_Migration7", x.MigrationId));
    }
    
    [ConditionalFact(Skip = "OBJECT_ID NOT SUPPORTED - SNOW-1887705 AND EnsureDeleted Initial Catalog not specified - SNOW-1887709")]
    public override async Task Can_apply_all_migrations_async()
    {
        using var db = Fixture.CreateContext();
        // TODO SNOW-1887709
        // await db.Database.EnsureDeletedAsync();

        await GiveMeSomeTimeAsync(db);

        await db.Database.MigrateAsync();

        var history = db.GetService<IHistoryRepository>();
        Assert.Collection(
            await history.GetAppliedMigrationsAsync(),
            x => Assert.Equal("00000000000001_Migration1", x.MigrationId),
            x => Assert.Equal("00000000000002_Migration2", x.MigrationId),
            x => Assert.Equal("00000000000003_Migration3", x.MigrationId),
            x => Assert.Equal("00000000000004_Migration4", x.MigrationId),
            x => Assert.Equal("00000000000005_Migration5", x.MigrationId),
            x => Assert.Equal("00000000000006_Migration6", x.MigrationId),
            x => Assert.Equal("00000000000007_Migration7", x.MigrationId));
    }
    
    [ConditionalFact(Skip = "OBJECT_ID NOT SUPPORTED - SNOW-1887705 AND EnsureDeleted Initial Catalog not specified - SNOW-1887709")]
    public override void Can_apply_one_migration()
    {
        using var db = Fixture.CreateContext();
        // TODO SNOW-1887709
        // db.Database.EnsureDeleted();

        GiveMeSomeTime(db);

        var migrator = db.GetService<IMigrator>();
        migrator.Migrate("Migration1");

        var history = db.GetService<IHistoryRepository>();
        Assert.Collection(
            history.GetAppliedMigrations(),
            x => Assert.Equal("00000000000001_Migration1", x.MigrationId));
    }
    
    [ConditionalFact(Skip = "OBJECT_ID NOT SUPPORTED - SNOW-1887705 AND EnsureDeleted Initial Catalog not specified - SNOW-1887709")]
    public override void Can_apply_range_of_migrations()
    {
        using var db = Fixture.CreateContext();
        // TODO SNOW-1887709
        // db.Database.EnsureDeleted();

        GiveMeSomeTime(db);

        var migrator = db.GetService<IMigrator>();
        migrator.Migrate("Migration6");

        var history = db.GetService<IHistoryRepository>();
        Assert.Collection(
            history.GetAppliedMigrations(),
            x => Assert.Equal("00000000000001_Migration1", x.MigrationId),
            x => Assert.Equal("00000000000002_Migration2", x.MigrationId),
            x => Assert.Equal("00000000000003_Migration3", x.MigrationId),
            x => Assert.Equal("00000000000004_Migration4", x.MigrationId),
            x => Assert.Equal("00000000000005_Migration5", x.MigrationId),
            x => Assert.Equal("00000000000006_Migration6", x.MigrationId));
    }
    
    [ConditionalFact(Skip = "OBJECT_ID NOT SUPPORTED - SNOW-1887705 AND EnsureDeleted Initial Catalog not specified - SNOW-1887709")]
    public override void Can_revert_all_migrations()
    {
        using var db = Fixture.CreateContext();
        // TODO SNOW-1887709
        // db.Database.EnsureDeleted();

        GiveMeSomeTime(db);

        var migrator = db.GetService<IMigrator>();
        migrator.Migrate("Migration5");
        migrator.Migrate(Migration.InitialDatabase);

        var history = db.GetService<IHistoryRepository>();
        Assert.Empty(history.GetAppliedMigrations());
    }
    
    [ConditionalFact(Skip = "OBJECT_ID NOT SUPPORTED - SNOW-1887705 AND EnsureDeleted Initial Catalog not specified - SNOW-1887709")]
    public override void Can_revert_one_migrations()
    {
        using var db = Fixture.CreateContext();
        // TODO SNOW-1887709
        // db.Database.EnsureDeleted();

        GiveMeSomeTime(db);

        var migrator = db.GetService<IMigrator>();
        migrator.Migrate("Migration5");
        migrator.Migrate("Migration4");

        var history = db.GetService<IHistoryRepository>();
        Assert.Collection(
            history.GetAppliedMigrations(),
            x => Assert.Equal("00000000000001_Migration1", x.MigrationId),
            x => Assert.Equal("00000000000002_Migration2", x.MigrationId),
            x => Assert.Equal("00000000000003_Migration3", x.MigrationId),
            x => Assert.Equal("00000000000004_Migration4", x.MigrationId));
    }

    #endregion

    public class SnowflakeMigrationsInfrastructureFixture : MigrationsInfrastructureFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => SnowflakeTestStoreFactory.Instance;
        

        public override MigrationsContext CreateContext()
        {
            var options = AddOptions(
                    new DbContextOptionsBuilder()
                        .UseSnowflake(
                            TestStore.ConnectionString,
                            b => b.ApplyConfiguration()
                                .CommandTimeout(SnowflakeTestStore.CommandTimeout)
                                .ReverseNullOrdering()
                            )
                    )
                .UseInternalServiceProvider(CreateServiceProvider())
                .ConfigureWarnings(b =>
                {
                    b.Ignore(SnowflakeEventId.StandardTableWarning);
                })
                .Options;
            
            return new MigrationsContext(options);
        }
        
        private static IServiceProvider CreateServiceProvider()
            => new ServiceCollection()
                .AddEntityFrameworkSnowflake()
                .BuildServiceProvider();
    }
}