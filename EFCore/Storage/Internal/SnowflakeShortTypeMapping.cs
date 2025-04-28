using System.Data;
using Microsoft.EntityFrameworkCore.Storage;

namespace Snowflake.EntityFrameworkCore.Storage.Internal;

/// <summary>
///    Represents a type mapping to a Snowflake SMALLINT column.
/// </summary>
public class SnowflakeShortTypeMapping : ShortTypeMapping
{
    /// <summary>
    ///  Represents a type mapping to a Snowflake SMALLINT column.
    /// </summary>
    public new static SnowflakeShortTypeMapping Default { get; } = new("smallint");

    /// <summary>
    ///   Initializes a new instance of the <see cref="SnowflakeShortTypeMapping" /> class.
    /// </summary>
    /// <param name="storeType"></param>
    /// <param name="dbType"></param>
    public SnowflakeShortTypeMapping(
        string storeType,
        DbType? dbType = System.Data.DbType.Int16)
        : base(storeType, dbType)
    {
    }

    /// <summary>
    ///  Initializes a new instance of the <see cref="SnowflakeShortTypeMapping" /> class.
    /// </summary> 
    protected SnowflakeShortTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters)
    {
    }

    /// <summary>
    ///     Creates a copy of this mapping.
    /// </summary>
    /// <param name="parameters">The parameters for this mapping.</param>
    /// <returns>The newly created mapping.</returns>
    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new SnowflakeShortTypeMapping(parameters);

    /// <inheritdoc />
    protected override string GenerateNonNullSqlLiteral(object value)
        => $"CAST({base.GenerateNonNullSqlLiteral(value)} AS {StoreType})";
}