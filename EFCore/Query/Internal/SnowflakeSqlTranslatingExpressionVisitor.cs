using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Query;
using ExpressionExtensions = Microsoft.EntityFrameworkCore.Query.ExpressionExtensions;

namespace Snowflake.EntityFrameworkCore.Query.Internal;

/// <summary>
/// Represents a SQL translating expression visitor for Snowflake.
/// </summary>
public class SnowflakeSqlTranslatingExpressionVisitor : RelationalSqlTranslatingExpressionVisitor
{
    private readonly SnowflakeQueryCompilationContext _queryCompilationContext;
    private readonly ISqlExpressionFactory _sqlExpressionFactory;

    private static readonly bool UseOldBehavior32432 =
        AppContext.TryGetSwitch("Microsoft.EntityFrameworkCore.Issue32432", out var enabled32432) && enabled32432;

    private static readonly HashSet<string> DateTimeDataTypes
        =
        [
            "date",
            "time",
            "timestamp_ntz",
            "timestamp_ltz",
            "timestamp_tz"
        ];

    private static readonly HashSet<Type> DateTimeClrTypes
        =
        [
            typeof(TimeOnly),
            typeof(DateOnly),
            typeof(TimeSpan),
            typeof(DateTime),
            typeof(DateTimeOffset)
        ];

    private static readonly HashSet<ExpressionType> ArithmeticOperatorTypes
        =
        [
            ExpressionType.Add,
            ExpressionType.Subtract,
            ExpressionType.Multiply,
            ExpressionType.Divide,
            ExpressionType.Modulo
        ];

    private static readonly MethodInfo StringStartsWithMethodInfo
        = typeof(string).GetRuntimeMethod(nameof(string.StartsWith), [typeof(string)])!;

    private static readonly MethodInfo StringEndsWithMethodInfo
        = typeof(string).GetRuntimeMethod(nameof(string.EndsWith), [typeof(string)])!;

    private static readonly MethodInfo StringContainsMethodInfo
        = typeof(string).GetRuntimeMethod(nameof(string.Contains), [typeof(string)])!;

    private static readonly MethodInfo EscapeLikePatternParameterMethod =
        typeof(SnowflakeSqlTranslatingExpressionVisitor).GetTypeInfo()
            .GetDeclaredMethod(nameof(ConstructLikePatternParameter))!;

    /// <summary>
    /// Creates a new instance of <see cref="SnowflakeSqlTranslatingExpressionVisitor" />
    /// </summary>
    /// <param name="dependencies"></param>
    /// <param name="queryCompilationContext"></param>
    /// <param name="queryableMethodTranslatingExpressionVisitor"></param>
    public SnowflakeSqlTranslatingExpressionVisitor(
        RelationalSqlTranslatingExpressionVisitorDependencies dependencies,
        SnowflakeQueryCompilationContext queryCompilationContext,
        QueryableMethodTranslatingExpressionVisitor queryableMethodTranslatingExpressionVisitor)
        : base(dependencies, queryCompilationContext, queryableMethodTranslatingExpressionVisitor)
    {
        _queryCompilationContext = queryCompilationContext;
        _sqlExpressionFactory = dependencies.SqlExpressionFactory;
    }

    /// <inheritdoc />
    protected override Expression VisitBinary(BinaryExpression binaryExpression)
    {
        if (binaryExpression.NodeType == ExpressionType.ArrayIndex
            && binaryExpression.Left.Type == typeof(byte[]))
        {
            return TranslateByteArrayElementAccess(
                binaryExpression.Left,
                binaryExpression.Right,
                binaryExpression.Type);
        }

        var visitedExpression = base.VisitBinary(binaryExpression);

        if (visitedExpression is SqlBinaryExpression sqlBinaryExpression
            && ArithmeticOperatorTypes.Contains(sqlBinaryExpression.OperatorType))
        {
            var inferredProviderType =
                GetProviderType(sqlBinaryExpression.Left) ?? GetProviderType(sqlBinaryExpression.Right);
            if (inferredProviderType != null)
            {
                if (DateTimeDataTypes.Contains(inferredProviderType))
                {
                    return QueryCompilationContext.NotTranslatedExpression;
                }
            }
            else
            {
                var leftType = sqlBinaryExpression.Left.Type;
                var rightType = sqlBinaryExpression.Right.Type;
                if (DateTimeClrTypes.Contains(leftType)
                    || DateTimeClrTypes.Contains(rightType))
                {
                    return QueryCompilationContext.NotTranslatedExpression;
                }
            }
        }

        return visitedExpression;
    }

