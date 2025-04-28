using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;

namespace Snowflake.EntityFrameworkCore.Query.Internal;

/// <summary>
///    Represents a temporal query root expression with a from-to range.
/// </summary>
public class TemporalFromToQueryRootExpression : TemporalRangeQueryRootExpression
{
    /// <summary>
    /// Creates a new instance of <see cref="TemporalFromToQueryRootExpression" />
    /// </summary>
    /// <param name="entityType"></param>
    /// <param name="from"></param>
    /// <param name="to"></param>
    public TemporalFromToQueryRootExpression(
        IEntityType entityType,
        DateTime from,
        DateTime to)
        : base(entityType, from, to)
    {
    }

    /// <summary>
    /// Creates a new instance of <see cref="TemporalFromToQueryRootExpression" />
    /// </summary>
    /// <param name="queryProvider"></param>
    /// <param name="entityType"></param>
    /// <param name="from"></param>
    /// <param name="to"></param>
    public TemporalFromToQueryRootExpression(
        IAsyncQueryProvider queryProvider,
        IEntityType entityType,
        DateTime from,
        DateTime to)
        : base(queryProvider, entityType, from, to)
    {
    }

    /// <inheritdoc />
    public override Expression DetachQueryProvider()
        => new TemporalFromToQueryRootExpression(EntityType, From, To);

    /// <inheritdoc />
    public override EntityQueryRootExpression UpdateEntityType(IEntityType entityType)
        => entityType.ClrType != EntityType.ClrType
           || entityType.Name != EntityType.Name
            ? throw new InvalidOperationException(CoreStrings.QueryRootDifferentEntityType(entityType.DisplayName()))
            : new TemporalFromToQueryRootExpression(entityType, From, To);

    /// <inheritdoc />
    protected override void Print(ExpressionPrinter expressionPrinter)
    {
        base.Print(expressionPrinter);
        expressionPrinter.Append($".TemporalFromTo({From}, {To})");
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj != null
           && (ReferenceEquals(this, obj)
               || obj is TemporalFromToQueryRootExpression queryRootExpression
               && Equals(queryRootExpression));

    private bool Equals(TemporalFromToQueryRootExpression queryRootExpression)
        => base.Equals(queryRootExpression);

    /// <inheritdoc />
    public override int GetHashCode()
        => base.GetHashCode();
}