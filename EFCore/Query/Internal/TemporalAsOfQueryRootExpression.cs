using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;

namespace Snowflake.EntityFrameworkCore.Query.Internal;

/// <summary>
///    An expression that represents a query root in a query expression tree.
/// </summary>
public class TemporalAsOfQueryRootExpression : TemporalQueryRootExpression
{
    /// <summary>
    ///    Creates a new instance of the <see cref="TemporalAsOfQueryRootExpression"/> class.
    /// </summary>
    /// <param name="entityType"></param>
    /// <param name="pointInTime"></param>
    public TemporalAsOfQueryRootExpression(IEntityType entityType, DateTime pointInTime)
        : base(entityType)
    {
        PointInTime = pointInTime;
    }

    /// <summary>
    ///  Creates a new instance of the <see cref="TemporalAsOfQueryRootExpression"/> class.
    /// </summary>
    /// <param name="queryProvider"></param>
    /// <param name="entityType"></param>
    /// <param name="pointInTime"></param>
    public TemporalAsOfQueryRootExpression(
        IAsyncQueryProvider queryProvider,
        IEntityType entityType,
        DateTime pointInTime)
        : base(queryProvider, entityType)
    {
        PointInTime = pointInTime;
    }

    /// <summary>
    ///   The point in time for the query.
    /// </summary>
    public virtual DateTime PointInTime { get; }

    /// <inheritdoc />
    public override Expression DetachQueryProvider()
        => new TemporalAsOfQueryRootExpression(EntityType, PointInTime);

    /// <inheritdoc />
    public override EntityQueryRootExpression UpdateEntityType(IEntityType entityType)
        => entityType.ClrType != EntityType.ClrType
            || entityType.Name != EntityType.Name
                ? throw new InvalidOperationException(CoreStrings.QueryRootDifferentEntityType(entityType.DisplayName()))
                : new TemporalAsOfQueryRootExpression(entityType, PointInTime);

    /// <inheritdoc />
    protected override void Print(ExpressionPrinter expressionPrinter)
    {
        base.Print(expressionPrinter);
        expressionPrinter.Append($".TemporalAsOf({PointInTime})");
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj != null
            && (ReferenceEquals(this, obj)
                || obj is TemporalAsOfQueryRootExpression queryRootExpression
                && Equals(queryRootExpression));

    private bool Equals(TemporalAsOfQueryRootExpression queryRootExpression)
        => base.Equals(queryRootExpression)
            && Equals(PointInTime, queryRootExpression.PointInTime);

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        hashCode.Add(base.GetHashCode());
        hashCode.Add(PointInTime);

        return hashCode.ToHashCode();
    }
}
