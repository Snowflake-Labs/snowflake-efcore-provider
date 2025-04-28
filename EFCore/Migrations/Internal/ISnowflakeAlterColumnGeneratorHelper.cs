namespace Snowflake.EntityFrameworkCore.Migrations.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

/// <summary>
/// Provides utility methods to emit ALTER COLUMN code.
/// </summary>
public interface ISnowflakeAlterColumnGeneratorHelper
{
    /// <summary>
    /// Defines a delegate type that should match a GetColumnType method signature.
    /// </summary>
    delegate string ColumnTypeDelegate(
        string schema, 
        string tableName, 
        string name, 
        ColumnOperation operation,
        IModel model);

    /// <summary>
    /// Emits code when the identity was altered in a column.
    /// </summary>
    /// <param name="operation">The alter column operation with all the new column information.</param>
    /// <param name="builder">The command builder to emit the sql code.</param>
    /// <param name="hasIdentity">Determines if the changed column had identity or not.</param>
    void OnIdentityChange(AlterColumnOperation operation, MigrationCommandListBuilder builder, bool hasIdentity);

    /// <summary>
    /// Emits code when the nullable constraint is changed in a column.
    /// </summary>
    /// <param name="operation">The alter column operation with all the new column information.</param>
    /// <param name="model">The database model.</param>
    /// <param name="builder">The command builder to emit the sql code.</param>
    /// <param name="columnTypeDelegate">The delegate type.</param>
    void OnNullConstraintChange(
        AlterColumnOperation operation,
        IModel model,
        MigrationCommandListBuilder builder,
        ColumnTypeDelegate columnTypeDelegate);

    /// <summary>
    /// Emits code when a column type is changed.
    /// </summary>
    /// <param name="operation">The alter column operation with all the new column information.</param>
    /// <param name="builder">The command builder to emit the sql code.</param>
    void OnColumnTypeChange(AlterColumnOperation operation, MigrationCommandListBuilder builder);

    /// <summary>
    /// Outputs a new add and drop column operation when the ALTER COLUMN command
    /// is not supported for the specific action.
    /// </summary>
    /// <param name="operation">The alter column operation with all the new column information.</param>
    /// <param name="model">The database model</param>
    /// <param name="dropColumnOperation">Output an add column operation with the new behaviour.</param>
    /// <param name="addColumnOperation">Output a drop column operation with the old behaviour.</param>
    void DropAndRecreate(
        AlterColumnOperation operation,
        IModel model,
        out DropColumnOperation dropColumnOperation, 
        out AddColumnOperation addColumnOperation);
}