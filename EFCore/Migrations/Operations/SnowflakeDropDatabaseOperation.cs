using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace Snowflake.EntityFrameworkCore.Migrations.Operations;

/// <summary>
///     A Snowflake-specific <see cref="MigrationOperation" /> to drop a database.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-migrations">Database migrations</see>, and
///     <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
///     for more information and examples.
/// </remarks>
public class SnowflakeDropDatabaseOperation : MigrationOperation
{
    /// <summary>
    ///     The name of the database.
    /// </summary>
    public virtual string Name { get; set; } = null!;
    
    /// <summary>
    /// Gets a value indicating whether all objects in the database, including tables with primary/unique keys
    /// that are referenced by foreign keys in other tables, should be dropped as well.
    /// </summary>
    public bool Cascade { get; set; } = true;
}
