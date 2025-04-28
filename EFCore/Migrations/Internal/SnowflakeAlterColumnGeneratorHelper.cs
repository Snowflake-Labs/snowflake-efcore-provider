
namespace Snowflake.EntityFrameworkCore.Migrations.Internal;

using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Storage;
using Snowflake.EntityFrameworkCore.Internal;

/// <inheritdoc />
public class SnowflakeAlterColumnGeneratorHelper : ISnowflakeAlterColumnGeneratorHelper
{
    private readonly IRelationalTypeMappingSource _typeMappingSource;
    private readonly ISqlGenerationHelper _sqlGenerationHelper;
    private readonly ISnowflakeHybridTableValidator _hybridTableValidator;


    /// <summary>
    /// Generates a new instance of the <see cref="SnowflakeAlterColumnGeneratorHelper"/> class.
    /// </summary>
    /// <param name="typeMappingSource">The type mapping source.</param>
    /// <param name="sqlGenerationHelper">The snowflake sql generation helper.</param>
    /// <param name="hybridTableValidator">The hybrid table validator.</param>
    public SnowflakeAlterColumnGeneratorHelper(
        IRelationalTypeMappingSource typeMappingSource,
        ISqlGenerationHelper sqlGenerationHelper,
        ISnowflakeHybridTableValidator hybridTableValidator)
    {
        _typeMappingSource = typeMappingSource;
        _sqlGenerationHelper = sqlGenerationHelper;
        _hybridTableValidator = hybridTableValidator;
    }

    /// <inheritdoc />
    public void OnIdentityChange(
        AlterColumnOperation operation,
        MigrationCommandListBuilder builder,
        bool hasIdentity)
    {
        if (hasIdentity)
        {
            builder
                .Append("ALTER TABLE ")
                .Append(this._sqlGenerationHelper.DelimitIdentifier(operation.Table, operation.Schema))
                .Append(" ALTER COLUMN ")
                .Append(this._sqlGenerationHelper.DelimitIdentifier(operation.Name))
                .Append(" DROP DEFAULT")
                .Append(this._sqlGenerationHelper.StatementTerminator)
                // TODO Review suppress transaction default value true or false
                .EndCommand(suppressTransaction: false);    
        }
        else
        {
            throw new NotSupportedException(SnowflakeStrings.AlterIdentityColumn);    
        }
    }

    /// <inheritdoc />
    public void OnNullConstraintChange(
        AlterColumnOperation operation,
        IModel model,
        MigrationCommandListBuilder builder,
        ISnowflakeAlterColumnGeneratorHelper.ColumnTypeDelegate columnTypeDelegate)
    {
        var defaultValueNotNull = operation.DefaultValueSql is not null || operation.DefaultValue is not null;

        if (!operation.IsNullable && defaultValueNotNull)
        {
            var defaultValue = this.GetDefaultValueForNonNullColumns(operation, model, columnTypeDelegate);
            
            builder
                .Append("UPDATE ")
                .Append(this._sqlGenerationHelper.DelimitIdentifier(operation.Table, operation.Schema))
                .Append(" SET ")
                .Append(this._sqlGenerationHelper.DelimitIdentifier(operation.Name))
                .Append(" = ")
                .Append(defaultValue)
                .Append(" WHERE ")
                .Append(this._sqlGenerationHelper.DelimitIdentifier(operation.Name))
                .Append(" IS NULL")
                .AppendLine(this._sqlGenerationHelper.StatementTerminator)
                // TODO Review suppress transaction default value true or false
                .EndCommand(suppressTransaction: false);
        }

        builder
            .Append("ALTER TABLE ")
            .Append(this._sqlGenerationHelper.DelimitIdentifier(operation.Table, operation.Schema))
            .Append(" ALTER COLUMN ")
            .Append(this._sqlGenerationHelper.DelimitIdentifier(operation.Name))
            .Append(operation.IsNullable ? " DROP NOT NULL" : " SET NOT NULL")
            .Append(this._sqlGenerationHelper.StatementTerminator)
            // TODO Review suppress transaction default value true or false
            .EndCommand(suppressTransaction: false);
    }

