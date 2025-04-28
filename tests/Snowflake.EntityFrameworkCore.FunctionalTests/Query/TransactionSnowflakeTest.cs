using Microsoft.EntityFrameworkCore;
using Snowflake.EntityFrameworkCore.Diagnostics;
using Snowflake.EntityFrameworkCore.Storage.Internal;
using Snowflake.EntityFrameworkCore.TestUtilities;

namespace Snowflake.EntityFrameworkCore.FunctionalTests.Query;

public class TransactionSnowflakeTest(TransactionSnowflakeFixture fixture)
    : TransactionTestBase<TransactionSnowflakeFixture>(fixture)
{
    #region Snowflake Driver does not support this feature

    [ConditionalTheory(Skip = "Snowflake Driver does not support this feature")]
    [InlineData(AutoTransactionBehavior.WhenNeeded)]
    [InlineData(AutoTransactionBehavior.Never)]
    [InlineData(AutoTransactionBehavior.Always)]
    public override Task QueryAsync_uses_explicit_transaction(AutoTransactionBehavior autoTransactionBehavior) =>
        Task.CompletedTask;

    [ConditionalTheory(Skip = "Snowflake Driver does not support this feature")]
    [InlineData(AutoTransactionBehavior.WhenNeeded)]
    [InlineData(AutoTransactionBehavior.Never)]
    [InlineData(AutoTransactionBehavior.Always)]
    public override void Query_uses_explicit_transaction(AutoTransactionBehavior autoTransactionBehavior)
    {
    }

    #endregion

    #region Savepoint not implemented (SNOW-1876750)

    [ConditionalTheory(Skip = "Skipped due SNOW-1876750")]
    [InlineData(true)]
    [InlineData(false)]
    public override Task Savepoint_name_is_quoted(bool async) => Task.CompletedTask;

    [ConditionalTheory(Skip = "Skipped due SNOW-1876750")]
    [InlineData(true)]
    [InlineData(false)]
    public override Task Savepoint_can_be_rolled_back(bool async) => Task.CompletedTask;

    [ConditionalTheory(Skip = "Skipped due SNOW-1876750")]
    [InlineData(true)]
    [InlineData(false)]
    public override Task Savepoint_can_be_released(bool async) => Task.CompletedTask;

    #endregion

    #region EnlistTransaction not implemented (SNOW-1876755)

    [ConditionalTheory(Skip = "Skipped due SNOW-1876755")]
    [InlineData(true, AutoTransactionBehavior.WhenNeeded)]
    [InlineData(true, AutoTransactionBehavior.Never)]
    [InlineData(true, AutoTransactionBehavior.Always)]
    [InlineData(false, AutoTransactionBehavior.WhenNeeded)]
    [InlineData(false, AutoTransactionBehavior.Never)]
    [InlineData(false, AutoTransactionBehavior.Always)]
    public override Task SaveChanges_uses_enlisted_transaction_connectionString(bool async,
        AutoTransactionBehavior autoTransactionBehavior) => Task.CompletedTask;

    [ConditionalTheory(Skip = "Skipped due SNOW-1876755")]
    [InlineData(true, AutoTransactionBehavior.WhenNeeded)]
    [InlineData(true, AutoTransactionBehavior.Never)]
    [InlineData(true, AutoTransactionBehavior.Always)]
    [InlineData(false, AutoTransactionBehavior.WhenNeeded)]
    [InlineData(false, AutoTransactionBehavior.Never)]
    [InlineData(false, AutoTransactionBehavior.Always)]
    public override Task SaveChanges_uses_enlisted_transaction_after_connection_closed(bool async,
        AutoTransactionBehavior autoTransactionBehavior) => Task.CompletedTask;

    [ConditionalTheory(Skip = "Skipped due SNOW-1876755")]
    [InlineData(true, AutoTransactionBehavior.WhenNeeded)]
    [InlineData(true, AutoTransactionBehavior.Never)]
    [InlineData(true, AutoTransactionBehavior.Always)]
    [InlineData(false, AutoTransactionBehavior.WhenNeeded)]
    [InlineData(false, AutoTransactionBehavior.Never)]
    [InlineData(false, AutoTransactionBehavior.Always)]
    public override Task SaveChanges_uses_enlisted_transaction(bool async,
        AutoTransactionBehavior autoTransactionBehavior) => Task.CompletedTask;

    [ConditionalFact(Skip = "Skipped due SNOW-1876755")]
    public override void UseTransaction_throws_if_enlisted_in_transaction()
    {
    }

    #endregion

    #region DbUpdateException not being thrown (SNOW-1877150)

    [ConditionalTheory(Skip = "Skipped due SNOW-1877150")]
    [InlineData(true)]
    [InlineData(false)]
    public override Task SaveChanges_can_be_used_with_AutoTransactionBehavior_Always(bool async) => Task.CompletedTask;

    [ConditionalTheory(Skip = "Skipped due SNOW-1877150")]
    [InlineData(true)]
    [InlineData(false)]
    public override Task SaveChanges_can_be_used_with_AutoTransactionBehavior_Never(bool async) => Task.CompletedTask;

    [ConditionalTheory(Skip = "Skipped due SNOW-1877150")]
    [InlineData(true)]
    [InlineData(false)]
    public override Task SaveChanges_can_be_used_with_AutoTransactionsEnabled_false(bool async) => Task.CompletedTask;

    [ConditionalTheory(Skip = "Skipped due SNOW-1877150")]
    [InlineData(true)]
    [InlineData(false)]
    public override Task SaveChanges_can_be_used_with_no_savepoint(bool async) => Task.CompletedTask;

    [ConditionalTheory(Skip = "Skipped due SNOW-1877150")]
    [InlineData(true)]
    [InlineData(false)]
    public override Task SaveChanges_implicitly_creates_savepoint(bool async) => Task.CompletedTask;

    [ConditionalTheory(Skip = "Skipped due SNOW-1877150")]
    [InlineData(true)]
    [InlineData(false)]
    public override Task SaveChanges_implicitly_starts_transaction_when_needed(bool async) => Task.CompletedTask;

    [ConditionalTheory(Skip = "Skipped due SNOW-1877150")]
    [InlineData(true, AutoTransactionBehavior.WhenNeeded)]
    [InlineData(true, AutoTransactionBehavior.Never)]
    [InlineData(true, AutoTransactionBehavior.Always)]
    [InlineData(false, AutoTransactionBehavior.WhenNeeded)]
    [InlineData(false, AutoTransactionBehavior.Never)]
    [InlineData(false, AutoTransactionBehavior.Always)]
    public override Task SaveChanges_uses_ambient_transaction(bool async,
        AutoTransactionBehavior autoTransactionBehavior) => Task.CompletedTask;

    [ConditionalTheory(Skip = "Skipped due SNOW-1877150")]
    [InlineData(true, AutoTransactionBehavior.WhenNeeded)]
    [InlineData(true, AutoTransactionBehavior.Never)]
    [InlineData(true, AutoTransactionBehavior.Always)]
    [InlineData(false, AutoTransactionBehavior.WhenNeeded)]
    [InlineData(false, AutoTransactionBehavior.Never)]
    [InlineData(false, AutoTransactionBehavior.Always)]
    public override Task SaveChanges_uses_explicit_transaction_with_failure_behavior(bool async,
        AutoTransactionBehavior autoTransactionBehavior) => Task.CompletedTask;

    #endregion

    protected override bool AmbientTransactionsSupported => false;

    protected override bool SnapshotSupported => false;

    protected override bool DirtyReadsOccur => false;

    protected override bool SavepointsSupported => false;

    protected override DbContext CreateContextWithConnectionString()
    {
        var options = Fixture.AddOptions(
                new DbContextOptionsBuilder()
                    .UseSnowflake(
                        TestStore.ConnectionString,
                        b => b.ApplyConfiguration().ReverseNullOrdering()
                            .ExecutionStrategy(c => new SnowflakeExecutionStrategy(c))))
            .UseInternalServiceProvider(Fixture.ServiceProvider)
            .ConfigureWarnings(b =>
            {
                b.Ignore(SnowflakeEventId.SavepointsDisabledBecauseOfMARS);
                b.Ignore(SnowflakeEventId.StandardTableWarning);
                b.Ignore(SnowflakeEventId.DecimalTypeKeyWarning);
                b.Ignore(SnowflakeEventId.DecimalTypeDefaultWarning);
            });

        return new DbContext(options.Options);
    }
}