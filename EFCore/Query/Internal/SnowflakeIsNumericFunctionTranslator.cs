using System.Collections.Generic;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Snowflake.EntityFrameworkCore.Query.Internal;

/// <summary>
/// Translates <see cref="SnowflakeDbFunctionsExtensions.IsNumeric(DbFunctions, string)" /> method calls into ISNUMERIC function calls.
/// </summary>
public class SnowflakeIsNumericFunctionTranslator : IMethodCallTranslator
{
    private readonly ISqlExpressionFactory _sqlExpressionFactory;

    private static readonly MethodInfo MethodInfo = typeof(SnowflakeDbFunctionsExtensions)
        .GetRuntimeMethod(nameof(SnowflakeDbFunctionsExtensions.IsNumeric),
            [typeof(DbFunctions), typeof(string)])!;

    /// <summary>
    /// Creates a new instance of <see cref="SnowflakeIsNumericFunctionTranslator" />
    /// </summary>
    /// <param name="sqlExpressionFactory"></param>
    public SnowflakeIsNumericFunctionTranslator(ISqlExpressionFactory sqlExpressionFactory)
    {
        _sqlExpressionFactory = sqlExpressionFactory;
    }

    /// <inheritdoc />
    public virtual SqlExpression? Translate(
        SqlExpression? instance,
        MethodInfo method,
        IReadOnlyList<SqlExpression> arguments,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
        => MethodInfo.Equals(method)
            ? _sqlExpressionFactory.Equal(
                _sqlExpressionFactory.Function(
                    "ISNUMERIC",
                    new[] { arguments[1] },
                    nullable: false,
                    argumentsPropagateNullability: new[] { false },
                    typeof(int)),
                _sqlExpressionFactory.Constant(1))
            : null;
}