using System;
using System.Data.Common;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data.SqlTypes;
using System.Globalization;
using System.Text;
using Snowflake.Data.Client;
using Snowflake.Data.Core;
using Snowflake.EntityFrameworkCore.Storage.Internal;

namespace Snowflake.EntityFrameworkCore.Query.Internal;

/// <summary>
/// Snowflake specific implementation of <see cref="IRelationalQueryStringFactory" />.
/// </summary>
public class SnowflakeQueryStringFactory : IRelationalQueryStringFactory
{
    private readonly IRelationalTypeMappingSource _typeMapper;

    /// <summary>
    ///    Initializes a new instance of the <see cref="SnowflakeQueryStringFactory" /> class.
    /// </summary>
    public SnowflakeQueryStringFactory(IRelationalTypeMappingSource typeMapper)
    {
        _typeMapper = typeMapper;
    }

    /// <inheritdoc />
    public virtual string Create(DbCommand command)
    {
        if (command.Parameters.Count == 0)
        {
            return command.CommandText;
        }

        var builder = new StringBuilder();
        foreach (DbParameter parameter in command.Parameters)
        {
            var typeName = TypeNameBuilder.CreateTypeName(parameter);

            builder
                .Append("DECLARE ")
                .Append(parameter.ParameterName)
                .Append(' ')
                .Append(typeName)
                .Append(" = ");

            if (parameter.Value == DBNull.Value || parameter.Value is null)
            {
                builder.Append("NULL");
            }
            else
            {
                var typeMapping = _typeMapper.FindMapping(parameter.Value.GetType(), typeName);

                builder
                    .Append(
                        parameter.Value is SqlBytes sqlBytes
                            ? new SnowflakeByteArrayTypeMapping(typeName).GenerateSqlLiteral(sqlBytes.Value)
                            : typeMapping != null
                                ? typeMapping.GenerateSqlLiteral(parameter.Value)
                                : parameter.Value.ToString());
            }

            builder.AppendLine(";");
        }

        return builder
            .AppendLine()
            .Append(command.CommandText).ToString();
    }
}

internal static class TypeNameBuilder
{
    private static StringBuilder AppendSize(this StringBuilder builder, DbParameter parameter)
    {
        if (parameter.Size > 0)
        {
            builder
                .Append('(')
                .Append(parameter.Size.ToString(CultureInfo.InvariantCulture))
                .Append(')');
        }

        return builder;
    }

    private static StringBuilder AppendSizeOrMax(this StringBuilder builder, DbParameter parameter)
    {
        if (parameter.Size > 0)
        {
            builder.AppendSize(parameter);
        }
        else if (parameter.Size == -1)
        {
            builder.Append("(max)");
        }

        return builder;
    }

    private static StringBuilder AppendPrecision(this StringBuilder builder, DbParameter parameter)
    {
        if (parameter.Precision > 0)
        {
            builder
                .Append('(')
                .Append(parameter.Precision.ToString(CultureInfo.InvariantCulture))
                .Append(')');
        }

        return builder;
    }

    private static StringBuilder AppendScale(this StringBuilder builder, DbParameter parameter)
    {
        if (parameter.Scale > 0)
        {
            builder
                .Append('(')
                .Append(parameter.Scale.ToString(CultureInfo.InvariantCulture))
                .Append(')');
        }

        return builder;
    }

    private static StringBuilder AppendPrecisionAndScale(this StringBuilder builder, DbParameter parameter)
    {
        if (parameter is { Precision: > 0, Scale: > 0 })
        {
            return builder
                .Append('(')
                .Append(parameter.Precision.ToString(CultureInfo.InvariantCulture))
                .Append(',')
                .Append(parameter.Scale.ToString(CultureInfo.InvariantCulture))
                .Append(')');
        }

        return builder.AppendPrecision(parameter);
    }

    public static string CreateTypeName(DbParameter parameter)
    {
        if (parameter is not SnowflakeDbParameter sqlParameter) return "variant";
        var builder = new StringBuilder();
        return (
            sqlParameter.SFDataType switch
            {
                SFDataType.FIXED => builder.Append("decimal").AppendPrecisionAndScale(parameter),
                SFDataType.DATE => builder.Append("date"),
                SFDataType.TIME => builder.Append("time"),
                SFDataType.TIMESTAMP_NTZ => builder.Append("timestamp_ntz"),
                SFDataType.TIMESTAMP_LTZ => builder.Append("timestamp_ltz"),
                SFDataType.TIMESTAMP_TZ => builder.Append("timestamp_tz"),

                SFDataType.VARIANT => builder.Append("variant"),
                SFDataType.ARRAY => builder.Append("array"),
                SFDataType.OBJECT => builder.Append("object"),
                _ => builder.Append("variant")
            }).ToString();
    }
}