using System.Collections.Generic;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Snowflake.EntityFrameworkCore.Query.Internal;

/// <summary>
/// Translates methods on <see cref="byte" /> to Snowflake specific SQL expressions.
/// </summary>
public class SnowflakeByteArrayMethodTranslator : IMethodCallTranslator
{
    private readonly ISqlExpressionFactory _sqlExpressionFactory;

    /// <summary>
    ///    Creates a new instance of the <see cref="SnowflakeByteArrayMethodTranslator" /> class.
    /// </summary>
    /// <param name="sqlExpressionFactory"></param>
    public SnowflakeByteArrayMethodTranslator(ISqlExpressionFactory sqlExpressionFactory)
    {
        _sqlExpressionFactory = sqlExpressionFactory;
    }

    /// <inheritdoc />
    public virtual SqlExpression? Translate(
        SqlExpression? instance,
        MethodInfo method,
        IReadOnlyList<SqlExpression> arguments,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        if (method.IsGenericMethod
            && method.GetGenericMethodDefinition().Equals(EnumerableMethods.Contains)
            && arguments[0].Type == typeof(byte[]))
        {
            var source = arguments[0];
            var sourceTypeMapping = source.TypeMapping;

            var value = arguments[1] is SqlConstantExpression constantValue
                ? (SqlExpression)_sqlExpressionFactory.Constant(new[] { (byte)constantValue.Value! }, sourceTypeMapping)
                : _sqlExpressionFactory.Convert(arguments[1], typeof(byte[]), sourceTypeMapping);

            return _sqlExpressionFactory.GreaterThan(
                _sqlExpressionFactory.Function(
                    "CHARINDEX",
                    [value, source],
                    nullable: true,
                    argumentsPropagateNullability: [true, true],
                    typeof(int)),
                _sqlExpressionFactory.Constant(0));
        }

        if (method.IsGenericMethod
            && method.GetGenericMethodDefinition().Equals(EnumerableMethods.FirstWithoutPredicate)
            && arguments[0].Type == typeof(byte[]))
        {
            return _sqlExpressionFactory.Convert(
                _sqlExpressionFactory.Function(
                    "SUBSTRING",
                    [arguments[0], _sqlExpressionFactory.Constant(1), _sqlExpressionFactory.Constant(1)],
                    nullable: true,
                    argumentsPropagateNullability: [true, true, true],
                    typeof(byte[])),
                method.ReturnType);
        }

        return null;
    }
}