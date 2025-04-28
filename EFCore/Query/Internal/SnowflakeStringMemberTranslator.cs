using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using System;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;

namespace Snowflake.EntityFrameworkCore.Query.Internal;

/// <summary>
/// Translates string member to Snowflake.
/// </summary>
public class SnowflakeStringMemberTranslator : IMemberTranslator
{
    private readonly ISqlExpressionFactory _sqlExpressionFactory;

    /// <summary>
    /// Create a new instance of <see cref="SnowflakeStringMemberTranslator"/>.
    /// </summary>
    /// <param name="sqlExpressionFactory"></param>
    public SnowflakeStringMemberTranslator(ISqlExpressionFactory sqlExpressionFactory)
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
        if (member.Name == nameof(string.Length)
            && instance?.Type == typeof(string))
        {
            return _sqlExpressionFactory.Convert(
                _sqlExpressionFactory.Function(
                    "LEN",
                    [instance],
                    nullable: true,
                    argumentsPropagateNullability: new[] { true },
                    typeof(long)),
                returnType);
        }

        return null;
    }
}