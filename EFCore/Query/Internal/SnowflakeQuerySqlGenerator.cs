using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Snowflake.EntityFrameworkCore.Infrastructure.Internal;
using Snowflake.EntityFrameworkCore.Internal;
using Snowflake.EntityFrameworkCore.Metadata.Internal;
using Snowflake.EntityFrameworkCore.Storage.Internal;
using Snowflake.EntityFrameworkCore.Utilities;

namespace Snowflake.EntityFrameworkCore.Query.Internal;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;

public class SnowflakeQuerySqlGenerator : QuerySqlGenerator
{
    private readonly IRelationalTypeMappingSource _typeMappingSource;
    private readonly ISqlGenerationHelper _sqlGenerationHelper;
    private readonly int _SnowflakeCompatibilityLevel;
    private readonly bool _snowflakeReverseNullOrderingEnabled;

    private static readonly bool UseOldBehavior32375 =
        AppContext.TryGetSwitch("Microsoft.EntityFrameworkCore.Issue32375", out var enabled32375) && enabled32375;

    public  SnowflakeQuerySqlGenerator(
        QuerySqlGeneratorDependencies dependencies,
        IRelationalTypeMappingSource typeMappingSource,
        ISnowflakeSingletonOptions SnowflakeSingletonOptions)
        : base(dependencies)
    {
        _typeMappingSource = typeMappingSource;
        _sqlGenerationHelper = dependencies.SqlGenerationHelper;
        _SnowflakeCompatibilityLevel = SnowflakeSingletonOptions.CompatibilityLevel;
        _snowflakeReverseNullOrderingEnabled = SnowflakeSingletonOptions.ReverseNullOrderingEnabled;
    }


    /// <inheritdoc />
    protected override bool TryGenerateWithoutWrappingSelect(SelectExpression selectExpression)
        // Snowflake doesn't support VALUES as a top-level statement, so we need to wrap the VALUES in a SELECT:
        // SELECT 1 AS x UNION VALUES (2), (3) -- simple
        // SELECT 1 AS x UNION SELECT * FROM (VALUES (2), (3)) AS f(x) -- Snowflake
        => selectExpression.Tables is not [ValuesExpression]
            && base.TryGenerateWithoutWrappingSelect(selectExpression);

    /// <inheritdoc />
    protected override  Expression VisitDelete(DeleteExpression deleteExpression)
    {
        var selectExpression = deleteExpression.SelectExpression;

        if (selectExpression.Offset == null
            && selectExpression.Having == null
            && selectExpression.Orderings.Count == 0
            && selectExpression.GroupBy.Count == 0
            && selectExpression.Projection.Count == 0)
        {
            Sql.Append("DELETE ");
            GenerateTop(selectExpression);

            Sql.AppendLine($"FROM {Dependencies.SqlGenerationHelper.DelimitIdentifier(deleteExpression.Table.Alias)}");

            Sql.Append("FROM ");
            GenerateList(selectExpression.Tables, e => Visit(e), sql => sql.AppendLine());

            if (selectExpression.Predicate != null)
            {
                Sql.AppendLine().Append("WHERE ");

                Visit(selectExpression.Predicate);
            }

            GenerateLimitOffset(selectExpression);

            return deleteExpression;
        }

        throw new InvalidOperationException(
            RelationalStrings.ExecuteOperationWithUnsupportedOperatorInSqlGeneration(nameof(RelationalQueryableExtensions.ExecuteDelete)));
    }

    /// <inheritdoc />
    protected override  void GenerateEmptyProjection(SelectExpression selectExpression)
    {
        base.GenerateEmptyProjection(selectExpression);
        if (selectExpression.Alias != null)
        {
            Sql.Append(" AS empty");
        }
    }

    /// <inheritdoc />
    protected override Expression VisitOrdering(OrderingExpression ordering)
    {
        var result = base.VisitOrdering(ordering);

        if (_snowflakeReverseNullOrderingEnabled)
        {
            Sql.Append(ordering.IsAscending ? " NULLS FIRST" : " NULLS LAST");
        }

        return result;
    }

