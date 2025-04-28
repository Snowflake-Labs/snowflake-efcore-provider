using Snowflake.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore.Query;

namespace Snowflake.EntityFrameworkCore.Query.Internal;

/// <inheritdoc />
public class SnowflakeQueryCompilationContextFactory : IQueryCompilationContextFactory
{
    private readonly ISnowflakeConnection _SnowflakeConnection;

    /// <summary>
    ///    Initializes a new instance of the <see cref="SnowflakeQueryCompilationContextFactory" /> class.
    /// </summary>
    /// <param name="dependencies"></param>
    /// <param name="relationalDependencies"></param>
    /// <param name="SnowflakeConnection"></param>
    public SnowflakeQueryCompilationContextFactory(
        QueryCompilationContextDependencies dependencies,
        RelationalQueryCompilationContextDependencies relationalDependencies,
        ISnowflakeConnection SnowflakeConnection)
    {
        Dependencies = dependencies;
        RelationalDependencies = relationalDependencies;
        _SnowflakeConnection = SnowflakeConnection;
    }

    /// <summary>
    ///     Dependencies for this service.
    /// </summary>
    protected virtual QueryCompilationContextDependencies Dependencies { get; }

    /// <summary>
    ///     Relational provider-specific dependencies for this service.
    /// </summary>
    protected virtual RelationalQueryCompilationContextDependencies RelationalDependencies { get; }

    /// <summary>
    ///    Creates a new QueryCompilationContext.
    /// </summary>
    /// <param name="async"></param>
    /// <returns></returns>
    public virtual QueryCompilationContext Create(bool async)
        => new SnowflakeQueryCompilationContext(
            Dependencies, RelationalDependencies, async, _SnowflakeConnection.IsMultipleActiveResultSetsEnabled);
}