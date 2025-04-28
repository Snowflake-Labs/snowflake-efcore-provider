using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Snowflake.EntityFrameworkCore.Query.Internal;

/// <summary>
/// Translates <see cref="DateOnly" /> members into DATE_PART expressions
/// </summary>
public class SnowflakeDateOnlyMemberTranslator : IMemberTranslator
{
    private static readonly Dictionary<string, string> DatePartMapping
        = new()
        {
            { nameof(DateTime.Year), "year" },
            { nameof(DateTime.Month), "month" },
            { nameof(DateTime.DayOfYear), "dayofyear" },
            { nameof(DateTime.Day), "day" }
        };

    private readonly ISqlExpressionFactory _sqlExpressionFactory;

    /// <summary>
    ///  Creates a new instance of <see cref="SnowflakeDateOnlyMemberTranslator" />
    /// </summary>
    /// <param name="sqlExpressionFactory"></param>
    public SnowflakeDateOnlyMemberTranslator(ISqlExpressionFactory sqlExpressionFactory)
    {
        _sqlExpressionFactory = sqlExpressionFactory;
    }

    /// <inheritdoc />
    public virtual SqlExpression? Translate(
        SqlExpression? instance,
        MemberInfo member,
        Type returnType,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
        => member.DeclaringType == typeof(DateOnly) && DatePartMapping.TryGetValue(member.Name, out var datePart)
            ? _sqlExpressionFactory.Function(
                "DATE_PART",
                [_sqlExpressionFactory.Fragment(datePart), instance!],
                nullable: true,
                argumentsPropagateNullability: [false, true],
                returnType)
            : null;
}