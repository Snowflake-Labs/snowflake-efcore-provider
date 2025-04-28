using System.Data;
using System;
using System.Data.Common;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Json;
using Snowflake.Data.Client;
using Snowflake.Data.Core;

namespace Snowflake.EntityFrameworkCore.Storage.Internal;

/// <summary>
///    Represents a <see cref="DateTime" /> type in Snowflake.
/// </summary>
public class SnowflakeDateTimeTypeMapping : DateTimeTypeMapping, ISnowflakeStringLiteralRequired
{
    private const string DateFormatConst = "'{0:yyyy-MM-dd}'";

    private readonly SqlDbType? _sqlDbType;

    // Note: this array will be accessed using the precision as an index
    // so the order of the entries in this array is important
    private readonly string[] _dateTimeFormats =
    {
        "'{0:yyyy-MM-dd HH:mm:ss}'",
        "'{0:yyyy-MM-dd HH:mm:ss.fK}'",
        "'{0:yyyy-MM-dd HH:mm:ss.ffK}'",
        "'{0:yyyy-MM-dd HH:mm:ss.fffK}'",
        "'{0:yyyy-MM-dd HH:mm:ss.ffffK}'",
        "'{0:yyyy-MM-dd HH:mm:ss.fffffK}'",
        "'{0:yyyy-MM-dd HH:mm:ss.ffffffK}'",
        "'{0:yyyy-MM-dd HH:mm:ss.fffffffK}'"
    };

    private readonly string[] _timeFormats =
    {
        "'{0:HH:mm:ss}'",
        "'{0:HH:mm:ss.f}'",
        "'{0:HH:mm:ss.ff}'",
        "'{0:HH:mm:ss.fff}'",
        "'{0:HH:mm:ss.ffff}'",
        "'{0:HH:mm:ss.fffff}'",
        "'{0:HH:mm:ss.ffffff}'",
        "'{0:HH:mm:ss.fffffff}'"
    };

    /// <summary>
    /// Creates a new instance of <see cref="SnowflakeDateTimeTypeMapping" />.
    /// </summary>
    /// <param name="storeType"></param>
    /// <param name="dbType"></param>
    /// <param name="sqlDbType"></param>
    /// <param name="storeTypePostfix"></param>
    public SnowflakeDateTimeTypeMapping(
        string storeType,
        DbType? dbType = System.Data.DbType.DateTime,
        SqlDbType? sqlDbType = null,
        StoreTypePostfix storeTypePostfix = StoreTypePostfix.Precision)
        : this(
            new RelationalTypeMappingParameters(
                new CoreTypeMappingParameters(typeof(DateTime),
                    jsonValueReaderWriter: JsonDateTimeReaderWriter.Instance),
                storeType,
                storeTypePostfix,
                dbType),
            sqlDbType)
    {
    }

    /// <summary>
    /// Creates a new instance of <see cref="SnowflakeDateTimeTypeMapping" />.
    /// </summary>
    /// <param name="parameters"></param>
    /// <param name="sqlDbType"></param>
    protected SnowflakeDateTimeTypeMapping(RelationalTypeMappingParameters parameters, SqlDbType? sqlDbType)
        : base(parameters)
    {
        _sqlDbType = sqlDbType;
    }

    /// <summary>
    ///    Gets the store type name.
    /// </summary>
    public virtual SqlDbType? SqlType
        => _sqlDbType;

    /// <inheritdoc />
    protected override void ConfigureParameter(DbParameter parameter)
    {
        base.ConfigureParameter(parameter);

        switch (StoreTypeNameBase.ToLower())
        {
            case "date":
                ((SnowflakeDbParameter)parameter).SFDataType = SFDataType.DATE;
                break;
            case "time":
                ((SnowflakeDbParameter)parameter).SFDataType = SFDataType.TIME;
                break;
            case "timestamp_ntz":
                ((SnowflakeDbParameter)parameter).SFDataType = SFDataType.TIMESTAMP_NTZ;
                break;
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
    }

    /// <inheritdoc />
    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new SnowflakeDateTimeTypeMapping(parameters, _sqlDbType);

    /// <inheritdoc />
    protected override string SqlLiteralFormatString
    {
        get
        {
            switch (StoreTypeNameBase.ToLower())
            {
                case "date":
                    return DateFormatConst;
                case "time":
                    if (Precision.HasValue)
                    {
                        var precision = Precision.Value;
                        if (precision >= 0 && precision <= 7)
                        {
                            return _timeFormats[precision];
                        }
                    }

                    return _timeFormats[7];


                default:
                    if (Precision.HasValue)
                    {
                        var precision = Precision.Value;
                        if (precision >= 0 && precision <= 7)
                        {
                            return _dateTimeFormats[precision];
                        }
                    }

                    return _dateTimeFormats[7];
            }
        }
    }
}