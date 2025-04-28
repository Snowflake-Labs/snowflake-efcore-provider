namespace Snowflake.EntityFrameworkCore.Migrations.Internal;
using Microsoft.EntityFrameworkCore.Metadata;

/// <summary>
/// Validation interface for hybrid tables.
/// </summary>
public interface ISnowflakeHybridTableValidator
{
    /// <summary>
    /// Gets a value indicating whether a table is hybrid or not.
    /// </summary>
    /// <param name="tableName">The table name.</param>
    /// <param name="schema">The schema to search the table.</param>
    /// <param name="model">The database model.</param>
    /// <param name="table">The found table.</param>
    /// <returns>True if the table is hybrid, otherwise false.</returns>
    bool IsHybridTable(string tableName, string schema, IModel model, out ITable? table);
}