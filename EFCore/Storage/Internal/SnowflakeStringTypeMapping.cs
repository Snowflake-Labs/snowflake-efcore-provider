using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data;
using System.Text;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Json;

namespace Snowflake.EntityFrameworkCore.Storage.Internal;

/// <summary>
/// Represents a Snowflake-specific string type mapping.
/// </summary>
public class SnowflakeStringTypeMapping : StringTypeMapping
{
    private const int UnicodeMax = 4000;
    private const int AnsiMax = 8000;

    private static readonly CaseInsensitiveValueComparer CaseInsensitiveValueComparer = new();

    private readonly bool _isUtf16;
    private readonly SqlDbType? _sqlDbType;
    private readonly int _maxSpecificSize;
    private readonly int _maxSize;

    /// <summary>
    /// The default <see cref="SnowflakeStringTypeMapping"/> instance, representing the "varchar" Snowflake data type.
    /// </summary>
    public new static SnowflakeStringTypeMapping Default { get; } = new();

    /// <summary>
    /// Creates a new instance of <see cref="SnowflakeStringTypeMapping" />
    /// </summary>
    /// <param name="storeType"></param>
    /// <param name="unicode"></param>
    /// <param name="size"></param>
    /// <param name="fixedLength"></param>
    /// <param name="sqlDbType"></param>
    /// <param name="storeTypePostfix"></param>
    /// <param name="useKeyComparison"></param>
    public SnowflakeStringTypeMapping(
        string? storeType = null,
        bool unicode = false,
        int? size = null,
        bool fixedLength = false,
        SqlDbType? sqlDbType = null,
        StoreTypePostfix? storeTypePostfix = null,
        bool useKeyComparison = false)
        : this(
            new RelationalTypeMappingParameters(
                new CoreTypeMappingParameters(
                    typeof(string),
                    comparer: useKeyComparison ? CaseInsensitiveValueComparer : null,
                    keyComparer: useKeyComparison ? CaseInsensitiveValueComparer : null,
                    jsonValueReaderWriter: JsonStringReaderWriter.Instance),
                storeType ?? GetDefaultStoreName(unicode, fixedLength),
                storeTypePostfix ?? StoreTypePostfix.Size,
                GetDbType(unicode, fixedLength),
                unicode,
                size is < 0 ? null : size,
                fixedLength),
            sqlDbType)
    {
    }

    private static string GetDefaultStoreName(bool unicode, bool fixedLength)
        => unicode
            ? fixedLength ? "nchar" : "nvarchar"
            : fixedLength
                ? "char"
                : "varchar";

    private static DbType? GetDbType(bool unicode, bool fixedLength)
        => unicode
            ? (fixedLength
                ? System.Data.DbType.StringFixedLength
                : System.Data.DbType.String)
            : (fixedLength
                ? System.Data.DbType.AnsiStringFixedLength
                : System.Data.DbType.AnsiString);

    /// <summary>
    /// Creates a new instance of <see cref="SnowflakeStringTypeMapping" />
    /// </summary>
    /// <param name="parameters"></param>
    /// <param name="sqlDbType"></param>
    protected SnowflakeStringTypeMapping(RelationalTypeMappingParameters parameters, SqlDbType? sqlDbType)
        : base(parameters)
    {
        if (parameters.Unicode)
        {
            _maxSpecificSize = parameters.Size is > 0 and <= UnicodeMax ? parameters.Size.Value : UnicodeMax;
            _maxSize = UnicodeMax;
        }
        else
        {
            _maxSpecificSize = parameters.Size is > 0 and <= AnsiMax ? parameters.Size.Value : AnsiMax;
            _maxSize = AnsiMax;
        }

        _isUtf16 = parameters.Unicode && parameters.StoreType.StartsWith("n", StringComparison.OrdinalIgnoreCase);
        _sqlDbType = sqlDbType;
    }

    /// <inheritdoc />
    protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
    {
        var dbType = parameters.Unicode ? GetDbType(parameters.Unicode, parameters.FixedLength) : parameters.DbType;

        parameters = new RelationalTypeMappingParameters(
            parameters.CoreParameters,
            parameters.StoreType,
            parameters.StoreTypePostfix,
            dbType,
            parameters.Unicode,
            parameters.Size is < 0 ? null : parameters.Size,
            parameters.FixedLength,
            parameters.Precision,
            parameters.Scale);

        return new SnowflakeStringTypeMapping(parameters, _sqlDbType);
    }

