using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Snowflake.EntityFrameworkCore.Utilities;

namespace Snowflake.EntityFrameworkCore.Query.Internal;

/// <summary>
/// Translates methods on <see cref="DateTime"/>, <see cref="DateTimeOffset"/> and <see cref="TimeSpan"/> to Snowflake SQL functions.
/// </summary>
public class SnowflakeDateTimeMethodTranslator : IMethodCallTranslator
{
    private readonly Dictionary<MethodInfo, string> _methodInfoDatePartMapping = new()
    {
        { typeof(DateTime).GetRuntimeMethod(nameof(DateTime.AddYears), [typeof(int)])!, "year" },
        { typeof(DateTime).GetRuntimeMethod(nameof(DateTime.AddMonths), [typeof(int)])!, "month" },
        { typeof(DateTime).GetRuntimeMethod(nameof(DateTime.AddDays), [typeof(double)])!, "day" },
        { typeof(DateTime).GetRuntimeMethod(nameof(DateTime.AddHours), [typeof(double)])!, "hour" },
        { typeof(DateTime).GetRuntimeMethod(nameof(DateTime.AddMinutes), [typeof(double)])!, "minute" },
        { typeof(DateTime).GetRuntimeMethod(nameof(DateTime.AddSeconds), [typeof(double)])!, "second" },
        { typeof(DateTime).GetRuntimeMethod(nameof(DateTime.AddMilliseconds), [typeof(double)])!, "millisecond" },
        { typeof(DateTimeOffset).GetRuntimeMethod(nameof(DateTimeOffset.AddYears), [typeof(int)])!, "year" },
        { typeof(DateTimeOffset).GetRuntimeMethod(nameof(DateTimeOffset.AddMonths), [typeof(int)])!, "month" },
        { typeof(DateTimeOffset).GetRuntimeMethod(nameof(DateTimeOffset.AddDays), [typeof(double)])!, "day" },
        { typeof(DateTimeOffset).GetRuntimeMethod(nameof(DateTimeOffset.AddHours), [typeof(double)])!, "hour" },
        { typeof(DateTimeOffset).GetRuntimeMethod(nameof(DateTimeOffset.AddMinutes), [typeof(double)])!, "minute" },
        { typeof(DateTimeOffset).GetRuntimeMethod(nameof(DateTimeOffset.AddSeconds), [typeof(double)])!, "second" },
        { typeof(DateTimeOffset).GetRuntimeMethod(nameof(DateTimeOffset.AddMilliseconds), [typeof(double)])!, "millisecond" }
    };

    private static readonly Dictionary<MethodInfo, string> _methodInfoDateDiffMapping = new()
    {
        { typeof(DateTimeOffset).GetRuntimeMethod(nameof(DateTimeOffset.ToUnixTimeSeconds), Type.EmptyTypes)!, "second" },
        { typeof(DateTimeOffset).GetRuntimeMethod(nameof(DateTimeOffset.ToUnixTimeMilliseconds), Type.EmptyTypes)!, "millisecond" }
    };

    private static readonly MethodInfo AtTimeZoneDateTimeOffsetMethodInfo = typeof(SnowflakeDbFunctionsExtensions)
        .GetRuntimeMethod(nameof(SnowflakeDbFunctionsExtensions.AtTimeZone), [typeof(DbFunctions), typeof(DateTimeOffset), typeof(string)])!;

    private static readonly MethodInfo AtTimeZoneDateTimeMethodInfo = typeof(SnowflakeDbFunctionsExtensions)
        .GetRuntimeMethod(
            nameof(SnowflakeDbFunctionsExtensions.AtTimeZone), [typeof(DbFunctions), typeof(DateTime), typeof(string)])!;

    private readonly ISqlExpressionFactory _sqlExpressionFactory;
    private readonly IRelationalTypeMappingSource _typeMappingSource;

    /// <summary>
    ///    Creates a new instance of <see cref="SnowflakeDateTimeMethodTranslator" />.
    /// </summary>
    /// <param name="sqlExpressionFactory"></param>
    /// <param name="typeMappingSource"></param>
    public SnowflakeDateTimeMethodTranslator(
        ISqlExpressionFactory sqlExpressionFactory,
        IRelationalTypeMappingSource typeMappingSource)
    {
        _sqlExpressionFactory = sqlExpressionFactory;
        _typeMappingSource = typeMappingSource;
    }

    /// <inheritdoc />
    public virtual SqlExpression? Translate(
        SqlExpression? instance,
        MethodInfo method,
        IReadOnlyList<SqlExpression> arguments,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        if (_methodInfoDatePartMapping.TryGetValue(method, out var datePart)
            && instance != null)
        {
            // Some Add methods accept a double, and Snowflake DateAdd does not accept number argument outside of int range
            if (arguments[0] is SqlConstantExpression { Value: double and (<= int.MinValue or >= int.MaxValue) })
            {
                return null;
            }

            // DATEADD defaults to interpreting its last argument as datetime, not datetime2.
            // Our default mapping for DateTime is datetime2, so we force constants to be datetime instead here.
            if (instance is SqlConstantExpression instanceConstant)
            {
                instance = instanceConstant.ApplyTypeMapping(
                    _typeMappingSource.FindMapping(typeof(DateTime), "datetime"));
            }

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

        if (method == AtTimeZoneDateTimeOffsetMethodInfo || method == AtTimeZoneDateTimeMethodInfo)
        {
            var (operand, timeZone) = (arguments[1], arguments[2]);

            RelationalTypeMapping? resultTypeMapping = null;

            // The AT TIME ZONE construct bubbles up the precision of its operand, so when invoked over datetime2(2) it returns a
            // datetimeoffset(2). So if the operand has a type mapping, bubble it up accordingly, otherwise allow the result type mapping
            // to be inferred.
            if (operand.TypeMapping is { } operandTypeMapping)
            {
                resultTypeMapping = operandTypeMapping.StoreTypeNameBase switch
                {
                    "datetimeoffset"
                        => operandTypeMapping,
                    "datetime" or "datetime2" or "smalldatetime"
                        => _typeMappingSource.FindMapping(
                            typeof(DateTimeOffset), "datetimeoffset", precision: operandTypeMapping.Precision),
                    _ => null
                };

                Check.DebugAssert(
                    resultTypeMapping is not null,
                    $"Unknown operand type mapping '{operandTypeMapping.StoreTypeNameBase}' when translating EF.Functions.AtTimeZone");
            }

            if (operand is SqlConstantExpression)
            {
                // Our constant representation for datetime/datetimeoffset is an untyped string literal, which the AT TIME ZONE expression
                // does not accept. Type it explicitly.
                operand = _sqlExpressionFactory.Convert(operand, operand.Type);
            }

            return new AtTimeZoneExpression(
                operand,
                _sqlExpressionFactory.ApplyTypeMapping(timeZone, _typeMappingSource.FindMapping("varchar")),
                typeof(DateTimeOffset),
                resultTypeMapping);
        }

        if (_methodInfoDateDiffMapping.TryGetValue(method, out var timePart))
        {
            return _sqlExpressionFactory.Function(
                "DATEDIFF",
                [
                    _sqlExpressionFactory.Fragment(timePart),
                    _sqlExpressionFactory.Constant(DateTimeOffset.UnixEpoch, instance!.TypeMapping),
                    instance
                ],
                nullable: true,
                argumentsPropagateNullability: [false, true, true],
                typeof(long));
        }

        return null;
    }
}