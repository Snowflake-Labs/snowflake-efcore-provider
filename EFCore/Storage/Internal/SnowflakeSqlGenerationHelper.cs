using System;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore.Storage;
using Snowflake.EntityFrameworkCore.Internal;
using Snowflake.EntityFrameworkCore.Utilities;

namespace Snowflake.EntityFrameworkCore.Storage.Internal;

/// <summary>
/// Snowflake specific SQL generation helper
/// </summary>
public class SnowflakeSqlGenerationHelper : RelationalSqlGenerationHelper
{
    /// <summary>
    /// Creates a new instance of <see cref="SnowflakeSqlGenerationHelper" />
    /// </summary>
    /// <param name="dependencies"></param>
    public SnowflakeSqlGenerationHelper(RelationalSqlGenerationHelperDependencies dependencies)
        : base(dependencies)
    {
    }

    /// <inheritdoc />
    public override string BatchTerminator
        => Environment.NewLine;

    /// <inheritdoc />
    public override string EscapeIdentifier(string identifier)
        => Check.NotEmpty(identifier, nameof(identifier)).Replace("]", "]]");

    /// <inheritdoc />
    public override void EscapeIdentifier(StringBuilder builder, string identifier)
    {
        var initialLength = builder.Length;
        builder.Append(identifier);
        builder.Replace("]", "]]", initialLength, identifier.Length);
    }

    /// <inheritdoc />
    public override string DelimitIdentifier(string identifier)
        => $"\"{EscapeIdentifier(Check.NotEmpty(identifier, nameof(identifier)))}\""; // Interpolation okay; strings

    /// <inheritdoc />
    public override void DelimitIdentifier(StringBuilder builder, string identifier)
    {
        Check.NotEmpty(identifier, nameof(identifier));

        builder.Append('"');
        EscapeIdentifier(builder, identifier);
        builder.Append('"');
    }

    /// <summary>
    /// Formats the given identifiers as a full qualified name and adds quotation to all of them by default.
    /// </summary>
    /// <param name="identifiers">The identifiers list.</param>
    /// <returns>The qualified name generated from the identifiers.</returns>
    public string DelimitIdentifier(string[] identifiers)
    {
        return string.Join('.', identifiers
            .Where(identifier => !string.IsNullOrEmpty(identifier))
            .Select(DelimitIdentifier));
    }

    /// <inheritdoc />
    public override string GenerateCreateSavepointStatement(string name)
        => throw new NotImplementedException();

    /// <inheritdoc />
    public override string GenerateRollbackToSavepointStatement(string name)
        => throw new NotImplementedException();

    /// <inheritdoc />
    public override string GenerateReleaseSavepointStatement(string name)
        => throw new NotSupportedException(SnowflakeStrings.NoSavepointRelease);

    /// <inheritdoc />
    public override string GenerateParameterNamePlaceholder(string name)
        => name.StartsWith('p') ? "?" : ":" + name;

    /// <inheritdoc />
    public override void GenerateParameterNamePlaceholder(StringBuilder builder, string name)
        => builder.Append(name.StartsWith("p") ? "?" : ":" + name);

    /// <inheritdoc />
    public override string GenerateParameterName(string name)
        // => name;
        => $"{TryConvertParameterNameToNumber(name)}";

    /// <inheritdoc />
    public override void GenerateParameterName(StringBuilder builder, string name)
        // => builder.Append(name);
        => builder.Append($"{TryConvertParameterNameToNumber(name)}");

    // INFO: multistatement doesn't support param binding by name in Snowflake
    private string TryConvertParameterNameToNumber(string name)
    {
        Int32 result;
        if (Int32.TryParse(name.Remove(0, 1), out result))
        {
            return (result + 1).ToString();
        }

        return name;
    }
}