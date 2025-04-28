using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;

namespace Snowflake.EntityFrameworkCore.Query.Internal;

/// <summary>
///    An expression that represents a query root with a temporal range filter.
/// </summary>
public class TemporalBetweenQueryRootExpression : TemporalRangeQueryRootExpression
{
    /// <summary>
    ///   Creates a new instance of the <see cref="TemporalBetweenQueryRootExpression" /> class.
    /// </summary>
    /// <param name="entityType"></param>
    /// <param name="from"></param>
    /// <param name="to"></param>
    public TemporalBetweenQueryRootExpression(
        IEntityType entityType,
        DateTime from,
        DateTime to)
        : base(entityType, from, to)
    {
    }

    /// <summary>
    ///   Creates a new instance of the <see cref="TemporalBetweenQueryRootExpression" /> class.
    /// </summary>
    /// <param name="queryProvider"></param>
    /// <param name="entityType"></param>
    /// <param name="from"></param>
    /// <param name="to"></param>
    public TemporalBetweenQueryRootExpression(
        IAsyncQueryProvider queryProvider,
        IEntityType entityType,
        DateTime from,
        DateTime to)
        : base(queryProvider, entityType, from, to)
    {
    }

    /// <inheritdoc />
    public override Expression DetachQueryProvider()
        => new TemporalBetweenQueryRootExpression(EntityType, From, To);

    /// <inheritdoc />
    public override EntityQueryRootExpression UpdateEntityType(IEntityType entityType)
        => entityType.ClrType != EntityType.ClrType
            || entityType.Name != EntityType.Name
                ? throw new InvalidOperationException(CoreStrings.QueryRootDifferentEntityType(entityType.DisplayName()))
                : new TemporalBetweenQueryRootExpression(entityType, From, To);

    /// <inheritdoc />
    protected override void Print(ExpressionPrinter expressionPrinter)
    {
        base.Print(expressionPrinter);
        expressionPrinter.Append($".TemporalBetween({From}, {To})");
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj != null
            && (ReferenceEquals(this, obj)
                || obj is TemporalBetweenQueryRootExpression queryRootExpression
                && Equals(queryRootExpression));

    private bool Equals(TemporalBetweenQueryRootExpression queryRootExpression)
        => base.Equals(queryRootExpression);

    /// <inheritdoc />
    public override int GetHashCode()
        => base.GetHashCode();
}
