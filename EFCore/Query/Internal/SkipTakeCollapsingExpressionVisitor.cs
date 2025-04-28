using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Snowflake.EntityFrameworkCore.Query.Internal;

using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;

public class SkipTakeCollapsingExpressionVisitor : ExpressionVisitor
{
    private readonly ISqlExpressionFactory _sqlExpressionFactory;

    private IReadOnlyDictionary<string, object> _parameterValues;
    private bool _canCache;

    
    public SkipTakeCollapsingExpressionVisitor(ISqlExpressionFactory sqlExpressionFactory)
    {
        _sqlExpressionFactory = sqlExpressionFactory;
        _parameterValues = null!;
    }

    
    public virtual Expression Process(
        Expression queryExpression,
        IReadOnlyDictionary<string, object?> parametersValues,
        out bool canCache)
    {
        _parameterValues = parametersValues;
        _canCache = true;

        var result = Visit(queryExpression);

        canCache = _canCache;

        return result;
    }

    /// <inheritdoc />
    protected override  Expression VisitExtension(Expression extensionExpression)
    {
        if (extensionExpression is SelectExpression selectExpression)
        {
            if (IsZero(selectExpression.Limit)
                && IsZero(selectExpression.Offset))
            {
                return selectExpression.Update(
                    selectExpression.Projection,
                    selectExpression.Tables,
                    selectExpression.GroupBy.Count > 0 ? selectExpression.Predicate : _sqlExpressionFactory.Constant(false),
                    selectExpression.GroupBy,
                    selectExpression.GroupBy.Count > 0 ? _sqlExpressionFactory.Constant(false) : null,
                    new List<OrderingExpression>(0),
                    limit: null,
                    offset: null);
            }

            bool IsZero(SqlExpression? sqlExpression)
            {
                switch (sqlExpression)
                {
                    case SqlConstantExpression { Value: int intValue }:
                        return intValue == 0;
                    case SqlParameterExpression parameter:
                        _canCache = false;
                        return _parameterValues[parameter.Name] is 0;

                    default:
                        return false;
                }
            }
        }

        return base.VisitExtension(extensionExpression);
    }
}
