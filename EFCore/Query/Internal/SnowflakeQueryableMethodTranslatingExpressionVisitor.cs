using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Snowflake.EntityFrameworkCore.Infrastructure.Internal;
using Snowflake.EntityFrameworkCore.Internal;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using Snowflake.EntityFrameworkCore.Storage.Internal;
using Snowflake.EntityFrameworkCore.Utilities;

namespace Snowflake.EntityFrameworkCore.Query.Internal;

/// <summary>
/// Snowflake-specific queryable method translating expression visitor.
/// </summary>
public class SnowflakeQueryableMethodTranslatingExpressionVisitor : RelationalQueryableMethodTranslatingExpressionVisitor
{
    private readonly SnowflakeQueryCompilationContext _queryCompilationContext;
    private readonly IRelationalTypeMappingSource _typeMappingSource;
    private readonly ISqlExpressionFactory _sqlExpressionFactory;
    private readonly int _SnowflakeCompatibilityLevel;

    private RelationalTypeMapping? _nvarcharMaxTypeMapping;

    private static readonly bool UseOldBehavior32374 =
        AppContext.TryGetSwitch("Microsoft.EntityFrameworkCore.Issue32374", out var enabled32374) && enabled32374;

    private static readonly bool UseOldBehavior33932 =
        AppContext.TryGetSwitch("Microsoft.EntityFrameworkCore.Issue33932", out var enabled33932) && enabled33932;


    /// <summary>
    ///    Creates a new instance of the <see cref="SnowflakeQueryableMethodTranslatingExpressionVisitor" /> class.
    /// </summary>
    /// <param name="dependencies"></param>
    /// <param name="relationalDependencies"></param>
    /// <param name="queryCompilationContext"></param>
    /// <param name="SnowflakeSingletonOptions"></param>
    public SnowflakeQueryableMethodTranslatingExpressionVisitor(
        QueryableMethodTranslatingExpressionVisitorDependencies dependencies,
        RelationalQueryableMethodTranslatingExpressionVisitorDependencies relationalDependencies,
        SnowflakeQueryCompilationContext queryCompilationContext,
        ISnowflakeSingletonOptions SnowflakeSingletonOptions)
        : base(dependencies, relationalDependencies, queryCompilationContext)
    {
        _queryCompilationContext = queryCompilationContext;
        _typeMappingSource = relationalDependencies.TypeMappingSource;
        _sqlExpressionFactory = relationalDependencies.SqlExpressionFactory;

        _SnowflakeCompatibilityLevel = SnowflakeSingletonOptions.CompatibilityLevel;
    }


    /// <summary>
    ///   Creates a new instance of the <see cref="SnowflakeQueryableMethodTranslatingExpressionVisitor" /> class.
    /// </summary>
    /// <param name="parentVisitor"></param>
    protected SnowflakeQueryableMethodTranslatingExpressionVisitor(
        SnowflakeQueryableMethodTranslatingExpressionVisitor parentVisitor)
        : base(parentVisitor)
    {
        _queryCompilationContext = parentVisitor._queryCompilationContext;
        _typeMappingSource = parentVisitor._typeMappingSource;
        _sqlExpressionFactory = parentVisitor._sqlExpressionFactory;

        _SnowflakeCompatibilityLevel = parentVisitor._SnowflakeCompatibilityLevel;
    }


    /// <inheritdoc />
    protected override QueryableMethodTranslatingExpressionVisitor CreateSubqueryVisitor()
        => new SnowflakeQueryableMethodTranslatingExpressionVisitor(this);


    /// <inheritdoc />
    protected override Expression VisitExtension(Expression extensionExpression)
    {
        if (extensionExpression is TemporalQueryRootExpression queryRootExpression)
        {
            var selectExpression = RelationalDependencies.SqlExpressionFactory.Select(queryRootExpression.EntityType);

            return new ShapedQueryExpression(
                selectExpression,
                new RelationalStructuralTypeShaperExpression(
                    queryRootExpression.EntityType,
                    new ProjectionBindingExpression(
                        selectExpression,
                        new ProjectionMember(),
                        typeof(ValueBuffer)),
                    false));
        }

        return base.VisitExtension(extensionExpression);
    }

