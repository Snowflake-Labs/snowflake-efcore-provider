using System.Data;
using System.Data.Common;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Json;

namespace Snowflake.EntityFrameworkCore.Storage.Internal;

/// <summary>
/// Represents a FLOAT data type for Snowflake.
/// </summary>
public class SnowflakeFloatTypeMapping : FloatTypeMapping
{
    /// <summary>
    /// The default <see cref="SnowflakeFloatTypeMapping" /> instance, with precision of 24.
    /// </summary>
    public new static SnowflakeFloatTypeMapping Default { get; } = new("real");


    /// <summary>
    /// Creates a new instance of <see cref="SnowflakeFloatTypeMapping" />
    /// </summary>
    /// <param name="storeType"></param>
    /// <param name="dbType"></param>
    /// <param name="storeTypePostfix"></param>
    public SnowflakeFloatTypeMapping(
        string storeType,
        DbType? dbType = System.Data.DbType.Single,
        StoreTypePostfix storeTypePostfix = StoreTypePostfix.Precision)
        : base(
            new RelationalTypeMappingParameters(
                new CoreTypeMappingParameters(typeof(float), jsonValueReaderWriter: JsonFloatReaderWriter.Instance),
                storeType,
                storeTypePostfix,
                dbType))
    {
    }


    /// <summary>
    /// Creates a new instance of <see cref="SnowflakeFloatTypeMapping" />
    /// </summary>
    /// <param name="parameters"></param>
    protected SnowflakeFloatTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters)
    {
    }

    /// <inheritdoc />
    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new SnowflakeFloatTypeMapping(parameters);

    /// <inheritdoc />
    protected override string GenerateNonNullSqlLiteral(object value)
        => $"CAST({base.GenerateNonNullSqlLiteral(value)} AS {StoreType})";

    /// <inheritdoc />
    protected override void ConfigureParameter(DbParameter parameter)
    {
        base.ConfigureParameter(parameter);

        if (Precision.HasValue
            && Precision.Value != -1)
        {
            // SqlClient wants this set as "size"
            parameter.Size = (byte)Precision.Value;
        }
    }
}