    /// <inheritdoc />
    protected override void ConfigureParameter(DbParameter parameter)
    {
        var value = parameter.Value;
        var length = (value as string)?.Length;

        if ((value == null
             || value == DBNull.Value)
            || (IsFixedLength
                && length == _maxSpecificSize
                && Size.HasValue))
        {
            // A fixed-length parameter where the value matches the length can remain a fixed-length parameter
            // because SQLClient will not do any padding or truncating.
            parameter.Size = _maxSpecificSize;
        }
        else
        {
            if (IsFixedLength)
            {
                // Force the parameter type to be not fixed length to avoid SQLClient truncation and padding.
                parameter.DbType = IsUnicode ? System.Data.DbType.String : System.Data.DbType.AnsiString;
            }

            // For strings and byte arrays, set the max length to the size facet if specified, or
            // 8000 bytes if no size facet specified, if the data will fit so as to avoid query cache
            // fragmentation by setting lots of different Size values otherwise set to the max bounded length
            // if the value will fit, otherwise set to -1 (unbounded) to avoid SQL client size inference.
            if (length <= _maxSpecificSize)
            {
                parameter.Size = _maxSpecificSize;
            }
            else if (length <= _maxSize)
            {
                parameter.Size = _maxSize;
            }
            else
            {
                parameter.Size = -1;
            }
        }
    }

    /// <inheritdoc />
    protected override string GenerateNonNullSqlLiteral(object value)
    {
        var stringValue = (string)value;
        var builder = new StringBuilder();

        var start = 0;
        int i;
        int length;
        var openApostrophe = false;
        var lastConcatStartPoint = 0;
        var concatCount = 1;
        var concatStartList = new List<int>();
        var insideConcat = false;
        for (i = 0; i < stringValue.Length; i++)
        {
            var lineFeed = stringValue[i] == '\n';
            var carriageReturn = stringValue[i] == '\r';
            var apostrophe = stringValue[i] == '\'';
            if (lineFeed || carriageReturn || apostrophe)
            {
                length = i - start;
                if (length != 0)
                {
                    if (!openApostrophe)
                    {
                        AddConcatOperatorIfNeeded();

                        builder.Append('\'');
                        openApostrophe = true;
                    }

                    builder.Append(stringValue.AsSpan().Slice(start, length));
                }

                if (lineFeed || carriageReturn)
                {
                    if (openApostrophe)
                    {
                        builder.Append('\'');
                        openApostrophe = false;
                    }

                    AddConcatOperatorIfNeeded();

                    builder
                        .Append("char(")
                        .Append(lineFeed ? "10" : "13")
                        .Append(')');
                }
                else if (apostrophe)
                {
                    if (!openApostrophe)
                    {
                        AddConcatOperatorIfNeeded();

                        builder.Append('\'');
                        openApostrophe = true;
                    }

                    builder.Append("''");
                }

                start = i + 1;
            }
        }

        length = i - start;
        if (length != 0)
        {
            if (!openApostrophe)
            {
                AddConcatOperatorIfNeeded();

                builder.Append('\'');
                openApostrophe = true;
            }

            builder.Append(stringValue.AsSpan().Slice(start, length));
        }

        if (openApostrophe)
        {
            builder.Append('\'');
        }

        for (var j = concatStartList.Count - 1; j >= 0; j--)
        {
            builder.Insert(concatStartList[j], "CONCAT(CAST(");
            builder.Append(')');
        }

        if (builder.Length == 0)
        {
            builder.Append("''");
        }

        return builder.ToString();

        void AddConcatOperatorIfNeeded()
        {
            if (builder.Length != 0)
            {
                if (!insideConcat)
                {
                    builder.Append(" AS ");

                    builder.Append("varchar(max))");
                    insideConcat = true;
                }

                builder.Append(", ");
                concatCount++;

                if (concatCount == 2)
                {
                    concatStartList.Add(lastConcatStartPoint);
                }

                if (concatCount == 254)
                {
                    lastConcatStartPoint = builder.Length;
                    concatCount = 1;
                    insideConcat = false;
                }
            }
        }
    }
}