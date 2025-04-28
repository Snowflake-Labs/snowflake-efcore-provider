using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Snowflake.EntityFrameworkCore.Internal;

namespace Snowflake.EntityFrameworkCore.Storage.Internal;

/// <summary>
/// Snowflake specific implementation of <see cref="IExecutionStrategy" />.
/// </summary>
public class SnowflakeExecutionStrategy : IExecutionStrategy
{
    /// <summary>
    ///    Creates a new instance of <see cref="SnowflakeExecutionStrategy" />.
    /// </summary>
    /// <param name="dependencies"></param>
    public SnowflakeExecutionStrategy(ExecutionStrategyDependencies dependencies)
    {
        Dependencies = dependencies;
    }

    /// <summary>
    /// Dependencies for this service.
    /// </summary>
    protected virtual ExecutionStrategyDependencies Dependencies { get; }

    /// <summary>
    ///    Indicates whether the strategy might retry the execution after a failure.
    /// </summary>
    public virtual bool RetriesOnFailure
        => false;

    /// <inheritdoc />
    public virtual TResult Execute<TState, TResult>(
        TState state,
        Func<DbContext, TState, TResult> operation,
        Func<DbContext, TState, ExecutionResult<TResult>>? verifySucceeded)
    {
        try
        {
            return operation(Dependencies.CurrentContext.Context, state);
        }
        catch (Exception ex) when (ExecutionStrategy.CallOnWrappedException(ex, SnowflakeTransientExceptionDetector.ShouldRetryOn))
        {
            throw new InvalidOperationException(SnowflakeStrings.TransientExceptionDetected, ex);
        }
    }

    /// <inheritdoc />
    public virtual async Task<TResult> ExecuteAsync<TState, TResult>(
        TState state,
        Func<DbContext, TState, CancellationToken, Task<TResult>> operation,
        Func<DbContext, TState, CancellationToken, Task<ExecutionResult<TResult>>>? verifySucceeded,
        CancellationToken cancellationToken)
    {
        try
        {
            return await operation(Dependencies.CurrentContext.Context, state, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ExecutionStrategy.CallOnWrappedException(ex, SnowflakeTransientExceptionDetector.ShouldRetryOn))
        {
            throw new InvalidOperationException(SnowflakeStrings.TransientExceptionDetected, ex);
        }
    }
}