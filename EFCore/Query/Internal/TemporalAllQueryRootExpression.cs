using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;

namespace Snowflake.EntityFrameworkCore.Query.Internal;

/// <summary>
/// Represents a query root expression for temporal queries.
/// </summary>
public class TemporalAllQueryRootExpression : TemporalQueryRootExpression
{
    /// <summary>
    /// Creates a new instance of <see cref="TemporalAllQueryRootExpression" />
    /// </summary>
    /// <param name="entityType"></param>
    public TemporalAllQueryRootExpression(IEntityType entityType)
        : base(entityType)
    {
    }

    /// <summary>
    /// Creates a new instance of <see cref="TemporalAllQueryRootExpression" />
    /// </summary>
    /// <param name="queryProvider"></param>
    /// <param name="entityType"></param>
    public TemporalAllQueryRootExpression(IAsyncQueryProvider queryProvider, IEntityType entityType)
        : base(queryProvider, entityType)
    {
    }

    /// <inheritdoc />
    public override Expression DetachQueryProvider()
        => new TemporalAllQueryRootExpression(EntityType);

    /// <inheritdoc />
    public override EntityQueryRootExpression UpdateEntityType(IEntityType entityType)
        => entityType.ClrType != EntityType.ClrType
           || entityType.Name != EntityType.Name
            ? throw new InvalidOperationException(CoreStrings.QueryRootDifferentEntityType(entityType.DisplayName()))
            : new TemporalAllQueryRootExpression(entityType);

    /// <inheritdoc />
    protected override void Print(ExpressionPrinter expressionPrinter)
    {
        base.Print(expressionPrinter);
        expressionPrinter.Append(".TemporalAll()");
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj != null
           && (ReferenceEquals(this, obj)
               || obj is TemporalAllQueryRootExpression queryRootExpression
               && Equals(queryRootExpression));

    private bool Equals(TemporalAllQueryRootExpression queryRootExpression)
        => base.Equals(queryRootExpression);

    /// <inheritdoc />
    public override int GetHashCode()
        => base.GetHashCode();
}