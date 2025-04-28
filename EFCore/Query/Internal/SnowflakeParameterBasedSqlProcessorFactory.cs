namespace Snowflake.EntityFrameworkCore.Query.Internal;

using Microsoft.EntityFrameworkCore.Query;

/// <summary>
///     A factory for creating <see cref="SnowflakeParameterBasedSqlProcessor" /> instances.
/// </summary>
public class SnowflakeParameterBasedSqlProcessorFactory : IRelationalParameterBasedSqlProcessorFactory
{
    /// <summary>
    /// Creates a new instance of <see cref="SnowflakeParameterBasedSqlProcessorFactory" />
    /// </summary>
    /// <param name="dependencies"></param>
    public SnowflakeParameterBasedSqlProcessorFactory(
        RelationalParameterBasedSqlProcessorDependencies dependencies)
    {
        Dependencies = dependencies;
    }

    /// <summary>
    ///     Relational provider-specific dependencies for this service.
    /// </summary>
    protected virtual RelationalParameterBasedSqlProcessorDependencies Dependencies { get; }

    /// <inheritdoc />
    public virtual RelationalParameterBasedSqlProcessor Create(bool useRelationalNulls)
        => new SnowflakeParameterBasedSqlProcessor(Dependencies, useRelationalNulls);
}
