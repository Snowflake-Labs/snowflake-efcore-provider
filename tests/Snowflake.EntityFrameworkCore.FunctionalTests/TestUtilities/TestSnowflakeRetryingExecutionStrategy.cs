using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Snowflake.EntityFrameworkCore.TestUtilities;

public class TestSnowflakeRetryingExecutionStrategy : SnowflakeRetryingExecutionStrategy
{
    private const bool ErrorNumberDebugMode = false;

    private static readonly int[] _additionalErrorNumbers =
    {
        -1, // Physical connection is not usable
        -2, // Timeout
        42008, // Mirroring (Only when a database is deleted and another one is created in fast succession)
        42019 // CREATE DATABASE operation failed
    };

    public TestSnowflakeRetryingExecutionStrategy()
        : base(
            new DbContext(
                new DbContextOptionsBuilder()
                    .EnableServiceProviderCaching(false)
                    .UseSnowflake(TestEnvironment.DefaultConnection).Options),
            DefaultMaxRetryCount, DefaultMaxDelay, _additionalErrorNumbers)
    {
    }

    public TestSnowflakeRetryingExecutionStrategy(DbContext context)
        : base(context, DefaultMaxRetryCount, DefaultMaxDelay, _additionalErrorNumbers)
    {
    }

    public TestSnowflakeRetryingExecutionStrategy(DbContext context, TimeSpan maxDelay)
        : base(context, DefaultMaxRetryCount, maxDelay, _additionalErrorNumbers)
    {
    }

    public TestSnowflakeRetryingExecutionStrategy(ExecutionStrategyDependencies dependencies)
        : base(dependencies, DefaultMaxRetryCount, DefaultMaxDelay, _additionalErrorNumbers)
    {
    }

    protected override bool ShouldRetryOn(Exception exception)
    {
        if (base.ShouldRetryOn(exception))
        {
            return true;
        }

        // TODO REVIEW
        // if (ErrorNumberDebugMode
        //     && exception is SnowflakeDbException sqlException)
        // {
        //     var message = "Didn't retry on";
        //     foreach (SnowflakeDbException err in sqlException.Errors)
        //     {
        //         message += " " + err.ErrorCode;
        //     }       
        //
        //     message += Environment.NewLine;
        //     throw new InvalidOperationException(message + exception, exception);
        // }

        return exception is InvalidOperationException { Message: "Internal .Net Framework Data Provider error 6." };
    }

    public new virtual TimeSpan? GetNextDelay(Exception lastException)
    {
        ExceptionsEncountered.Add(lastException);
        return base.GetNextDelay(lastException);
    }
}
