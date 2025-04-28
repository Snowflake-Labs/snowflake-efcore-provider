using System.Data.Common;
using Microsoft.EntityFrameworkCore.Storage;
using Snowflake.Data.Client;
using Snowflake.Data.Core;

namespace Snowflake.EntityFrameworkCore.Storage.Internal;

/// <summary>
/// 
/// </summary>
public class SnowflakeVariantTypeMapping : RelationalTypeMapping
{
    /// <summary>
    /// The default <see cref="SnowflakeVariantTypeMapping"/> instance, representing the "variant" Snowflake data type.
    /// </summary>
    public static SnowflakeVariantTypeMapping Default { get; } = new("variant");

    /// <summary>
    /// Creates a new instance of <see cref="SnowflakeVariantTypeMapping" /> 
    /// </summary>
    /// <param name="storeType"></param>
    public SnowflakeVariantTypeMapping(string storeType)
        : base(storeType, typeof(string), System.Data.DbType.Object)
    {
    }

    /// <summary>
    /// Creates a new instance of <see cref="SnowflakeVariantTypeMapping" />
    /// </summary>
    /// <param name="parameters"></param>
    protected SnowflakeVariantTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters)
    {
    }


    /// <inheritdoc />
    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new SnowflakeVariantTypeMapping(parameters);

    /// <inheritdoc />
    protected override void ConfigureParameter(DbParameter parameter)
    {
        base.ConfigureParameter(parameter);

        ((SnowflakeDbParameter)parameter).SFDataType = SFDataType.VARIANT;
    }
}