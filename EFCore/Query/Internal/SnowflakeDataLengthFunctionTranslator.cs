using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;

namespace Snowflake.EntityFrameworkCore.Query.Internal;

/// <summary>
/// Translates data length functions to Snowflake.
/// </summary>
public class SnowflakeDataLengthFunctionTranslator : IMethodCallTranslator
{
    private static readonly List<string> LongReturningTypes =
    [
        "nvarchar()",
        "varchar()",
        "varbinary()"
    ];

    private static readonly HashSet<MethodInfo> MethodInfoDataLengthMapping
        =
        [
            typeof(SnowflakeDbFunctionsExtensions).GetRuntimeMethod(
                nameof(SnowflakeDbFunctionsExtensions.DataLength), [typeof(DbFunctions), typeof(string)])!,

            typeof(SnowflakeDbFunctionsExtensions).GetRuntimeMethod(
                nameof(SnowflakeDbFunctionsExtensions.DataLength), [typeof(DbFunctions), typeof(bool?)])!,

            typeof(SnowflakeDbFunctionsExtensions).GetRuntimeMethod(
                nameof(SnowflakeDbFunctionsExtensions.DataLength), [typeof(DbFunctions), typeof(double?)])!,

            typeof(SnowflakeDbFunctionsExtensions).GetRuntimeMethod(
                nameof(SnowflakeDbFunctionsExtensions.DataLength), [typeof(DbFunctions), typeof(decimal?)])!,

            typeof(SnowflakeDbFunctionsExtensions).GetRuntimeMethod(
                nameof(SnowflakeDbFunctionsExtensions.DataLength), [typeof(DbFunctions), typeof(DateTime?)])!,

            typeof(SnowflakeDbFunctionsExtensions).GetRuntimeMethod(
                nameof(SnowflakeDbFunctionsExtensions.DataLength), [typeof(DbFunctions), typeof(TimeSpan?)])!,

            typeof(SnowflakeDbFunctionsExtensions).GetRuntimeMethod(
                nameof(SnowflakeDbFunctionsExtensions.DataLength),
                [typeof(DbFunctions), typeof(DateTimeOffset?)])!,

            typeof(SnowflakeDbFunctionsExtensions).GetRuntimeMethod(
                nameof(SnowflakeDbFunctionsExtensions.DataLength), [typeof(DbFunctions), typeof(byte[])])!,

            typeof(SnowflakeDbFunctionsExtensions).GetRuntimeMethod(
                nameof(SnowflakeDbFunctionsExtensions.DataLength), [typeof(DbFunctions), typeof(Guid?)])!
        ];

    private readonly ISqlExpressionFactory _sqlExpressionFactory;

    /// <summary>
    /// Creates a new instance of <see cref="SnowflakeDataLengthFunctionTranslator"/>.
    /// </summary>
    /// <param name="sqlExpressionFactory"></param>
    public SnowflakeDataLengthFunctionTranslator(ISqlExpressionFactory sqlExpressionFactory)
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
        if (MethodInfoDataLengthMapping.Contains(method))
        {
            var argument = arguments[1];
            if (argument.TypeMapping == null)
            {
                argument = _sqlExpressionFactory.ApplyDefaultTypeMapping(argument);
            }

            if (LongReturningTypes.Contains(argument.TypeMapping!.StoreType))
            {
                var result = _sqlExpressionFactory.Function(
                    "LENGTH",
                    arguments.Skip(1),
                    nullable: true,
                    argumentsPropagateNullability: [true],
                    typeof(long));

                return _sqlExpressionFactory.Convert(result, method.ReturnType.UnwrapNullableType());
            }

            return _sqlExpressionFactory.Function(
                "LENGTH",
                arguments.Skip(1),
                nullable: true,
                argumentsPropagateNullability: [true],
                method.ReturnType.UnwrapNullableType());
        }

        return null;
    }
}