    #region Aggregate functions

    // We override these for Snowflake to add tracking whether we're inside an aggregate function context, since Snowflake doesn't
    // support subqueries (or aggregates) within them.

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateAverage(ShapedQueryExpression source, LambdaExpression? selector,
        Type resultType)
    {
        var previousInAggregateFunction = _queryCompilationContext.InAggregateFunction;
        _queryCompilationContext.InAggregateFunction = true;
        var result = base.TranslateAverage(source, selector, resultType);
        _queryCompilationContext.InAggregateFunction = previousInAggregateFunction;
        return result;
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateSum(ShapedQueryExpression source, LambdaExpression? selector,
        Type resultType)
    {
        var previousInAggregateFunction = _queryCompilationContext.InAggregateFunction;
        _queryCompilationContext.InAggregateFunction = true;
        var result = base.TranslateSum(source, selector, resultType);
        _queryCompilationContext.InAggregateFunction = previousInAggregateFunction;
        return result;
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateCount(ShapedQueryExpression source, LambdaExpression? predicate)
    {
        var previousInAggregateFunction = _queryCompilationContext.InAggregateFunction;
        _queryCompilationContext.InAggregateFunction = true;
        var result = base.TranslateCount(source, predicate);
        _queryCompilationContext.InAggregateFunction = previousInAggregateFunction;
        return result;
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateLongCount(ShapedQueryExpression source,
        LambdaExpression? predicate)
    {
        var previousInAggregateFunction = _queryCompilationContext.InAggregateFunction;
        _queryCompilationContext.InAggregateFunction = true;
        var result = base.TranslateLongCount(source, predicate);
        _queryCompilationContext.InAggregateFunction = previousInAggregateFunction;
        return result;
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateMax(ShapedQueryExpression source, LambdaExpression? selector,
        Type resultType)
    {
        var previousInAggregateFunction = _queryCompilationContext.InAggregateFunction;
        _queryCompilationContext.InAggregateFunction = true;
        var result = base.TranslateMax(source, selector, resultType);
        _queryCompilationContext.InAggregateFunction = previousInAggregateFunction;
        return result;
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateMin(ShapedQueryExpression source, LambdaExpression? selector,
        Type resultType)
    {
        var previousInAggregateFunction = _queryCompilationContext.InAggregateFunction;
        _queryCompilationContext.InAggregateFunction = true;
        var result = base.TranslateMin(source, selector, resultType);
        _queryCompilationContext.InAggregateFunction = previousInAggregateFunction;
        return result;
    }

    #endregion Aggregate functions

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslatePrimitiveCollection(
        SqlExpression sqlExpression,
        IProperty? property,
        string tableAlias)
    {
        if (_SnowflakeCompatibilityLevel < 130)
        {
            AddTranslationErrorDetails(
                SnowflakeStrings.CompatibilityLevelTooLowForScalarCollections(_SnowflakeCompatibilityLevel));

            return null;
        }

        // Generate the OPENJSON function expression, and wrap it in a SelectExpression.

        // Note that where the elementTypeMapping is known (i.e. collection columns), we immediately generate OPENJSON with a WITH clause
        // (i.e. with a columnInfo), which determines the type conversion to apply to the JSON elements coming out.
        // For parameter collections, the element type mapping will only be inferred and applied later (see
        // SnowflakeInferredTypeMappingApplier below), at which point the we'll apply it to add the WITH clause.
        var elementTypeMapping = (RelationalTypeMapping?)sqlExpression.TypeMapping?.ElementTypeMapping;

        var openJsonExpression = elementTypeMapping is null
            ? new SnowflakeOpenJsonExpression(tableAlias, sqlExpression)
            : new SnowflakeOpenJsonExpression(
                tableAlias, sqlExpression,
                columnInfos:
                [
                    new SnowflakeOpenJsonExpression.ColumnInfo
                    {
                        Name = "value",
                        TypeMapping = elementTypeMapping,
                        Path = Array.Empty<PathSegment>()
                    }
                ]);

        var elementClrType = sqlExpression.Type.GetSequenceType();

        // If this is a collection property, get the element's nullability out of metadata. Otherwise, this is a parameter property, in
        // which case we only have the CLR type (note that we cannot produce different SQLs based on the nullability of an *element* in
        // a parameter collection - our caching mechanism only supports varying by the nullability of the parameter itself (i.e. the
        // collection).
        var isElementNullable = property?.GetElementType()!.IsNullable;

        var selectExpression = new SelectExpression(
            openJsonExpression,
            columnName: "value",
            columnType: elementClrType,
            columnTypeMapping: elementTypeMapping,
            isElementNullable,
            identifierColumnName: "key",
            identifierColumnType: typeof(string),
            identifierColumnTypeMapping: _typeMappingSource.FindMapping("nvarchar(4000)"));

        // OPENJSON doesn't guarantee the ordering of the elements coming out; when using OPENJSON without WITH, a [key] column is returned
        // with the JSON array's ordering, which we can ORDER BY; this option doesn't exist with OPENJSON with WITH, unfortunately.
        // However, OPENJSON with WITH has better performance, and also applies JSON-specific conversions we cannot be done otherwise
        // (e.g. OPENJSON with WITH does base64 decoding for VARBINARY).
        // Here we generate OPENJSON with WITH, but also add an ordering by [key] - this is a temporary invalid representation.
        // In SnowflakeQueryTranslationPostprocessor, we'll post-process the expression; if the ORDER BY was stripped (e.g. because of
        // IN, EXISTS or a set operation), we'll just leave the OPENJSON with WITH. If not, we'll convert the OPENJSON with WITH to an
        // OPENJSON without WITH.
        // Note that the OPENJSON 'key' column is an nvarchar - we convert it to an int before sorting.
        selectExpression.AppendOrdering(
            new OrderingExpression(
                _sqlExpressionFactory.Convert(
                    selectExpression.CreateColumnExpression(
                        openJsonExpression,
                        "key",
                        typeof(string),
                        typeMapping: _typeMappingSource.FindMapping("nvarchar(4000)"),
                        columnNullable: false),
                    typeof(int),
                    _typeMappingSource.FindMapping(typeof(int))),
                ascending: true));

        var shaperExpression = (Expression)new ProjectionBindingExpression(
            selectExpression, new ProjectionMember(), elementClrType.MakeNullable());
        if (shaperExpression.Type != elementClrType)
        {
            Check.DebugAssert(
                elementClrType.MakeNullable() == shaperExpression.Type,
                "expression.Type must be nullable of targetType");

            shaperExpression = Expression.Convert(shaperExpression, elementClrType);
        }

        return new ShapedQueryExpression(selectExpression, shaperExpression);
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression TransformJsonQueryToTable(JsonQueryExpression jsonQueryExpression)
    {
        // Calculate the table alias for the OPENJSON expression based on the last named path segment
        // (or the JSON column name if there are none)
        var lastNamedPathSegment = jsonQueryExpression.Path.LastOrDefault(ps => ps.PropertyName is not null);
        var tableAlias = char
            .ToLowerInvariant((lastNamedPathSegment.PropertyName ?? jsonQueryExpression.JsonColumn.Name)[0]).ToString();

        // We now add all of projected entity's the properties and navigations into the OPENJSON's WITH clause. Note that navigations
        // get AS JSON, which projects out the JSON sub-document for them as text, which can be further navigated into.
        var columnInfos = new List<SnowflakeOpenJsonExpression.ColumnInfo>();

        // We're only interested in properties which actually exist in the JSON, filter out uninteresting shadow keys
        foreach (var property in GetAllPropertiesInHierarchy(jsonQueryExpression.EntityType))
        {
            if (property.GetJsonPropertyName() is string jsonPropertyName)
            {
                columnInfos.Add(
                    new SnowflakeOpenJsonExpression.ColumnInfo
                    {
                        Name = jsonPropertyName,
                        TypeMapping = property.GetRelationalTypeMapping(),
                        Path = [new(jsonPropertyName)],
                        AsJson = property.GetRelationalTypeMapping().ElementTypeMapping is not null
                    });
            }
        }

        foreach (var navigation in GetAllNavigationsInHierarchy(jsonQueryExpression.EntityType)
                     .Where(
                         n => n.ForeignKey.IsOwnership
                              && n.TargetEntityType.IsMappedToJson()
                              && n.ForeignKey.PrincipalToDependent == n))
        {
            var jsonNavigationName = navigation.TargetEntityType.GetJsonPropertyName();
            Check.DebugAssert(jsonNavigationName is not null,
                $"No JSON property name for navigation {navigation.Name}");

            columnInfos.Add(
                new SnowflakeOpenJsonExpression.ColumnInfo
                {
                    Name = jsonNavigationName,
                    TypeMapping = _nvarcharMaxTypeMapping ??= _typeMappingSource.FindMapping("nvarchar(max)")!,
                    Path = [new(jsonNavigationName)],
                    AsJson = true
                });
        }

        var openJsonExpression = new SnowflakeOpenJsonExpression(
            tableAlias, jsonQueryExpression.JsonColumn, jsonQueryExpression.Path, columnInfos);

        var selectExpression = new SelectExpression(
            jsonQueryExpression,
            openJsonExpression,
            "key",
            typeof(string),
            _typeMappingSource.FindMapping("nvarchar(4000)")!);

        // See note on OPENJSON and ordering in TranslateCollection
        selectExpression.AppendOrdering(
            new OrderingExpression(
                _sqlExpressionFactory.Convert(
                    selectExpression.CreateColumnExpression(
                        openJsonExpression,
                        "key",
                        typeof(string),
                        typeMapping: _typeMappingSource.FindMapping("nvarchar(4000)"),
                        columnNullable: false),
                    typeof(int),
                    _typeMappingSource.FindMapping(typeof(int))),
                ascending: true));

        return new ShapedQueryExpression(
            selectExpression,
            new RelationalStructuralTypeShaperExpression(
                jsonQueryExpression.EntityType,
                new ProjectionBindingExpression(
                    selectExpression,
                    new ProjectionMember(),
                    typeof(ValueBuffer)),
                false));

        static IEnumerable<IProperty> GetAllPropertiesInHierarchy(IEntityType entityType)
            => entityType.GetAllBaseTypes().Concat(entityType.GetDerivedTypesInclusive())
                .SelectMany(t => t.GetDeclaredProperties());

        static IEnumerable<INavigation> GetAllNavigationsInHierarchy(IEntityType entityType)
            => entityType.GetAllBaseTypes().Concat(entityType.GetDerivedTypesInclusive())
                .SelectMany(t => t.GetDeclaredNavigations());
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateContains(ShapedQueryExpression source, Expression item)
    {
        var translatedSource = base.TranslateContains(source, item);

        // Snowflake does not support subqueries inside aggregate functions (e.g. COUNT(SELECT * FROM OPENJSON(@p)...)).
        // As a result, we track whether we're within an aggregate function; if we are, and we see the regular Contains translation
        // (which uses IN with an OPENJSON subquery - incompatible), we transform it to the old-style IN+constants translation (as if a
        // low Snowflake compatibility level were defined)
        if (!UseOldBehavior32374
            && _queryCompilationContext.InAggregateFunction
            && translatedSource is not null
            && TryGetProjection(translatedSource, out var projection)
            && projection is InExpression
            {
                Item: var translatedItem,
                Subquery:
                {
                    Tables:
                    [
                        SnowflakeOpenJsonExpression { Arguments: [SqlParameterExpression parameter] } openJsonExpression
                    ],
                    GroupBy: [],
                    Having: null,
                    IsDistinct: false,
                    Limit: null,
                    Offset: null,
                    Orderings: [],
                    Projection: [{ Expression: ColumnExpression { Name: "value", Table: var projectionColumnTable } }]
                } subquery
            }
            && (UseOldBehavior33932 || subquery.Predicate is null)
            && projectionColumnTable == openJsonExpression)
        {
            var newInExpression = _sqlExpressionFactory.In(translatedItem, parameter);
            return source.UpdateQueryExpression(_sqlExpressionFactory.Select(newInExpression));
        }

        return translatedSource;
    }

    /// <inheritdoc />
    protected override ShapedQueryExpression? TranslateElementAtOrDefault(
        ShapedQueryExpression source,
        Expression index,
        bool returnDefault)
    {
        if (!returnDefault
            && source.QueryExpression is SelectExpression
            {
                Tables: [SnowflakeOpenJsonExpression { Arguments: [var jsonArrayColumn] } openJsonExpression],
                GroupBy: [],
                Having: null,
                IsDistinct: false,
                Limit: null,
                Offset: null,
                // We can only apply the indexing if the JSON array is ordered by its natural ordered, i.e. by the "key" column that
                // we created in TranslateCollection. For example, if another ordering has been applied (e.g. by the JSON elements
                // themselves), we can no longer simply index into the original array.
                Orderings:
                [
                    {
                        Expression: SqlUnaryExpression
                        {
                            OperatorType: ExpressionType.Convert,
                            Operand: ColumnExpression { Name: "key", Table: var orderingTable }
                        }
                    }
                ]
            } selectExpression
            && TranslateExpression(index) is { } translatedIndex
            && (UseOldBehavior33932 || selectExpression.Predicate is null)
            && orderingTable == openJsonExpression)
        {
            // Index on JSON array

            // Extract the column projected out of the source, and simplify the subquery to a simple JsonScalarExpression
            var shaperExpression = source.ShaperExpression;
            if (shaperExpression is UnaryExpression { NodeType: ExpressionType.Convert } unaryExpression
                && unaryExpression.Operand.Type.IsNullableType()
                && unaryExpression.Operand.Type.UnwrapNullableType() == unaryExpression.Type)
            {
                shaperExpression = unaryExpression.Operand;
            }

            if (shaperExpression is ProjectionBindingExpression projectionBindingExpression
                && selectExpression.GetProjection(projectionBindingExpression) is SqlExpression projection)
            {
                // OPENJSON's value column is an nvarchar(max); if this is a collection column whose type mapping is know, the projection
                // contains a CAST node which we unwrap
                var projectionColumn = projection switch
                {
                    ColumnExpression c => c,
                    SqlUnaryExpression { OperatorType: ExpressionType.Convert, Operand: ColumnExpression c } => c,
                    _ => null
                };

                if (projectionColumn is not null)
                {
                    // If the inner expression happens to itself be a JsonScalarExpression, simply append the two paths to avoid creating
                    // JSON_VALUE within JSON_VALUE.
                    var (json, path) = jsonArrayColumn is JsonScalarExpression innerJsonScalarExpression
                        ? (innerJsonScalarExpression.Json,
                            innerJsonScalarExpression.Path.Append(new PathSegment(translatedIndex)).ToArray())
                        : (jsonArrayColumn, new PathSegment[] { new(translatedIndex) });

                    var translation = new JsonScalarExpression(
                        json,
                        path,
                        projection.Type,
                        projection.TypeMapping,
                        projectionColumn.IsNullable);

                    return source.UpdateQueryExpression(_sqlExpressionFactory.Select(translation));
                }
            }
        }

        return base.TranslateElementAtOrDefault(source, index, returnDefault);
    }

    /// <inheritdoc />
    protected override bool IsNaturallyOrdered(SelectExpression selectExpression)
        => selectExpression is
           {
               Tables: [SnowflakeOpenJsonExpression openJsonExpression, ..],
               Orderings:
               [
                   {
                       Expression: SqlUnaryExpression
                       {
                           OperatorType: ExpressionType.Convert,
                           Operand: ColumnExpression { Name: "key", Table: var orderingTable }
                       },
                       IsAscending: true
                   }
               ]
           }
           && orderingTable == openJsonExpression;

    /// <inheritdoc />
    protected override bool IsValidSelectExpressionForExecuteDelete(
        SelectExpression selectExpression,
        StructuralTypeShaperExpression shaper,
        [NotNullWhen(true)] out TableExpression? tableExpression)
    {
        if (selectExpression.Offset == null
            && selectExpression.GroupBy.Count == 0
            && selectExpression.Having == null
            && selectExpression.Orderings.Count == 0)
        {
            TableExpressionBase table;
            if (selectExpression.Tables.Count == 1)
            {
                table = selectExpression.Tables[0];
            }
            else
            {
                var projectionBindingExpression = (ProjectionBindingExpression)shaper.ValueBufferExpression;
                var projection =
                    (StructuralTypeProjectionExpression)selectExpression.GetProjection(projectionBindingExpression);
                var column = projection.BindProperty(shaper.StructuralType.GetProperties().First());
                table = column.Table;
                if (table is JoinExpressionBase joinExpressionBase)
                {
                    table = joinExpressionBase.Table;
                }
            }

            if (table is TableExpression te)
            {
                tableExpression = te;
                return true;
            }
        }

        tableExpression = null;
        return false;
    }

    /// <inheritdoc />
    protected override bool IsValidSelectExpressionForExecuteUpdate(
        SelectExpression selectExpression,
        TableExpressionBase table,
        [NotNullWhen(true)] out TableExpression? tableExpression)
    {
        if (selectExpression is
            {
                Offset: null,
                IsDistinct: false,
                GroupBy: [],
                Having: null,
                Orderings: []
            })
        {
            if (selectExpression.Tables.Count > 1 && table is JoinExpressionBase joinExpressionBase)
            {
                table = joinExpressionBase.Table;
            }

            if (table is TableExpression te)
            {
                tableExpression = te;
                return true;
            }
        }

        tableExpression = null;
        return false;
    }

    /// 
    private bool TryGetProjection(ShapedQueryExpression shapedQueryExpression,
        [NotNullWhen(true)] out SqlExpression? projection)
    {
        var shaperExpression = shapedQueryExpression.ShaperExpression;
        // No need to check ConvertChecked since this is convert node which we may have added during projection
        if (shaperExpression is UnaryExpression { NodeType: ExpressionType.Convert } unaryExpression
            && unaryExpression.Operand.Type.IsNullableType()
            && unaryExpression.Operand.Type.UnwrapNullableType() == unaryExpression.Type)
        {
            shaperExpression = unaryExpression.Operand;
        }

        if (shapedQueryExpression.QueryExpression is SelectExpression selectExpression
            && shaperExpression is ProjectionBindingExpression projectionBindingExpression
            && selectExpression.GetProjection(projectionBindingExpression) is SqlExpression sqlExpression)
        {
            projection = sqlExpression;
            return true;
        }

        projection = null;
        return false;
    }

    /// <inheritdoc />
    private sealed class TemporalAnnotationApplyingExpressionVisitor : ExpressionVisitor
    {
        private readonly Func<TableExpression, TableExpressionBase> _annotationApplyingFunc;

        /// <summary>
        ///    Creates a new instance of the <see cref="TemporalAnnotationApplyingExpressionVisitor" /> class.
        /// </summary>
        public TemporalAnnotationApplyingExpressionVisitor(
            Func<TableExpression, TableExpressionBase> annotationApplyingFunc)
        {
            _annotationApplyingFunc = annotationApplyingFunc;
        }

        /// <inheritdoc />
        [return: NotNullIfNotNull("expression")]
        public override Expression? Visit(Expression? expression)
            => expression is TableExpression tableExpression
                ? _annotationApplyingFunc(tableExpression)
                : base.Visit(expression);
    }


    /// <inheritdoc />
    protected override Expression ApplyInferredTypeMappings(
        Expression expression,
        IReadOnlyDictionary<(TableExpressionBase, string), RelationalTypeMapping?> inferredTypeMappings)
        => new SnowflakeInferredTypeMappingApplier(
                RelationalDependencies.Model, _typeMappingSource, _sqlExpressionFactory, inferredTypeMappings)
            .Visit(expression);


    /// <inheritdoc />
    protected class SnowflakeInferredTypeMappingApplier : RelationalInferredTypeMappingApplier
    {
        private readonly IRelationalTypeMappingSource _typeMappingSource;

        /// <summary>
        ///   Creates a new instance of the <see cref="SnowflakeInferredTypeMappingApplier" /> class.
        /// </summary>
        /// <param name="model"></param>
        /// <param name="typeMappingSource"></param>
        /// <param name="sqlExpressionFactory"></param>
        /// <param name="inferredTypeMappings"></param>
        public SnowflakeInferredTypeMappingApplier(
            IModel model,
            IRelationalTypeMappingSource typeMappingSource,
            ISqlExpressionFactory sqlExpressionFactory,
            IReadOnlyDictionary<(TableExpressionBase, string), RelationalTypeMapping?> inferredTypeMappings)
            : base(model, sqlExpressionFactory, inferredTypeMappings)
        {
            _typeMappingSource = typeMappingSource;
        }

        /// <inheritdoc />
        protected override Expression VisitExtension(Expression expression)
            => expression switch
            {
                SnowflakeOpenJsonExpression openJsonExpression
                    when TryGetInferredTypeMapping(openJsonExpression, "value", out var typeMapping)
                    => ApplyTypeMappingsOnOpenJsonExpression(openJsonExpression, [typeMapping]),

                _ => base.VisitExtension(expression)
            };

        /// <summary>
        ///  Applies the inferred type mapping to the given <see cref="SnowflakeOpenJsonExpression" />.
        /// </summary>
        /// <param name="openJsonExpression"></param>
        /// <param name="typeMappings"></param>
        /// <returns></returns>
        /// <exception cref="UnreachableException"></exception>
        protected virtual SnowflakeOpenJsonExpression ApplyTypeMappingsOnOpenJsonExpression(
            SnowflakeOpenJsonExpression openJsonExpression,
            IReadOnlyList<RelationalTypeMapping> typeMappings)
        {
            Check.DebugAssert(typeMappings.Count == 1, "typeMappings.Count == 1");
            var elementTypeMapping = typeMappings[0];

            // Constant queryables are translated to VALUES, no need for JSON.
            // Column queryables have their type mapping from the model, so we don't ever need to apply an inferred mapping on them.
            if (openJsonExpression.JsonExpression is not SqlParameterExpression
                {
                    TypeMapping: null
                } parameterExpression)
            {
                Check.DebugAssert(
                    openJsonExpression.JsonExpression.TypeMapping is not null,
                    "Non-parameter expression without a type mapping in ApplyTypeMappingsOnOpenJsonExpression");
                return openJsonExpression;
            }

            Check.DebugAssert(
                openJsonExpression.Path is null,
                "OpenJsonExpression path is non-null when applying an inferred type mapping");
            Check.DebugAssert(
                openJsonExpression.ColumnInfos is null,
                "OpenJsonExpression has no ColumnInfos when applying an inferred type mapping");

            // We need to apply the inferred type mapping in two places: the collection type mapping on the parameter expanded by OPENJSON,
            // and on the WITH clause determining the conversion out on the Snowflake side

            // First, find the collection type mapping and apply it to the parameter
            if (_typeMappingSource.FindMapping(parameterExpression.Type, Model, elementTypeMapping) is not
                SnowflakeStringTypeMapping
                    {
                        ElementTypeMapping: not null
                    }
                    parameterTypeMapping)
            {
                throw new UnreachableException(
                    "A SnowflakeStringTypeMapping collection type mapping could not be found");
            }

            return openJsonExpression.Update(
                parameterExpression.ApplyTypeMapping(parameterTypeMapping),
                path: null,
                [
                    new SnowflakeOpenJsonExpression.ColumnInfo("value", elementTypeMapping, Array.Empty<PathSegment>())
                ]);
        }
    }
}