using System.Data;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Json;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Snowflake.EntityFrameworkCore.Storage.Internal;

/// <summary>
/// Represents a Snowflake-specific long type mapping.
/// </summary>
public class SnowflakeLongTypeMapping : LongTypeMapping
{
    /// <summary>
    /// The default <see cref="SnowflakeLongTypeMapping"/> instance, representing the "bigint" Snowflake data type.
    /// </summary>
    public new static SnowflakeLongTypeMapping Default { get; } = new("bigint");

    /// <summary>
    /// Creates a new instance of <see cref="SnowflakeLongTypeMapping" />
    /// </summary>
    /// <param name="storeType"></param>
    /// <param name="converter"></param>
    /// <param name="comparer"></param>
    /// <param name="providerValueComparer"></param>
    /// <param name="dbType"></param>
    public SnowflakeLongTypeMapping(
        string storeType,
        ValueConverter? converter = null,
        ValueComparer? comparer = null,
        ValueComparer? providerValueComparer = null,
        DbType? dbType = System.Data.DbType.Int64)
        : this(
            new RelationalTypeMappingParameters(
                new CoreTypeMappingParameters(
                    typeof(long), converter, comparer, providerValueComparer,
                    jsonValueReaderWriter: JsonInt64ReaderWriter.Instance),
                storeType,
                dbType: dbType))
    {
    }

    /// <summary>
    /// Creates a new instance of <see cref="SnowflakeLongTypeMapping" />
    /// </summary>
    /// <param name="parameters"></param>
    protected SnowflakeLongTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters)
    {
    }

    /// <inheritdoc />
    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new SnowflakeLongTypeMapping(parameters);

    /// <inheritdoc />
    protected override string GenerateNonNullSqlLiteral(object value)
        => $"CAST({base.GenerateNonNullSqlLiteral(value)} AS {StoreType})";
}