    /// <inheritdoc />
    protected override  Expression VisitUpdate(UpdateExpression updateExpression)
    {
        var selectExpression = updateExpression.SelectExpression;

        if (selectExpression.Offset == null
            && selectExpression.Having == null
            && selectExpression.Orderings.Count == 0
            && selectExpression.GroupBy.Count == 0
            && selectExpression.Projection.Count == 0)
        {
            Sql.Append("UPDATE ");
            GenerateTop(selectExpression);

            Sql.AppendLine($"{Dependencies.SqlGenerationHelper.DelimitIdentifier(updateExpression.Table.Alias)}");
            Sql.Append("SET ");
            Visit(updateExpression.ColumnValueSetters[0].Column);
            Sql.Append(" = ");
            Visit(updateExpression.ColumnValueSetters[0].Value);

            using (Sql.Indent())
            {
                foreach (var columnValueSetter in updateExpression.ColumnValueSetters.Skip(1))
                {
                    Sql.AppendLine(",");
                    Visit(columnValueSetter.Column);
                    Sql.Append(" = ");
                    Visit(columnValueSetter.Value);
                }
            }

            Sql.AppendLine().Append("FROM ");
            GenerateList(selectExpression.Tables, e => Visit(e), sql => sql.AppendLine());

            if (selectExpression.Predicate != null)
            {
                Sql.AppendLine().Append("WHERE ");
                Visit(selectExpression.Predicate);
            }

            return updateExpression;
        }

        throw new InvalidOperationException(
            RelationalStrings.ExecuteOperationWithUnsupportedOperatorInSqlGeneration(nameof(RelationalQueryableExtensions.ExecuteUpdate)));
    }

    /// <inheritdoc />
    protected override  Expression VisitValues(ValuesExpression valuesExpression)
    {
        base.VisitValues(valuesExpression);

        // Snowflake VALUES supports setting the projects column names: FROM (VALUES (1), (2)) AS v(foo)
        Sql.Append("(");

        for (var i = 0; i < valuesExpression.ColumnNames.Count; i++)
        {
            if (i > 0)
            {
                Sql.Append(", ");
            }

            Sql.Append(_sqlGenerationHelper.DelimitIdentifier(valuesExpression.ColumnNames[i]));
        }

        Sql.Append(")");

        return valuesExpression;
    }

    /// <inheritdoc />
    protected override  void GenerateValues(ValuesExpression valuesExpression)
    {
        if (!UseOldBehavior32375 && valuesExpression.RowValues.Count == 0)
        {
            throw new InvalidOperationException(RelationalStrings.EmptyCollectionNotSupportedAsInlineQueryRoot);
        }

        // Snowflake supports providing the names of columns projected out of VALUES: (VALUES (1, 3), (2, 4)) AS x(a, b)
        // (this is implemented in VisitValues above).
        // But since other databases sometimes don't, the default relational implementation is complex, involving a SELECT for the first row
        // and a UNION All on the rest. Override to do the nice simple thing.

        var rowValues = valuesExpression.RowValues;

        Sql.Append("VALUES ");

        for (var i = 0; i < rowValues.Count; i++)
        {
            if (i > 0)
            {
                Sql.Append(", ");
            }

            Visit(valuesExpression.RowValues[i]);
        }
    }

    /// <inheritdoc />
    protected override  void GenerateTop(SelectExpression selectExpression)
    {
    }

    /// <inheritdoc />
    protected override  void GenerateOrderings(SelectExpression selectExpression)
    {
        base.GenerateOrderings(selectExpression);

        // In Snowflake, if an offset is specified, then an ORDER BY clause must also exist.
        // Generate a fake one.
        if (!selectExpression.Orderings.Any() && selectExpression.Offset != null)
        {
            Sql.AppendLine().Append("ORDER BY (SELECT 1)");
        }
    }

    /// <inheritdoc />
    protected override  void GenerateLimitOffset(SelectExpression selectExpression)
    {
        // Note: For Limit without Offset, Snowflake generates TOP()
        if (selectExpression.Offset != null)
        {
            if (selectExpression.Limit == null)
            {
                Sql.AppendLine().Append("LIMIT NULL");
            }

            Sql.AppendLine()
                .Append("OFFSET ");

            Visit(selectExpression.Offset);

            if (selectExpression.Limit != null)
            {
                Sql.Append(" ROWS");

                Sql.Append(" FETCH NEXT ");

                Visit(selectExpression.Limit);

                Sql.Append(" ROWS ONLY");
            }
        }
        else if (selectExpression.Limit != null)
        {
            Sql.AppendLine()
                .Append("LIMIT ");
                
            Visit(selectExpression.Limit);

            Sql.AppendLine();
        }
    }

    protected  virtual Expression VisitSnowflakeAggregateFunction(SnowflakeAggregateFunctionExpression aggregateFunctionExpression)
    {
        Sql.Append(aggregateFunctionExpression.Name);

        Sql.Append("(");
        GenerateList(aggregateFunctionExpression.Arguments, e => Visit(e));
        Sql.Append(")");

        if (aggregateFunctionExpression.Orderings.Count > 0)
        {
            Sql.Append(" WITHIN GROUP (ORDER BY ");
            GenerateList(aggregateFunctionExpression.Orderings, e => Visit(e));
            Sql.Append(")");
        }

        return aggregateFunctionExpression;
    }