    /// <inheritdoc />
    public void OnColumnTypeChange(AlterColumnOperation operation, MigrationCommandListBuilder builder)
    {
        if (operation.ColumnType == null)
        {
            throw new InvalidOperationException(SnowflakeStrings.AlterColumnType);
        }

        builder
            .Append("ALTER TABLE ")
            .Append(this._sqlGenerationHelper.DelimitIdentifier(operation.Table, operation.Schema))
            .Append(" ALTER COLUMN ")
            .Append(this._sqlGenerationHelper.DelimitIdentifier(operation.Name))
            .Append(" SET DATA TYPE ")
            .Append(operation.ColumnType)
            .Append(this._sqlGenerationHelper.StatementTerminator)
            // TODO Review suppress transaction default value true or false
            .EndCommand(suppressTransaction: false);
    }

    /// <inheritdoc />
    public void DropAndRecreate(
        AlterColumnOperation operation,
        IModel model,
        out DropColumnOperation dropColumnOperation,
        out AddColumnOperation addColumnOperation)
    {
        dropColumnOperation = new DropColumnOperation
        {
            Schema = operation.Schema,
            Table = operation.Table,
            Name = operation.Name
        };

        var isHybridTable =
            this._hybridTableValidator.IsHybridTable(operation.Table, operation.Schema, model, out var table);
        var isPrimaryKey = table?.PrimaryKey?.Columns.Any(c => c.Name == operation.Name);

        if (isHybridTable && isPrimaryKey == true)
        {
            throw new InvalidOperationException("Primary key cannot be removed on Hybrid Tables.");
        }

        if (table?.Columns.Count() < 2)
        {
            throw new InvalidOperationException(
                "There should be at least 2 columns before drop and recreate. Table must not have no columns.");
        }

        var column = table?.Columns
            .FirstOrDefault(c => c.Name == operation.Name);

        if (column != null)
        {
            dropColumnOperation.AddAnnotations(column.GetAnnotations());
        }

        addColumnOperation = new AddColumnOperation
        {
            Schema = operation.Schema,
            Table = operation.Table,
            Name = operation.Name,
            ClrType = operation.ClrType,
            ColumnType = operation.ColumnType,
            IsUnicode = operation.IsUnicode,
            IsFixedLength = operation.IsFixedLength,
            MaxLength = operation.MaxLength,
            Precision = operation.Precision,
            Scale = operation.Scale,
            IsRowVersion = operation.IsRowVersion,
            IsNullable = operation.IsNullable,
            DefaultValue = operation.DefaultValue,
            DefaultValueSql = operation.DefaultValueSql,
            ComputedColumnSql = operation.ComputedColumnSql,
            IsStored = operation.IsStored,
            Comment = operation.Comment,
            Collation = operation.Collation
        };

        addColumnOperation.AddAnnotations(operation.GetAnnotations());
    }

    private string GetDefaultValueForNonNullColumns(
        AlterColumnOperation operation,
        IModel model,
        ISnowflakeAlterColumnGeneratorHelper.ColumnTypeDelegate columnTypeDelegate)
    {
        if (operation.DefaultValueSql != null)
        {
            return operation.DefaultValueSql;
        }

        if (operation.DefaultValue == null)
        {
            throw new InvalidOperationException(
                $"{nameof(ColumnOperation.DefaultValue)} cannot be null.");
        }

        var type = operation.ColumnType ??
                   columnTypeDelegate(operation.Schema, operation.Table, operation.Name, operation, model);

        var typeMapping = this.GetTypeMapping(operation.DefaultValue, type);
        return typeMapping.GenerateSqlLiteral(operation.DefaultValue);
    }

    private RelationalTypeMapping GetTypeMapping(object defaultValue, string type)
    {
        var typeMapping = type != null 
            ? this._typeMappingSource.FindMapping(defaultValue.GetType(), type) 
            : null;
        
        return typeMapping ?? this._typeMappingSource.GetMappingForValue(defaultValue);
    }
}