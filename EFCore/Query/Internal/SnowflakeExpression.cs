using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Snowflake.EntityFrameworkCore.Query.Internal;

/// <summary>
/// Snowflake specific expression
/// </summary>
public static class SnowflakeExpression
{
    /// <summary>
    /// Aggregate function
    /// </summary>
    /// <param name="sqlExpressionFactory"></param>
    /// <param name="name"></param>
    /// <param name="arguments"></param>
    /// <param name="enumerableExpression"></param>
    /// <param name="enumerableArgumentIndex"></param>
    /// <param name="nullable"></param>
    /// <param name="argumentsPropagateNullability"></param>
    /// <param name="returnType"></param>
    /// <param name="typeMapping"></param>
    /// <returns></returns>
    public static SqlFunctionExpression AggregateFunction(
        ISqlExpressionFactory sqlExpressionFactory,
        string name,
        IEnumerable<SqlExpression> arguments,
        EnumerableExpression enumerableExpression,
        int enumerableArgumentIndex,
        bool nullable,
        IEnumerable<bool> argumentsPropagateNullability,
        Type returnType,
        RelationalTypeMapping? typeMapping = null)
        => new(
            name,
            ProcessAggregateFunctionArguments(sqlExpressionFactory, arguments, enumerableExpression, enumerableArgumentIndex),
            nullable,
            argumentsPropagateNullability,
            returnType,
            typeMapping);

    /// <summary>
    /// Aggregate function with ordering
    /// </summary>
    /// <param name="sqlExpressionFactory"></param>
    /// <param name="name"></param>
    /// <param name="arguments"></param>
    /// <param name="enumerableExpression"></param>
    /// <param name="enumerableArgumentIndex"></param>
    /// <param name="nullable"></param>
    /// <param name="argumentsPropagateNullability"></param>
    /// <param name="returnType"></param>
    /// <param name="typeMapping"></param>
    /// <returns></returns>
    public static SqlExpression AggregateFunctionWithOrdering(
        ISqlExpressionFactory sqlExpressionFactory,
        string name,
        IEnumerable<SqlExpression> arguments,
        EnumerableExpression enumerableExpression,
        int enumerableArgumentIndex,
        bool nullable,
        IEnumerable<bool> argumentsPropagateNullability,
        Type returnType,
        RelationalTypeMapping? typeMapping = null)
        => enumerableExpression.Orderings.Count == 0
            ? AggregateFunction(
                sqlExpressionFactory, name, arguments, enumerableExpression, enumerableArgumentIndex, nullable,
                argumentsPropagateNullability, returnType, typeMapping)
            : new SnowflakeAggregateFunctionExpression(
                name,
                ProcessAggregateFunctionArguments(sqlExpressionFactory, arguments, enumerableExpression, enumerableArgumentIndex),
                enumerableExpression.Orderings,
                nullable,
                argumentsPropagateNullability,
                returnType,
                typeMapping);

    private static IReadOnlyList<SqlExpression> ProcessAggregateFunctionArguments(
        ISqlExpressionFactory sqlExpressionFactory,
        IEnumerable<SqlExpression> arguments,
        EnumerableExpression enumerableExpression,
        int enumerableArgumentIndex)
    {
        var argIndex = 0;
        var typeMappedArguments = new List<SqlExpression>();

        foreach (var argument in arguments)
        {
            var modifiedArgument = sqlExpressionFactory.ApplyDefaultTypeMapping(argument);

            if (argIndex == enumerableArgumentIndex)
            {
                // This is the argument representing the enumerable inputs to be aggregated.
                // Wrap it with a CASE/WHEN for the predicate and with DISTINCT, if necessary.
                if (enumerableExpression.Predicate != null)
                {
                    modifiedArgument = sqlExpressionFactory.Case(
                        new List<CaseWhenClause> { new(enumerableExpression.Predicate, modifiedArgument) },
                        elseResult: null);
                }

                if (enumerableExpression.IsDistinct)
                {
                    modifiedArgument = new DistinctExpression(modifiedArgument);
                }
            }

            typeMappedArguments.Add(modifiedArgument);

            argIndex++;
        }

        return typeMappedArguments;
    }
}