    protected  virtual Expression VisitCastFunctionExpression(SnowflakeCastFunctionExpression castFunctionExpression)
    {
        Sql.Append(castFunctionExpression.CastPart.Sql);
        Visit(castFunctionExpression.Source);
        Sql.Append(castFunctionExpression.AsPart.Sql);
        Visit(castFunctionExpression.Target);
        Sql.Append(castFunctionExpression.ClosePart.Sql);

        return castFunctionExpression;
    }

    /// <inheritdoc />
    protected override  Expression VisitExtension(Expression extensionExpression)
    {
        switch (extensionExpression)
        {
            case SnowflakeAggregateFunctionExpression aggregateFunctionExpression:
                return VisitSnowflakeAggregateFunction(aggregateFunctionExpression);

            case SnowflakeOpenJsonExpression openJsonExpression:
                return VisitOpenJsonExpression(openJsonExpression);

            case SnowflakeCastFunctionExpression castFunctionExpression:
                return VisitCastFunctionExpression(castFunctionExpression);
        }

        return base.VisitExtension(extensionExpression);
    }
    //  TODO REVIEW   
    // // TODO: for some reason not called when LET expression is setting a concatenation of strings 
    // protected override string GenerateOperator(SqlBinaryExpression expression)
    //     => expression.NodeType == ExpressionType.Add
    //        && expression.Type == typeof(string)
    //         ? " || "
    //         : base.GenerateOperator(expression);
    //

    /// <inheritdoc />
    protected override string GetOperator(SqlBinaryExpression expression)
    {
        Check.NotNull(expression, nameof(expression));

        return expression.OperatorType switch
        {
            ExpressionType.Add when expression.Type == typeof(string) => " || ",
            ExpressionType.Or => " OR ",
            ExpressionType.And => " AND ",
            _ => base.GetOperator(expression)
        };
    }
    
    

    /// <inheritdoc />
    protected override  Expression VisitJsonScalar(JsonScalarExpression jsonScalarExpression)
    {
        // TODO: Stop producing empty JsonScalarExpressions, #30768
        var path = jsonScalarExpression.Path;
        if (path.Count == 0)
        {
            Visit(jsonScalarExpression.Json);
            return jsonScalarExpression;
        }

        if (jsonScalarExpression.TypeMapping is SnowflakeJsonTypeMapping
            || jsonScalarExpression.TypeMapping?.ElementTypeMapping is not null)
        {
            Sql.Append("JSON_QUERY(");
        }
        else
        {
            Sql.Append(jsonScalarExpression.TypeMapping is StringTypeMapping ? "JSON_VALUE(" : "CAST(JSON_VALUE(");
        }

        Visit(jsonScalarExpression.Json);

        Sql.Append(", ");
        GenerateJsonPath(jsonScalarExpression.Path);
        Sql.Append(")");

        if (jsonScalarExpression.TypeMapping is not SnowflakeJsonTypeMapping and not StringTypeMapping)
        {
            Sql.Append(" AS ");
            Sql.Append(jsonScalarExpression.TypeMapping!.StoreType);
            Sql.Append(")");
        }

        return jsonScalarExpression;
    }

