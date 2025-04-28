namespace Snowflake.EntityFrameworkCore.Migrations.Internal;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Snowflake.EntityFrameworkCore.Metadata;
using Snowflake.EntityFrameworkCore.Metadata.Internal;

/// <inheritdoc />
public class SnowflakeIdentityValidator : ISnowflakeIdentityValidator
{
    /// <inheritdoc />
    public bool IsIdentity(ColumnOperation operation)
        => operation[SnowflakeAnnotationNames.Identity] != null
           || operation[SnowflakeAnnotationNames.ValueGenerationStrategy] as SnowflakeValueGenerationStrategy?
           == SnowflakeValueGenerationStrategy.IdentityColumn;
}