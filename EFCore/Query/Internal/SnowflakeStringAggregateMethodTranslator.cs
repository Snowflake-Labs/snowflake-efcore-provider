using System.Collections.Generic;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Snowflake.EntityFrameworkCore.Query.Internal;

/// <summary>
/// Translates string aggregate methods to Snowflake's LISTAGG function.
/// </summary>
public class SnowflakeStringAggregateMethodTranslator : IAggregateMethodCallTranslator
{
    private static readonly MethodInfo StringConcatMethod
        = typeof(string).GetRuntimeMethod(nameof(string.Concat), [typeof(IEnumerable<string>)])!;

    private static readonly MethodInfo StringJoinMethod
        = typeof(string).GetRuntimeMethod(nameof(string.Join), [typeof(string), typeof(IEnumerable<string>)])!;

    private readonly ISqlExpressionFactory _sqlExpressionFactory;
    private readonly IRelationalTypeMappingSource _typeMappingSource;

    /// <summary>
    /// Creates a new instance of <see cref="SnowflakeStringAggregateMethodTranslator" />
    /// </summary>
    /// <param name="sqlExpressionFactory"></param>
    /// <param name="typeMappingSource"></param>
    public SnowflakeStringAggregateMethodTranslator(
        ISqlExpressionFactory sqlExpressionFactory,
        IRelationalTypeMappingSource typeMappingSource)
    {
        _sqlExpressionFactory = sqlExpressionFactory;
        _typeMappingSource = typeMappingSource;
    }

    /// <inheritdoc />
    public virtual SqlExpression? Translate(
        MethodInfo method,
        EnumerableExpression source,
        IReadOnlyList<SqlExpression> arguments,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {

        if (source.Selector is not SqlExpression sqlExpression
            || (method != StringJoinMethod && method != StringConcatMethod))
        {
            return null;
        }

        // STRING_AGG enlarges the return type size (e.g. for input VARCHAR(5), it returns VARCHAR(8000)).
        var resultTypeMapping = sqlExpression.TypeMapping;
        if (resultTypeMapping?.Size != null)
        {
            if (resultTypeMapping is { IsUnicode: true, Size: < 4000 })
            {
                resultTypeMapping = _typeMappingSource.FindMapping(
                    typeof(string),
                    resultTypeMapping.StoreTypeNameBase,
                    unicode: true,
                    size: 4000);
            }
            else if (resultTypeMapping is { IsUnicode: false, Size: < 8000 })
            {
                resultTypeMapping = _typeMappingSource.FindMapping(
                    typeof(string),
                    resultTypeMapping.StoreTypeNameBase,
                    unicode: false,
                    size: 8000);
            }
        }

        // STRING_AGG filters out nulls, but string.Join treats them as empty strings; coalesce unless we know we're aggregating over
        // a non-nullable column.
        if (sqlExpression is not ColumnExpression { IsNullable: false })
        {
            sqlExpression = _sqlExpressionFactory.Coalesce(
                sqlExpression,
                _sqlExpressionFactory.Constant(string.Empty, typeof(string)));
        }

        // STRING_AGG returns null when there are no rows (or non-null values), but string.Join returns an empty string.
        return
            _sqlExpressionFactory.Coalesce(
                SnowflakeExpression.AggregateFunctionWithOrdering(
                    _sqlExpressionFactory,
                    "LISTAGG",
                    [
                        sqlExpression,
                        _sqlExpressionFactory.ApplyTypeMapping(
                            method == StringJoinMethod ? arguments[0] : _sqlExpressionFactory.Constant(string.Empty, typeof(string)),
                            sqlExpression.TypeMapping)
                    ],
                    source,
                    enumerableArgumentIndex: 0,
                    nullable: true,
                    argumentsPropagateNullability: [false, true],
                    typeof(string)),
                _sqlExpressionFactory.Constant(string.Empty, typeof(string)),
                resultTypeMapping);
    }
}
