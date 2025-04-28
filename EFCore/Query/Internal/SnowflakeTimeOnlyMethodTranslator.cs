using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Snowflake.EntityFrameworkCore.Query.Internal;

/// <summary>
/// Translates methods on <see cref="TimeOnly"/> to Snowflake SQL functions.
/// </summary>
public class SnowflakeTimeOnlyMethodTranslator : IMethodCallTranslator
{
    private static readonly MethodInfo AddHoursMethod = typeof(TimeOnly).GetRuntimeMethod(
        nameof(TimeOnly.AddHours), [typeof(double)])!;

    private static readonly MethodInfo AddMinutesMethod = typeof(TimeOnly).GetRuntimeMethod(
        nameof(TimeOnly.AddMinutes), [typeof(double)])!;

    private static readonly MethodInfo IsBetweenMethod = typeof(TimeOnly).GetRuntimeMethod(
        nameof(TimeOnly.IsBetween), [typeof(TimeOnly), typeof(TimeOnly)])!;

    private readonly ISqlExpressionFactory _sqlExpressionFactory;

    /// <summary>
    ///    Creates a new instance of <see cref="SnowflakeTimeOnlyMethodTranslator" />.
    /// </summary>
    /// <param name="sqlExpressionFactory"></param>
    public SnowflakeTimeOnlyMethodTranslator(ISqlExpressionFactory sqlExpressionFactory)
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
        if (method.DeclaringType != typeof(TimeOnly) || instance is null)
        {
            return null;
        }

        if (method == AddHoursMethod || method == AddMinutesMethod)
        {
            var datePart = method == AddHoursMethod ? "hour" : "minute";

            // Some Add methods accept a double, and Snowflake DateAdd does not accept number argument outside of int range
            if (arguments[0] is SqlConstantExpression { Value: double and (<= int.MinValue or >= int.MaxValue) })
            {
                return null;
            }

            instance = _sqlExpressionFactory.ApplyDefaultTypeMapping(instance);

            return _sqlExpressionFactory.Function(
                "DATEADD",
                [
                    _sqlExpressionFactory.Fragment(datePart), _sqlExpressionFactory.Convert(arguments[0], typeof(int)),
                    instance
                ],
                nullable: true,
                argumentsPropagateNullability: [false, true, true],
                instance.Type,
                instance.TypeMapping);
        }

        // Translate TimeOnly.IsBetween to a >= b AND a < c.
        // Since a is evaluated multiple times, only translate for simple constructs (i.e. avoid duplicating complex subqueries).
        if (method == IsBetweenMethod
            && instance is ColumnExpression or SqlConstantExpression or SqlParameterExpression)
        {
            var typeMapping = ExpressionExtensions.InferTypeMapping(instance, arguments[0], arguments[1]);
            instance = _sqlExpressionFactory.ApplyTypeMapping(instance, typeMapping);

            return _sqlExpressionFactory.And(
                _sqlExpressionFactory.GreaterThanOrEqual(
                    instance,
                    _sqlExpressionFactory.ApplyTypeMapping(arguments[0], typeMapping)),
                _sqlExpressionFactory.LessThan(
                    instance,
                    _sqlExpressionFactory.ApplyTypeMapping(arguments[1], typeMapping)));
        }

        return null;
    }
}