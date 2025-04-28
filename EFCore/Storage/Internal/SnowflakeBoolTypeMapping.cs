using System.Data;
using Microsoft.EntityFrameworkCore.Storage;

namespace Snowflake.EntityFrameworkCore.Storage.Internal;

/// <summary>
///   Represents a Snowflake-specific boolean type mapping.
/// </summary>
public class SnowflakeBoolTypeMapping : BoolTypeMapping
{
    /// <summary>
    ///  The default <see cref="SnowflakeBoolTypeMapping"/> instance, representing the "boolean" Snowflake data type.
    /// </summary>
    public new static SnowflakeBoolTypeMapping Default { get; } = new("boolean");

    /// <summary>
    /// Creates a new instance of <see cref="SnowflakeBoolTypeMapping" />
    /// </summary>
    /// <param name="storeType"></param>
    /// <param name="dbType"></param>
    public SnowflakeBoolTypeMapping(
        string storeType,
        DbType? dbType = System.Data.DbType.Boolean)
        : base(storeType, dbType)
    {
    }

    /// <summary>
    /// Creates a new instance of <see cref="SnowflakeBoolTypeMapping" />
    /// </summary>
    /// <param name="parameters"></param>
    protected SnowflakeBoolTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters)
    {
    }

    /// <summary>
    ///     Creates a copy of this mapping.
    /// </summary>
    /// <param name="parameters">The parameters for this mapping.</param>
    /// <returns>The newly created mapping.</returns>
    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new SnowflakeBoolTypeMapping(parameters);

    /// <inheritdoc />
    protected override string GenerateNonNullSqlLiteral(object value)
        => $"CAST({base.GenerateNonNullSqlLiteral(value)} AS {StoreType})";
}