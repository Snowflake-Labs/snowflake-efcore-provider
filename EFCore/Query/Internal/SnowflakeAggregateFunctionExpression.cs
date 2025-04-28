using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Snowflake.EntityFrameworkCore.Query.Internal;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;

public class SnowflakeAggregateFunctionExpression : SqlExpression
{
    
    public SnowflakeAggregateFunctionExpression(
        string name,
        IReadOnlyList<SqlExpression> arguments,
        IReadOnlyList<OrderingExpression> orderings,
        bool nullable,
        IEnumerable<bool> argumentsPropagateNullability,
        Type type,
        RelationalTypeMapping? typeMapping)
        : base(type, typeMapping)
    {
        Name = name;
        Arguments = arguments.ToList();
        Orderings = orderings;
        IsNullable = nullable;
        ArgumentsPropagateNullability = argumentsPropagateNullability.ToList();
    }

    
    public virtual string Name { get; }

    
    public virtual IReadOnlyList<SqlExpression> Arguments { get; }

    
    public virtual IReadOnlyList<OrderingExpression> Orderings { get; }

    
    public virtual bool IsNullable { get; }

    
    public virtual IReadOnlyList<bool> ArgumentsPropagateNullability { get; }

    /// <inheritdoc />
    protected override  Expression VisitChildren(ExpressionVisitor visitor)
    {
        SqlExpression[]? arguments = null;
        for (var i = 0; i < Arguments.Count; i++)
        {
            var visitedArgument = (SqlExpression)visitor.Visit(Arguments[i]);
            if (visitedArgument != Arguments[i] && arguments is null)
            {
                arguments = new SqlExpression[Arguments.Count];

                for (var j = 0; j < i; j++)
                {
                    arguments[j] = Arguments[j];
                }
            }

            if (arguments is not null)
            {
                arguments[i] = visitedArgument;
            }
        }

        OrderingExpression[]? orderings = null;
        for (var i = 0; i < Orderings.Count; i++)
        {
            var visitedOrdering = (OrderingExpression)visitor.Visit(Orderings[i]);
            if (visitedOrdering != Orderings[i] && orderings is null)
            {
                orderings = new OrderingExpression[Orderings.Count];

                for (var j = 0; j < i; j++)
                {
                    orderings[j] = Orderings[j];
                }
            }

            if (orderings is not null)
            {
                orderings[i] = visitedOrdering;
            }
        }

        return arguments is not null || orderings is not null
            ? new SnowflakeAggregateFunctionExpression(
                Name,
                arguments ?? Arguments,
                orderings ?? Orderings,
                IsNullable,
                ArgumentsPropagateNullability,
                Type,
                TypeMapping)
            : this;
    }

    
    public virtual SnowflakeAggregateFunctionExpression ApplyTypeMapping(RelationalTypeMapping? typeMapping)
        => new(
            Name,
            Arguments,
            Orderings,
            IsNullable,
            ArgumentsPropagateNullability,
            Type,
            typeMapping ?? TypeMapping);

    
    public virtual SnowflakeAggregateFunctionExpression Update(
        IReadOnlyList<SqlExpression> arguments,
        IReadOnlyList<OrderingExpression> orderings)
        => (ReferenceEquals(arguments, Arguments) || arguments.SequenceEqual(Arguments))
            && (ReferenceEquals(orderings, Orderings) || orderings.SequenceEqual(Orderings))
                ? this
                : new SnowflakeAggregateFunctionExpression(
                    Name,
                    arguments,
                    orderings,
                    IsNullable,
                    ArgumentsPropagateNullability,
                    Type,
                    TypeMapping);

    /// <inheritdoc />
    protected override void Print(ExpressionPrinter expressionPrinter)
    {
        expressionPrinter.Append(Name);

        expressionPrinter.Append("(");
        expressionPrinter.VisitCollection(Arguments);
        expressionPrinter.Append(")");

        if (Orderings.Count > 0)
        {
            expressionPrinter.Append(" WITHIN GROUP (ORDER BY ");
            expressionPrinter.VisitCollection(Orderings);
            expressionPrinter.Append(")");
        }
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj is SnowflakeAggregateFunctionExpression SnowflakeFunctionExpression && Equals(SnowflakeFunctionExpression);

    private bool Equals(SnowflakeAggregateFunctionExpression? other)
        => ReferenceEquals(this, other)
            || other is not null
            && base.Equals(other)
            && Name == other.Name
            && Arguments.SequenceEqual(other.Arguments)
            && Orderings.SequenceEqual(other.Orderings);

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(base.GetHashCode());
        hash.Add(Name);

        for (var i = 0; i < Arguments.Count; i++)
        {
            hash.Add(Arguments[i]);
        }

        for (var i = 0; i < Orderings.Count; i++)
        {
            hash.Add(Orderings[i]);
        }

        return hash.ToHashCode();
    }
}
