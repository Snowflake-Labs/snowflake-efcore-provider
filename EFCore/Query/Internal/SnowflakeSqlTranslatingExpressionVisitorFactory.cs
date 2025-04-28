using Microsoft.EntityFrameworkCore.Query;

namespace Snowflake.EntityFrameworkCore.Query.Internal;

/// <inheritdoc />
public class SnowflakeSqlTranslatingExpressionVisitorFactory : IRelationalSqlTranslatingExpressionVisitorFactory
{
    /// <summary>
    ///    Creates a new instance of <see cref="SnowflakeSqlTranslatingExpressionVisitorFactory" />
    /// </summary>
    /// <param name="dependencies"></param>
    public SnowflakeSqlTranslatingExpressionVisitorFactory(
        RelationalSqlTranslatingExpressionVisitorDependencies dependencies)
    {
        Dependencies = dependencies;
    }

    /// <summary>
    ///     Relational provider-specific dependencies for this service.
    /// </summary>
    protected virtual RelationalSqlTranslatingExpressionVisitorDependencies Dependencies { get; }

    /// <inheritdoc />
    public virtual RelationalSqlTranslatingExpressionVisitor Create(
        QueryCompilationContext queryCompilationContext,
        QueryableMethodTranslatingExpressionVisitor queryableMethodTranslatingExpressionVisitor)
        => new SnowflakeSqlTranslatingExpressionVisitor(
            Dependencies,
            (SnowflakeQueryCompilationContext)queryCompilationContext,
            queryableMethodTranslatingExpressionVisitor);
}