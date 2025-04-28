using System;
using System.Data;
using System.Data.Common;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Json;

namespace Snowflake.EntityFrameworkCore.Storage.Internal;

/// <summary>
/// Represents a Snowflake-specific double type mapping.
/// </summary>
public class SnowflakeDoubleTypeMapping : DoubleTypeMapping
{
    /// <summary>
    /// The default <see cref="SnowflakeDoubleTypeMapping"/> instance, representing the "float" Snowflake data type.
    /// </summary>
    public new static SnowflakeDoubleTypeMapping Default { get; } = new("float");

    private static readonly MethodInfo GetFloatMethod
        = typeof(DbDataReader).GetRuntimeMethod(nameof(DbDataReader.GetFloat), new[] { typeof(int) })!;

    /// <summary>
    /// Creates a new instance of <see cref="SnowflakeDoubleTypeMapping" />
    /// </summary>
    /// <param name="storeType"></param>
    /// <param name="dbType"></param>
    /// <param name="storeTypePostfix"></param>
    public SnowflakeDoubleTypeMapping(
        string storeType,
        DbType? dbType = System.Data.DbType.Double,
        StoreTypePostfix storeTypePostfix = StoreTypePostfix.Precision)
        : base(
            new RelationalTypeMappingParameters(
                new CoreTypeMappingParameters(typeof(double), jsonValueReaderWriter: JsonDoubleReaderWriter.Instance),
                storeType,
                storeTypePostfix,
                dbType))
    {
    }

    /// <summary>
    /// Creates a new instance of <see cref="SnowflakeDoubleTypeMapping" />
    /// </summary>
    /// <param name="parameters"></param>
    protected SnowflakeDoubleTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters)
    {
    }

    /// <inheritdoc />
    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new SnowflakeDoubleTypeMapping(parameters);

    /// <inheritdoc />
    protected override string GenerateNonNullSqlLiteral(object value)
    {
        var literal = base.GenerateNonNullSqlLiteral(value);

        var doubleValue = Convert.ToDouble(value);
        return !literal.Contains('E')
               && !literal.Contains('e')
               && !double.IsNaN(doubleValue)
               && !double.IsInfinity(doubleValue)
            ? literal + "E0"
            : literal;
    }

    /// <inheritdoc />
    public override MethodInfo GetDataReaderMethod()
        => Precision is <= 24 ? GetFloatMethod : base.GetDataReaderMethod();

    /// <inheritdoc />
    public override Expression CustomizeDataReaderExpression(Expression expression)
    {
        if (Precision is <= 24)
        {
            expression = Expression.Convert(expression, typeof(double));
        }

        return base.CustomizeDataReaderExpression(expression);
    }

    /// <inheritdoc />
    protected override void ConfigureParameter(DbParameter parameter)
    {
        base.ConfigureParameter(parameter);

        if (Precision.HasValue
            && Precision.Value != -1)
        {
            // SqlClient wants this set as "size"
            parameter.Size = Precision.Value;
        }
    }
}