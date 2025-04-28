namespace Snowflake.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

public class SnowflakeCreateSchemaOperation : EnsureSchemaOperation
{
    /// <summary>
    ///  The name of the database where the schema belongs.
    /// </summary>
    public virtual string DatabaseName { get; set; } = string.Empty;
}