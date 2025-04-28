using Microsoft.EntityFrameworkCore.Query;
using Snowflake.EntityFrameworkCore.Infrastructure.Internal;

namespace Snowflake.EntityFrameworkCore.Query.Internal;

/// <summary>
/// Snowflake specific implementation of <see cref="IQueryableMethodTranslatingExpressionVisitorFactory" />
/// </summary>
public class SnowflakeQueryableMethodTranslatingExpressionVisitorFactory : IQueryableMethodTranslatingExpressionVisitorFactory
{
    private readonly ISnowflakeSingletonOptions _SnowflakeSingletonOptions;

    /// <summary>
    /// Creates a new instance of <see cref="SnowflakeQueryableMethodTranslatingExpressionVisitorFactory" />
    /// </summary>
    /// <param name="dependencies"></param>
    /// <param name="relationalDependencies"></param>
    /// <param name="SnowflakeSingletonOptions"></param>
    public SnowflakeQueryableMethodTranslatingExpressionVisitorFactory(
        QueryableMethodTranslatingExpressionVisitorDependencies dependencies,
        RelationalQueryableMethodTranslatingExpressionVisitorDependencies relationalDependencies,
        ISnowflakeSingletonOptions SnowflakeSingletonOptions)
    {
        Dependencies = dependencies;
        RelationalDependencies = relationalDependencies;
        _SnowflakeSingletonOptions = SnowflakeSingletonOptions;
    }

    /// <summary>
    ///     Dependencies for this service.
    /// </summary>
    protected virtual QueryableMethodTranslatingExpressionVisitorDependencies Dependencies { get; }

    /// <summary>
    ///     Relational provider-specific dependencies for this service.
    /// </summary>
    protected virtual RelationalQueryableMethodTranslatingExpressionVisitorDependencies RelationalDependencies { get; }

    /// <inheritdoc />
    public virtual QueryableMethodTranslatingExpressionVisitor Create(QueryCompilationContext queryCompilationContext)
        => new SnowflakeQueryableMethodTranslatingExpressionVisitor(
            Dependencies, RelationalDependencies, (SnowflakeQueryCompilationContext)queryCompilationContext,
            _SnowflakeSingletonOptions);
}