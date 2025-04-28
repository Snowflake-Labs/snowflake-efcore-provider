using System.Collections.Generic;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Snowflake.EntityFrameworkCore.Query.Internal;

/// <summary>
///   A <see cref="IMethodCallTranslator" /> for translating <see cref="SnowflakeDbFunctionsExtensions.IsDate" /> method calls.
/// </summary>
public class SnowflakeIsDateFunctionTranslator : IMethodCallTranslator
{
    private readonly ISqlExpressionFactory _sqlExpressionFactory;

    private static readonly MethodInfo MethodInfo = typeof(SnowflakeDbFunctionsExtensions)
        .GetRuntimeMethod(nameof(SnowflakeDbFunctionsExtensions.IsDate),
            [typeof(DbFunctions), typeof(string)])!;

    /// <summary>
    ///    Creates a new instance of the <see cref="SnowflakeIsDateFunctionTranslator" /> class.
    /// </summary>
    public SnowflakeIsDateFunctionTranslator(ISqlExpressionFactory sqlExpressionFactory)
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
            ? _sqlExpressionFactory.Convert(
                _sqlExpressionFactory.Function(
                    "ISDATE",
                    new[] { arguments[1] },
                    nullable: true,
                    argumentsPropagateNullability: new[] { true },
                    MethodInfo.ReturnType),
                MethodInfo.ReturnType)
            : null;
}