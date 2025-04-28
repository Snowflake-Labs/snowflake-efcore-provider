using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using ExpressionExtensions = Microsoft.EntityFrameworkCore.Query.ExpressionExtensions;

namespace Snowflake.EntityFrameworkCore.Query.Internal;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;

public class SnowflakeMathTranslator : IMethodCallTranslator
{
    private static readonly Dictionary<MethodInfo, string> SupportedMethodTranslations = new()
    {
        { typeof(Math).GetRuntimeMethod(nameof(Math.Abs), new[] { typeof(decimal) })!, "ABS" },
        { typeof(Math).GetRuntimeMethod(nameof(Math.Abs), new[] { typeof(double) })!, "ABS" },
        { typeof(Math).GetRuntimeMethod(nameof(Math.Abs), new[] { typeof(float) })!, "ABS" },
        { typeof(Math).GetRuntimeMethod(nameof(Math.Abs), new[] { typeof(int) })!, "ABS" },
        { typeof(Math).GetRuntimeMethod(nameof(Math.Abs), new[] { typeof(long) })!, "ABS" },
        { typeof(Math).GetRuntimeMethod(nameof(Math.Abs), new[] { typeof(sbyte) })!, "ABS" },
        { typeof(Math).GetRuntimeMethod(nameof(Math.Abs), new[] { typeof(short) })!, "ABS" },
        { typeof(Math).GetRuntimeMethod(nameof(Math.Ceiling), new[] { typeof(decimal) })!, "CEIL" },
        { typeof(Math).GetRuntimeMethod(nameof(Math.Ceiling), new[] { typeof(double) })!, "CEIL" },
        { typeof(Math).GetRuntimeMethod(nameof(Math.Floor), new[] { typeof(decimal) })!, "FLOOR" },
        { typeof(Math).GetRuntimeMethod(nameof(Math.Floor), new[] { typeof(double) })!, "FLOOR" },
        { typeof(Math).GetRuntimeMethod(nameof(Math.Pow), new[] { typeof(double), typeof(double) })!, "POWER" },
        { typeof(Math).GetRuntimeMethod(nameof(Math.Exp), new[] { typeof(double) })!, "EXP" },
        { typeof(Math).GetRuntimeMethod(nameof(Math.Log10), new[] { typeof(double) })!, "LOG" },
        { typeof(Math).GetRuntimeMethod(nameof(Math.Log), new[] { typeof(double) })!, "LN" },
        { typeof(Math).GetRuntimeMethod(nameof(Math.Log), new[] { typeof(double), typeof(double) })!, "LOG" },
        { typeof(Math).GetRuntimeMethod(nameof(Math.Sqrt), new[] { typeof(double) })!, "SQRT" },
        { typeof(Math).GetRuntimeMethod(nameof(Math.Acos), new[] { typeof(double) })!, "ACOS" },
        { typeof(Math).GetRuntimeMethod(nameof(Math.Asin), new[] { typeof(double) })!, "ASIN" },
        { typeof(Math).GetRuntimeMethod(nameof(Math.Atan), new[] { typeof(double) })!, "ATAN" },
        { typeof(Math).GetRuntimeMethod(nameof(Math.Atan2), new[] { typeof(double), typeof(double) })!, "ATAN2" },
        { typeof(Math).GetRuntimeMethod(nameof(Math.Cos), new[] { typeof(double) })!, "COS" },
        { typeof(Math).GetRuntimeMethod(nameof(Math.Sin), new[] { typeof(double) })!, "SIN" },
        { typeof(Math).GetRuntimeMethod(nameof(Math.Tan), new[] { typeof(double) })!, "TAN" },
        { typeof(Math).GetRuntimeMethod(nameof(Math.Sign), new[] { typeof(decimal) })!, "SIGN" },
        { typeof(Math).GetRuntimeMethod(nameof(Math.Sign), new[] { typeof(double) })!, "SIGN" },
        { typeof(Math).GetRuntimeMethod(nameof(Math.Sign), new[] { typeof(float) })!, "SIGN" },
        { typeof(Math).GetRuntimeMethod(nameof(Math.Sign), new[] { typeof(int) })!, "SIGN" },
        { typeof(Math).GetRuntimeMethod(nameof(Math.Sign), new[] { typeof(long) })!, "SIGN" },
        { typeof(Math).GetRuntimeMethod(nameof(Math.Sign), new[] { typeof(sbyte) })!, "SIGN" },
        { typeof(Math).GetRuntimeMethod(nameof(Math.Sign), new[] { typeof(short) })!, "SIGN" },
        { typeof(double).GetRuntimeMethod(nameof(double.DegreesToRadians), new[] { typeof(double) })!, "RADIANS" },
        { typeof(double).GetRuntimeMethod(nameof(double.RadiansToDegrees), new[] { typeof(double) })!, "DEGREES" },
        { typeof(MathF).GetRuntimeMethod(nameof(MathF.Ceiling), new[] { typeof(float) })!, "CEIL" },
        { typeof(MathF).GetRuntimeMethod(nameof(MathF.Floor), new[] { typeof(float) })!, "FLOOR" },
        { typeof(MathF).GetRuntimeMethod(nameof(MathF.Pow), new[] { typeof(float), typeof(float) })!, "POWER" },
        { typeof(MathF).GetRuntimeMethod(nameof(MathF.Exp), new[] { typeof(float) })!, "EXP" },
        { typeof(MathF).GetRuntimeMethod(nameof(MathF.Log10), new[] { typeof(float) })!, "LOG" },
        { typeof(MathF).GetRuntimeMethod(nameof(MathF.Log), new[] { typeof(float) })!, "LN" },
        { typeof(MathF).GetRuntimeMethod(nameof(MathF.Log), new[] { typeof(float), typeof(float) })!, "LOG" },
        { typeof(MathF).GetRuntimeMethod(nameof(MathF.Sqrt), new[] { typeof(float) })!, "SQRT" },
        { typeof(MathF).GetRuntimeMethod(nameof(MathF.Acos), new[] { typeof(float) })!, "ACOS" },
        { typeof(MathF).GetRuntimeMethod(nameof(MathF.Asin), new[] { typeof(float) })!, "ASIN" },
        { typeof(MathF).GetRuntimeMethod(nameof(MathF.Atan), new[] { typeof(float) })!, "ATAN" },
        { typeof(MathF).GetRuntimeMethod(nameof(MathF.Atan2), new[] { typeof(float), typeof(float) })!, "ATAN2" },
        { typeof(MathF).GetRuntimeMethod(nameof(MathF.Cos), new[] { typeof(float) })!, "COS" },
        { typeof(MathF).GetRuntimeMethod(nameof(MathF.Sin), new[] { typeof(float) })!, "SIN" },
        { typeof(MathF).GetRuntimeMethod(nameof(MathF.Tan), new[] { typeof(float) })!, "TAN" },
        { typeof(float).GetRuntimeMethod(nameof(float.DegreesToRadians), new[] { typeof(float) })!, "RADIANS" },
        { typeof(float).GetRuntimeMethod(nameof(float.RadiansToDegrees), new[] { typeof(float) })!, "DEGREES" },


        { typeof(Math).GetRuntimeMethod(nameof(Math.Max), new[] { typeof(decimal), typeof(decimal) })!, "GREATEST" },
        { typeof(Math).GetRuntimeMethod(nameof(Math.Max), new[] { typeof(double), typeof(double) })!, "GREATEST" },
        { typeof(Math).GetRuntimeMethod(nameof(Math.Max), new[] { typeof(float), typeof(float) })!, "GREATEST" },
        { typeof(Math).GetRuntimeMethod(nameof(Math.Max), new[] { typeof(int), typeof(int) })!, "GREATEST" },
        { typeof(Math).GetRuntimeMethod(nameof(Math.Max), new[] { typeof(long), typeof(long) })!, "GREATEST" },
        { typeof(Math).GetRuntimeMethod(nameof(Math.Max), new[] { typeof(short), typeof(short) })!, "GREATEST" },
        { typeof(MathF).GetRuntimeMethod(nameof(MathF.Max), new[] { typeof(float), typeof(float) })!, "GREATEST" },

        { typeof(Math).GetRuntimeMethod(nameof(Math.Min), new[] { typeof(decimal), typeof(decimal) })!, "LEAST" },
        { typeof(Math).GetRuntimeMethod(nameof(Math.Min), new[] { typeof(double), typeof(double) })!, "LEAST" },
        { typeof(Math).GetRuntimeMethod(nameof(Math.Min), new[] { typeof(float), typeof(float) })!, "LEAST" },
        { typeof(Math).GetRuntimeMethod(nameof(Math.Min), new[] { typeof(int), typeof(int) })!, "LEAST" },
        { typeof(Math).GetRuntimeMethod(nameof(Math.Min), new[] { typeof(long), typeof(long) })!, "LEAST" },
        { typeof(Math).GetRuntimeMethod(nameof(Math.Min), new[] { typeof(short), typeof(short) })!, "LEAST" },
        { typeof(MathF).GetRuntimeMethod(nameof(MathF.Min), new[] { typeof(float), typeof(float) })!, "LEAST" },
    };

