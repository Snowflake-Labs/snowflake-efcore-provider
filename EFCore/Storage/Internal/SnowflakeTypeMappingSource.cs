using System.Collections;
using System.Data;
using System.Text.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Snowflake.EntityFrameworkCore.Storage.Internal;

/// <summary>
/// Represents a Snowflake-specific decimal type mapping.
/// </summary>
public class SnowflakeTypeMappingSource : RelationalTypeMappingSource
{
    private static readonly bool UseOldBehavior32898 =
        AppContext.TryGetSwitch("Microsoft.EntityFrameworkCore.Issue32898", out var enabled32898) && enabled32898;

    private static readonly SnowflakeFloatTypeMapping RealAlias
        = new("placeholder", storeTypePostfix: StoreTypePostfix.None);

    private static readonly SnowflakeByteArrayTypeMapping Rowversion
        = new(
            "rowversion",
            size: 8,
            comparer: new ValueComparer<byte[]>(
                (v1, v2) => StructuralComparisons.StructuralEqualityComparer.Equals(v1, v2),
                v => StructuralComparisons.StructuralEqualityComparer.GetHashCode(v),
                v => v.ToArray()),
            storeTypePostfix: StoreTypePostfix.None);

    private static readonly SnowflakeLongTypeMapping LongRowversion
        = new(
            "rowversion",
            converter: new NumberToBytesConverter<long>(),
            providerValueComparer: new ValueComparer<byte[]>(
                (v1, v2) => StructuralComparisons.StructuralEqualityComparer.Equals(v1, v2),
                v => StructuralComparisons.StructuralEqualityComparer.GetHashCode(v),
                v => v.ToArray()),
            dbType: DbType.Binary);

    private static readonly SnowflakeLongTypeMapping UlongRowversion
        = new(
            "rowversion",
            converter: new NumberToBytesConverter<ulong>(),
            providerValueComparer: new ValueComparer<byte[]>(
                (v1, v2) => StructuralComparisons.StructuralEqualityComparer.Equals(v1, v2),
                v => StructuralComparisons.StructuralEqualityComparer.GetHashCode(v),
                v => v.ToArray()),
            dbType: DbType.Binary);

    private static readonly SnowflakeStringTypeMapping FixedLengthUnicodeString
        = new(unicode: true, fixedLength: true);

    private static readonly SnowflakeStringTypeMapping TextUnicodeString
        = new("ntext", unicode: true, sqlDbType: SqlDbType.NText, storeTypePostfix: StoreTypePostfix.None);

    private static readonly SnowflakeStringTypeMapping VariableLengthUnicodeString
        = new(unicode: true);

    private static readonly SnowflakeStringTypeMapping VariableLengthMaxUnicodeString
        = new("nvarchar(max)", unicode: true, storeTypePostfix: StoreTypePostfix.None);

    private static readonly SnowflakeStringTypeMapping FixedLengthAnsiString
        = new(fixedLength: true);

    private readonly SnowflakeStringTypeMapping SnowflakeString
        = new("varchar", unicode: true);

    private static readonly SnowflakeStringTypeMapping TextAnsiString
        = new("text", sqlDbType: SqlDbType.Text, storeTypePostfix: StoreTypePostfix.None);

    private static readonly SnowflakeStringTypeMapping VariableLengthMaxAnsiString
        = new("varchar()", storeTypePostfix: StoreTypePostfix.None); // TODO

    private static readonly SnowflakeByteArrayTypeMapping ImageBinary
        = new("image", sqlDbType: SqlDbType.Image);

    private static readonly SnowflakeByteArrayTypeMapping VariableLengthMaxBinary
        = new("varbinary", storeTypePostfix: StoreTypePostfix.None);

    private static readonly SnowflakeByteArrayTypeMapping FixedLengthBinary
        = new(fixedLength: true);

    private static readonly SnowflakeDateTimeTypeMapping DateAsDateTime
        = new("date", DbType.Date);

    private static readonly SnowflakeDateTimeTypeMapping Datetime
        = new("datetime", DbType.DateTime);

    private static readonly SnowflakeDateTimeTypeMapping Time
        = new("time", DbType.Time);

    private static readonly SnowflakeDateTimeTypeMapping TimestampNtz
        = new("timestamp_ntz", DbType.DateTime);

