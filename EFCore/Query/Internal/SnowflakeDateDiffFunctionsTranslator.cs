using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;

namespace Snowflake.EntityFrameworkCore.Query.Internal;

/// <summary>
///   Translates the DateDiff functions to Snowflake.
/// </summary>
public class SnowflakeDateDiffFunctionsTranslator : IMethodCallTranslator
{
    private readonly Dictionary<MethodInfo, string> _methodInfoDateDiffMapping
        = new()
        {
            {
                typeof(SnowflakeDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SnowflakeDbFunctionsExtensions.DateDiffYear),
                    [typeof(DbFunctions), typeof(DateTime), typeof(DateTime)])!,
                "year"
            },
            {
                typeof(SnowflakeDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SnowflakeDbFunctionsExtensions.DateDiffYear),
                    [typeof(DbFunctions), typeof(DateTime?), typeof(DateTime?)])!,
                "year"
            },
            {
                typeof(SnowflakeDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SnowflakeDbFunctionsExtensions.DateDiffYear),
                    [typeof(DbFunctions), typeof(DateTimeOffset), typeof(DateTimeOffset)])!,
                "year"
            },
            {
                typeof(SnowflakeDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SnowflakeDbFunctionsExtensions.DateDiffYear),
                    [typeof(DbFunctions), typeof(DateTimeOffset?), typeof(DateTimeOffset?)])!,
                "year"
            },
            {
                typeof(SnowflakeDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SnowflakeDbFunctionsExtensions.DateDiffYear),
                    [typeof(DbFunctions), typeof(DateOnly), typeof(DateOnly)])!,
                "year"
            },
            {
                typeof(SnowflakeDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SnowflakeDbFunctionsExtensions.DateDiffYear),
                    [typeof(DbFunctions), typeof(DateOnly?), typeof(DateOnly?)])!,
                "year"
            },
            {
                typeof(SnowflakeDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SnowflakeDbFunctionsExtensions.DateDiffMonth),
                    [typeof(DbFunctions), typeof(DateTime), typeof(DateTime)])!,
                "month"
            },
            {
                typeof(SnowflakeDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SnowflakeDbFunctionsExtensions.DateDiffMonth),
                    [typeof(DbFunctions), typeof(DateTime?), typeof(DateTime?)])!,
                "month"
            },
            {
                typeof(SnowflakeDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SnowflakeDbFunctionsExtensions.DateDiffMonth),
                    [typeof(DbFunctions), typeof(DateTimeOffset), typeof(DateTimeOffset)])!,
                "month"
            },
            {
                typeof(SnowflakeDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SnowflakeDbFunctionsExtensions.DateDiffMonth),
                    [typeof(DbFunctions), typeof(DateTimeOffset?), typeof(DateTimeOffset?)])!,
                "month"
            },
            {
                typeof(SnowflakeDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SnowflakeDbFunctionsExtensions.DateDiffMonth),
                    [typeof(DbFunctions), typeof(DateOnly), typeof(DateOnly)])!,
                "month"
            },
            {
                typeof(SnowflakeDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SnowflakeDbFunctionsExtensions.DateDiffMonth),
                    [typeof(DbFunctions), typeof(DateOnly?), typeof(DateOnly?)])!,
                "month"
            },
            {
                typeof(SnowflakeDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SnowflakeDbFunctionsExtensions.DateDiffDay),
                    [typeof(DbFunctions), typeof(DateTime), typeof(DateTime)])!,
                "day"
            },
            {
                typeof(SnowflakeDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SnowflakeDbFunctionsExtensions.DateDiffDay),
                    [typeof(DbFunctions), typeof(DateTime?), typeof(DateTime?)])!,
                "day"
            },
            {
                typeof(SnowflakeDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SnowflakeDbFunctionsExtensions.DateDiffDay),
                    [typeof(DbFunctions), typeof(DateTimeOffset), typeof(DateTimeOffset)])!,
                "day"
            },
            {
                typeof(SnowflakeDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SnowflakeDbFunctionsExtensions.DateDiffDay),
                    [typeof(DbFunctions), typeof(DateTimeOffset?), typeof(DateTimeOffset?)])!,
                "day"
            },
            {
                typeof(SnowflakeDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SnowflakeDbFunctionsExtensions.DateDiffDay),
                    [typeof(DbFunctions), typeof(DateOnly), typeof(DateOnly)])!,
                "day"
            },
            {
                typeof(SnowflakeDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SnowflakeDbFunctionsExtensions.DateDiffDay),
                    [typeof(DbFunctions), typeof(DateOnly?), typeof(DateOnly?)])!,
                "day"
            },
            {
                typeof(SnowflakeDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SnowflakeDbFunctionsExtensions.DateDiffHour),
                    [typeof(DbFunctions), typeof(DateTime), typeof(DateTime)])!,
                "hour"
            },
            {
                typeof(SnowflakeDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SnowflakeDbFunctionsExtensions.DateDiffHour),
                    [typeof(DbFunctions), typeof(DateTime?), typeof(DateTime?)])!,
                "hour"
            },
            {
                typeof(SnowflakeDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SnowflakeDbFunctionsExtensions.DateDiffHour),
                    [typeof(DbFunctions), typeof(DateTimeOffset), typeof(DateTimeOffset)])!,
                "hour"
            },
            {
                typeof(SnowflakeDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SnowflakeDbFunctionsExtensions.DateDiffHour),
                    [typeof(DbFunctions), typeof(DateTimeOffset?), typeof(DateTimeOffset?)])!,
                "hour"
            },
            {
                typeof(SnowflakeDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SnowflakeDbFunctionsExtensions.DateDiffHour),
                    [typeof(DbFunctions), typeof(TimeSpan), typeof(TimeSpan)])!,
                "hour"
            },
            {
                typeof(SnowflakeDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SnowflakeDbFunctionsExtensions.DateDiffHour),
                    [typeof(DbFunctions), typeof(TimeSpan?), typeof(TimeSpan?)])!,
                "hour"
            },
            {
                typeof(SnowflakeDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SnowflakeDbFunctionsExtensions.DateDiffHour),
                    [typeof(DbFunctions), typeof(TimeOnly), typeof(TimeOnly)])!,
                "hour"
            },
            {
                typeof(SnowflakeDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SnowflakeDbFunctionsExtensions.DateDiffHour),
                    [typeof(DbFunctions), typeof(TimeOnly?), typeof(TimeOnly?)])!,
                "hour"
            },
            {
                typeof(SnowflakeDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SnowflakeDbFunctionsExtensions.DateDiffHour),
                    [typeof(DbFunctions), typeof(DateOnly), typeof(DateOnly)])!,
                "hour"
            },
            {
                typeof(SnowflakeDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SnowflakeDbFunctionsExtensions.DateDiffHour),
                    [typeof(DbFunctions), typeof(DateOnly?), typeof(DateOnly?)])!,
                "hour"
            },
            {
                typeof(SnowflakeDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SnowflakeDbFunctionsExtensions.DateDiffMinute),
                    [typeof(DbFunctions), typeof(DateTime), typeof(DateTime)])!,
                "minute"
            },
            {
                typeof(SnowflakeDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SnowflakeDbFunctionsExtensions.DateDiffMinute),
                    [typeof(DbFunctions), typeof(DateTime?), typeof(DateTime?)])!,
                "minute"
            },
            {
                typeof(SnowflakeDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SnowflakeDbFunctionsExtensions.DateDiffMinute),
                    [typeof(DbFunctions), typeof(DateTimeOffset), typeof(DateTimeOffset)])!,
                "minute"
            },
            {
                typeof(SnowflakeDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SnowflakeDbFunctionsExtensions.DateDiffMinute),
                    [typeof(DbFunctions), typeof(DateTimeOffset?), typeof(DateTimeOffset?)])!,
                "minute"
            },
            {
                typeof(SnowflakeDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SnowflakeDbFunctionsExtensions.DateDiffMinute),
                    [typeof(DbFunctions), typeof(TimeSpan), typeof(TimeSpan)])!,
                "minute"
            },
            {
                typeof(SnowflakeDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SnowflakeDbFunctionsExtensions.DateDiffMinute),
                    [typeof(DbFunctions), typeof(TimeSpan?), typeof(TimeSpan?)])!,
                "minute"
            },
            {
                typeof(SnowflakeDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SnowflakeDbFunctionsExtensions.DateDiffMinute),
                    [typeof(DbFunctions), typeof(TimeOnly), typeof(TimeOnly)])!,
                "minute"
            },
            {
                typeof(SnowflakeDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SnowflakeDbFunctionsExtensions.DateDiffMinute),
                    [typeof(DbFunctions), typeof(TimeOnly?), typeof(TimeOnly?)])!,
                "minute"
            },
            {
                typeof(SnowflakeDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SnowflakeDbFunctionsExtensions.DateDiffMinute),
                    [typeof(DbFunctions), typeof(DateOnly), typeof(DateOnly)])!,
                "minute"
            },
            {
                typeof(SnowflakeDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SnowflakeDbFunctionsExtensions.DateDiffMinute),
                    [typeof(DbFunctions), typeof(DateOnly?), typeof(DateOnly?)])!,
                "minute"
            },
            {
                typeof(SnowflakeDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SnowflakeDbFunctionsExtensions.DateDiffSecond),
                    [typeof(DbFunctions), typeof(DateTime), typeof(DateTime)])!,
                "second"
            },
            {
                typeof(SnowflakeDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SnowflakeDbFunctionsExtensions.DateDiffSecond),
                    [typeof(DbFunctions), typeof(DateTime?), typeof(DateTime?)])!,
                "second"
            },
            {
                typeof(SnowflakeDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SnowflakeDbFunctionsExtensions.DateDiffSecond),
                    [typeof(DbFunctions), typeof(DateTimeOffset), typeof(DateTimeOffset)])!,
                "second"
            },
            {
                typeof(SnowflakeDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SnowflakeDbFunctionsExtensions.DateDiffSecond),
                    [typeof(DbFunctions), typeof(DateTimeOffset?), typeof(DateTimeOffset?)])!,
                "second"
            },
            {
                typeof(SnowflakeDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SnowflakeDbFunctionsExtensions.DateDiffSecond),
                    [typeof(DbFunctions), typeof(TimeSpan), typeof(TimeSpan)])!,
                "second"
            },
            {
                typeof(SnowflakeDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SnowflakeDbFunctionsExtensions.DateDiffSecond),
                    [typeof(DbFunctions), typeof(TimeSpan?), typeof(TimeSpan?)])!,
                "second"
            },
            {
                typeof(SnowflakeDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SnowflakeDbFunctionsExtensions.DateDiffSecond),
                    [typeof(DbFunctions), typeof(TimeOnly), typeof(TimeOnly)])!,
                "second"
            },
            {
                typeof(SnowflakeDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SnowflakeDbFunctionsExtensions.DateDiffSecond),
                    [typeof(DbFunctions), typeof(TimeOnly?), typeof(TimeOnly?)])!,
                "second"
            },
            {
                typeof(SnowflakeDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SnowflakeDbFunctionsExtensions.DateDiffSecond),
                    [typeof(DbFunctions), typeof(DateOnly), typeof(DateOnly)])!,
                "second"
            },
            {
                typeof(SnowflakeDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SnowflakeDbFunctionsExtensions.DateDiffSecond),
                    [typeof(DbFunctions), typeof(DateOnly?), typeof(DateOnly?)])!,
                "second"
            },
            {
                typeof(SnowflakeDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SnowflakeDbFunctionsExtensions.DateDiffMillisecond),
                    [typeof(DbFunctions), typeof(DateTime), typeof(DateTime)])!,
                "millisecond"
            },
            {
                typeof(SnowflakeDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SnowflakeDbFunctionsExtensions.DateDiffMillisecond),
                    [typeof(DbFunctions), typeof(DateTime?), typeof(DateTime?)])!,
                "millisecond"
            },
            {
                typeof(SnowflakeDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SnowflakeDbFunctionsExtensions.DateDiffMillisecond),
                    [typeof(DbFunctions), typeof(DateTimeOffset), typeof(DateTimeOffset)])!,
                "millisecond"
            },
            {
                typeof(SnowflakeDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SnowflakeDbFunctionsExtensions.DateDiffMillisecond),
                    [typeof(DbFunctions), typeof(DateTimeOffset?), typeof(DateTimeOffset?)])!,
                "millisecond"
            },
            {
                typeof(SnowflakeDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SnowflakeDbFunctionsExtensions.DateDiffMillisecond),
                    [typeof(DbFunctions), typeof(TimeSpan), typeof(TimeSpan)])!,
                "millisecond"
            },
            {
                typeof(SnowflakeDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SnowflakeDbFunctionsExtensions.DateDiffMillisecond),
                    [typeof(DbFunctions), typeof(TimeSpan?), typeof(TimeSpan?)])!,
                "millisecond"
            },
            {
                typeof(SnowflakeDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SnowflakeDbFunctionsExtensions.DateDiffMillisecond),
                    [typeof(DbFunctions), typeof(TimeOnly), typeof(TimeOnly)])!,
                "millisecond"
            },
            {
                typeof(SnowflakeDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SnowflakeDbFunctionsExtensions.DateDiffMillisecond),
                    [typeof(DbFunctions), typeof(TimeOnly?), typeof(TimeOnly?)])!,
                "millisecond"
            },
            {
                typeof(SnowflakeDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SnowflakeDbFunctionsExtensions.DateDiffMillisecond),
                    [typeof(DbFunctions), typeof(DateOnly), typeof(DateOnly)])!,
                "millisecond"
            },
            {
                typeof(SnowflakeDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SnowflakeDbFunctionsExtensions.DateDiffMillisecond),
                    [typeof(DbFunctions), typeof(DateOnly?), typeof(DateOnly?)])!,
                "millisecond"
            },
            {
                typeof(SnowflakeDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SnowflakeDbFunctionsExtensions.DateDiffMicrosecond),
                    [typeof(DbFunctions), typeof(DateTime), typeof(DateTime)])!,
                "microsecond"
            },
            {
                typeof(SnowflakeDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SnowflakeDbFunctionsExtensions.DateDiffMicrosecond),
                    [typeof(DbFunctions), typeof(DateTime?), typeof(DateTime?)])!,
                "microsecond"
            },
            {
                typeof(SnowflakeDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SnowflakeDbFunctionsExtensions.DateDiffMicrosecond),
                    [typeof(DbFunctions), typeof(DateTimeOffset), typeof(DateTimeOffset)])!,
                "microsecond"
            },
            {
                typeof(SnowflakeDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SnowflakeDbFunctionsExtensions.DateDiffMicrosecond),
                    [typeof(DbFunctions), typeof(DateTimeOffset?), typeof(DateTimeOffset?)])!,
                "microsecond"
            },
            {
                typeof(SnowflakeDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SnowflakeDbFunctionsExtensions.DateDiffMicrosecond),
                    [typeof(DbFunctions), typeof(TimeSpan), typeof(TimeSpan)])!,
                "microsecond"
            },
            {
                typeof(SnowflakeDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SnowflakeDbFunctionsExtensions.DateDiffMicrosecond),
                    [typeof(DbFunctions), typeof(TimeSpan?), typeof(TimeSpan?)])!,
                "microsecond"
            },
            {
                typeof(SnowflakeDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SnowflakeDbFunctionsExtensions.DateDiffMicrosecond),
                    [typeof(DbFunctions), typeof(TimeOnly), typeof(TimeOnly)])!,
                "microsecond"
            },
            {
                typeof(SnowflakeDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SnowflakeDbFunctionsExtensions.DateDiffMicrosecond),
                    [typeof(DbFunctions), typeof(TimeOnly?), typeof(TimeOnly?)])!,
                "microsecond"
            },
            {
                typeof(SnowflakeDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SnowflakeDbFunctionsExtensions.DateDiffMicrosecond),
                    [typeof(DbFunctions), typeof(DateOnly), typeof(DateOnly)])!,
                "microsecond"
            },
            {
                typeof(SnowflakeDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SnowflakeDbFunctionsExtensions.DateDiffMicrosecond),
                    [typeof(DbFunctions), typeof(DateOnly?), typeof(DateOnly?)])!,
                "microsecond"
            },
            {
                typeof(SnowflakeDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SnowflakeDbFunctionsExtensions.DateDiffNanosecond),
                    [typeof(DbFunctions), typeof(DateTime), typeof(DateTime)])!,
                "nanosecond"
            },
            {
                typeof(SnowflakeDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SnowflakeDbFunctionsExtensions.DateDiffNanosecond),
                    [typeof(DbFunctions), typeof(DateTime?), typeof(DateTime?)])!,
                "nanosecond"
            },
            {
                typeof(SnowflakeDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SnowflakeDbFunctionsExtensions.DateDiffNanosecond),
                    [typeof(DbFunctions), typeof(DateTimeOffset), typeof(DateTimeOffset)])!,
                "nanosecond"
            },
            {
                typeof(SnowflakeDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SnowflakeDbFunctionsExtensions.DateDiffNanosecond),
                    [typeof(DbFunctions), typeof(DateTimeOffset?), typeof(DateTimeOffset?)])!,
                "nanosecond"
            },
            {
                typeof(SnowflakeDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SnowflakeDbFunctionsExtensions.DateDiffNanosecond),
                    [typeof(DbFunctions), typeof(TimeSpan), typeof(TimeSpan)])!,
                "nanosecond"
            },
            {
                typeof(SnowflakeDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SnowflakeDbFunctionsExtensions.DateDiffNanosecond),
                    [typeof(DbFunctions), typeof(TimeSpan?), typeof(TimeSpan?)])!,
                "nanosecond"
            },
            {
                typeof(SnowflakeDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SnowflakeDbFunctionsExtensions.DateDiffNanosecond),
                    [typeof(DbFunctions), typeof(TimeOnly), typeof(TimeOnly)])!,
                "nanosecond"
            },
            {
                typeof(SnowflakeDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SnowflakeDbFunctionsExtensions.DateDiffNanosecond),
                    [typeof(DbFunctions), typeof(TimeOnly?), typeof(TimeOnly?)])!,
                "nanosecond"
            },
            {
                typeof(SnowflakeDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SnowflakeDbFunctionsExtensions.DateDiffNanosecond),
                    [typeof(DbFunctions), typeof(DateOnly), typeof(DateOnly)])!,
                "nanosecond"
            },
            {
                typeof(SnowflakeDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SnowflakeDbFunctionsExtensions.DateDiffNanosecond),
                    [typeof(DbFunctions), typeof(DateOnly?), typeof(DateOnly?)])!,
                "nanosecond"
            },
            {
                typeof(SnowflakeDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SnowflakeDbFunctionsExtensions.DateDiffWeek),
                    [typeof(DbFunctions), typeof(DateTime), typeof(DateTime)])!,
                "week"
            },
            {
                typeof(SnowflakeDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SnowflakeDbFunctionsExtensions.DateDiffWeek),
                    [typeof(DbFunctions), typeof(DateTime?), typeof(DateTime?)])!,
                "week"
            },
            {
                typeof(SnowflakeDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SnowflakeDbFunctionsExtensions.DateDiffWeek),
                    [typeof(DbFunctions), typeof(DateTimeOffset), typeof(DateTimeOffset)])!,
                "week"
            },
            {
                typeof(SnowflakeDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SnowflakeDbFunctionsExtensions.DateDiffWeek),
                    [typeof(DbFunctions), typeof(DateTimeOffset?), typeof(DateTimeOffset?)])!,
                "week"
            },
            {
                typeof(SnowflakeDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SnowflakeDbFunctionsExtensions.DateDiffWeek),
                    [typeof(DbFunctions), typeof(DateOnly), typeof(DateOnly)])!,
                "week"
            },
            {
                typeof(SnowflakeDbFunctionsExtensions).GetRuntimeMethod(
                    nameof(SnowflakeDbFunctionsExtensions.DateDiffWeek),
                    [typeof(DbFunctions), typeof(DateOnly?), typeof(DateOnly?)])!,
                "week"
            }
        };

    private readonly ISqlExpressionFactory _sqlExpressionFactory;

    /// <summary>
    ///    Initializes a new instance of the <see cref="SnowflakeDateDiffFunctionsTranslator" /> class.
    /// </summary>
    /// <param name="sqlExpressionFactory"></param>
    public SnowflakeDateDiffFunctionsTranslator(
        ISqlExpressionFactory sqlExpressionFactory)
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
        if (_methodInfoDateDiffMapping.TryGetValue(method, out var datePart))
        {
            var startDate = arguments[1];
            var endDate = arguments[2];
            var typeMapping = ExpressionExtensions.InferTypeMapping(startDate, endDate);

            startDate = _sqlExpressionFactory.ApplyTypeMapping(startDate, typeMapping);
            endDate = _sqlExpressionFactory.ApplyTypeMapping(endDate, typeMapping);

            return _sqlExpressionFactory.Function(
                "DATEDIFF",
                [_sqlExpressionFactory.Fragment(datePart), startDate, endDate],
                nullable: true,
                argumentsPropagateNullability: [false, true, true],
                typeof(int));
        }

        return null;
    }
}