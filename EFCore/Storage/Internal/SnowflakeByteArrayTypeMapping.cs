using System;
using System.Data.Common;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data;
using System.Globalization;
using System.Text;
using Microsoft.EntityFrameworkCore.Storage.Json;
using Snowflake.Data.Client;

namespace Snowflake.EntityFrameworkCore.Storage.Internal;

/// <summary>
/// Represents a Snowflake-specific byte array type mapping.
/// </summary>
public class SnowflakeByteArrayTypeMapping : ByteArrayTypeMapping
{
    private const int MaxSize = 8000;

    private readonly SqlDbType? _sqlDbType;

    /// <summary>
    /// The default <see cref="SnowflakeByteArrayTypeMapping"/> instance, representing the "varbinary" Snowflake data type.
    /// </summary>
    public new static SnowflakeByteArrayTypeMapping Default { get; } = new();

    /// <summary>
    /// Creates a new instance of <see cref="SnowflakeByteArrayTypeMapping" />
    /// </summary>
    /// <param name="storeType"></param>
    /// <param name="size"></param>
    /// <param name="fixedLength"></param>
    /// <param name="comparer"></param>
    /// <param name="sqlDbType"></param>
    /// <param name="storeTypePostfix"></param>
    public SnowflakeByteArrayTypeMapping(
        string? storeType = null,
        int? size = null,
        bool fixedLength = false,
        ValueComparer? comparer = null,
        SqlDbType? sqlDbType = null,
        StoreTypePostfix? storeTypePostfix = null)
        : this(
            new RelationalTypeMappingParameters(
                new CoreTypeMappingParameters(typeof(byte[]), null, comparer,
                    jsonValueReaderWriter: JsonByteArrayReaderWriter.Instance),
                storeType ?? (fixedLength ? "binary" : "varbinary"),
                storeTypePostfix ?? StoreTypePostfix.Size,
                System.Data.DbType.Binary,
                size: size,
                fixedLength: fixedLength),
            sqlDbType)
    {
    }

    /// <summary>
    /// Creates a new instance of <see cref="SnowflakeByteArrayTypeMapping" />
    /// </summary>
    /// <param name="parameters"></param>
    /// <param name="sqlDbType"></param>
    protected SnowflakeByteArrayTypeMapping(RelationalTypeMappingParameters parameters, SqlDbType? sqlDbType)
        : base(parameters)
    {
        _sqlDbType = sqlDbType;
    }

    private static int CalculateSize(int? size)
        => size is > 0 and < MaxSize ? size.Value : MaxSize;

    /// <inheritdoc />
    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new SnowflakeByteArrayTypeMapping(parameters, _sqlDbType);

    /// <inheritdoc />
    protected override void ConfigureParameter(DbParameter parameter)
    {
        var value = parameter.Value;
        var length = (value as byte[])?.Length;
        var maxSpecificSize = CalculateSize(Size);

        if (_sqlDbType.HasValue
            && parameter is SnowflakeDbParameter sqlParameter) // To avoid crashing wrapping providers
        {
            //sqlParameter.SFDataType = _sqlDbType.Value; // TODO Map types
        }

        if (value == null
            || value == DBNull.Value)
        {
            parameter.Size = maxSpecificSize;
        }
        else
        {
            if (length != null
                && length <= maxSpecificSize)
            {
                // Fixed-sized parameters get exact length to avoid padding/truncation.
                parameter.Size = IsFixedLength ? length.Value : maxSpecificSize;
            }
            else if (length is <= MaxSize)
            {
                parameter.Size = IsFixedLength ? length.Value : MaxSize;
            }
            else
            {
                parameter.Size = -1;
            }
        }
    }

    /// <inheritdoc />
    protected override string GenerateNonNullSqlLiteral(object value)
    {
        var builder = new StringBuilder();
        builder.Append("0x");

        foreach (var @byte in (byte[])value)
        {
            builder.Append(@byte.ToString("X2", CultureInfo.InvariantCulture));
        }

        return builder.ToString();
    }
}