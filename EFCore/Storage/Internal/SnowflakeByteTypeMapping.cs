using System.Data;
using Microsoft.EntityFrameworkCore.Storage;

namespace Snowflake.EntityFrameworkCore.Storage.Internal;

/// <summary>
///    Represents a Snowflake-specific tinyint type mapping.
/// </summary>
public class SnowflakeByteTypeMapping : ByteTypeMapping
{
    /// <summary>
    ///   The default <see cref="SnowflakeByteTypeMapping"/> instance, representing the "tinyint" Snowflake data type.
    /// </summary>
    public new static SnowflakeByteTypeMapping Default { get; } = new("tinyint");

    /// <summary>
    /// Creates a new instance of <see cref="SnowflakeByteTypeMapping" />
    /// </summary>
    /// <param name="storeType"></param>
    /// <param name="dbType"></param>
    public SnowflakeByteTypeMapping(
        string storeType,
        DbType? dbType = System.Data.DbType.Byte)
        : base(storeType, dbType)
    {
    }

    /// <summary>
    /// Creates a new instance of <see cref="SnowflakeByteTypeMapping" />
    /// </summary>
    /// <param name="parameters"></param>
    protected SnowflakeByteTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters)
    {
    }

    /// <summary>
    ///     Creates a copy of this mapping.
    /// </summary>
    /// <param name="parameters">The parameters for this mapping.</param>
    /// <returns>The newly created mapping.</returns>
    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new SnowflakeByteTypeMapping(parameters);

    /// <inheritdoc />
    protected override string GenerateNonNullSqlLiteral(object value)
        => $"CAST({base.GenerateNonNullSqlLiteral(value)} AS {StoreType})";
}