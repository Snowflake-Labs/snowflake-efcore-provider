using System;
using System.Data.Common;
using System.Globalization;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Json;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Snowflake.EntityFrameworkCore.Storage.Internal;

/// <summary>
/// Represents a Snowflake-specific time-only type mapping.
/// </summary>
public class SnowflakeTimeOnlyTypeMapping : TimeOnlyTypeMapping
{
    private readonly string[] _timeFormats =
    {
        @"'{0:HH\:mm\:ss}'",
        @"'{0:HH\:mm\:ss\.F}'",
        @"'{0:HH\:mm\:ss\.FF}'",
        @"'{0:HH\:mm\:ss\.FFF}'",
        @"'{0:HH\:mm\:ss\.FFFF}'",
        @"'{0:HH\:mm\:ss\.FFFFF}'",
        @"'{0:HH\:mm\:ss\.FFFFFF}'",
        @"'{0:HH\:mm\:ss\.FFFFFFF}'"
    };

    public new static SnowflakeTimeOnlyTypeMapping Default { get; } = new("time");

    /// <inheritdoc />
    public override ValueConverter<TimeOnly, DateTime> Converter => new(
        v => new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day, v.Hour, v.Minute, v.Second),
        v => TimeOnly.FromDateTime(v));

    internal SnowflakeTimeOnlyTypeMapping(string storeType,
        StoreTypePostfix storeTypePostfix = StoreTypePostfix.Precision)
        : base(
            new RelationalTypeMappingParameters(
                new CoreTypeMappingParameters(typeof(TimeOnly),
                    jsonValueReaderWriter: JsonTimeOnlyReaderWriter.Instance),
                storeType,
                storeTypePostfix,
                System.Data.DbType.Time))
    {
    }

    /// <summary>
    /// Creates a new instance of <see cref="SnowflakeTimeOnlyTypeMapping" />
    /// </summary>
    /// <param name="parameters"></param>
    protected SnowflakeTimeOnlyTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters)
    {
    }

    /// <inheritdoc />
    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new SnowflakeTimeOnlyTypeMapping(parameters);

    /// <inheritdoc />
    protected override void ConfigureParameter(DbParameter parameter)
    {
        base.ConfigureParameter(parameter);
        if (Precision.HasValue)
        {
            // Snowflake accepts a scale, but in EF a scale along isn't supported (without precision).
            // So the actual value is contained as precision in scale, but sent as Scale to Snowflake.
            parameter.Scale = unchecked((byte)Precision.Value);
        }
    }

    /// <inheritdoc />
    protected override string SqlLiteralFormatString
        => _timeFormats[Precision is >= 0 and <= 7 ? Precision.Value : 7];

    /// <inheritdoc />
    protected override string GenerateNonNullSqlLiteral(object value)
    {
        var ticks = value is TimeOnly only ? only.Ticks : ((DateTime)value).TimeOfDay.Ticks;
        return ticks % 10000000 == 0
            ? string.Format(CultureInfo.InvariantCulture, _timeFormats[0], value)
            : string.Format(CultureInfo.InvariantCulture, SqlLiteralFormatString, value);
    }
}