    private static readonly SnowflakeDateTimeOffsetTypeMapping TimestampLtz
        = new("timestamp_ltz", DbType.DateTimeOffset);

    private static readonly SnowflakeDateTimeOffsetTypeMapping TimestampTz
        = new("timestamp_tz", DbType.DateTimeOffset);

    private static readonly SnowflakeDateTimeTypeMapping Datetime2Alias
        = new("placeholder", DbType.DateTime2, null, StoreTypePostfix.None);

    private static readonly DoubleTypeMapping DoubleAlias
        = new SnowflakeDoubleTypeMapping("placeholder", storeTypePostfix: StoreTypePostfix.None);

    private static readonly SnowflakeDateTimeOffsetTypeMapping DatetimeoffsetAlias
        = new("placeholder", DbType.DateTimeOffset, StoreTypePostfix.None);

    private static readonly SnowflakeDecimalTypeMapping Decimal
        = new("decimal", precision: 18, scale: 0);

    private static readonly SnowflakeDecimalTypeMapping DecimalAlias
        = new("placeholder", precision: 18, scale: 2, storeTypePostfix: StoreTypePostfix.None);

    private static readonly SnowflakeDecimalTypeMapping Money
        = new("money", DbType.Currency, sqlDbType: SqlDbType.Money, storeTypePostfix: StoreTypePostfix.None);

    private static readonly SnowflakeDecimalTypeMapping SmallMoney
        = new("smallmoney", DbType.Currency, sqlDbType: SqlDbType.SmallMoney, storeTypePostfix: StoreTypePostfix.None);

    private static readonly SnowflakeTimeOnlyTypeMapping TimeAlias
        = new("placeholder", StoreTypePostfix.None);

    private static readonly GuidTypeMapping Uniqueidentifier
        = new("string(36)");

    private static readonly SnowflakeStringTypeMapping Xml
        = new("xml", unicode: true, storeTypePostfix: StoreTypePostfix.None);

    private static readonly Dictionary<Type, RelationalTypeMapping> _clrTypeMappings;

    private static readonly Dictionary<Type, RelationalTypeMapping> _clrNoFacetTypeMappings;

    private static readonly Dictionary<string, RelationalTypeMapping[]> _storeTypeMappings;