    /// <inheritdoc />
    protected override Expression VisitUnary(UnaryExpression unaryExpression)
    {
        if (unaryExpression.NodeType == ExpressionType.ArrayLength
            && unaryExpression.Operand.Type == typeof(byte[]))
        {
            if (!(base.Visit(unaryExpression.Operand) is SqlExpression sqlExpression))
            {
                return QueryCompilationContext.NotTranslatedExpression;
            }

            var isBinaryMaxDataType = GetProviderType(sqlExpression) == "varbinary(max)" ||
                                      sqlExpression is SqlParameterExpression;
            var dataLengthSqlFunction = Dependencies.SqlExpressionFactory.Function(
                "LENGTH",
                [sqlExpression],
                nullable: true,
                argumentsPropagateNullability: [true],
                isBinaryMaxDataType ? typeof(long) : typeof(int));

            return isBinaryMaxDataType
                ? Dependencies.SqlExpressionFactory.Convert(dataLengthSqlFunction, typeof(int))
                : dataLengthSqlFunction;
        }

        return base.VisitUnary(unaryExpression);
    }

    /// <inheritdoc />
    protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
    {
        var method = methodCallExpression.Method;

        if (method.IsGenericMethod
            && method.GetGenericMethodDefinition() == EnumerableMethods.ElementAt
            && methodCallExpression.Arguments[0].Type == typeof(byte[]))
        {
            return TranslateByteArrayElementAccess(
                methodCallExpression.Arguments[0],
                methodCallExpression.Arguments[1],
                methodCallExpression.Type);
        }

        if (method == StringStartsWithMethodInfo
            && TryTranslateStartsEndsWithContains(
                methodCallExpression.Object!, methodCallExpression.Arguments[0], StartsEndsWithContains.StartsWith,
                out var translation1))
        {
            return translation1;
        }

        if (method == StringEndsWithMethodInfo
            && TryTranslateStartsEndsWithContains(
                methodCallExpression.Object!, methodCallExpression.Arguments[0], StartsEndsWithContains.EndsWith,
                out var translation2))
        {
            return translation2;
        }

        if (method == StringContainsMethodInfo
            && TryTranslateStartsEndsWithContains(
                methodCallExpression.Object!, methodCallExpression.Arguments[0], StartsEndsWithContains.Contains,
                out var translation3))
        {
            return translation3;
        }

        return base.VisitMethodCall(methodCallExpression);

        bool TryTranslateStartsEndsWithContains(
            Expression instance,
            Expression pattern,
            StartsEndsWithContains methodType,
            [NotNullWhen(true)] out SqlExpression? translation)
        {
            if (Visit(instance) is not SqlExpression translatedInstance
                || Visit(pattern) is not SqlExpression translatedPattern)
            {
                translation = null;
                return false;
            }

            var stringTypeMapping = ExpressionExtensions.InferTypeMapping(translatedInstance, translatedPattern);

            translatedInstance = _sqlExpressionFactory.ApplyTypeMapping(translatedInstance, stringTypeMapping);
            translatedPattern = _sqlExpressionFactory.ApplyTypeMapping(translatedPattern, stringTypeMapping);

            switch (translatedPattern)
            {
                case SqlConstantExpression patternConstant:
                {
                    // The pattern is constant. Aside from null and empty string, we escape all special characters (%, _, \) and send a
                    // simple LIKE
                    translation = patternConstant.Value switch
                    {
                        null => _sqlExpressionFactory.Like(translatedInstance,
                            _sqlExpressionFactory.Constant(null, stringTypeMapping)),

                        // In .NET, all strings start with/end with/contain the empty string, but SQL LIKE return false for empty patterns.
                        // Return % which always matches instead.
                        // Note that we don't just return a true constant, since null strings shouldn't match even an empty string
                        // (but SqlNullabilityProcess will convert this to a true constant if the instance is non-nullable)
                        "" => _sqlExpressionFactory.Like(translatedInstance, _sqlExpressionFactory.Constant("%")),

                        string s => s.Any(IsLikeWildChar)
                            ? _sqlExpressionFactory.Like(
                                translatedInstance,
                                _sqlExpressionFactory.Constant(
                                    methodType switch
                                    {
                                        StartsEndsWithContains.StartsWith => EscapeLikePattern(s) + '%',
                                        StartsEndsWithContains.EndsWith => '%' + EscapeLikePattern(s),
                                        StartsEndsWithContains.Contains => $"%{EscapeLikePattern(s)}%",

                                        _ => throw new ArgumentOutOfRangeException(nameof(methodType), methodType, null)
                                    }))
                            : _sqlExpressionFactory.Like(
                                translatedInstance,
                                _sqlExpressionFactory.Constant(
                                    methodType switch
                                    {
                                        StartsEndsWithContains.StartsWith => s + '%',
                                        StartsEndsWithContains.EndsWith => '%' + s,
                                        StartsEndsWithContains.Contains => $"%{s}%",

                                        _ => throw new ArgumentOutOfRangeException(nameof(methodType), methodType, null)
                                    })),

                        _ => throw new UnreachableException()
                    };

                    return true;
                }

                case SqlParameterExpression patternParameter
                    when patternParameter.Name.StartsWith(QueryCompilationContext.QueryParameterPrefix,
                        StringComparison.Ordinal):
                {
                    // The pattern is a parameter, register a runtime parameter that will contain the rewritten LIKE pattern, where
                    // all special characters have been escaped.
                    var lambda = Expression.Lambda(
                        Expression.Call(
                            EscapeLikePatternParameterMethod,
                            QueryCompilationContext.QueryContextParameter,
                            Expression.Constant(patternParameter.Name),
                            Expression.Constant(methodType)),
                        QueryCompilationContext.QueryContextParameter);

                    var escapedPatternParameter = UseOldBehavior32432
                        ? _queryCompilationContext.RegisterRuntimeParameter(patternParameter.Name + "_rewritten",
                            lambda)
                        : _queryCompilationContext.RegisterRuntimeParameter(
                            $"{patternParameter.Name}_{methodType.ToString().ToLower(CultureInfo.InvariantCulture)}",
                            lambda);

                    translation = _sqlExpressionFactory.Like(
                        translatedInstance,
                        new SqlParameterExpression(escapedPatternParameter.Name!, escapedPatternParameter.Type,
                            stringTypeMapping));

                    return true;
                }

                default:
                    // The pattern is a column or a complex expression; the possible special characters in the pattern cannot be escaped,
                    // preventing us from translating to LIKE.
                    translation = methodType switch
                    {
                        // For StartsWith/EndsWith, use LEFT or RIGHT instead to extract substring and compare:
                        // WHERE instance IS NOT NULL AND pattern IS NOT NULL AND LEFT(instance, LEN(pattern)) = pattern
                        // This is less efficient than LIKE (i.e. StartsWith does an index scan instead of seek), but we have no choice.
                        // Note that we compensate for the case where both the instance and the pattern are null (null.StartsWith(null)); a
                        // simple equality would yield true in that case, but we want false. We technically
                        StartsEndsWithContains.StartsWith or StartsEndsWithContains.EndsWith
                            => _sqlExpressionFactory.AndAlso(
                                _sqlExpressionFactory.IsNotNull(translatedInstance),
                                _sqlExpressionFactory.AndAlso(
                                    _sqlExpressionFactory.IsNotNull(translatedPattern),
                                    _sqlExpressionFactory.Equal(
                                        _sqlExpressionFactory.Function(
                                            methodType is StartsEndsWithContains.StartsWith ? "LEFT" : "RIGHT",
                                            [
                                                translatedInstance,
                                                _sqlExpressionFactory.Function(
                                                    "LEN",
                                                    [translatedPattern],
                                                    nullable: true,
                                                    argumentsPropagateNullability: [true],
                                                    typeof(int))
                                            ],
                                            nullable: true,
                                            argumentsPropagateNullability: [true, true],
                                            typeof(string),
                                            stringTypeMapping),
                                        translatedPattern))),

                        // For Contains, just use CHARINDEX and check if the result is greater than 0.
                        // Add a check to return null when the pattern is an empty string (and the string isn't null)
                        StartsEndsWithContains.Contains
                            => _sqlExpressionFactory.AndAlso(
                                _sqlExpressionFactory.IsNotNull(translatedInstance),
                                _sqlExpressionFactory.AndAlso(
                                    _sqlExpressionFactory.IsNotNull(translatedPattern),
                                    _sqlExpressionFactory.OrElse(
                                        _sqlExpressionFactory.GreaterThan(
                                            _sqlExpressionFactory.Function(
                                                "CHARINDEX",
                                                [translatedPattern, translatedInstance],
                                                nullable: true,
                                                argumentsPropagateNullability: [true, true],
                                                typeof(int)),
                                            _sqlExpressionFactory.Constant(0)),
                                        _sqlExpressionFactory.Like(
                                            translatedPattern,
                                            _sqlExpressionFactory.Constant(string.Empty, stringTypeMapping))))),

                        _ => throw new UnreachableException()
                    };

                    return true;
            }
        }
    }

