using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Snowflake.EntityFrameworkCore.Query.Internal;

/// <summary>
/// Translates <see cref="TimeSpan" /> members to Snowflake SQL functions.
/// </summary>
public class SnowflakeTimeSpanMemberTranslator : IMemberTranslator
{
    private static readonly Dictionary<string, double> DurationToNumbers = new()
    {
        { nameof(TimeSpan.TotalDays), 86400 },
        { nameof(TimeSpan.TotalHours), 3600 },
        { nameof(TimeSpan.TotalMinutes), 60 },
        { nameof(TimeSpan.TotalMilliseconds), 0.001 }
    };

    private static readonly Dictionary<string, string> DatePartMappings = new()
    {
        { nameof(TimeSpan.Hours), "hour" },
        { nameof(TimeSpan.Minutes), "minute" },
        { nameof(TimeSpan.Seconds), "second" },
        { nameof(TimeSpan.Milliseconds), "millisecond" }
    };

    private SqlFunctionExpression DatePart(string part, SqlExpression instance)
        => _sqlExpressionFactory.Function(
            "DATE_PART",
            [_sqlExpressionFactory.Constant(part), instance],
            nullable: true,
            argumentsPropagateNullability: [false, true],
            typeof(double));

    private readonly ISqlExpressionFactory _sqlExpressionFactory;

    /// <summary>
    /// Creates a new instance of <see cref="SnowflakeTimeSpanMemberTranslator" />
    /// </summary>
    public SnowflakeTimeSpanMemberTranslator(ISqlExpressionFactory sqlExpressionFactory)
    {
        _sqlExpressionFactory = sqlExpressionFactory;
    }

    /// <inheritdoc />
    public virtual SqlExpression? Translate(
        SqlExpression? instance,
        MemberInfo member,
        Type returnType,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        if (member.DeclaringType != typeof(TimeSpan) || instance == null)
        {
            return null;
        }

        if (DurationToNumbers.TryGetValue(member.Name, out var number))
        {
            return _sqlExpressionFactory.Divide(DatePart("epoch", instance), _sqlExpressionFactory.Constant(number));
        }

        return DatePartMappings.TryGetValue(member.Name, out var value) ? DatePart(value, instance) : null;
    }
}