    static SnowflakeTypeMappingSource()
    {
        _clrTypeMappings
            = new Dictionary<Type, RelationalTypeMapping>
            {
                { typeof(int), IntTypeMapping.Default },
                { typeof(long), SnowflakeLongTypeMapping.Default },
                { typeof(DateOnly), SnowflakeDateOnlyTypeMapping.Default },
                { typeof(DateTime), TimestampNtz },
                { typeof(Guid), Uniqueidentifier },
                { typeof(bool), SnowflakeBoolTypeMapping.Default },
                { typeof(byte), SnowflakeByteTypeMapping.Default },
                { typeof(double), SnowflakeDoubleTypeMapping.Default },
                { typeof(DateTimeOffset), TimestampTz },
                { typeof(short), SnowflakeShortTypeMapping.Default },
                { typeof(float), SnowflakeFloatTypeMapping.Default },
                { typeof(decimal), SnowflakeDecimalTypeMapping.Default },
                { typeof(TimeOnly), SnowflakeTimeOnlyTypeMapping.Default },
                { typeof(TimeSpan), SnowflakeTimeSpanTypeMapping.Default },
                { typeof(JsonElement), SnowflakeJsonTypeMapping.Default }
            };

        _clrNoFacetTypeMappings
            = new Dictionary<Type, RelationalTypeMapping>
            {
                { typeof(DateTime), Datetime2Alias },
                { typeof(DateTimeOffset), DatetimeoffsetAlias },
                { typeof(TimeOnly), TimeAlias },
                { typeof(double), DoubleAlias },
                { typeof(float), RealAlias },
                { typeof(decimal), DecimalAlias }
            };

        _storeTypeMappings
            = new Dictionary<string, RelationalTypeMapping[]>(StringComparer.OrdinalIgnoreCase)
            {
                { "bigint", [SnowflakeLongTypeMapping.Default] },
                { "binary varying", [SnowflakeByteArrayTypeMapping.Default] },
                { "binary", [FixedLengthBinary] },
                { "boolean", [SnowflakeBoolTypeMapping.Default] },
                { "char varying", [SnowflakeStringTypeMapping.Default] },
                { "char varying(max)", [VariableLengthMaxAnsiString] },
                { "char", [FixedLengthAnsiString] },
                { "character varying", [SnowflakeStringTypeMapping.Default] },
                { "character varying(max)", [VariableLengthMaxAnsiString] },
                { "character", [FixedLengthAnsiString] },
                { "decimal", [Decimal] },
                { "double precision", [SnowflakeDoubleTypeMapping.Default] },
                { "float", [SnowflakeDoubleTypeMapping.Default] },
                { "image", [ImageBinary] },
                { "int", [IntTypeMapping.Default] },
                { "money", [Money] },
                { "national char varying", [VariableLengthUnicodeString] },
                { "national char varying(max)", [VariableLengthMaxUnicodeString] },
                { "national character varying", [VariableLengthUnicodeString] },
                { "national character varying(max)", [VariableLengthMaxUnicodeString] },
                { "national character", [FixedLengthUnicodeString] },
                { "nchar", [FixedLengthUnicodeString] },
                { "ntext", [TextUnicodeString] },
                { "number", [Decimal] },
                { "numeric", [Decimal] },
                { "nvarchar", [VariableLengthMaxUnicodeString] },
                { "real", [SnowflakeFloatTypeMapping.Default] },
                { "rowversion", [Rowversion] },
                { "smallint", [SnowflakeShortTypeMapping.Default] },
                { "smallmoney", [SmallMoney] },
                { "text", [TextAnsiString] },
                { "timestamp", [Rowversion] },
                { "tinyint", [SnowflakeByteTypeMapping.Default] },
                { "varchar(36)", [Uniqueidentifier] }, // TODO:
                { "varbinary", [SnowflakeByteArrayTypeMapping.Default] },
                { "varchar", [VariableLengthMaxAnsiString] },
                { "xml", [Xml] },

                // Date & Time Data Types
                { "date", [DateAsDateTime] },
                { "time", [Time] },
                { "datetime", [Datetime] },
                { "timestamp_ntz", [TimestampNtz] },
                { "timestamp_ltz", [TimestampLtz] },
                { "timestamp_tz", [TimestampTz] },

                // Semi-Structured Data Types
                { "variant", [SnowflakeVariantTypeMapping.Default] },
            };
    }

    /// <summary>
    /// Creates a new instance of <see cref="SnowflakeTypeMappingSource" />
    /// </summary>
    /// <param name="dependencies"></param>
    /// <param name="relationalDependencies"></param>
    public SnowflakeTypeMappingSource(
        TypeMappingSourceDependencies dependencies,
        RelationalTypeMappingSourceDependencies relationalDependencies)
        : base(dependencies, relationalDependencies)
    {
    }

    /// <inheritdoc />
    protected override RelationalTypeMapping? FindMapping(in RelationalTypeMappingInfo mappingInfo)
        => base.FindMapping(mappingInfo)
           ?? FindRawMapping(mappingInfo)?.WithTypeMappingInfo(mappingInfo);

