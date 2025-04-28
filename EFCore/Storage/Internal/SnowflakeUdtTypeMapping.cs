using System;
using System.Data;
using System.Data.Common;
using System.Linq.Expressions;
using System.Threading;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Snowflake.EntityFrameworkCore.Storage.Internal;

/// <summary>
/// Represents a Snowflake-specific UDT type mapping.
/// </summary>
public class SnowflakeUdtTypeMapping : RelationalTypeMapping
{
    private static Action<DbParameter, string>? _udtTypeNameSetter;

    /// <summary>
    /// Creates a new instance of <see cref="SnowflakeUdtTypeMapping" />
    /// </summary>
    /// <param name="clrType"></param>
    /// <param name="storeType"></param>
    /// <param name="literalGenerator"></param>
    /// <param name="storeTypePostfix"></param>
    /// <param name="udtTypeName"></param>
    /// <param name="converter"></param>
    /// <param name="comparer"></param>
    /// <param name="keyComparer"></param>
    /// <param name="dbType"></param>
    /// <param name="unicode"></param>
    /// <param name="size"></param>
    /// <param name="fixedLength"></param>
    /// <param name="precision"></param>
    /// <param name="scale"></param>
    public SnowflakeUdtTypeMapping(
        Type clrType,
        string storeType,
        Func<object, Expression> literalGenerator,
        StoreTypePostfix storeTypePostfix = StoreTypePostfix.None,
        string? udtTypeName = null,
        ValueConverter? converter = null,
        ValueComparer? comparer = null,
        ValueComparer? keyComparer = null,
        DbType? dbType = null,
        bool unicode = false,
        int? size = null,
        bool fixedLength = false,
        int? precision = null,
        int? scale = null)
        : base(
            new RelationalTypeMappingParameters(
                new CoreTypeMappingParameters(
                    clrType, converter, comparer, keyComparer), storeType, storeTypePostfix, dbType, unicode, size,
                fixedLength,
                precision, scale))

    {
        LiteralGenerator = literalGenerator;
        UdtTypeName = udtTypeName ?? storeType;
    }

    /// <summary>
    /// Creates a new instance of <see cref="SnowflakeUdtTypeMapping" />
    /// </summary>
    /// <param name="parameters"></param>
    /// <param name="literalGenerator"></param>
    /// <param name="udtTypeName"></param>
    protected SnowflakeUdtTypeMapping(
        RelationalTypeMappingParameters parameters,
        Func<object, Expression> literalGenerator,
        string? udtTypeName)
        : base(parameters)
    {
        LiteralGenerator = literalGenerator;
        UdtTypeName = udtTypeName ?? parameters.StoreType;
    }

    /// <summary>
    /// The name of the UDT type.
    /// </summary>
    public virtual string UdtTypeName { get; }

    /// <summary>
    /// The literal generator for this mapping.
    /// </summary>
    public virtual Func<object, Expression> LiteralGenerator { get; }

    /// <inheritdoc />
    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        => new SnowflakeUdtTypeMapping(parameters, LiteralGenerator, UdtTypeName);

    /// <inheritdoc />
    protected override void ConfigureParameter(DbParameter parameter)
        => SetUdtTypeName(parameter);

    private void SetUdtTypeName(DbParameter parameter)
    {
        LazyInitializer.EnsureInitialized(
            ref _udtTypeNameSetter,
            () => CreateUdtTypeNameAccessor(parameter.GetType()));

        if (parameter.Value != null
            && parameter.Value != DBNull.Value)
        {
            _udtTypeNameSetter(parameter, UdtTypeName);
        }
    }

    /// <inheritdoc />
    public override Expression GenerateCodeLiteral(object value)
        => LiteralGenerator(value);

    private static Action<DbParameter, string> CreateUdtTypeNameAccessor(Type paramType)
    {
        var paramParam = Expression.Parameter(typeof(DbParameter), "parameter");
        var valueParam = Expression.Parameter(typeof(string), "value");

        return Expression.Lambda<Action<DbParameter, string>>(
            Expression.Call(
                Expression.Convert(paramParam, paramType),
                paramType.GetProperty("UdtTypeName")!.SetMethod!,
                valueParam),
            paramParam,
            valueParam).Compile();
    }
}