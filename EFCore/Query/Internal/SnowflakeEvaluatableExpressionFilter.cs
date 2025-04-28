using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;

namespace Snowflake.EntityFrameworkCore.Query.Internal;

/// <summary>
///    A <see cref="RelationalEvaluatableExpressionFilter" /> for Snowflake-specific expressions.
/// </summary>
public class SnowflakeEvaluatableExpressionFilter : RelationalEvaluatableExpressionFilter
{
    /// <summary>
    ///    Creates a new instance of the <see cref="SnowflakeEvaluatableExpressionFilter" /> class.
    /// </summary>
    /// <param name="dependencies"></param>
    /// <param name="relationalDependencies"></param>
    public SnowflakeEvaluatableExpressionFilter(
        EvaluatableExpressionFilterDependencies dependencies,
        RelationalEvaluatableExpressionFilterDependencies relationalDependencies)
        : base(dependencies, relationalDependencies)
    {
    }

    /// <inheritdoc />
    public override bool IsEvaluatableExpression(Expression expression, IModel model)
    {
        if (expression is MethodCallExpression methodCallExpression
            && methodCallExpression.Method.DeclaringType == typeof(SnowflakeDbFunctionsExtensions))
        {
            return false;
        }

        return base.IsEvaluatableExpression(expression, model);
    }
}