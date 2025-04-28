using Microsoft.EntityFrameworkCore.Query;

namespace Snowflake.EntityFrameworkCore.Query.Internal;

/// <summary>
/// Snowflake-specific query compilation context.
/// </summary>
public class SnowflakeQueryCompilationContext : RelationalQueryCompilationContext
{
    private readonly bool _multipleActiveResultSetsEnabled;


    /// <summary>
    ///    Creates a new instance of the <see cref="SnowflakeQueryCompilationContext" /> class.
    /// </summary>
    /// <param name="dependencies"></param>
    /// <param name="relationalDependencies"></param>
    /// <param name="async"></param>
    /// <param name="multipleActiveResultSetsEnabled"></param>
    public SnowflakeQueryCompilationContext(
        QueryCompilationContextDependencies dependencies,
        RelationalQueryCompilationContextDependencies relationalDependencies,
        bool async,
        bool multipleActiveResultSetsEnabled)
        : base(dependencies, relationalDependencies, async)
    {
        _multipleActiveResultSetsEnabled = multipleActiveResultSetsEnabled;
    }


    /// <inheritdoc />
    public override bool IsBuffering
        => base.IsBuffering
           || (QuerySplittingBehavior == Microsoft.EntityFrameworkCore.QuerySplittingBehavior.SplitQuery
               && !_multipleActiveResultSetsEnabled);

    /// <summary>
    ///     Tracks whether translation is currently within the argument of an aggregate method (e.g. MAX, COUNT); Snowflake does not
    ///     allow subqueries and aggregates in that context.
    /// </summary>
    public virtual bool InAggregateFunction { get; set; }
}