    private void GenerateJsonPath(IReadOnlyList<PathSegment> path)
    {
        Sql.Append("'$");

        foreach (var pathSegment in path)
        {
            switch (pathSegment)
            {
                case { PropertyName: string propertyName }:
                    Sql.Append(".").Append(propertyName);
                    break;

                case { ArrayIndex: SqlExpression arrayIndex }:
                    Sql.Append("[");

                    // JSON functions such as JSON_VALUE only support arbitrary expressions for the path parameter in Snowflake 2017 and
                    // above; before that, arguments must be constant strings.
                    if (arrayIndex is SqlConstantExpression)
                    {
                        Visit(arrayIndex);
                    }
                    else if (_SnowflakeCompatibilityLevel >= 140)
                    {
                        Sql.Append("' + CAST(");
                        Visit(arrayIndex);
                        Sql.Append(" AS ");
                        Sql.Append(_typeMappingSource.GetMapping(typeof(string)).StoreType);
                        Sql.Append(") + '");
                    }
                    else
                    {
                        throw new InvalidOperationException(
                            SnowflakeStrings.JsonValuePathExpressionsNotSupported(_SnowflakeCompatibilityLevel));
                    }

                    Sql.Append("]");
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        Sql.Append("'");
    }

    protected  virtual Expression VisitOpenJsonExpression(SnowflakeOpenJsonExpression openJsonExpression)
    {
        // The second argument is the JSON path, which is represented as a list of PathSegments, from which we generate a SQL jsonpath
        // expression.
        Sql.Append("OPENJSON(");

        Visit(openJsonExpression.JsonExpression);

        if (openJsonExpression.Path is not null)
        {
            Sql.Append(", ");
            GenerateJsonPath(openJsonExpression.Path);
        }

        Sql.Append(")");

        if (openJsonExpression.ColumnInfos is not null)
        {
            Sql.Append(" WITH (");

            if (openJsonExpression.ColumnInfos is [var singleColumnInfo])
            {
                GenerateColumnInfo(singleColumnInfo);
            }
            else
            {
                Sql.AppendLine();
                using var _ = Sql.Indent();

                for (var i = 0; i < openJsonExpression.ColumnInfos.Count; i++)
                {
                    var columnInfo = openJsonExpression.ColumnInfos[i];

                    if (i > 0)
                    {
                        Sql.AppendLine(",");
                    }

                    GenerateColumnInfo(columnInfo);
                }

                Sql.AppendLine();
            }

            Sql.Append(")");

            void GenerateColumnInfo(SnowflakeOpenJsonExpression.ColumnInfo columnInfo)
            {
                Sql
                    .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(columnInfo.Name))
                    .Append(" ")
                    .Append(columnInfo.TypeMapping.StoreType);

                if (columnInfo.Path is not null)
                {
                    Sql.Append(" ");
                    GenerateJsonPath(columnInfo.Path);
                }

                if (columnInfo.AsJson)
                {
                    Sql.Append(" AS JSON");
                }
            }
        }

        Sql.Append(AliasSeparator).Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(openJsonExpression.Alias));

        return openJsonExpression;
    }

    /// <inheritdoc />
    protected override void CheckComposableSqlTrimmed(ReadOnlySpan<char> sql)
    {
        base.CheckComposableSqlTrimmed(sql);

        if (sql.StartsWith("WITH", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(RelationalStrings.FromSqlNonComposable);
        }
    }

    /// <inheritdoc />
    protected override  bool TryGetOperatorInfo(SqlExpression expression, out int precedence, out bool isAssociative)
    {
        (precedence, isAssociative) = expression switch
        {
            SqlBinaryExpression sqlBinaryExpression => sqlBinaryExpression.OperatorType switch
            {
                ExpressionType.Multiply => (900, true),
                ExpressionType.Divide => (900, false),
                ExpressionType.Modulo => (900, false),
                ExpressionType.Add => (800, true),
                ExpressionType.Subtract => (800, false),
                ExpressionType.And => (700, true),
                ExpressionType.Or => (700, true),
                ExpressionType.LeftShift => (700, true),
                ExpressionType.RightShift => (700, true),
                ExpressionType.LessThan => (600, false),
                ExpressionType.LessThanOrEqual => (600, false),
                ExpressionType.GreaterThan => (600, false),
                ExpressionType.GreaterThanOrEqual => (600, false),
                ExpressionType.Equal => (500, false),
                ExpressionType.NotEqual => (500, false),
                ExpressionType.AndAlso => (200, true),
                ExpressionType.OrElse => (100, true),

                _ => default,
            },

            SqlUnaryExpression sqlUnaryExpression => sqlUnaryExpression.OperatorType switch
            {
                ExpressionType.Convert => (1300, false),
                ExpressionType.Not when sqlUnaryExpression.Type != typeof(bool) => (1200, false),
                ExpressionType.Negate => (1100, false),
                ExpressionType.Equal => (500, false), // IS NULL
                ExpressionType.NotEqual => (500, false), // IS NOT NULL
                ExpressionType.Not when sqlUnaryExpression.Type == typeof(bool) => (300, false),

                _ => default,
            },

            CollateExpression => (900, false),
            LikeExpression => (350, false),
            AtTimeZoneExpression => (1200, false),

            // On Snowflake, JsonScalarExpression renders as a function (JSON_VALUE()), so there's never a need for parentheses.
            JsonScalarExpression => (9999, false),

            _ => default,
        };

        return precedence != default;
    }

    private void GenerateList<T>(
        IReadOnlyList<T> items,
        Action<T> generationAction,
        Action<IRelationalCommandBuilder>? joinAction = null)
    {
        joinAction ??= (isb => isb.Append(", "));

        for (var i = 0; i < items.Count; i++)
        {
            if (i > 0)
            {
                joinAction(Sql);
            }

            generationAction(items[i]);
        }
    }
}