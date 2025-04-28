using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Snowflake.EntityFrameworkCore.Query.Internal;

/// <summary>
/// Snowflake specific implementation of <see cref="SqlNullabilityProcessor"/>.
/// </summary>
public class SnowflakeSqlNullabilityProcessor : SqlNullabilityProcessor
{
    /// <summary>
    /// Creates a new instance of <see cref="SnowflakeSqlNullabilityProcessor"/>.
    /// </summary>
    /// <param name="dependencies"></param>
    /// <param name="useRelationalNulls"></param>
    public SnowflakeSqlNullabilityProcessor(
        RelationalParameterBasedSqlProcessorDependencies dependencies,
        bool useRelationalNulls)
        : base(dependencies, useRelationalNulls)
    {
    }

    /// <inheritdoc />
    protected override SqlExpression VisitCustomSqlExpression(
        SqlExpression sqlExpression,
        bool allowOptimizedExpansion,
        out bool nullable)
        => sqlExpression switch
        {
            SnowflakeAggregateFunctionExpression aggregateFunctionExpression
                => VisitSnowflakeAggregateFunction(aggregateFunctionExpression, allowOptimizedExpansion, out nullable),
            SnowflakeCastFunctionExpression castFunctionExpression
                => VisitSnowflakeCastFunction(castFunctionExpression, allowOptimizedExpansion, out nullable),

            _ => base.VisitCustomSqlExpression(sqlExpression, allowOptimizedExpansion, out nullable)
        };

    /// <summary>
    /// Visits a <see cref="SnowflakeAggregateFunctionExpression"/>.
    /// </summary>
    /// <param name="aggregateFunctionExpression"></param>
    /// <param name="allowOptimizedExpansion"></param>
    /// <param name="nullable"></param>
    /// <returns></returns>
    protected virtual SqlExpression VisitSnowflakeAggregateFunction(
        SnowflakeAggregateFunctionExpression aggregateFunctionExpression,
        bool allowOptimizedExpansion,
        out bool nullable)
    {
        nullable = aggregateFunctionExpression.IsNullable;

        SqlExpression[]? arguments = null;
        for (var i = 0; i < aggregateFunctionExpression.Arguments.Count; i++)
        {
            var visitedArgument = Visit(aggregateFunctionExpression.Arguments[i], out _);
            if (visitedArgument != aggregateFunctionExpression.Arguments[i] && arguments is null)
            {
                arguments = new SqlExpression[aggregateFunctionExpression.Arguments.Count];

                for (var j = 0; j < i; j++)
                {
                    arguments[j] = aggregateFunctionExpression.Arguments[j];
                }
            }

            if (arguments is not null)
            {
                arguments[i] = visitedArgument;
            }
        }

        OrderingExpression[]? orderings = null;
        for (var i = 0; i < aggregateFunctionExpression.Orderings.Count; i++)
        {
            var ordering = aggregateFunctionExpression.Orderings[i];
            var visitedOrdering = ordering.Update(Visit(ordering.Expression, out _));
            if (visitedOrdering != aggregateFunctionExpression.Orderings[i] && orderings is null)
            {
                orderings = new OrderingExpression[aggregateFunctionExpression.Orderings.Count];

                for (var j = 0; j < i; j++)
                {
                    orderings[j] = aggregateFunctionExpression.Orderings[j];
                }
            }

            if (orderings is not null)
            {
                orderings[i] = visitedOrdering;
            }
        }

        return arguments is not null || orderings is not null
            ? aggregateFunctionExpression.Update(
                arguments ?? aggregateFunctionExpression.Arguments,
                orderings ?? aggregateFunctionExpression.Orderings)
            : aggregateFunctionExpression;
    }

    /// <summary>
    ///  Visits a <see cref="SnowflakeCastFunctionExpression"/>.
    /// </summary>
    /// <param name="castFunctionExpression"></param>
    /// <param name="allowOptimizedExpansion"></param>
    /// <param name="nullable"></param>
    /// <returns></returns>
    protected virtual SqlExpression VisitSnowflakeCastFunction(
        SnowflakeCastFunctionExpression castFunctionExpression,
        bool allowOptimizedExpansion,
        out bool nullable)
    {
        var innerExpression = Visit(castFunctionExpression.Source, allowOptimizedExpansion, out var isInnerNullable);

        nullable = isInnerNullable;

        return castFunctionExpression.Update(innerExpression, castFunctionExpression.Target);
    }

    /// <inheritdoc />
    protected override bool PreferExistsToInWithCoalesce
        => true;

    /// <inheritdoc />
    protected override bool IsCollectionTable(TableExpressionBase table, [NotNullWhen(true)] out Expression? collection)
    {
        if (table is SnowflakeOpenJsonExpression { Arguments: [var argument] })
        {
            collection = argument;
            return true;
        }

        return base.IsCollectionTable(table, out collection);
    }

    /// <inheritdoc />
    protected override TableExpressionBase UpdateParameterCollection(
        TableExpressionBase table,
        SqlParameterExpression newCollectionParameter)
        => table is SnowflakeOpenJsonExpression { Arguments: [SqlParameterExpression] } openJsonExpression
            ? openJsonExpression.Update(newCollectionParameter, path: null)
            : base.UpdateParameterCollection(table, newCollectionParameter);
}