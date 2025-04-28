using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using Snowflake.EntityFrameworkCore.Utilities;

namespace Snowflake.EntityFrameworkCore.Query.Internal;

/// <summary>
/// Snowflake specific implementation of <see cref="RelationalParameterBasedSqlProcessor" />
/// </summary>
public class SnowflakeParameterBasedSqlProcessor : RelationalParameterBasedSqlProcessor
{
    /// <summary>
    /// Creates a new instance of <see cref="SnowflakeParameterBasedSqlProcessor" />
    /// </summary>
    /// <param name="dependencies"></param>
    /// <param name="useRelationalNulls"></param>
    public SnowflakeParameterBasedSqlProcessor(
        RelationalParameterBasedSqlProcessorDependencies dependencies,
        bool useRelationalNulls)
        : base(dependencies, useRelationalNulls)
    {
    }

    /// <inheritdoc />
    public override Expression Optimize(
        Expression queryExpression,
        IReadOnlyDictionary<string, object?> parametersValues,
        out bool canCache)
    {
        var optimizedQueryExpression = base.Optimize(queryExpression, parametersValues, out canCache);

        optimizedQueryExpression = new SkipTakeCollapsingExpressionVisitor(Dependencies.SqlExpressionFactory)
            .Process(optimizedQueryExpression, parametersValues, out var canCache2);

        canCache &= canCache2;

        return new SearchConditionConvertingExpressionVisitor(Dependencies.SqlExpressionFactory).Visit(optimizedQueryExpression);
    }

    /// <inheritdoc />
    protected override Expression ProcessSqlNullability(
        Expression selectExpression,
        IReadOnlyDictionary<string, object?> parametersValues,
        out bool canCache)
    {
        Check.NotNull(selectExpression, nameof(selectExpression));
        Check.NotNull(parametersValues, nameof(parametersValues));

        return new SnowflakeSqlNullabilityProcessor(Dependencies, UseRelationalNulls).Process(
            selectExpression, parametersValues, out canCache);
    }
}