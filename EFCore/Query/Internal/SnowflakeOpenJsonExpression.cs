using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Snowflake.EntityFrameworkCore.Query.Internal;

/// <summary>
///     An expression that represents a Snowflake OPENJSON function call in a SQL tree.
/// </summary>
public class SnowflakeOpenJsonExpression : TableValuedFunctionExpression, IClonableTableExpressionBase
{
    /// <summary>
    ///    The JSON expression to be parsed.
    /// </summary>
    public virtual SqlExpression JsonExpression
        => Arguments[0];


    /// <summary>
    ///   The path to the JSON property to be parsed.
    /// </summary>
    public virtual IReadOnlyList<PathSegment>? Path { get; }


    /// <summary>
    ///  The column information for the JSON properties to be parsed.
    /// </summary>
    public virtual IReadOnlyList<ColumnInfo>? ColumnInfos { get; }


    /// <summary>
    ///  Creates a new instance of <see cref="SnowflakeOpenJsonExpression" />
    /// </summary>
    /// <param name="alias"></param>
    /// <param name="jsonExpression"></param>
    /// <param name="path"></param>
    /// <param name="columnInfos"></param>
    public SnowflakeOpenJsonExpression(
        string alias,
        SqlExpression jsonExpression,
        IReadOnlyList<PathSegment>? path = null,
        IReadOnlyList<ColumnInfo>? columnInfos = null)
        : base(alias, "OPENJSON", schema: null, builtIn: true, [jsonExpression])
    {
        Path = path;
        ColumnInfos = columnInfos;
    }


    /// <inheritdoc />
    protected override Expression VisitChildren(ExpressionVisitor visitor)
    {
        var visitedJsonExpression = (SqlExpression)visitor.Visit(JsonExpression);

        PathSegment[]? visitedPath = null;

        if (Path is not null)
        {
            for (var i = 0; i < Path.Count; i++)
            {
                var segment = Path[i];
                PathSegment newSegment;

                if (segment.PropertyName is not null)
                {
                    // PropertyName segments are (currently) constants, nothing to visit.
                    newSegment = segment;
                }
                else
                {
                    var newArrayIndex = (SqlExpression)visitor.Visit(segment.ArrayIndex)!;
                    if (newArrayIndex == segment.ArrayIndex)
                    {
                        newSegment = segment;
                    }
                    else
                    {
                        newSegment = new PathSegment(newArrayIndex);

                        if (visitedPath is null)
                        {
                            visitedPath = new PathSegment[Path.Count];
                            for (var j = 0; j < i; i++)
                            {
                                visitedPath[j] = Path[j];
                            }
                        }
                    }
                }

                if (visitedPath is not null)
                {
                    visitedPath[i] = newSegment;
                }
            }
        }

        return Update(visitedJsonExpression, visitedPath ?? Path, ColumnInfos);
    }

    /// <summary>
    ///  Updates the current instance of <see cref="SnowflakeOpenJsonExpression" />
    /// </summary>
    /// <param name="jsonExpression"></param>
    /// <param name="path"></param>
    /// <param name="columnInfos"></param>
    /// <returns></returns>
    public virtual SnowflakeOpenJsonExpression Update(
        SqlExpression jsonExpression,
        IReadOnlyList<PathSegment>? path,
        IReadOnlyList<ColumnInfo>? columnInfos = null)
        => jsonExpression == JsonExpression
           && (ReferenceEquals(path, Path) || path is not null && Path is not null && path.SequenceEqual(Path))
           && (ReferenceEquals(columnInfos, ColumnInfos)
               || columnInfos is not null && ColumnInfos is not null && columnInfos.SequenceEqual(ColumnInfos))
            ? this
            : new SnowflakeOpenJsonExpression(Alias, jsonExpression, path, columnInfos);

    /// <summary>
    /// Clones the current instance of <see cref="SnowflakeOpenJsonExpression" />
    /// </summary>
    /// <returns></returns>
    public virtual TableExpressionBase Clone()
    {
        var clone = new SnowflakeOpenJsonExpression(Alias, JsonExpression, Path, ColumnInfos);

        foreach (var annotation in GetAnnotations())
        {
            clone.AddAnnotation(annotation.Name, annotation.Value);
        }

        return clone;
    }

    /// <inheritdoc />
    protected override void Print(ExpressionPrinter expressionPrinter)
    {
        expressionPrinter.Append(Name);
        expressionPrinter.Append("(");
        expressionPrinter.Visit(JsonExpression);

        if (Path is not null)
        {
            expressionPrinter
                .Append(", '")
                .Append(string.Join(".", Path.Select(e => e.ToString())))
                .Append("'");
        }

        expressionPrinter.Append(")");

        if (ColumnInfos is not null)
        {
            expressionPrinter.Append(" WITH (");

            for (var i = 0; i < ColumnInfos.Count; i++)
            {
                var columnInfo = ColumnInfos[i];

                if (i > 0)
                {
                    expressionPrinter.Append(", ");
                }

                expressionPrinter
                    .Append(columnInfo.Name)
                    .Append(" ")
                    .Append(columnInfo.TypeMapping.StoreType);

                if (columnInfo.Path is not null)
                {
                    expressionPrinter
                        .Append(" '")
                        .Append(string.Join(".", columnInfo.Path.Select(e => e.ToString())))
                        .Append("'");
                }

                if (columnInfo.AsJson)
                {
                    expressionPrinter.Append(" AS JSON");
                }
            }

            expressionPrinter.Append(")");
        }

        PrintAnnotations(expressionPrinter);

        expressionPrinter.Append(" AS ");
        expressionPrinter.Append(Alias);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => ReferenceEquals(this, obj) || (obj is SnowflakeOpenJsonExpression openJsonExpression && Equals(openJsonExpression));

    private bool Equals(SnowflakeOpenJsonExpression other)
    {
        if (!base.Equals(other) || ColumnInfos?.Count != other.ColumnInfos?.Count)
        {
            return false;
        }

        if (ReferenceEquals(ColumnInfos, other.ColumnInfos))
        {
            return true;
        }

        for (var i = 0; i < ColumnInfos!.Count; i++)
        {
            var (columnInfo, otherColumnInfo) = (ColumnInfos[i], other.ColumnInfos![i]);

            if (columnInfo.Name != otherColumnInfo.Name
                || !columnInfo.TypeMapping.Equals(otherColumnInfo.TypeMapping)
                || (columnInfo.Path is null != otherColumnInfo.Path is null
                    || (columnInfo.Path is not null
                        && otherColumnInfo.Path is not null
                        && columnInfo.Path.SequenceEqual(otherColumnInfo.Path))))
            {
                return false;
            }
        }

        return true;
    }

    /// <inheritdoc />
    public override int GetHashCode()
        => base.GetHashCode();

    /// <summary>
    /// Represents a JSON column in a <see cref="SnowflakeOpenJsonExpression" />
    /// </summary>
    /// <param name="Name"></param>
    /// <param name="TypeMapping"></param>
    /// <param name="Path"></param>
    /// <param name="AsJson"></param>
    public readonly record struct ColumnInfo(
        string Name,
        RelationalTypeMapping TypeMapping,
        IReadOnlyList<PathSegment>? Path = null,
        bool AsJson = false);
}