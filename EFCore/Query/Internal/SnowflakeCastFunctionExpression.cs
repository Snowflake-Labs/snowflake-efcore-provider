using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;

namespace Snowflake.EntityFrameworkCore.Query.Internal;

/// <inheritdoc />
public class SnowflakeCastFunctionExpression : SqlExpression
{
    /// <inheritdoc />
    public SnowflakeCastFunctionExpression(
        SqlExpression source,
        SqlFragmentExpression targetType,
        Type type,
        RelationalTypeMapping typeMapping)
        : base(type, typeMapping)
    {
        Source = source;
        Target = targetType;
    }

    /// <summary>
    /// The source expression
    /// </summary>
    public SqlExpression Source { get; }

    /// <summary>
    /// The target expression
    /// </summary>
    public SqlFragmentExpression Target { get; }

    /// <summary>
    /// The cast part of the expression
    /// </summary>
    public readonly SqlFragmentExpression CastPart = new("CAST(");

    /// <summary>
    /// The as part of the expression
    /// </summary>
    public readonly SqlFragmentExpression AsPart = new(" AS ");

    /// <summary>
    /// The close part of the expression
    /// </summary>
    public readonly SqlFragmentExpression ClosePart = new(")");


    /// <summary>
    /// Update the source and target expressions
    /// </summary>
    /// <param name="source"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    public virtual SnowflakeCastFunctionExpression Update(SqlExpression source, SqlFragmentExpression target)
    {
        if (Equals(source, Source) && Equals(target, Target))
        {
            return this;
        }

        return new SnowflakeCastFunctionExpression(source, target, Type, TypeMapping);
    }

    /// <inheritdoc />
    protected override Expression VisitChildren(ExpressionVisitor visitor)
    {
        var visitedSource = (SqlExpression)visitor.Visit(Source);
        var visitedTarget = (SqlFragmentExpression)visitor.Visit(Target);

        return Update(visitedSource, visitedTarget);
    }

    /// <inheritdoc />
    protected override void Print(ExpressionPrinter expressionPrinter)
    {
        expressionPrinter.Append(CastPart.Sql);
        expressionPrinter.Visit(Source);
        expressionPrinter.Append(AsPart.Sql);
        expressionPrinter.Visit(Target);
        expressionPrinter.Append(ClosePart.Sql);
    }
}