    /// <summary>
    /// Constructs the pattern for the LIKE expression.
    /// </summary>
    /// <param name="queryContext"></param>
    /// <param name="baseParameterName"></param>
    /// <param name="methodType"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    /// <exception cref="UnreachableException"></exception>
    private static string? ConstructLikePatternParameter(
        QueryContext queryContext,
        string baseParameterName,
        StartsEndsWithContains methodType)
        => queryContext.ParameterValues[baseParameterName] switch
        {
            null => null,

            // In .NET, all strings start/end with the empty string, but SQL LIKE return false for empty patterns.
            // Return % which always matches instead.
            "" => "%",

            string s => methodType switch
            {
                StartsEndsWithContains.StartsWith => EscapeLikePattern(s) + '%',
                StartsEndsWithContains.EndsWith => '%' + EscapeLikePattern(s),
                StartsEndsWithContains.Contains => $"%{EscapeLikePattern(s)}%",
                _ => throw new ArgumentOutOfRangeException(nameof(methodType), methodType, null)
            },

            _ => throw new UnreachableException()
        };

    /// <summary>
    /// Represents the type of the method call that is being translated.
    /// </summary>
    private enum StartsEndsWithContains
    {
        StartsWith,
        EndsWith,
        Contains
    }

    private static bool IsLikeWildChar(char c)
        => c is '%' or '_' or '[';

