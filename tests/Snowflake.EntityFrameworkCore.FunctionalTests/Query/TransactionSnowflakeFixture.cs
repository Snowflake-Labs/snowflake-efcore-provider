using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Snowflake.EntityFrameworkCore.Infrastructure;
using Snowflake.EntityFrameworkCore.Storage.Internal;
using Snowflake.EntityFrameworkCore.TestUtilities;

namespace Snowflake.EntityFrameworkCore.FunctionalTests.Query;

public class TransactionSnowflakeFixture : TransactionTestBase<TransactionSnowflakeFixture>.TransactionFixtureBase
{
    protected override ITestStoreFactory TestStoreFactory
        => SnowflakeTestStoreFactory.Instance;

    public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
    {
        new SnowflakeDbContextOptionsBuilder(base.AddOptions(builder))
            .ExecutionStrategy(c => new SnowflakeExecutionStrategy(c));
        return builder;
    }
}