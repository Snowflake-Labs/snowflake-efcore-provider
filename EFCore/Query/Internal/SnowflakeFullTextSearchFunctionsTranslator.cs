using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Snowflake.EntityFrameworkCore.Internal;

namespace Snowflake.EntityFrameworkCore.Query.Internal;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;

public class SnowflakeFullTextSearchFunctionsTranslator : IMethodCallTranslator
{
    private const string FreeTextFunctionName = "FREETEXT";
    private const string ContainsFunctionName = "CONTAINS";

    private static readonly MethodInfo FreeTextMethodInfo
        = typeof(SnowflakeDbFunctionsExtensions).GetRuntimeMethod(
            nameof(SnowflakeDbFunctionsExtensions.FreeText), new[] { typeof(DbFunctions), typeof(object), typeof(string) })!;

    private static readonly MethodInfo FreeTextMethodInfoWithLanguage
        = typeof(SnowflakeDbFunctionsExtensions).GetRuntimeMethod(
            nameof(SnowflakeDbFunctionsExtensions.FreeText),
            new[] { typeof(DbFunctions), typeof(object), typeof(string), typeof(int) })!;

    private static readonly MethodInfo ContainsMethodInfo
        = typeof(SnowflakeDbFunctionsExtensions).GetRuntimeMethod(
            nameof(SnowflakeDbFunctionsExtensions.Contains), new[] { typeof(DbFunctions), typeof(object), typeof(string) })!;

    private static readonly MethodInfo ContainsMethodInfoWithLanguage
        = typeof(SnowflakeDbFunctionsExtensions).GetRuntimeMethod(
            nameof(SnowflakeDbFunctionsExtensions.Contains),
            new[] { typeof(DbFunctions), typeof(object), typeof(string), typeof(int) })!;

    private static readonly IDictionary<MethodInfo, string> FunctionMapping
        = new Dictionary<MethodInfo, string>
        {
            { FreeTextMethodInfo, FreeTextFunctionName },
            { FreeTextMethodInfoWithLanguage, FreeTextFunctionName },
            { ContainsMethodInfo, ContainsFunctionName },
            { ContainsMethodInfoWithLanguage, ContainsFunctionName }
        };

    private readonly ISqlExpressionFactory _sqlExpressionFactory;

    
    public SnowflakeFullTextSearchFunctionsTranslator(ISqlExpressionFactory sqlExpressionFactory)
    {
        _sqlExpressionFactory = sqlExpressionFactory;
    }

    
    public virtual SqlExpression? Translate(
        SqlExpression? instance,
        MethodInfo method,
        IReadOnlyList<SqlExpression> arguments,
        IDiagnosticsLogger<DbLoggerCategory.Query> logger)
    {
        if (FunctionMapping.TryGetValue(method, out var functionName))
        {
            var propertyReference = arguments[1];
            if (propertyReference is not ColumnExpression)
            {
                throw new InvalidOperationException(SnowflakeStrings.InvalidColumnNameForFreeText);
            }

            var typeMapping = propertyReference.TypeMapping;
            var freeText = propertyReference.Type == arguments[2].Type
                ? _sqlExpressionFactory.ApplyTypeMapping(arguments[2], typeMapping)
                : _sqlExpressionFactory.ApplyDefaultTypeMapping(arguments[2]);

            var functionArguments = new List<SqlExpression> { propertyReference, freeText };

            if (arguments.Count == 4)
            {
                functionArguments.Add(
                    _sqlExpressionFactory.Fragment($"LANGUAGE {((SqlConstantExpression)arguments[3]).Value}"));
            }

            return _sqlExpressionFactory.Function(
                functionName,
                functionArguments,
                nullable: true,
                // TODO: don't propagate for now
                argumentsPropagateNullability: functionArguments.Select(_ => false).ToList(),
                typeof(bool));
        }

        return null;
    }
}
