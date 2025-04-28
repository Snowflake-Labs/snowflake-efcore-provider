using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;

namespace Snowflake.EntityFrameworkCore.Query.Internal;

/// <summary>
///    Represents a temporal query root expression that represents a temporal range query.
/// </summary>
public class TemporalContainedInQueryRootExpression : TemporalRangeQueryRootExpression
{
    /// <summary>
    /// Creates a new instance of <see cref="TemporalContainedInQueryRootExpression" />
    ///</summary>
    public TemporalContainedInQueryRootExpression(
        IEntityType entityType,
        DateTime from,
        DateTime to)
        : base(entityType, from, to)
    {
    }

    /// <summary>
    /// Creates a new instance of <see cref="TemporalContainedInQueryRootExpression" />
    ///</summary>
    public TemporalContainedInQueryRootExpression(
        IAsyncQueryProvider queryProvider,
        IEntityType entityType,
        DateTime from,
        DateTime to)
        : base(queryProvider, entityType, from, to)
    {
    }

    /// <inheritdoc />
    public override Expression DetachQueryProvider()
        => new TemporalContainedInQueryRootExpression(EntityType, From, To);

    /// <inheritdoc />
    public override EntityQueryRootExpression UpdateEntityType(IEntityType entityType)
        => entityType.ClrType != EntityType.ClrType
           || entityType.Name != EntityType.Name
            ? throw new InvalidOperationException(CoreStrings.QueryRootDifferentEntityType(entityType.DisplayName()))
            : new TemporalContainedInQueryRootExpression(entityType, From, To);

    /// <inheritdoc />
    protected override void Print(ExpressionPrinter expressionPrinter)
    {
        base.Print(expressionPrinter);
        expressionPrinter.Append($".TemporalContainedIn({From}, {To})");
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj != null
           && (ReferenceEquals(this, obj)
               || obj is TemporalContainedInQueryRootExpression queryRootExpression
               && Equals(queryRootExpression));

    private bool Equals(TemporalContainedInQueryRootExpression queryRootExpression)
        => base.Equals(queryRootExpression);

    /// <inheritdoc />
    public override int GetHashCode()
        => base.GetHashCode();
}