    private RelationalTypeMapping? FindRawMapping(RelationalTypeMappingInfo mappingInfo)
    {
        var clrType = mappingInfo.ClrType;
        var storeTypeName = mappingInfo.StoreTypeName;

        if (storeTypeName != null)
        {
            var storeTypeNameBase = mappingInfo.StoreTypeNameBase;
            if (storeTypeNameBase!.StartsWith("[", StringComparison.Ordinal)
                && storeTypeNameBase.EndsWith("]", StringComparison.Ordinal))
            {
                storeTypeNameBase = storeTypeNameBase[1..^1];
            }

            if (clrType == typeof(float)
                && mappingInfo.Precision is <= 24
                && (storeTypeNameBase.Equals("float", StringComparison.OrdinalIgnoreCase)
                    || storeTypeNameBase.Equals("double precision", StringComparison.OrdinalIgnoreCase)))
            {
                return SnowflakeFloatTypeMapping.Default;
            }

            if (_storeTypeMappings.TryGetValue(storeTypeName, out var mappings)
                || _storeTypeMappings.TryGetValue(storeTypeNameBase, out mappings))
            {
                // We found the user-specified store type. No CLR type was provided - we're probably scaffolding from an existing database,
                // take the first mapping as the default.
                if (clrType is null)
                {
                    return mappings[0];
                }

                // A CLR type was provided - look for a mapping between the store and CLR types. If not found, fail
                // immediately.
                foreach (var m in mappings)
                {
                    if (m.ClrType == clrType)
                    {
                        return m;
                    }
                }

                return null;
            }

            // Snowflake supports aliases (e.g. CREATE TYPE datetimeAlias FROM datetime2(6))
            // Since we don't know the store name above, usually we end up in the clrType-only lookup below and everything goes well.
            // However, when a facet is specified (length/precision/scale), that facet would get appended to the store type; we don't want
            // this in the case of aliased types, since the facet is already part of the type. So we check whether the CLR type supports
            // facets, and return a special type mapping that doesn't support facets.
            if (clrType != null
                && _clrNoFacetTypeMappings.TryGetValue(clrType, out var mapping))
            {
                return mapping;
            }
        }

        if (clrType != null)
        {
            if (_clrTypeMappings.TryGetValue(clrType, out var mapping))
            {
                return mapping;
            }

            if (clrType == typeof(ulong) && mappingInfo.IsRowVersion == true)
            {
                return UlongRowversion;
            }

            if (clrType == typeof(long) && mappingInfo.IsRowVersion == true)
            {
                return LongRowversion;
            }

            if (clrType == typeof(string))
            {
                var isAnsi = mappingInfo.IsUnicode == false;
                var isFixedLength = mappingInfo.IsFixedLength == true;
                var maxSize = isAnsi ? 8000 : 4000;

                var size = mappingInfo.Size ?? (mappingInfo.IsKeyOrIndex ? isAnsi ? 900 : 450 : null);
                if (size < 0 || size > maxSize)
                {
                    size = isFixedLength ? maxSize : null;
                }

                if (size == null
                    && storeTypeName == null
                    && !mappingInfo.IsKeyOrIndex)
                {
                    return SnowflakeString;
                }

                return new SnowflakeStringTypeMapping(
                    unicode: !isAnsi,
                    size: size,
                    fixedLength: isFixedLength,
                    storeTypePostfix: storeTypeName == null ? StoreTypePostfix.Size : StoreTypePostfix.None,
                    useKeyComparison: UseOldBehavior32898 ? mappingInfo.IsKeyOrIndex : mappingInfo.IsKey);
            }

            if (clrType == typeof(byte[]))
            {
                if (mappingInfo.IsRowVersion == true)
                {
                    return Rowversion;
                }

                if (mappingInfo.ElementTypeMapping == null)
                {
                    var isFixedLength = mappingInfo.IsFixedLength == true;

                    var size = mappingInfo.Size ?? (mappingInfo.IsKeyOrIndex ? 900 : null);
                    if (size is < 0 or > 8000)
                    {
                        size = isFixedLength ? 8000 : null;
                    }

                    return size == null
                        ? VariableLengthMaxBinary
                        : new SnowflakeByteArrayTypeMapping(
                            size: size,
                            fixedLength: isFixedLength,
                            storeTypePostfix: storeTypeName == null ? StoreTypePostfix.Size : StoreTypePostfix.None);
                }
            }
        }

        return null;
    }

    private static readonly List<string> NameBasesUsingPrecision =
    [
        "decimal",
        "dec",
        "number",
        "datetime",
        "time",
        "timestamp_ntz",
        "timestamp_ltz",
        "timestamp_tz"
    ];

    /// <inheritdoc />
    protected override string? ParseStoreTypeName(
        string? storeTypeName,
        ref bool? unicode,
        ref int? size,
        ref int? precision,
        ref int? scale)
    {
        if (storeTypeName == null)
        {
            return null;
        }

        var originalSize = size;
        var parsedName = base.ParseStoreTypeName(storeTypeName, ref unicode, ref size, ref precision, ref scale);

        if (size.HasValue
            && NameBasesUsingPrecision.Any(n => storeTypeName.StartsWith(n, StringComparison.OrdinalIgnoreCase)))
        {
            precision = size;
            size = originalSize;
        }
        else if (storeTypeName.Trim().EndsWith("(max)", StringComparison.OrdinalIgnoreCase))
        {
            size = -1;
        }

        return parsedName;
    }
}