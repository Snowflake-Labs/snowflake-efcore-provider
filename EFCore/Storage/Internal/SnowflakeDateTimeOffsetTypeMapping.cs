using System;
using System.Data;
using System.Data.Common;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Json;
using Snowflake.Data.Client;
using Snowflake.Data.Core;

namespace Snowflake.EntityFrameworkCore.Storage.Internal;

/// <summary>
/// Represents a Snowflake-specific DateTimeOffset type mapping.
/// </summary>
public class SnowflakeDateTimeOffsetTypeMapping : DateTimeOffsetTypeMapping, ISnowflakeStringLiteralRequired
{
    // Note: this array will be accessed using the precision as an index
    // so the order of the entries in this array is important
    private readonly string[] _dateTimeOffsetFormats =
    [
        "'{0:yyyy-MM-dd HH:mm:sszzz}'",
        "'{0:yyyy-MM-dd HH:mm:ss.fzzz}'",
        "'{0:yyyy-MM-dd HH:mm:ss.ffzzz}'",
        "'{0:yyyy-MM-dd HH:mm:ss.fffzzz}'",
        "'{0:yyyy-MM-dd HH:mm:ss.ffffzzz}'",
        "'{0:yyyy-MM-dd HH:mm:ss.fffffzzz}'",
        "'{0:yyyy-MM-dd HH:mm:ss.ffffffzzz}'",
        "'{0:yyyy-MM-dd HH:mm:ss.fffffffzzz}'"
    ];

    /// <summary>
    /// The default <see cref="SnowflakeDateTimeOffsetTypeMapping"/> instance, representing the "datetimeoffset" Snowflake data type.
    /// </summary>
    public new static SnowflakeDateTimeOffsetTypeMapping Default { get; } = new("datetimeoffset");

    /// <summary>
    /// Creates a new instance of <see cref="SnowflakeDateTimeOffsetTypeMapping" />
    /// </summary>
    /// <param name="storeType"></param>
    /// <param name="dbType"></param>
    /// <param name="storeTypePostfix"></param>
    public SnowflakeDateTimeOffsetTypeMapping(
        string storeType,
        DbType? dbType = System.Data.DbType.DateTimeOffset,
        StoreTypePostfix storeTypePostfix = StoreTypePostfix.Precision)
        : base(
            new RelationalTypeMappingParameters(
                new CoreTypeMappingParameters(typeof(DateTimeOffset),
                    jsonValueReaderWriter: JsonDateTimeOffsetReaderWriter.Instance),
                storeType,
                storeTypePostfix,
                dbType))
    {
    }

    /// <summary>
    /// Creates a new instance of <see cref="SnowflakeDateTimeOffsetTypeMapping" />
    /// </summary>
    /// <param name="parameters"></param>
    protected SnowflakeDateTimeOffsetTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters)
    {
    }

    /// <inheritdoc />
    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new SnowflakeDateTimeOffsetTypeMapping(parameters);

    /// <inheritdoc />
    protected override string SqlLiteralFormatString
    {
        get
        {
            if (Precision.HasValue)
            {
                var precision = Precision.Value;
                if (precision is <= 7 and >= 0)
                {
                    return _dateTimeOffsetFormats[precision];
                }
            }

            return _dateTimeOffsetFormats[7];
        }
    }

    /// <inheritdoc />
    protected override void ConfigureParameter(DbParameter parameter)
    {
        base.ConfigureParameter(parameter);

        switch (StoreTypeNameBase.ToLower())
        {
            case "timestamp_tz":
                ((SnowflakeDbParameter)parameter).SFDataType = SFDataType.TIMESTAMP_TZ;
                break;
            case "timestamp_ltz":
                ((SnowflakeDbParameter)parameter).SFDataType = SFDataType.TIMESTAMP_LTZ;
                break;
        }

        if (Size.HasValue
            && Size.Value != -1)
        {
            parameter.Size = Size.Value;
        }

        if (Precision.HasValue)
        {
            // Workaround for inconsistent definition of precision/scale between EF and SQLClient for VarTime types
            parameter.Scale = (byte)Precision.Value;
        }
    }
}