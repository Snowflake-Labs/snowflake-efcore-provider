using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;

namespace Snowflake.EntityFrameworkCore.Query.Internal;

/// <summary>
/// Represents a query root expression for temporal tables.
/// </summary>
public abstract class TemporalQueryRootExpression : EntityQueryRootExpression
{
    /// <summary>
    /// Creates a new instance of the <see cref="TemporalQueryRootExpression"/> class.
    /// </summary>
    /// <param name="entityType"></param>
    protected TemporalQueryRootExpression(IEntityType entityType)
        : base(entityType)
    {
    }

    /// <summary>
    /// Creates a new instance of the <see cref="TemporalQueryRootExpression"/> class.
    /// </summary>
    /// <param name="asyncQueryProvider"></param>
    /// <param name="entityType"></param>
    protected TemporalQueryRootExpression(IAsyncQueryProvider asyncQueryProvider, IEntityType entityType)
        : base(asyncQueryProvider, entityType)
    {
    }

    /// <inheritdoc />
    protected override Expression VisitChildren(ExpressionVisitor visitor)
        => this;
}