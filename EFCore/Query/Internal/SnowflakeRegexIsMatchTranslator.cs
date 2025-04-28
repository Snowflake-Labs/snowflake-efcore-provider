using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Snowflake.EntityFrameworkCore.Query.Internal;

/// <summary>
///     Translates Regex.IsMatch calls into Snowflake functions for database-side processing.
/// </summary>
/// <remarks>
///     https://docs.snowflake.com/en/sql-reference/functions/regexp_like
/// </remarks>
public class SnowflakeRegexIsMatchTranslator : IMethodCallTranslator
{
    private static readonly MethodInfo IsMatch =
        typeof(Regex).GetRuntimeMethod(nameof(Regex.IsMatch), [typeof(string), typeof(string)])!;

    private static readonly MethodInfo IsMatchWithRegexOptions =
        typeof(Regex).GetRuntimeMethod(nameof(Regex.IsMatch), [typeof(string), typeof(string), typeof(RegexOptions)])!;

    private readonly ISqlExpressionFactory _sqlExpressionFactory;

    /// <summary>
    ///    Initializes a new instance of the <see cref="SnowflakeRegexIsMatchTranslator" /> class.
    /// </summary>
    /// <param name="sqlExpressionFactory"></param>
    public SnowflakeRegexIsMatchTranslator(ISqlExpressionFactory sqlExpressionFactory)
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
        if (method != IsMatch && method != IsMatchWithRegexOptions)
        {
            return null;
        }

        var input = arguments[0];
        var pattern = arguments[1];
        var typeMapping = ExpressionExtensions.InferTypeMapping(input, pattern);

        input = _sqlExpressionFactory.ApplyTypeMapping(input, typeMapping);
        pattern = _sqlExpressionFactory.ApplyTypeMapping(pattern, typeMapping);

        if (method == IsMatch || arguments[2] is SqlConstantExpression { Value: RegexOptions.None })
        {
            return _sqlExpressionFactory.Function(
                "REGEXP_LIKE",
                [input, pattern],
                nullable: true,
                argumentsPropagateNullability: [true, true],
                typeof(bool));
        }

        if (arguments[2] is SqlConstantExpression { Value: RegexOptions regexOptions })
        {
            var modifier = new StringBuilder();

            if (regexOptions.HasFlag(RegexOptions.Multiline))
            {
                regexOptions &= ~RegexOptions.Multiline;
                modifier.Append('m');
            }

            if (regexOptions.HasFlag(RegexOptions.Singleline))
            {
                regexOptions &= ~RegexOptions.Singleline;
                modifier.Append('s');
            }

            if (regexOptions.HasFlag(RegexOptions.IgnoreCase))
            {
                regexOptions &= ~RegexOptions.IgnoreCase;
                modifier.Append('i');
            }

            if (regexOptions.HasFlag(RegexOptions.IgnorePatternWhitespace))
            {
                regexOptions &= ~RegexOptions.IgnorePatternWhitespace;
                modifier.Append('x');
            }

            return regexOptions == 0
                ? _sqlExpressionFactory.Function(
                    "REGEXP_LIKE",
                    [input, pattern, _sqlExpressionFactory.Constant(modifier.ToString())],
                    nullable: true,
                    argumentsPropagateNullability: [true, true],
                    typeof(bool))
                : null;
        }

        return null;
    }
}