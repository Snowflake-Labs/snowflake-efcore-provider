using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Snowflake.EntityFrameworkCore.Query.Internal;

/// <summary>
/// Translates <see cref="TimeOnly"/> members into DATE_PART expressions.
/// </summary>
public class SnowflakeTimeOnlyMemberTranslator : IMemberTranslator
{
    private static readonly Dictionary<string, string> DatePartMappings = new()
    {
        { nameof(TimeOnly.Hour), "hour" },
        { nameof(TimeOnly.Minute), "minute" },
        { nameof(TimeOnly.Second), "second" },
        { nameof(TimeOnly.Millisecond), "millisecond" }
    };

    private readonly ISqlExpressionFactory _sqlExpressionFactory;

    /// <summary>
    /// Creates a new instance of <see cref="SnowflakeTimeOnlyMemberTranslator" />
    /// </summary>
    /// <param name="sqlExpressionFactory"></param>
    public SnowflakeTimeOnlyMemberTranslator(ISqlExpressionFactory sqlExpressionFactory)
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
        if (member.DeclaringType == typeof(TimeOnly) && DatePartMappings.TryGetValue(member.Name, out var value))
        {
            return _sqlExpressionFactory.Function(
                "DATE_PART",
                [_sqlExpressionFactory.Fragment(value), instance!],
                nullable: true,
                argumentsPropagateNullability: new[] { false, true },
                returnType);
        }

        return null;
    }
}