    private static readonly IEnumerable<MethodInfo> TruncateMethodInfos = new[]
    {
        typeof(Math).GetRuntimeMethod(nameof(Math.Truncate), new[] { typeof(decimal) })!,
        typeof(Math).GetRuntimeMethod(nameof(Math.Truncate), new[] { typeof(double) })!,
        typeof(MathF).GetRuntimeMethod(nameof(MathF.Truncate), new[] { typeof(float) })!
    };

    private static readonly IEnumerable<MethodInfo> RoundMethodInfos = new[]
    {
        typeof(Math).GetRuntimeMethod(nameof(Math.Round), new[] { typeof(decimal) })!,
        typeof(Math).GetRuntimeMethod(nameof(Math.Round), new[] { typeof(double) })!,
        typeof(Math).GetRuntimeMethod(nameof(Math.Round), new[] { typeof(decimal), typeof(int) })!,
        typeof(Math).GetRuntimeMethod(nameof(Math.Round), new[] { typeof(double), typeof(int) })!,
        typeof(MathF).GetRuntimeMethod(nameof(MathF.Round), new[] { typeof(float) })!,
        typeof(MathF).GetRuntimeMethod(nameof(MathF.Round), new[] { typeof(float), typeof(int) })!
    };

    private readonly ISqlExpressionFactory _sqlExpressionFactory;

    
    public SnowflakeMathTranslator(ISqlExpressionFactory sqlExpressionFactory)
    {
        _sqlExpressionFactory = sqlExpressionFactory;
    }

    
    public virtual SqlExpression? Translate(
        SqlExpression? instance,
        MethodInfo method,
        IReadOnlyList<SqlExpression> arguments,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        if (SupportedMethodTranslations.TryGetValue(method, out var sqlFunctionName))
        {
            var typeMapping = arguments.Count == 1
                ? ExpressionExtensions.InferTypeMapping(arguments[0])
                : ExpressionExtensions.InferTypeMapping(arguments[0], arguments[1]);

            var newArguments = new SqlExpression[arguments.Count];
            newArguments[0] = _sqlExpressionFactory.ApplyTypeMapping(arguments[0], typeMapping);

            if (arguments.Count == 2)
            {
                newArguments[1] = _sqlExpressionFactory.ApplyTypeMapping(arguments[1], typeMapping);
            }


            if (sqlFunctionName == "LOG")
            {
                newArguments = newArguments.Length == 2
                    ?
                    // LOG(base, argument)
                    [newArguments[1], newArguments[0]]
                    :
                    // LOG(10, argument)
                    [_sqlExpressionFactory.Constant(10), newArguments[0]];
            }

            return _sqlExpressionFactory.Function(
                sqlFunctionName,
                newArguments,
                nullable: true,
                argumentsPropagateNullability: newArguments.Select(_ => true).ToArray(),
                method.ReturnType,
                sqlFunctionName == "SIGN" ? null : typeMapping);
        }

        if (TruncateMethodInfos.Contains(method))
        {
            var argument = arguments[0];
            // C# has Round over decimal/double/float only so our argument will be one of those types (compiler puts convert node)
            // In database result will be same type except for float which returns double which we need to cast back to float.
            var resultType = argument.Type;
            if (resultType == typeof(float))
            {
                resultType = typeof(double);
            }

            var result = (SqlExpression)_sqlExpressionFactory.Function(
                "TRUNC",
                new[] { argument, _sqlExpressionFactory.Constant(0) },
                nullable: true,
                argumentsPropagateNullability: new[] { true, false, false },
                resultType);

            if (argument.Type == typeof(float))
            {
                result = _sqlExpressionFactory.Convert(result, typeof(float));
            }

            return _sqlExpressionFactory.ApplyTypeMapping(result, argument.TypeMapping);
        }

        if (RoundMethodInfos.Contains(method))
        {
            var argument = arguments[0];
            var digits = arguments.Count == 2 ? arguments[1] : _sqlExpressionFactory.Constant(0);
            // C# has Round over decimal/double/float only so our argument will be one of those types (compiler puts convert node)
            // In database result will be same type except for float which returns double which we need to cast back to float.
            var resultType = argument.Type;
            if (resultType == typeof(float))
            {
                resultType = typeof(double);
            }

            var result = (SqlExpression)_sqlExpressionFactory.Function(
                "ROUND",
                new[] { argument, digits },
                nullable: true,
                argumentsPropagateNullability: new[] { true, true },
                resultType);

            if (argument.Type == typeof(float))
            {
                result = _sqlExpressionFactory.Convert(result, typeof(float));
            }

            return _sqlExpressionFactory.ApplyTypeMapping(result, argument.TypeMapping);
        }

        return null;
    }
}