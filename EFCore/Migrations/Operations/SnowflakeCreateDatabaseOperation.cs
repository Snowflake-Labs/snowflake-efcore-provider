using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace Snowflake.EntityFrameworkCore.Migrations.Operations;

using System.Diagnostics;

/// <summary>
///     A Snowflake-specific <see cref="MigrationOperation" /> to create a database.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-migrations">Database migrations</see>, and
///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
///     for more information and examples.
/// </remarks>
[DebuggerDisplay("CREATE DATABASE {Name}")]
public class SnowflakeCreateDatabaseOperation : DatabaseOperation
{
    /// <summary>
    ///     The name of the database.
    /// </summary>
    public virtual string Name { get; set; } = null!;
}
