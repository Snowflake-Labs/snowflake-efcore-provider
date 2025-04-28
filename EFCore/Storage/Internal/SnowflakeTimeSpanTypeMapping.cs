using System.Data;
using System.Globalization;
using Microsoft.EntityFrameworkCore.Storage.Json;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Data.Common;
using Microsoft.EntityFrameworkCore.Storage;

namespace Snowflake.EntityFrameworkCore.Storage.Internal;

/// <summary>
/// Represents a Snowflake-specific time span type mapping.
/// </summary>
public class SnowflakeTimeSpanTypeMapping : TimeSpanTypeMapping
{
    // Note: this array will be accessed using the precision as an index
    // so the order of the entries in this array is important
    private readonly string[] _timeFormats =
    {
        @"'{0:hh\:mm\:ss}'",
        @"'{0:hh\:mm\:ss\.F}'",
        @"'{0:hh\:mm\:ss\.FF}'",
        @"'{0:hh\:mm\:ss\.FFF}'",
        @"'{0:hh\:mm\:ss\.FFFF}'",
        @"'{0:hh\:mm\:ss\.FFFFF}'",
        @"'{0:hh\:mm\:ss\.FFFFFF}'",
        @"'{0:hh\:mm\:ss\.FFFFFFF}'"
    };

    /// <summary>
    /// The default <see cref="SnowflakeTimeSpanTypeMapping"/> instance, representing the "time" Snowflake data type.
    /// </summary>
    public new static SnowflakeTimeSpanTypeMapping Default { get; } = new("time");

    /// <inheritdoc />
    public override ValueConverter<TimeSpan, DateTime> Converter => new(
        v => DateTime.MinValue.Add(v),
        v => v.TimeOfDay);

    /// <summary>
    /// Creates a new instance of <see cref="SnowflakeTimeSpanTypeMapping" />
    /// </summary>
    /// <param name="storeType"></param>
    /// <param name="dbType"></param>
    /// <param name="storeTypePostfix"></param>
    public SnowflakeTimeSpanTypeMapping(
        string storeType,
        DbType? dbType = System.Data.DbType.Time,
        StoreTypePostfix storeTypePostfix = StoreTypePostfix.Precision)
        : base(
            new RelationalTypeMappingParameters(
                new CoreTypeMappingParameters(typeof(TimeSpan),
                    jsonValueReaderWriter: JsonTimeSpanReaderWriter.Instance),
                storeType,
                storeTypePostfix,
                dbType))
    {
    }

    /// <summary>
    /// Creates a new instance of <see cref="SnowflakeTimeSpanTypeMapping" />
    /// </summary>
    /// <param name="parameters"></param>
    protected SnowflakeTimeSpanTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters)
    {
    }

    /// <inheritdoc />
    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new SnowflakeTimeSpanTypeMapping(parameters);

    /// <inheritdoc />
    protected override void ConfigureParameter(DbParameter parameter)
    {
        base.ConfigureParameter(parameter);
        if (Precision.HasValue)
        {
            parameter.Scale = (byte)Precision.Value;
        }
    }

    /// <inheritdoc />
    protected override string SqlLiteralFormatString
        => _timeFormats[Precision is >= 0 and <= 7 ? Precision.Value : 7];

    /// <inheritdoc />
    protected override string GenerateNonNullSqlLiteral(object value)
        => value is TimeSpan { Milliseconds: 0 } // Handle trailing decimal separator when no fractional seconds
            ? string.Format(CultureInfo.InvariantCulture, _timeFormats[0], value)
            : string.Format(CultureInfo.InvariantCulture, SqlLiteralFormatString, value);
}