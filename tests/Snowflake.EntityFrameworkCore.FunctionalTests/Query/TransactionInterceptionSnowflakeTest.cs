using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Snowflake.EntityFrameworkCore.TestUtilities;

namespace Snowflake.EntityFrameworkCore.FunctionalTests.Query;

public abstract class TransactionInterceptionSnowflakeTestBase(
    TransactionInterceptionSnowflakeTestBase.InterceptionSnowflakeFixtureBase fixture)
    : TransactionInterceptionTestBase(fixture)
{
    public abstract class InterceptionSnowflakeFixtureBase : InterceptionFixtureBase
    {
        protected override string StoreName
            => "TransactionInterception";

        protected override ITestStoreFactory TestStoreFactory
            => SnowflakeTestStoreFactory.Instance;

        protected override IServiceCollection InjectInterceptors(
            IServiceCollection serviceCollection,
            IEnumerable<IInterceptor> injectedInterceptors)
            => base.InjectInterceptors(serviceCollection.AddEntityFrameworkSnowflake(), injectedInterceptors);
    }

    #region Snowflake Driver does not support this feature

    [ConditionalTheory(Skip = "Snowflake Driver does not support this feature")]
    [InlineData(false)]
    [InlineData(true)]
    public override Task Intercept_BeginTransaction_with_isolation_level(bool async) => Task.CompletedTask;

    [ConditionalTheory(Skip = "Snowflake Driver does not support this feature")]
    [InlineData(false)]
    [InlineData(true)]
    public override Task Intercept_CreateSavepoint(bool async) => Task.CompletedTask;

    [ConditionalTheory(Skip = "Snowflake Driver does not support this feature")]
    [InlineData(false, true)]
    [InlineData(true, true)]
    [InlineData(false, false)]
    [InlineData(true, false)]
    public override Task Intercept_error_on_commit_or_rollback(bool async, bool commit) => Task.CompletedTask;

    #endregion

    #region Savepoint not implemented (SNOW-1876750)

    [ConditionalTheory(Skip = "Skipped due SNOW-1876750")]
    [InlineData(false)]
    [InlineData(true)]
    public override Task Intercept_ReleaseSavepoint(bool async) => Task.CompletedTask;

    [ConditionalTheory(Skip = "Skipped due SNOW-1876750")]
    [InlineData(false)]
    [InlineData(true)]
    public override Task Intercept_RollbackToSavepoint(bool async) => Task.CompletedTask;

    #endregion

    public class TransactionInterceptionSnowflakeTest(
        TransactionInterceptionSnowflakeTest.InterceptionSnowflakeFixture fixture)
        : TransactionInterceptionSnowflakeTestBase(fixture),
            IClassFixture<TransactionInterceptionSnowflakeTest.InterceptionSnowflakeFixture>
    {
        public class InterceptionSnowflakeFixture : InterceptionSnowflakeFixtureBase
        {
            protected override bool ShouldSubscribeToDiagnosticListener
                => false;
        }
    }

    public class TransactionInterceptionWithDiagnosticsSnowflakeTest(
        TransactionInterceptionWithDiagnosticsSnowflakeTest.InterceptionSnowflakeFixture fixture)
        : TransactionInterceptionSnowflakeTestBase(fixture),
            IClassFixture<TransactionInterceptionWithDiagnosticsSnowflakeTest.InterceptionSnowflakeFixture>
    {
        public class InterceptionSnowflakeFixture : InterceptionSnowflakeFixtureBase
        {
            protected override bool ShouldSubscribeToDiagnosticListener
                => true;
        }
    }
}