    /// <summary>
    /// Escapes the special characters in the given pattern so that they are treated as literals in a LIKE expression.
    /// </summary>
    /// <param name="pattern"></param>
    /// <returns></returns>
    private static string EscapeLikePattern(string pattern)
    {
        int i;
        for (i = 0; i < pattern.Length; i++)
        {
            var c = pattern[i];
            if (IsLikeWildChar(c))
            {
                break;
            }
        }

        if (i == pattern.Length) // No special characters were detected, just return the original pattern string
        {
            return pattern;
        }

        var builder = new StringBuilder(pattern, 0, i, pattern.Length + 10);

        for (; i < pattern.Length; i++)
        {
            var c = pattern[i];

            builder.Append(c);
        }

        return builder.ToString();
    }

    /// <inheritdoc />
    protected override bool TryTranslateAggregateMethodCall(
        MethodCallExpression methodCallExpression,
        [NotNullWhen(true)] out SqlExpression? translation)
    {
        var previousInAggregateFunction = _queryCompilationContext.InAggregateFunction;
        _queryCompilationContext.InAggregateFunction = true;

        var result = base.TryTranslateAggregateMethodCall(methodCallExpression, out translation);

        _queryCompilationContext.InAggregateFunction = previousInAggregateFunction;

        return result;
    }

    /// <summary>
    /// Translates the element access of a byte array.
    /// </summary>
    /// <param name="array"></param>
    /// <param name="index"></param>
    /// <param name="resultType"></param>
    /// <returns></returns>
    private Expression TranslateByteArrayElementAccess(Expression array, Expression index, Type resultType)
    {
        var visitedArray = Visit(array);
        var visitedIndex = Visit(index);

        return visitedArray is SqlExpression sqlArray
               && visitedIndex is SqlExpression sqlIndex
            ? Dependencies.SqlExpressionFactory.Convert(
                Dependencies.SqlExpressionFactory.Function(
                    "SUBSTRING",
                    [
                        sqlArray,
                        Dependencies.SqlExpressionFactory.Add(
                            Dependencies.SqlExpressionFactory.ApplyDefaultTypeMapping(sqlIndex),
                            Dependencies.SqlExpressionFactory.Constant(1)),
                        Dependencies.SqlExpressionFactory.Constant(1)
                    ],
                    nullable: true,
                    argumentsPropagateNullability: [true, true, true],
                    typeof(byte[])),
                resultType)
            : QueryCompilationContext.NotTranslatedExpression;
    }

    /// <summary>
    /// Gets the provider type of the given expression.
    /// </summary>
    /// <param name="expression"></param>
    /// <returns></returns>
    private static string? GetProviderType(SqlExpression expression)
        => expression.TypeMapping?.StoreType;
}