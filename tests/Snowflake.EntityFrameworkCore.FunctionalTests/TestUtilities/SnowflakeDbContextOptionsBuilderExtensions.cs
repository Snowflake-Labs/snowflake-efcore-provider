using Microsoft.EntityFrameworkCore;
using Snowflake.EntityFrameworkCore.Infrastructure;

namespace Snowflake.EntityFrameworkCore.TestUtilities;

public static class SnowflakeDbContextOptionsBuilderExtensions
{
    public static SnowflakeDbContextOptionsBuilder ApplyConfiguration(this SnowflakeDbContextOptionsBuilder optionsBuilder)
    {
        // TODO REMOVE
        // var maxBatch = TestEnvironment.GetInt(nameof(SqlServerDbContextOptionsBuilder.MaxBatchSize));
        // if (maxBatch.HasValue)
        // {
        //     optionsBuilder.MaxBatchSize(maxBatch.Value);
        // }

        // TODO REVIEW
        optionsBuilder.UseQuerySplittingBehavior(QuerySplittingBehavior.SingleQuery);

        optionsBuilder.ExecutionStrategy(d => new TestSnowflakeRetryingExecutionStrategy(d));

        optionsBuilder.CommandTimeout(SnowflakeTestStore.CommandTimeout);

        return optionsBuilder;
    }
}
