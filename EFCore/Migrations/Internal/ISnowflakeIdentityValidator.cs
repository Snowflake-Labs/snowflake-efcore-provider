namespace Snowflake.EntityFrameworkCore.Migrations.Internal;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

/// <summary>
/// Validates identity for columns.
/// </summary>
public interface ISnowflakeIdentityValidator
{
    /// <summary>
    /// Get whether a column has identity.
    /// </summary>
    /// <param name="operation">The column operation.</param>
    /// <returns>True if the column has identity; otherwise false.</returns>
    bool IsIdentity(ColumnOperation operation);
}