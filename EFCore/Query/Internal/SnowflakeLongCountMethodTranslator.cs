using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Snowflake.EntityFrameworkCore.Query.Internal;

/// <summary>
/// Translates the Long Count method to Snowflake SQL.
/// </summary>
public class SnowflakeLongCountMethodTranslator : IAggregateMethodCallTranslator
{
    private readonly ISqlExpressionFactory _sqlExpressionFactory;

    /// <summary>
    ///    Initializes a new instance of the <see cref="SnowflakeLongCountMethodTranslator" /> class.
    /// </summary>
    /// <param name="sqlExpressionFactory"></param>
    public SnowflakeLongCountMethodTranslator(ISqlExpressionFactory sqlExpressionFactory)
    {
        _sqlExpressionFactory = sqlExpressionFactory;
    }
    
    /// <inheritdoc />
    public virtual SqlExpression? Translate(
        MethodInfo method,
        EnumerableExpression source,
        IReadOnlyList<SqlExpression> arguments,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        if (method.DeclaringType == typeof(Queryable)
            && method.IsGenericMethod
            && method.GetGenericMethodDefinition() is MethodInfo genericMethod
            && (genericMethod == QueryableMethods.LongCountWithoutPredicate
                || genericMethod == QueryableMethods.LongCountWithPredicate))
        {
            var sqlExpression = (source.Selector as SqlExpression) ?? _sqlExpressionFactory.Fragment("*");
            if (source.Predicate != null)
            {
                if (sqlExpression is SqlFragmentExpression)
                {
                    sqlExpression = _sqlExpressionFactory.Constant(1);
                }

                sqlExpression = _sqlExpressionFactory.Case(
                    new List<CaseWhenClause> { new(source.Predicate, sqlExpression) },
                    elseResult: null);
            }

            if (source.IsDistinct)
            {
                sqlExpression = new DistinctExpression(sqlExpression);
            }

            return _sqlExpressionFactory.ApplyDefaultTypeMapping(
                _sqlExpressionFactory.Function(
                    "COUNT",
                    new[] { sqlExpression },
                    nullable: false,
                    argumentsPropagateNullability: new[] { false },
                    typeof(long)));
        }

        return null;
    }
}
