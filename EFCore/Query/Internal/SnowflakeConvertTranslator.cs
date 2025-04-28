using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;

namespace Snowflake.EntityFrameworkCore.Query.Internal;

/// <summary>
/// Translates <see cref="Convert" /> method calls into Snowflake CAST expressions.
/// </summary>
public class SnowflakeConvertTranslator : IMethodCallTranslator
{
    private static readonly Dictionary<string, Tuple<string, Type>> TypeMapping = new()
    {
        [nameof(Convert.ToBoolean)] = new Tuple<string, Type>("boolean", typeof(bool)),
        [nameof(Convert.ToByte)] = new Tuple<string, Type>("tinyint", typeof(byte)),
        [nameof(Convert.ToDecimal)] = new Tuple<string, Type>("decimal(18, 2)", typeof(decimal)),
        [nameof(Convert.ToDouble)] = new Tuple<string, Type>("float", typeof(double)),
        [nameof(Convert.ToInt16)] = new Tuple<string, Type>("smallint", typeof(int)),
        [nameof(Convert.ToInt32)] = new Tuple<string, Type>("int", typeof(int)),
        [nameof(Convert.ToInt64)] = new Tuple<string, Type>("bigint", typeof(int)),
        [nameof(Convert.ToString)] = new Tuple<string, Type>("varchar", typeof(string)),
    };

    private static readonly List<Type> SupportedTypes = new()
    {
        typeof(bool),
        typeof(byte),
        typeof(DateTime),
        typeof(decimal),
        typeof(double),
        typeof(float),
        typeof(int),
        typeof(long),
        typeof(short),
        typeof(string)
    };

    private static readonly MethodInfo[] SupportedMethods
        = TypeMapping.Keys
            .SelectMany(
                t => typeof(Convert).GetTypeInfo().GetDeclaredMethods(t)
                    .Where(
                        m => m.GetParameters().Length == 1
                             && SupportedTypes.Contains(m.GetParameters().First().ParameterType)))
            .ToArray();

    private readonly ISqlExpressionFactory _sqlExpressionFactory;

    private readonly IRelationalTypeMappingSource _typeMappingSource;


    /// <summary>
    /// Creates a new instance of <see cref="SnowflakeConvertTranslator" />
    /// </summary>
    /// <param name="sqlExpressionFactory"></param>
    /// <param name="typeMappingSource"></param>
    public SnowflakeConvertTranslator(ISqlExpressionFactory sqlExpressionFactory,
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
        => TypeMapping.TryGetValue(method.Name, out var methodMapping)
            ? new SnowflakeCastFunctionExpression(
                instance,
                _sqlExpressionFactory.Fragment(methodMapping.Item1),
                methodMapping.Item2,
                _typeMappingSource.FindMapping(methodMapping.Item2))
            : null;
}