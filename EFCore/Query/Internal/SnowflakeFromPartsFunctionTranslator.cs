using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Snowflake.EntityFrameworkCore.Query.Internal;

/// <summary>
/// Translates the Snowflake specific functions that are built from parts.
/// </summary>
public class SnowflakeFromPartsFunctionTranslator : IMethodCallTranslator
{
    private static readonly MethodInfo DateFromPartsMethodInfo = typeof(SnowflakeDbFunctionsExtensions)
        .GetRuntimeMethod(
            nameof(SnowflakeDbFunctionsExtensions.DateFromParts),
            [typeof(DbFunctions), typeof(int), typeof(int), typeof(int)])!;

    private static readonly IDictionary<MethodInfo, (string FunctionName, string ReturnType)> MethodFunctionMapping
        = new Dictionary<MethodInfo, (string, string)>
        {
            { DateFromPartsMethodInfo, ("DATEFROMPARTS", "date") }
        };

    private readonly ISqlExpressionFactory _sqlExpressionFactory;
    private readonly IRelationalTypeMappingSource _typeMappingSource;

    /// <summary>
    ///  Creates a new instance of <see cref="SnowflakeFromPartsFunctionTranslator" />
    /// </summary>
    /// <param name="sqlExpressionFactory"></param>
    /// <param name="typeMappingSource"></param>
    public SnowflakeFromPartsFunctionTranslator(
        ISqlExpressionFactory sqlExpressionFactory,
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
    {
        if (MethodFunctionMapping.TryGetValue(method, out var value))
        {
            return _sqlExpressionFactory.Function(
                value.FunctionName,
                arguments.Skip(1),
                nullable: true,
                argumentsPropagateNullability: arguments.Skip(1).Select(_ => true),
                method.ReturnType,
                _typeMappingSource.FindMapping(method.ReturnType, value.ReturnType));
        }

        return null;
    }
}