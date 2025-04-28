namespace Snowflake.EntityFrameworkCore.Storage.Internal;

using Microsoft.EntityFrameworkCore.Storage;

/// <summary>
/// Snowflake specific implementation of <see cref="RelationalExecutionStrategyFactory" />
/// </summary>
public class SnowflakeExecutionStrategyFactory : RelationalExecutionStrategyFactory
{
    /// <summary>
    /// Creates a new instance of <see cref="SnowflakeExecutionStrategyFactory" />
    /// </summary>
    /// <param name="dependencies"></param>
    public SnowflakeExecutionStrategyFactory(
        ExecutionStrategyDependencies dependencies)
        : base(dependencies)
    {
    }

    /// <inheritdoc />
    protected override IExecutionStrategy CreateDefaultStrategy(ExecutionStrategyDependencies dependencies)
        => new SnowflakeExecutionStrategy(dependencies);
}