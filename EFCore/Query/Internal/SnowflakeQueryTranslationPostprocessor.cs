using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Snowflake.EntityFrameworkCore.Internal;

namespace Snowflake.EntityFrameworkCore.Query.Internal;

/// <summary>
/// Snowflake specific query translation postprocessor.
/// </summary>
public class SnowflakeQueryTranslationPostprocessor : RelationalQueryTranslationPostprocessor
{
    private readonly SnowflakeJsonPostprocessor _jsonPostprocessor;
    private readonly SkipWithoutOrderByInSplitQueryVerifier _skipWithoutOrderByInSplitQueryVerifier = new();
    private readonly ApplyValidatingVisitor _applyValidator = new();

    /// <summary>
    /// Creates a new instance of <see cref="SnowflakeQueryTranslationPostprocessor" />
    /// </summary>
    /// <param name="dependencies"></param>
    /// <param name="relationalDependencies"></param>
    /// <param name="queryCompilationContext"></param>
    /// <param name="typeMappingSource"></param>
    public SnowflakeQueryTranslationPostprocessor(
        QueryTranslationPostprocessorDependencies dependencies,
        RelationalQueryTranslationPostprocessorDependencies relationalDependencies,
        QueryCompilationContext queryCompilationContext,
        IRelationalTypeMappingSource typeMappingSource)
        : base(dependencies, relationalDependencies, queryCompilationContext)
    {
        _jsonPostprocessor = new SnowflakeJsonPostprocessor(typeMappingSource, relationalDependencies.SqlExpressionFactory);
    }

    /// <inheritdoc />
    public override Expression Process(Expression query)
    {
        query = base.Process(query);
        _applyValidator.Visit(query);
        query = _jsonPostprocessor.Process(query);
        _skipWithoutOrderByInSplitQueryVerifier.Visit(query);

        return query;
    }

    /// <summary>
    /// Visits the expression tree to ensure that the query does not contain a Skip without an OrderBy in a split query.
    /// </summary>
    private sealed class SkipWithoutOrderByInSplitQueryVerifier : ExpressionVisitor
    {
        /// <inheritdoc />
        [return: NotNullIfNotNull("expression")]
        public override Expression? Visit(Expression? expression)
        {
            switch (expression)
            {
                case ShapedQueryExpression shapedQueryExpression:
                    Visit(shapedQueryExpression.ShaperExpression);
                    return shapedQueryExpression;

                case RelationalSplitCollectionShaperExpression relationalSplitCollectionShaperExpression:
                    foreach (var table in relationalSplitCollectionShaperExpression.SelectExpression.Tables)
                    {
                        Visit(table);
                    }

                    Visit(relationalSplitCollectionShaperExpression.InnerShaper);

                    return relationalSplitCollectionShaperExpression;

                case SelectExpression { Offset: not null, Orderings.Count: 0 }:
                    throw new InvalidOperationException(SnowflakeStrings.SplitQueryOffsetWithoutOrderBy);

                case NonQueryExpression nonQueryExpression:
                    return nonQueryExpression;

                default:
                    return base.Visit(expression);
            }
        }
    }

    /// <summary>
    /// Visits the expression tree to ensure that the query does not contain an Apply expression.
    /// </summary>
    private sealed class ApplyValidatingVisitor : ExpressionVisitor
    {
        /// <inheritdoc />
        protected override Expression VisitExtension(Expression extensionExpression)
        {
            if (extensionExpression is ShapedQueryExpression shapedQueryExpression)
            {
                Visit(shapedQueryExpression.QueryExpression);
                Visit(shapedQueryExpression.ShaperExpression);

                return extensionExpression;
            }

            if (extensionExpression is SelectExpression selectExpression
                && selectExpression.Tables.Any(t => t is CrossApplyExpression or OuterApplyExpression))
            {
                throw new InvalidOperationException(SnowflakeStrings.ApplyNotSupported);
            }

            return base.VisitExtension(extensionExpression);
        }
    }
}