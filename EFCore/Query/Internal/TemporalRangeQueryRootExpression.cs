using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;

namespace Snowflake.EntityFrameworkCore.Query.Internal;

/// <summary>
///    Represents a temporal range query root expression.
/// </summary>
public abstract class TemporalRangeQueryRootExpression : TemporalQueryRootExpression
{
    /// <summary>
    ///    Creates a new instance of the <see cref="TemporalRangeQueryRootExpression" /> class.
    /// </summary>
    /// <param name="entityType"></param>
    /// <param name="from"></param>
    /// <param name="to"></param>
    protected TemporalRangeQueryRootExpression(
        IEntityType entityType,
        DateTime from,
        DateTime to)
        : base(entityType)
    {
        From = from;
        To = to;
    }

    /// <summary>
    ///   Creates a new instance of the <see cref="TemporalRangeQueryRootExpression" /> class.
    /// </summary>
    /// <param name="queryProvider"></param>
    /// <param name="entityType"></param>
    /// <param name="from"></param>
    /// <param name="to"></param>
    protected TemporalRangeQueryRootExpression(
        IAsyncQueryProvider queryProvider,
        IEntityType entityType,
        DateTime from,
        DateTime to)
        : base(queryProvider, entityType)
    {
        From = from;
        To = to;
    }

    /// <summary>
    ///  Gets the start of the temporal range.
    /// </summary>
    public virtual DateTime From { get; }

    /// <summary>
    /// Gets the end of the temporal range.
    /// </summary>
    public virtual DateTime To { get; }

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj != null
           && (ReferenceEquals(this, obj)
               || obj is TemporalRangeQueryRootExpression queryRootExpression
               && Equals(queryRootExpression));

    private bool Equals(TemporalRangeQueryRootExpression queryRootExpression)
        => base.Equals(queryRootExpression)
           && Equals(From, queryRootExpression.From)
           && Equals(To, queryRootExpression.To);

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        hashCode.Add(base.GetHashCode());
        hashCode.Add(From);
        hashCode.Add(To);

        return hashCode.ToHashCode();
    }
}