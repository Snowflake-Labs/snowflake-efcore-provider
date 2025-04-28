using System.Data;
using System.Data.Common;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Json;

namespace Snowflake.EntityFrameworkCore.Storage.Internal;

/// <summary>
/// Represents a Snowflake-specific decimal type mapping.
/// </summary>
public class SnowflakeDecimalTypeMapping : DecimalTypeMapping
{
    private readonly SqlDbType? _sqlDbType;

    /// <summary>
    /// The default <see cref="SnowflakeDecimalTypeMapping"/> instance, representing the "decimal(18, 2)" Snowflake data type.
    /// </summary>
    public new static SnowflakeDecimalTypeMapping Default { get; } = new("decimal(18, 2)", precision: 18, scale: 2);

    /// <summary>
    /// Creates a new instance of <see cref="SnowflakeDecimalTypeMapping" />
    /// </summary>
    /// <param name="storeType"></param>
    /// <param name="dbType"></param>
    /// <param name="precision"></param>
    /// <param name="scale"></param>
    /// <param name="sqlDbType"></param>
    /// <param name="storeTypePostfix"></param>
    public SnowflakeDecimalTypeMapping(
        string storeType,
        DbType? dbType = System.Data.DbType.Decimal,
        int? precision = null,
        int? scale = null,
        SqlDbType? sqlDbType = null,
        StoreTypePostfix storeTypePostfix = StoreTypePostfix.PrecisionAndScale)
        : this(
            new RelationalTypeMappingParameters(
                    new CoreTypeMappingParameters(typeof(decimal),
                        jsonValueReaderWriter: JsonDecimalReaderWriter.Instance),
                    storeType,
                    storeTypePostfix,
                    dbType)
                .WithPrecisionAndScale(precision, scale), sqlDbType)
    {
    }

    /// <summary>
    /// Creates a new instance of <see cref="SnowflakeDecimalTypeMapping" />
    /// </summary>
    /// <param name="parameters"></param>
    /// <param name="sqlDbType"></param>
    protected SnowflakeDecimalTypeMapping(RelationalTypeMappingParameters parameters, SqlDbType? sqlDbType)
        : base(parameters)
    {
        _sqlDbType = sqlDbType;
    }

    /// <summary>
    /// The <see cref="SqlDbType" /> to use for the column.
    /// </summary>
    public virtual SqlDbType? SqlType
        => _sqlDbType;

    /// <inheritdoc />
    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new SnowflakeDecimalTypeMapping(parameters, _sqlDbType);

    /// <inheritdoc />
    protected override void ConfigureParameter(DbParameter parameter)
    {
        base.ConfigureParameter(parameter);

        if (_sqlDbType != null)
        {
            // TODO FIX ((SqlParameter)parameter).SqlDbType = _sqlDbType.Value;
        }

        if (Size.HasValue
            && Size.Value != -1)
        {
            parameter.Size = Size.Value;
        }

        if (Precision.HasValue)
        {
            parameter.Precision = (byte)Precision.Value;
        }

        if (Scale.HasValue)
        {
            parameter.Scale = (byte)Scale.Value;
        }
    }
}