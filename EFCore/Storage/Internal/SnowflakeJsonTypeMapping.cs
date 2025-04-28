using System.Text;
using System.Text.Json;
using System.Data.Common;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Storage;

namespace Snowflake.EntityFrameworkCore.Storage.Internal;

/// <summary>
/// Represents a Snowflake-specific JSON type mapping.
/// </summary>
public class SnowflakeJsonTypeMapping : JsonTypeMapping
{
    private static readonly MethodInfo GetStringMethod
        = typeof(DbDataReader).GetRuntimeMethod(nameof(DbDataReader.GetString), [typeof(int)])!;

    private static readonly PropertyInfo UTF8Property
        = typeof(Encoding).GetProperty(nameof(Encoding.UTF8))!;

    private static readonly MethodInfo EncodingGetBytesMethod
        = typeof(Encoding).GetMethod(nameof(Encoding.GetBytes), [typeof(string)])!;

    private static readonly ConstructorInfo MemoryStreamConstructor
        = typeof(MemoryStream).GetConstructor([typeof(byte[])])!;

    /// <summary>
    /// The default <see cref="SnowflakeJsonTypeMapping"/> instance, representing the "nvarchar(max)" Snowflake data type.
    /// </summary>
    public static SnowflakeJsonTypeMapping Default { get; } = new("nvarchar(max)");

    /// <summary>
    /// Creates a new instance of <see cref="SnowflakeJsonTypeMapping" />
    /// </summary>
    /// <param name="storeType"></param>
    public SnowflakeJsonTypeMapping(string storeType)
        : base(storeType, typeof(JsonElement), System.Data.DbType.String)
    {
    }

    /// <inheritdoc />
    public override MethodInfo GetDataReaderMethod()
        => GetStringMethod;

    /// <inheritdoc />
    public override Expression CustomizeDataReaderExpression(Expression expression)
        => Expression.New(
            MemoryStreamConstructor,
            Expression.Call(
                Expression.Property(null, UTF8Property),
                EncodingGetBytesMethod,
                expression));

    /// <inheritdoc />
    protected SnowflakeJsonTypeMapping(RelationalTypeMappingParameters parameters)
        : base(parameters)
    {
    }

    /// <summary>
    /// Escapes a SQL literal.
    /// </summary>
    /// <param name="literal"></param>
    /// <returns></returns>
    protected virtual string EscapeSqlLiteral(string literal)
        => literal.Replace("'", "''");

    /// <inheritdoc />
    protected override string GenerateNonNullSqlLiteral(object value)
        => $"'{EscapeSqlLiteral(JsonSerializer.Serialize(value))}'";

    /// <inheritdoc />
    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new SnowflakeJsonTypeMapping(parameters);
}