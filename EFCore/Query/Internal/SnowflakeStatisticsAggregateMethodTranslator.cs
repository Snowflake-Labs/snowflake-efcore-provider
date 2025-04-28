using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;

namespace Snowflake.EntityFrameworkCore.Query.Internal;

/// <summary>
/// Translates aggregate functions to Snowflake.
/// </summary>
public class SnowflakeStatisticsAggregateMethodTranslator : IAggregateMethodCallTranslator
{
    private readonly ISqlExpressionFactory _sqlExpressionFactory;
    private readonly RelationalTypeMapping _doubleTypeMapping;

    /// <summary>
    /// Create a new instance of <see cref="SnowflakeStatisticsAggregateMethodTranslator"/>.
    /// </summary>
    /// <param name="sqlExpressionFactory"></param>
    /// <param name="typeMappingSource"></param>
    public SnowflakeStatisticsAggregateMethodTranslator(
        ISqlExpressionFactory sqlExpressionFactory,
        IRelationalTypeMappingSource typeMappingSource)
    {
        _sqlExpressionFactory = sqlExpressionFactory;
        _doubleTypeMapping = typeMappingSource.FindMapping(typeof(double))!;
    }

    /// <inheritdoc />
    public virtual SqlExpression? Translate(
        MethodInfo method,
        EnumerableExpression source,
        IReadOnlyList<SqlExpression> arguments,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {

        if (method.DeclaringType != typeof(SnowflakeDbFunctionsExtensions)
            || source.Selector is not SqlExpression sqlExpression)
        {
            return null;
        }

        var functionName = method.Name switch
        {
            nameof(SnowflakeDbFunctionsExtensions.StandardDeviationSample) => "STDEV",
            nameof(SnowflakeDbFunctionsExtensions.StandardDeviationPopulation) => "STDEVP",
            nameof(SnowflakeDbFunctionsExtensions.VarianceSample) => "VAR",
            nameof(SnowflakeDbFunctionsExtensions.VariancePopulation) => "VARP",
            _ => null
        };

        if (functionName is null)
        {
            return null;
        }

        return SnowflakeExpression.AggregateFunction(
            _sqlExpressionFactory,
            functionName,
            [sqlExpression],
            source,
            enumerableArgumentIndex: 0,
            nullable: true,
            argumentsPropagateNullability: new[] { false },
            typeof(double),
            _doubleTypeMapping);
    }
}
