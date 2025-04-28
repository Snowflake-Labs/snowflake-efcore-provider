namespace Snowflake.EntityFrameworkCore.Migrations.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore;
using Snowflake.EntityFrameworkCore.Metadata.Internal;

public class SnowflakeHybridTableValidator : ISnowflakeHybridTableValidator
{
    /// <inheritdoc />
    public bool IsHybridTable(string tableName, string schema, IModel model, out ITable table)
    {
        table = model?
            .GetRelationalModel()
            .FindTable(tableName, schema);

        return table?.FindAnnotation(SnowflakeAnnotationNames.HybridTable) != null;
    }
}