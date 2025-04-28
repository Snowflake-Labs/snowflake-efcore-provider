using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Snowflake.EntityFrameworkCore.Query.Internal;

/// <summary>
/// Translates <see cref="DateTime"/> members into Snowflake specific SQL date functions.
/// </summary>
public class SnowflakeDateTimeMemberTranslator : IMemberTranslator
{
    private static readonly Dictionary<string, string> DatePartMapping
        = new()
        {
            { nameof(DateTime.Year), "year" },
            { nameof(DateTime.Month), "month" },
            { nameof(DateTime.DayOfYear), "dayofyear" },
            { nameof(DateTime.Day), "day" },
            { nameof(DateTime.Hour), "hour" },
            { nameof(DateTime.Minute), "minute" },
            { nameof(DateTime.Second), "second" },
            { nameof(DateTime.Millisecond), "millisecond" }
        };

    private readonly ISqlExpressionFactory _sqlExpressionFactory;
    private readonly IRelationalTypeMappingSource _typeMappingSource;

    /// <summary>
    ///  The supported types for this translator
    /// </summary>
    private readonly HashSet<Type> _supportedTypes =
    [
        typeof(DateTime),
        typeof(DateTimeOffset),
        typeof(DateTime?),
        typeof(DateTimeOffset?)
    ];

    /// <summary>
    /// Creates a new instance of <see cref="SnowflakeDateTimeMemberTranslator" />
    /// </summary>
    /// <param name="sqlExpressionFactory"></param>
    /// <param name="typeMappingSource"></param>
    public SnowflakeDateTimeMemberTranslator(
        ISqlExpressionFactory sqlExpressionFactory,
        IRelationalTypeMappingSource typeMappingSource)
    {
        _sqlExpressionFactory = sqlExpressionFactory;
        _typeMappingSource = typeMappingSource;
    }

    /// <inheritdoc />
    public virtual SqlExpression? Translate(
        SqlExpression? instance,
        MemberInfo member,
        Type returnType,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        var declaringType = member.DeclaringType;

        if (_supportedTypes.Contains(declaringType))
        {
            var memberName = member.Name;

            if (DatePartMapping.TryGetValue(memberName, out var datePart))
            {
                return _sqlExpressionFactory.Function(
                    "DATE_PART",
                    [_sqlExpressionFactory.Fragment(datePart), instance!],
                    nullable: true,
                    argumentsPropagateNullability: [false, true],
                    returnType);
            }

            switch (memberName)
            {
                case nameof(DateTime.Date):
                    return new SnowflakeCastFunctionExpression(
                        instance!,
                        _sqlExpressionFactory.Fragment("DATE"),
                        returnType,
                        _typeMappingSource.FindMapping(typeof(DateTime)));

                case nameof(DateTime.Now):
                    return _sqlExpressionFactory.Function(
                        declaringType == typeof(DateTime) ? "GETDATE" : "SYSDATE",
                        [],
                        nullable: false,
                        argumentsPropagateNullability: [],
                        returnType,
                        _typeMappingSource.FindMapping(returnType));

                case nameof(DateTime.UtcNow):
                    var serverTranslation = _sqlExpressionFactory.Function(
                        declaringType == typeof(DateTime) ? "CURRENT_DATE" : "CURRENT_TIMESTAMP",
                        [],
                        nullable: false,
                        argumentsPropagateNullability: [],
                        returnType,
                        _typeMappingSource.FindMapping(returnType));

                    return declaringType == typeof(DateTime)
                        ? serverTranslation
                        : _sqlExpressionFactory.Convert(serverTranslation, returnType);

                case nameof(DateTime.Today):
                    return new SnowflakeCastFunctionExpression(
                        _sqlExpressionFactory.Function(
                            "GETDATE",
                            [],
                            nullable: false,
                            argumentsPropagateNullability: [],
                            typeof(DateTime),
                            _typeMappingSource.FindMapping(typeof(DateTime))),
                        _sqlExpressionFactory.Fragment("DATE"),
                        returnType,
                        _typeMappingSource.FindMapping(returnType));
            }
        }

        return null;
    }
}