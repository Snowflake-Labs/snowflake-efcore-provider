using System;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Data.Common;
using Microsoft.EntityFrameworkCore.Storage;
using Snowflake.Data.Client;
using Snowflake.Data.Core;

namespace Snowflake.EntityFrameworkCore.Storage.Internal;

/// <summary>
/// Represents a Snowflake-specific date-only type mapping.
/// </summary>
public class SnowflakeDateOnlyTypeMapping : DateOnlyTypeMapping
{
    /// <summary>
    /// Default type mapping for <see cref="DateOnly" />.
    /// </summary>
    public new static SnowflakeDateOnlyTypeMapping Default { get; } = new("date");

    /// <summary>
    /// Creates a new instance of this class.
    /// </summary>
    /// <param name="storeType"></param>
    internal SnowflakeDateOnlyTypeMapping(string storeType)
        : base(storeType)
    {
    }

    /// <inheritdoc />
    public override ValueConverter<DateOnly, DateTime> Converter => new(
        v => v.ToDateTime(TimeOnly.MinValue),
        v => new DateOnly(v.Year, v.Month, v.Day));

    /// <summary>
    /// Creates a new instance of this class.
    /// </summary>
    /// <param name="parameters"></param>
    protected SnowflakeDateOnlyTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters)
    {
    }

    /// <inheritdoc />
    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new SnowflakeDateOnlyTypeMapping(parameters);

    /// <inheritdoc />
    protected override void ConfigureParameter(DbParameter parameter)
    {
        base.ConfigureParameter(parameter);

        // Workaround for SqlClient issue: https://github.com/dotnet/runtime/issues/22386
        ((SnowflakeDbParameter)parameter).SFDataType = SFDataType.DATE;
    }

    /// <inheritdoc />
    protected override string SqlLiteralFormatString
        => "'{0:yyyy-MM-dd}'";
}