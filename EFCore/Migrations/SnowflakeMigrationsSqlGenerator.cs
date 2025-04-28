using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Snowflake.EntityFrameworkCore.Extensions.Internal;
using Snowflake.EntityFrameworkCore.Internal;
using Snowflake.EntityFrameworkCore.Metadata;
using Snowflake.EntityFrameworkCore.Metadata.Internal;
using Snowflake.EntityFrameworkCore.Migrations.Internal;
using Snowflake.EntityFrameworkCore.Migrations.Operations;
using Snowflake.EntityFrameworkCore.Storage.Internal;
using Snowflake.EntityFrameworkCore.Update.Internal;
using Snowflake.EntityFrameworkCore.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Snowflake.EntityFrameworkCore.Migrations;

/// <summary>
///     Snowflake-specific implementation of <see cref="MigrationsSqlGenerator" />.
/// </summary>
/// <remarks>
///     <para>
///         The service lifetime is <see cref="ServiceLifetime.Scoped" />. This means that each
///         <see cref="DbContext" /> instance will use its own instance of this service.
///         The implementation may depend on other services registered with any lifetime.
///         The implementation does not need to be thread-safe.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-migrations">Database migrations</see>, and
///         <see href="https://aka.ms/efcore-docs-Snowflake">Accessing Snowflake and Azure SQL databases with EF Core</see>
///         for more information and examples.
///     </para>
/// </remarks>
public class SnowflakeMigrationsSqlGenerator : MigrationsSqlGenerator
{
    private const string ColumnType = "COLUMN";
    private const string TableType = "TABLE";
    private const string SequenceType = "SEQUENCE";
    private readonly IDiagnosticsLogger<DbLoggerCategory.Migrations> _logger;
    private readonly ISnowflakeConnection _snowflakeConnection;
    private readonly ICommandBatchPreparer _commandBatchPreparer;
    private readonly ISnowflakeAlterColumnGeneratorHelper _alterColumnGeneratorHelper;
    private readonly ISnowflakeHybridTableValidator _hybridTableValidator;
    private readonly ISnowflakeIdentityValidator _identityValidator;
    private IReadOnlyList<MigrationOperation> _operations = null!;
    private int _variableCounter;
    private readonly ISet<string> _temporalDataTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "date", "datetime", "time", "timestamp", "timestamp_ntz", "timestamp_ltz", "timestamp_tz" };

    /// <summary>
    ///     Creates a new <see cref="SnowflakeMigrationsSqlGenerator" /> instance.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this service.</param>
    /// <param name="commandBatchPreparer">The command batch preparer.</param>
    /// <param name="alterColumnGeneratorHelper"></param>
    /// <param name="hybridTableValidator">The hybrid table validator.</param>
    /// <param name="identityValidator">The identity validator.</param>
    /// <param name="logger">The migrations logger.</param>
    /// <param name="snowflakeConnection">The snowflake connection instance.</param>
    public SnowflakeMigrationsSqlGenerator(
        MigrationsSqlGeneratorDependencies dependencies,
        ICommandBatchPreparer commandBatchPreparer,
        ISnowflakeAlterColumnGeneratorHelper alterColumnGeneratorHelper,
        ISnowflakeHybridTableValidator hybridTableValidator,
        ISnowflakeIdentityValidator identityValidator,
        IDiagnosticsLogger<DbLoggerCategory.Migrations> logger,
        ISnowflakeConnection snowflakeConnection)
        : base(dependencies)
    {
        _commandBatchPreparer = commandBatchPreparer;
        _alterColumnGeneratorHelper = alterColumnGeneratorHelper;
        _hybridTableValidator = hybridTableValidator;
        _identityValidator = identityValidator;
        _logger = logger;
        _snowflakeConnection = snowflakeConnection;
    }

    private ISqlGenerationHelper SqlGenerationHelper => Dependencies.SqlGenerationHelper;

    /// <summary>
    ///     Generates commands from a list of operations.
    /// </summary>
    /// <param name="operations">The operations.</param>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <param name="options">The options to use when generating commands.</param>
    /// <returns>The list of commands to be executed or scripted.</returns>
    public override IReadOnlyList<MigrationCommand> Generate(
        IReadOnlyList<MigrationOperation> operations,
        IModel? model = null,
        MigrationsSqlGenerationOptions options = MigrationsSqlGenerationOptions.Default)
    {
        _operations = operations;
        try
        {
            // Note: output is a list of DML commands to be executed 
            return base.Generate(RewriteOperations(operations, model, options), model, options);
        }
        finally
        {
            _operations = null!;
        }
    }

    /// <summary>
    ///     Builds commands for the given <see cref="MigrationOperation" /> by making calls on the given
    ///     <see cref="MigrationCommandListBuilder" />.
    /// </summary>
    /// <remarks>
    ///     This method uses a double-dispatch mechanism to call the <see cref="O:MigrationsSqlGenerator.Generate" /> method
    ///     that is specific to a certain subtype of <see cref="MigrationOperation" />. Typically database providers
    ///     will override these specific methods rather than this method. However, providers can override
    ///     this methods to handle provider-specific operations.
    /// </remarks>
    /// <param name="operation">The operation.</param>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <param name="builder">The command builder to use to build the commands.</param>
    protected override void Generate(MigrationOperation operation, IModel? model, MigrationCommandListBuilder builder)
    {
        switch (operation)
        {
            case SnowflakeCreateDatabaseOperation createDatabaseOperation:
                Generate(createDatabaseOperation, model, builder);
                break;
            case SnowflakeDropDatabaseOperation dropDatabaseOperation:
                Generate(dropDatabaseOperation, model, builder);
                break;
            case SnowflakeCreateSchemaOperation createSchemaOperation:
                Generate(createSchemaOperation, model, builder);
                break;
            default:
                base.Generate(operation, model, builder);
                break;
        }
    }

    /// <inheritdoc />
    protected override void Generate(
        AddCheckConstraintOperation operation,
        IModel? model,
        MigrationCommandListBuilder builder)
    {
        throw new NotSupportedException(SnowflakeStrings.AddCheckConstraintNotSupported(operation.Name, operation.Table));
    }

    /// <inheritdoc />
    protected override void Generate(DropCheckConstraintOperation operation, IModel? model,
        MigrationCommandListBuilder builder)
    {
        throw new NotSupportedException(SnowflakeStrings.DropCheckConstraintNotSupported(operation.Name, operation.Table));
    }

    /// <summary>
    ///     Builds commands for the given <see cref="AddForeignKeyOperation" /> by making calls on the given
    ///     <see cref="MigrationCommandListBuilder" />.
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <param name="builder">The command builder to use to build the commands.</param>
    /// <param name="terminate">Indicates whether or not to terminate the command after generating SQL for the operation.</param>
    protected override void Generate(
        AddForeignKeyOperation operation,
        IModel? model,
        MigrationCommandListBuilder builder,
        bool terminate = true)
    {
        if (this._hybridTableValidator.IsHybridTable(operation.Table, operation.Schema, model, out var table))
        {
            throw new InvalidOperationException(SnowflakeStrings.ConstraintOperationNotSupported(nameof(AddForeignKeyOperation), operation.Table));
        }
        
        base.Generate(operation, model, builder, terminate: false);

        if (terminate)
        {
            builder
                .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator)
                .EndCommand(
                    // TODO Review suppress transaction default value true or false
                    suppressTransaction:
                    false);
        }
    }

    protected override void ForeignKeyConstraint(AddForeignKeyOperation operation, IModel model, MigrationCommandListBuilder builder)
    {
        if (operation.OnDelete != ReferentialAction.NoAction)
        {
            _logger.MigrationOperationOptionNotSupported(nameof(operation.OnDelete), nameof(AddForeignKeyOperation));
        }
        
        if (operation.OnUpdate != ReferentialAction.NoAction)
        {
            _logger.MigrationOperationOptionNotSupported(nameof(operation.OnUpdate), nameof(AddForeignKeyOperation));
        }
        base.ForeignKeyConstraint(operation, model, builder);
    }
    
    /// <inheritdoc />
    protected override void Generate(AddUniqueConstraintOperation operation, IModel model, MigrationCommandListBuilder builder)
    {
        
        if (_hybridTableValidator.IsHybridTable(operation.Table, operation.Schema, model, out _))
        {
            throw new NotSupportedException(SnowflakeStrings.ConstraintOperationNotSupported(nameof(AddUniqueConstraintOperation), operation.Table));
        }

        base.Generate(operation, model, builder);
        this.Dependencies.Logger.Logger.LogWarning(SnowflakeStrings.UniqueConstraintNotCreated);
    }

    /// <summary>
    ///     Builds commands for the given <see cref="AddPrimaryKeyOperation" /> by making calls on the given
    ///     <see cref="MigrationCommandListBuilder" />.
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <param name="builder">The command builder to use to build the commands.</param>
    /// <param name="terminate">Indicates whether or not to terminate the command after generating SQL for the operation.</param>
    protected override void Generate(
        AddPrimaryKeyOperation operation,
        IModel? model,
        MigrationCommandListBuilder builder,
        bool terminate = true)
    {
        if (this._hybridTableValidator.IsHybridTable(operation.Table, operation.Schema, model, out _))
        {
            throw new InvalidOperationException(SnowflakeStrings.ConstraintOperationNotSupported(nameof(AddPrimaryKeyOperation), operation.Table));
        }

        base.Generate(operation, model, builder, terminate: false);

        if (terminate)
        {
            builder
                .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator)
                .EndCommand(
                    // TODO Review suppress transaction default value true or false
                    suppressTransaction: false);
        }
    }

    /// <summary>
    ///     Builds commands for the given <see cref="AlterColumnOperation" />
    ///     by making calls on the given <see cref="MigrationCommandListBuilder" />.
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <param name="builder">The command builder to use to build the commands.</param>
    protected override void Generate(
        AlterColumnOperation operation,
        IModel? model,
        MigrationCommandListBuilder builder)
    {
        if (operation.Collation != operation.OldColumn.Collation)
        {
            throw new NotSupportedException(SnowflakeStrings.AlterCollationColumn);
        }

        var oldColumnHasIdentity = this._identityValidator.IsIdentity(operation.OldColumn);
        var newColumnHasIdentity = this._identityValidator.IsIdentity(operation);

        if (oldColumnHasIdentity != newColumnHasIdentity)
        {
            this._alterColumnGeneratorHelper.OnIdentityChange(operation, builder, oldColumnHasIdentity);
        }

        if (operation.ComputedColumnSql != operation.OldColumn.ComputedColumnSql)
        {
            this._alterColumnGeneratorHelper.DropAndRecreate(operation, model, out var dropColumnOperation,
                out var addColumnOperation);
            Generate(dropColumnOperation, model, builder);
            Generate(addColumnOperation, model, builder);
            return;
        }

        if (operation.IsNullable != operation.OldColumn.IsNullable)
        {
            this._alterColumnGeneratorHelper.OnNullConstraintChange(operation, model, builder, this.GetColumnType);
        }

        if (operation.ColumnType != operation.OldColumn.ColumnType)
        {
            this._alterColumnGeneratorHelper.OnColumnTypeChange(operation, builder);
        }

        if (operation.Comment == operation.OldColumn.Comment)
        {
            return;
        }

        if (operation.OldColumn.Comment != null)
        {
            this.DropComment(
                builder,
                operation.Table,
                operation.Schema,
                operation.Name);
        }

        if (operation.Comment != null)
        {
            this.AddComment(
                builder, operation.Comment,
                operation.Table,
                ColumnType,
                operation.Schema,
                operation.Name);
        }
    }

    /// <summary>
    ///     Builds commands for the given <see cref="RenameSequenceOperation" />
    ///     by making calls on the given <see cref="MigrationCommandListBuilder" />.
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <param name="builder">The command builder to use to build the commands.</param>
    protected override void Generate(
        RenameSequenceOperation operation, 
        IModel? model,
        MigrationCommandListBuilder builder)
    {
        var name = operation.Name;
        var isUpdatingName = !string.IsNullOrEmpty(operation.NewName) && operation.NewName != name;
        var isUpdatingSchema =  !string.IsNullOrEmpty(operation.NewSchema) && operation.NewSchema != operation.Schema;
        if (isUpdatingSchema)
        {
            this.Transfer(operation.NewSchema, operation.Schema, operation.NewName, name, SequenceType, builder);
        }
        else if (isUpdatingName)
        {
            this.Rename(name, operation.NewName, operation.Schema, SequenceType, builder);
        }
        else
        {
            throw new InvalidOperationException(SnowflakeStrings.InvalidRenameOperation(nameof(operation)));
        }

        builder.EndCommand();
    }

    /// <summary>
    ///     Builds commands for the given <see cref="RestartSequenceOperation" /> by making calls on the given
    ///     <see cref="MigrationCommandListBuilder" />, and then terminates the final command.
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <param name="builder">The command builder to use to build the commands.</param>
    protected override void Generate(
        RestartSequenceOperation operation,
        IModel? model,
        MigrationCommandListBuilder builder)
    {
        throw new NotSupportedException(SnowflakeStrings.RestartSequenceNotSupported);
    }

    /// <summary>
    ///     Builds commands for the given <see cref="CreateTableOperation" /> by making calls on the given
    ///     <see cref="MigrationCommandListBuilder" />.
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <param name="builder">The command builder to use to build the commands.</param>
    /// <param name="terminate">Indicates whether or not to terminate the command after generating SQL for the operation.</param>
    protected override void Generate(
        CreateTableOperation operation,
        IModel? model,
        MigrationCommandListBuilder builder,
        bool terminate = true)
    {
        Check.NotNull(operation, nameof(operation));
        Check.NotNull(builder, nameof(builder));

        var tableType = GetTableType(operation);
        // TODO: add index support
        builder
            .Append($"CREATE ")
            .Append(tableType)
            .Append("TABLE ")
            .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name, operation.Schema))
            .AppendLine(" (");

        using (builder.Indent())
        {
            CreateTableColumns(operation, model, builder);
            CreateTableConstraints(operation, model, builder);
        }

        builder.Append(")");
        
        if (operation.Comment != null)
        {
            builder.AppendLine()
                .Append("COMMENT=")
                .Append($"'{operation.Comment}'");
        }

        if (terminate)
        {
            builder.AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);
            EndStatement(builder, true);
        }
    }

    /// <summary>
    /// Gets the type of the table (e.g. HYBRID, TEMPORARY, etc.)
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <returns>A string that represents the type of the table, ending in whitespace.</returns>
    private string GetTableType(CreateTableOperation operation)
    {
        // Other possibilities are:
        // - TEMPORARY
        // - TRANSIENT
        // - EXTERNAL
        // - ICEBERG
        // - DYNAMIC

        return operation[SnowflakeAnnotationNames.HybridTable] as bool? == true ? "HYBRID " : string.Empty;
    }

    /// <summary>
    ///     Builds commands for the given <see cref="RenameTableOperation" />
    ///     by making calls on the given <see cref="MigrationCommandListBuilder" />.
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <param name="builder">The command builder to use to build the commands.</param>
    protected override void Generate(
        RenameTableOperation operation,
        IModel? model,
        MigrationCommandListBuilder builder)
    {
        var differentNames = operation.NewName != null && operation.Name != operation.NewName;
        var differentSchemas = operation.NewName != null && operation.Schema != operation.NewSchema;

        if (differentNames)
        {
            this.Rename(operation.Name, operation.NewName, operation.Schema, TableType, builder);
        }
        else if (differentSchemas)
        {
            this.Transfer(operation.NewSchema, operation.Schema, operation.NewName, operation.Name, TableType, builder);
        }

        builder.EndCommand();
    }

    /// <summary>
    ///     Builds commands for the given <see cref="CreateIndexOperation" /> by making calls on the given
    ///     <see cref="MigrationCommandListBuilder" />.
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <param name="builder">The command builder to use to build the commands.</param>
    /// <param name="terminate">Indicates whether or not to terminate the command after generating SQL for the operation.</param>
    protected override void Generate(
        CreateIndexOperation operation,
        IModel? model,
        MigrationCommandListBuilder builder,
        bool terminate = true)
    {
        if (string.IsNullOrEmpty(operation.Table))
        {
            throw new InvalidOperationException(SnowflakeStrings.IndexTableRequired);
        }

        if (!this._hybridTableValidator.IsHybridTable(operation.Table, operation.Schema, model, out _))
        {
            throw new InvalidOperationException(SnowflakeStrings.IndexOnlySupportedWithinHybridTables(operation.Name, operation.Table));
        }

        builder.Append("CREATE ");

        IndexTraits(operation, model, builder);

        builder
            .Append("INDEX ")
            .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name))
            .Append(" ON ")
            .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Table, operation.Schema))
            .Append(" (");

        GenerateIndexColumnList(operation, model, builder);

        builder.Append(")");

        IndexOptions(operation, model, builder);

        if (terminate)
        {
            builder.AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);
            EndStatement(builder);
        }
    }

    /// <summary>
    ///     Returns a SQL fragment for the column list of an index from a <see cref="CreateIndexOperation" />.
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <param name="builder">The command builder to use to add the SQL fragment.</param>
    protected override void GenerateIndexColumnList(CreateIndexOperation operation, IModel? model, MigrationCommandListBuilder builder)
    {
        for (var i = 0; i < operation.Columns.Length; i++)
        {
            if (i > 0)
            {
                builder.Append(", ");
            }

            builder.Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Columns[i]));
        }
    }

    /// <summary>
    ///     Builds commands for the given <see cref="DropPrimaryKeyOperation" /> by making calls on the given
    ///     <see cref="MigrationCommandListBuilder" />.
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <param name="builder">The command builder to use to build the commands.</param>
    /// <param name="terminate">Indicates whether or not to terminate the command after generating SQL for the operation.</param>
    protected override void Generate(
        DropPrimaryKeyOperation operation,
        IModel? model,
        MigrationCommandListBuilder builder,
        bool terminate = true)
    {
        if (this._hybridTableValidator.IsHybridTable(operation.Table, operation.Schema, model, out _))
        {
            throw new InvalidOperationException(SnowflakeStrings.ConstraintOperationNotSupported(nameof(DropPrimaryKeyOperation), operation.Table));
        }

        base.Generate(operation, model, builder, terminate: false);
        if (terminate)
        {
            builder
                .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator)
                .EndCommand(
                    // TODO Review suppress transaction default value true or false
                    suppressTransaction: false);
        }
    }

    /// <summary>
    ///     Builds commands for the given <see cref="EnsureSchemaOperation" />
    ///     by making calls on the given <see cref="MigrationCommandListBuilder" />.
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <param name="builder">The command builder to use to build the commands.</param>
    protected override void Generate(
        EnsureSchemaOperation operation,
        IModel? model,
        MigrationCommandListBuilder builder)
    {
        builder
            .Append($"CREATE SCHEMA IF NOT EXISTS {Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name)}")
            .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator)
            .EndCommand();
    }

    /// <summary>
    ///     Builds commands for the given <see cref="CreateSequenceOperation" /> by making calls on the given
    ///     <see cref="MigrationCommandListBuilder" />, and then terminates the final command.
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <param name="builder">The command builder to use to build the commands.</param>
    protected override void Generate(
        CreateSequenceOperation operation,
        IModel? model,
        MigrationCommandListBuilder builder)
    {
        builder
            .Append("CREATE SEQUENCE IF NOT EXISTS ")
            .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name, operation.Schema));

        builder
            .Append(" START WITH ")
            .Append(IntegerConstant(operation.StartValue));

        SequenceOptions(operation, model, builder);

        builder
            .Append(" ORDER");

        builder.AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);

        EndStatement(builder);
    }

    /// <inheritdoc />
    protected override void Generate(
        DropSequenceOperation operation,
        IModel model,
        MigrationCommandListBuilder builder)
    {
        builder
            .Append("DROP SEQUENCE IF EXISTS ")
            .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name, operation.Schema))
            .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);

        EndStatement(builder);
    }

    /// <summary>
    ///     Builds commands for the given <see cref="SnowflakeCreateDatabaseOperation" />
    ///     by making calls on the given <see cref="MigrationCommandListBuilder" />.
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <param name="builder">The command builder to use to build the commands.</param>
    protected virtual void Generate(
        SnowflakeCreateDatabaseOperation operation,
        IModel? model,
        MigrationCommandListBuilder builder)
    {
        builder
            .Append("CREATE DATABASE ")
            .Append(operation.Name);

        if (!string.IsNullOrEmpty(operation.Collation))
        {
            builder
                .AppendLine()
                .Append("DEFAULT_DDL_COLLATION ")
                .Append("= ")
                .Append($"'{operation.Collation}'");
        }

        builder.Append(Dependencies.SqlGenerationHelper.StatementTerminator);
        EndStatement(builder);
    }

    /// <summary>
    ///     Builds commands for the given <see cref="AlterDatabaseOperation" />
    ///     by making calls on the given <see cref="MigrationCommandListBuilder" />.
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <param name="builder">The command builder to use to build the commands.</param>
    protected override void Generate(
        AlterDatabaseOperation operation,
        IModel? model,
        MigrationCommandListBuilder builder)
    {
        if (operation.Collation != operation.OldDatabase.Collation)
        {
            builder
                .AppendLine("BEGIN");

            using (builder.Indent())
            {
                // Declare the database and assign CURRENT DATABASE to it.
                builder
                    .Append("LET db_name := '\"' || REPLACE(CURRENT_DATABASE(), '\"', '\"\"') || '\"'")
                    .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);
                // Execute the ALTER DATABASE command.
                builder
                    .Append($"ALTER DATABASE IDENTIFIER(:db_name) SET DEFAULT_DDL_COLLATION = '{operation.Collation}'")
                    .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);
            }

            builder
                .Append("END")
                .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);
        }

        builder.EndCommand(suppressTransaction: true);
    }

    /// <summary>
    ///     Builds commands for the given <see cref="AlterTableOperation" />
    ///     by making calls on the given <see cref="MigrationCommandListBuilder" />.
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <param name="builder">The command builder to use to build the commands.</param>
    protected override void Generate(AlterTableOperation operation, IModel? model, MigrationCommandListBuilder builder)
    {
        if (operation.OldTable.Comment == operation.Comment)
        {
            return;
        }
        
        var dropDescription = operation.OldTable.Comment != null;
        if (dropDescription)
        {
            DropComment(builder, operation.Name, operation.Schema);
        }

        if (operation.Comment != null)
        {
            AddComment(
                builder,
                operation.Comment,
                operation.Name,
                TableType,
                operation.Schema);
        }
    }

    /// <summary>
    ///     Builds commands for the given <see cref="DropForeignKeyOperation" /> by making calls on the given
    ///     <see cref="MigrationCommandListBuilder" />.
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <param name="builder">The command builder to use to build the commands.</param>
    /// <param name="terminate">Indicates whether or not to terminate the command after generating SQL for the operation.</param>
    protected override void Generate(
        DropForeignKeyOperation operation,
        IModel? model,
        MigrationCommandListBuilder builder,
        bool terminate = true)
    {
        if (this._hybridTableValidator.IsHybridTable(operation.Table, operation.Schema, model, out _))
        {
            throw new InvalidOperationException(SnowflakeStrings.ConstraintOperationNotSupported(nameof(DropForeignKeyOperation), operation.Table));
        }

        base.Generate(operation, model, builder, terminate: false);

        if (terminate)
        {
            builder
                .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator)
                // TODO Review suppress transaction default value true or false
                .EndCommand(
                    suppressTransaction:
                    false);
        }
    }

    /// <summary>
    ///     Builds commands for the given <see cref="DropIndexOperation" />
    ///     by making calls on the given <see cref="MigrationCommandListBuilder" />.
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <param name="builder">The command builder to use to build the commands.</param>
    /// <param name="terminate">Indicates whether or not to terminate the command after generating SQL for the operation.</param>
    protected override void Generate(
        DropIndexOperation operation,
        IModel? model,
        MigrationCommandListBuilder builder,
        bool terminate)
    {
        if (string.IsNullOrEmpty(operation.Table))
        {
            throw new InvalidOperationException(SnowflakeStrings.IndexTableRequired);
        }
        
        if (!this._hybridTableValidator.IsHybridTable(operation.Table, operation.Schema, model, out _))
        {
            throw new InvalidOperationException(SnowflakeStrings.IndexOnlySupportedWithinHybridTables(operation.Name, operation.Table));
        }

        builder
            .Append("DROP INDEX IF EXISTS ")
            .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Table, operation.Schema))
            .Append(".")
            .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name));

        if (terminate)
        {
            builder
                .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator)
                // TODO Review suppress transaction default value true or false
                .EndCommand(suppressTransaction: false); // memoryOptimized);
        }
    }

    /// <summary>
    ///     Builds commands for the given <see cref="RenameColumnOperation" />
    ///     by making calls on the given <see cref="MigrationCommandListBuilder" />.
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <param name="builder">The command builder to use to build the commands.</param>
    protected override void Generate(RenameColumnOperation operation, IModel? model, MigrationCommandListBuilder builder)
        => builder
            .Append("ALTER TABLE ")
            .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Table, operation.Schema))
            .Append(" RENAME COLUMN ")
            .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name))
            .Append(" TO ")
            .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.NewName))
            .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator)
            .EndCommand();

    /// <summary>
    ///     Builds commands for the given <see cref="InsertDataOperation" /> by making calls on the given
    ///     <see cref="MigrationCommandListBuilder" />.
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <param name="builder">The command builder to use to build the commands.</param>
    /// <param name="terminate">Indicates whether or not to terminate the command after generating SQL for the operation.</param>
    protected override void Generate(
        InsertDataOperation operation,
        IModel? model,
        MigrationCommandListBuilder builder,
        bool terminate = true)
    {
        var updateSqlGenerator = (ISnowflakeUpdateSqlGenerator)Dependencies.UpdateSqlGenerator;

        foreach (var modificationCommand in GenerateModificationCommands(operation, model))
        {
            var sqlBuilder = new StringBuilder();
            updateSqlGenerator.AppendInsertOperation(sqlBuilder, modificationCommand, commandPosition: 0);

            builder.Append(sqlBuilder.ToString());

            if (terminate)
            {
                builder.EndCommand();
            }
        }
    }

    /// <inheritdoc />
    protected override void Generate(DeleteDataOperation operation, IModel? model, MigrationCommandListBuilder builder)
        => GenerateExecWhenIdempotent(builder, b => base.Generate(operation, model, b));

    /// <inheritdoc />
    protected override void Generate(UpdateDataOperation operation, IModel? model, MigrationCommandListBuilder builder)
        => GenerateExecWhenIdempotent(builder, b => base.Generate(operation, model, b));

    /// <summary>
    ///     Generates a SQL fragment configuring a sequence with the given options.
    /// </summary>
    /// <param name="schema">The schema that contains the sequence, or <see langword="null" /> to use the default schema.</param>
    /// <param name="name">The sequence name.</param>
    /// <param name="operation">The sequence options.</param>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <param name="builder">The command builder to use to add the SQL fragment.</param>
    protected override void SequenceOptions(
        string? schema,
        string name,
        SequenceOperation operation,
        IModel? model,
        MigrationCommandListBuilder builder)
    {
        builder
            .Append(" INCREMENT BY ")
            .Append(IntegerConstant(operation.IncrementBy));
    }


    /// <summary>
    ///     Generates a SQL fragment for a column definition for the given column metadata.
    /// </summary>
    /// <param name="schema">The schema that contains the table, or <see langword="null" /> to use the default schema.</param>
    /// <param name="table">The table that contains the column.</param>
    /// <param name="name">The column name.</param>
    /// <param name="operation">The column metadata.</param>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <param name="builder">The command builder to use to add the SQL fragment.</param>
    protected override void ColumnDefinition(
        string? schema,
        string table,
        string name,
        ColumnOperation operation,
        IModel? model,
        MigrationCommandListBuilder builder)
    {
        if (operation.ComputedColumnSql != null)
        {
            ComputedColumnDefinition(schema, table, name, operation, model, builder);

            return;
        }

        var columnType = operation.ColumnType ?? GetColumnType(schema, table, name, operation, model)!;
        builder
            .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(name))
            .Append(" ")
            .Append(columnType);

        if (operation.Collation != null)
        {
            builder
                .Append(" COLLATE ")
                .Append($"'{operation.Collation}'");
        }

        builder.Append(operation.IsNullable ? " NULL" : " NOT NULL");

        if (!string.Equals(columnType, "rowversion", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(columnType, "timestamp", StringComparison.OrdinalIgnoreCase))
        {
            // rowversion/timestamp columns cannot have default values, but also don't need them when adding a new column.
            DefaultValue(operation.DefaultValue, operation.DefaultValueSql, columnType, builder);
        }

        var identity = operation[SnowflakeAnnotationNames.Identity] as string;
        if (identity != null
            || operation[SnowflakeAnnotationNames.ValueGenerationStrategy] as SnowflakeValueGenerationStrategy?
            == SnowflakeValueGenerationStrategy.IdentityColumn)
        {
            builder.Append(" IDENTITY");

            if (!string.IsNullOrEmpty(identity)
                && identity != "1, 1")
            {
                builder
                    .Append("(")
                    .Append(identity)
                    .Append(")");
            }
        }

        if (operation.Comment != null)
        {
            builder.Append(" COMMENT ")
                .Append($"'{operation.Comment}'");
        }
    }

    /// <summary>
    ///     Generates a SQL fragment for a computed column definition for the given column metadata.
    /// </summary>
    /// <param name="schema">The schema that contains the table, or <see langword="null" /> to use the default schema.</param>
    /// <param name="table">The table that contains the column.</param>
    /// <param name="name">The column name.</param>
    /// <param name="operation">The column metadata.</param>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <param name="builder">The command builder to use to add the SQL fragment.</param>
    protected override void ComputedColumnDefinition(
        string? schema,
        string table,
        string name,
        ColumnOperation operation,
        IModel? model,
        MigrationCommandListBuilder builder)
    {
        var columnType = operation.ColumnType ?? GetColumnType(schema, table, name, operation, model)!;
        builder.Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(name))
            .Append($" {columnType}");

        builder
            .Append(" AS ")
            .Append($"({operation.ComputedColumnSql})");

        if (operation.Collation != null)
        {
            this.Dependencies.Logger.Logger.LogWarning(
                "Collation is not supported within computed columns in Snowflake.");
        }

        if (operation.IsStored == true)
        {
            this.Dependencies.Logger.Logger.LogWarning(
                "Storing is not supported within computed columns in Snowflake.");
        }
    }
    
    /// <inheritdoc/>
    protected override void DefaultValue(
        object? defaultValue,
        string? defaultValueSql,
        string? columnType,
        MigrationCommandListBuilder builder)
    {
        base.DefaultValue(defaultValue, defaultValueSql, columnType, builder);
        
        if (defaultValueSql == null && defaultValue != null && _temporalDataTypes.Contains(columnType))
        {
            builder.Append($"::{columnType}");
        }
    }

    /// <summary>
    ///     Generates a rename.
    /// </summary>
    /// <param name="name">The old name.</param>
    /// <param name="newName">The new name.</param>
    /// <param name="schema"></param>
    /// <param name="type">If not <see langword="null" />, then appends literal for type of object being renamed (e.g. column or index.)</param>
    /// <param name="builder">The command builder to use to build the commands.</param>
    protected virtual void Rename(
        string name,
        string newName,
        string schema,
        string? type,
        MigrationCommandListBuilder builder)
    {
        builder.Append($"ALTER {type} ")
            .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(name, schema))
            .Append(" RENAME TO ")
            .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(newName, schema))
            .Append(this.SqlGenerationHelper.StatementTerminator);
    }

    /// <summary>
    ///     Generates a transfer from one schema to another..
    /// </summary>
    /// <param name="newSchema">The schema to transfer to.</param>
    /// <param name="schema">The schema to transfer from.</param>
    /// <param name="newName">The new name to assign</param>
    /// <param name="name">The name of the item to transfer.</param>
    /// <param name="objectType"></param>
    /// <param name="builder">The command builder to use to build the commands.</param>
    protected virtual void Transfer(
        string? newSchema,
        string? schema,
        string? newName,
        string name,
        string objectType,
        MigrationCommandListBuilder builder)
    {
        if (newSchema == null)
        {
            builder
                .AppendLine("BEGIN");

            using (builder.Indent())
            {
                // Declare the database and assign CURRENT DATABASE to it.
                builder
                    .Append($"LET new_object_name := CURRENT_SCHEMA() || '.' || '{name}'")
                    .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);
                // Execute the ALTER DATABASE command.
                builder
                    .Append($"ALTER {objectType} ")
                    .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(name, schema))
                    .Append(" RENAME TO IDENTIFIER(:new_object_name)")
                    .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);
            }

            builder
                .Append("END")
                .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);
        }
        else
        {
            builder
                .Append($"ALTER {objectType} ")
                .Append(schema != null ? Dependencies.SqlGenerationHelper.DelimitIdentifier(schema) : _snowflakeConnection.Schema)
                .Append(".")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(name))
                .Append(" RENAME TO ")
                .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(newName ?? name, newSchema))
                .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);
        }
    }

    /// <summary>
    ///     Generates a SQL fragment for traits of an index from a <see cref="CreateIndexOperation" />,
    ///     <see cref="AddPrimaryKeyOperation" />, or <see cref="AddUniqueConstraintOperation" />.
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <param name="builder">The command builder to use to add the SQL fragment.</param>
    protected override void IndexTraits(MigrationOperation operation, IModel? model,
        MigrationCommandListBuilder builder)
    {
    }

    /// <summary>
    ///     Generates a SQL fragment for extras (filter, included columns, options) of an index from a <see cref="CreateIndexOperation" />.
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <param name="builder">The command builder to use to add the SQL fragment.</param>
    protected override void IndexOptions(CreateIndexOperation operation, IModel? model,
        MigrationCommandListBuilder builder)
    {
        var unsupportedOptions = new List<string>();

        if (operation.IsUnique)
        {
            unsupportedOptions.Add(nameof(operation.IsUnique));
        }

        if (operation[SnowflakeAnnotationNames.Include] is IReadOnlyList<string> { Count: > 0 })
        {
            throw new InvalidOperationException(SnowflakeStrings.IndexWithIncludeOptionIsNotCurrentlySupported(operation.Name, operation.Table));
        }

        if (operation.IsDescending?.Length > 0)
        {
            unsupportedOptions.Add(nameof(operation.IsUnique));
        }

        if (!string.IsNullOrEmpty(operation.Filter))
        {
            unsupportedOptions.Add(nameof(operation.Filter));
        }

        foreach (var option in unsupportedOptions)
            _logger.MigrationOperationOptionNotSupported(option, nameof(CreateIndexOperation));
    }

    /// <summary>
    ///     Generates a SQL fragment for the given referential action.
    /// </summary>
    /// <param name="referentialAction">The referential action.</param>
    /// <param name="builder">The command builder to use to add the SQL fragment.</param>
    protected override void ForeignKeyAction(ReferentialAction referentialAction, MigrationCommandListBuilder builder)
    {
        builder.Append("NO ACTION");
    }

    /// <inheritdoc />
    protected override void Generate(
        AddColumnOperation operation,
        IModel? model,
        MigrationCommandListBuilder builder,
        bool terminate = true)
    {
        var hasDefaultValue = operation.DefaultValue != null || operation.DefaultValueSql != null;
        if (hasDefaultValue && _temporalDataTypes.Contains(operation.ColumnType))
        {
            throw new InvalidOperationException(SnowflakeStrings.DefaultValueInAddColumnNotSupported(operation.ColumnType));
        }
        
        if (this._identityValidator.IsIdentity(operation))
        {
            // Snowflake doesn't allow default and autoincrement expressions at the same time.
            operation.DefaultValue = null;
        }

        // Snowflake doesn't support rowversion
        operation.IsRowVersion = false;

        base.Generate(operation, model, builder, terminate: false);

        if (!terminate)
        {
            return;
        }

        builder.AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator)
            // TODO Review suppress transaction default value true or false
            .EndCommand(suppressTransaction: false);
    }

    /// <summary>
    ///     Gets the list of indexes that need to be rebuilt when the given column is changing.
    /// </summary>
    /// <param name="column">The column.</param>
    /// <param name="currentOperation">The operation which may require a rebuild.</param>
    /// <returns>The list of indexes affected.</returns>
    protected virtual IEnumerable<ITableIndex> GetIndexesToRebuild(
        IColumn? column,
        MigrationOperation currentOperation)
    {
        if (column == null)
        {
            yield break;
        }

        var table = column.Table;
        var createIndexOperations = _operations.SkipWhile(o => o != currentOperation).Skip(1)
            .OfType<CreateIndexOperation>().Where(o => o.Table == table.Name && o.Schema == table.Schema).ToList();
        foreach (var index in table.Indexes)
        {
            var indexName = index.Name;
            if (createIndexOperations.Any(o => o.Name == indexName))
            {
                continue;
            }

            if (index.Columns.Any(c => c == column))
            {
                yield return index;
            }
            else if (index[SnowflakeAnnotationNames.Include] is IReadOnlyList<string> includeColumns
                     && includeColumns.Contains(column.Name))
            {
                yield return index;
            }
        }
    }

    /// <summary>
    ///     Generates SQL to drop the given indexes.
    /// </summary>
    /// <param name="indexes">The indexes to drop.</param>
    /// <param name="builder">The command builder to use to build the commands.</param>
    protected virtual void DropIndexes(
        IEnumerable<ITableIndex> indexes,
        MigrationCommandListBuilder builder)
    {
        foreach (var index in indexes)
        {
            var table = index.Table;
            var operation = new DropIndexOperation
            {
                Schema = table.Schema,
                Table = table.Name,
                Name = index.Name
            };
            operation.AddAnnotations(index.GetAnnotations());

            Generate(operation, table.Model.Model, builder, terminate: false);
            builder.AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);
        }
    }

    /// <summary>
    ///     Generates SQL to create the given indexes.
    /// </summary>
    /// <param name="indexes">The indexes to create.</param>
    /// <param name="builder">The command builder to use to build the commands.</param>
    protected virtual void CreateIndexes(
        IEnumerable<ITableIndex> indexes,
        MigrationCommandListBuilder builder)
    {
        foreach (var index in indexes)
        {
            Generate(CreateIndexOperation.CreateFrom(index), index.Table.Model.Model, builder, terminate: false);
            builder.AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator);
        }
    }

    /// <summary>
    ///  Generates add commands for comments on tables and columns.
    /// </summary>
    /// <param name="builder">The command builder to use to build the commands.</param>
    /// <param name="comment">The new description to be applied.</param>
    /// <param name="objectType">The type of the object to add the comment (COLUMN, TABLE, etc).</param>
    /// <param name="objectSchema">The schema where the object belongs.</param>
    /// <param name="column">The name of the column.</param>
    /// <param name="objectName"></param>
    protected virtual void AddComment(
        MigrationCommandListBuilder builder,
        string comment,
        string objectName,
        string objectType,
        string? objectSchema = null,
        string? column = null)
    {
        var generationHelper = (SnowflakeSqlGenerationHelper)Dependencies.SqlGenerationHelper;
        objectName = generationHelper.DelimitIdentifier([objectSchema, objectName, column]);

        builder
            .Append("COMMENT IF EXISTS ON ")
            .Append($"{objectType} {objectName} ")
            .Append("IS ")
            .Append($"'{comment}'")
            .Append(Dependencies.SqlGenerationHelper.StatementTerminator)
            // TODO Review suppress transaction default value true or false
            .EndCommand(suppressTransaction: false);
    }

    /// <summary>
    ///  Generates drop commands for comments on tables and columns.
    /// </summary>
    /// <param name="builder">The command builder to use to build the commands.</param>
    /// <param name="schema">The schema name</param>
    /// <param name="column">The name of the column.</param>
    /// <param name="tableName">The table name</param>
    protected virtual void DropComment(
        MigrationCommandListBuilder builder,
        string tableName,
        string schema,
        string? column = null)
    {
        builder
            .Append("ALTER TABLE ")
            .Append(this.SqlGenerationHelper.DelimitIdentifier(tableName, schema));

        if (column != null)
        {
            builder
                .Append(" ALTER COLUMN ")
                .Append(this.SqlGenerationHelper.DelimitIdentifier(column));
        }

        builder
            .Append(" UNSET COMMENT")
            .Append(this.SqlGenerationHelper.StatementTerminator)
            .EndCommand();
    }

    /// <summary>
    ///     Checks whether or not <see cref="CreateIndexOperation" /> should have a filter generated for it by
    ///     Migrations.
    /// </summary>
    /// <param name="operation">The index creation operation.</param>
    /// <param name="model">The target model.</param>
    /// <returns><see langword="true" /> if a filter should be generated.</returns>
    protected virtual bool UseLegacyIndexFilters(CreateIndexOperation operation, IModel? model)
        => (!TryGetVersion(model, out var version) || VersionComparer.Compare(version, "2.0.0") < 0)
           && operation.Filter is null
           && operation.IsUnique
           && model?.GetRelationalModel().FindTable(operation.Table, operation.Schema) is var table
           && operation.Columns.Any(c => table?.FindColumn(c)?.IsNullable != false);

    private static string IntegerConstant(long value)
        => string.Format(CultureInfo.InvariantCulture, "{0}", value);

    private void GenerateExecWhenIdempotent(
        MigrationCommandListBuilder builder,
        Action<MigrationCommandListBuilder> generate)
    {
        if (Options.HasFlag(MigrationsSqlGenerationOptions.Idempotent))
        {
            var subBuilder = new MigrationCommandListBuilder(Dependencies);
            generate(subBuilder);

            var command = subBuilder.GetCommandList().Single();
            builder
                .Append("EXEC(N'")
                .Append(command.CommandText.TrimEnd('\n', '\r', ';').Replace("'", "''"))
                .Append("')")
                .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator)
                .EndCommand(command.TransactionSuppressed);

            return;
        }

        generate(builder);
    }

    private void Generate(
        SnowflakeCreateSchemaOperation operation,
        IModel? model,
        MigrationCommandListBuilder builder)
    {
        builder
            .Append($"CREATE SCHEMA IF NOT EXISTS {operation.DatabaseName}.{SqlGenerationHelper.DelimitIdentifier(operation.Name)}")
            .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator)
            .EndCommand();
    }

    private void Generate(
        SnowflakeDropDatabaseOperation operation,
        IModel? model,
        MigrationCommandListBuilder builder)
    {
        builder.Append($"DROP DATABASE IF EXISTS {operation.Name}");

        if (operation.Cascade)
        {
            builder.Append(" CASCADE");
        }

        builder
            .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator)
            .EndCommand();
    }

    /// <summary>
    ///     Builds commands for the given <see cref="DropSchemaOperation" /> by making calls on the given
    ///     <see cref="MigrationCommandListBuilder" />, and then terminates the final command.
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <param name="builder">The command builder to use to build the commands.</param>
    protected override void Generate(
        DropSchemaOperation operation,
        IModel? model,
        MigrationCommandListBuilder builder)
    {
        builder.Append($"DROP SCHEMA IF EXISTS {operation.Name}").Append(" RESTRICT");

        builder
            .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator)
            .EndCommand();

        EndStatement(builder);
    }

    /// <summary>
    ///     Builds commands for the given <see cref="DropTableOperation" /> by making calls on the given
    ///     <see cref="MigrationCommandListBuilder" />, and then terminates the final command.
    /// </summary>
    /// <param name="operation">The operation.</param>
    /// <param name="model">The target model which may be <see langword="null" /> if the operations exist without a model.</param>
    /// <param name="builder">The command builder to use to build the commands.</param>
    protected override void Generate(
        DropTableOperation operation,
        IModel? model,
        MigrationCommandListBuilder builder,
        bool terminate = true)
    {
        builder.Append("DROP TABLE IF EXISTS ")
            .Append(Dependencies.SqlGenerationHelper.DelimitIdentifier(operation.Name, operation.Schema))
            .Append(" RESTRICT");

        if (terminate)
        {
            builder
                .AppendLine(Dependencies.SqlGenerationHelper.StatementTerminator)
                // TODO Review suppress transaction default value true or false
                .EndCommand(suppressTransaction: false);
        }

        EndStatement(builder);
    }

    /// <inheritdoc />
    protected override void CheckConstraint(AddCheckConstraintOperation operation, IModel model, MigrationCommandListBuilder builder)
    {
        throw new NotSupportedException(SnowflakeStrings.AddCheckConstraintNotSupported(operation.Name, operation.Table));
    }

    private static bool HasDifferences(IEnumerable<IAnnotation> source, IEnumerable<IAnnotation> target)
    {
        var targetAnnotations = target.ToDictionary(a => a.Name);

        var count = 0;
        foreach (var sourceAnnotation in source)
        {
            if (!targetAnnotations.TryGetValue(sourceAnnotation.Name, out var targetAnnotation)
                || !Equals(sourceAnnotation.Value, targetAnnotation.Value))
            {
                return true;
            }

            count++;
        }

        return count != targetAnnotations.Count;
    }

    private IReadOnlyList<MigrationOperation> RewriteOperations(
        IReadOnlyList<MigrationOperation> migrationOperations,
        IModel? model,
        MigrationsSqlGenerationOptions options)
    {
        var operations = new List<MigrationOperation>();

        var availableSchemas = new List<string>();

        foreach (var operation in migrationOperations)
        {
            if (operation is EnsureSchemaOperation ensureSchemaOperation)
            {
                availableSchemas.Add(ensureSchemaOperation.Name);
            }

            var isTemporal = operation[SnowflakeAnnotationNames.IsTemporal] as bool? == true;
            if (isTemporal)
            {
                string? table = null;
                string? schema = null;

                if (operation is ITableMigrationOperation tableMigrationOperation)
                {
                    table = tableMigrationOperation.Table;
                    schema = tableMigrationOperation.Schema;
                }

                // TODO Review suppress transaction default value true or false
                var suppressTransaction = table is not null && false;

                schema ??= model?.GetDefaultSchema();

                switch (operation)
                {
                    case AlterColumnOperation alterColumnOperation:
                        // if only difference is in temporal annotations being removed or history table changed etc - we can ignore this operation
                        if (!CanSkipAlterColumnOperation(alterColumnOperation.OldColumn, alterColumnOperation))
                        {
                            operations.Add(operation);

                            // when modifying a period column, we need to perform the operations as a normal column first, and only later enable period
                            // removing the period information now, so that when we generate SQL that modifies the column we won't be making them auto generated as period
                            // (making column auto generated is not allowed in ALTER COLUMN statement)
                            // in later operation we enable the period and the period columns get set to auto generated automatically
                            //
                            // if the column is not period we just remove temporal information - it's no longer needed and could affect the generated sql
                            // we will generate all the necessary operations involved with temporal tables here
                            alterColumnOperation.RemoveAnnotation(SnowflakeAnnotationNames.IsTemporal);

                            // this is the case where we are not converting from normal table to temporal
                            // just a normal modification to a column on a temporal table
                            // in that case we need to double check if we need have disabled versioning earlier in this migration
                            // if so, we need to mirror the operation to the history table
                            if (alterColumnOperation.OldColumn[SnowflakeAnnotationNames.IsTemporal] as bool? == true)
                            {
                                alterColumnOperation.OldColumn.RemoveAnnotation(SnowflakeAnnotationNames.IsTemporal);


                                // TODO: test what happens if default value just changes (from temporal to temporal)
                            }
                        }

                        break;
                    default:
                        operations.Add(operation);
                        break;
                }
            }
            else
            {
                if (operation is AddForeignKeyOperation foreignKeyOperation)
                {
                    CreateTableOperation? referenceTableOperation = null;
                    CreateTableOperation? principalTableOperation = null;

                    foreach (var opt in operations)
                    {
                        if (opt is CreateTableOperation createTableOperation)
                        {
                            if (createTableOperation.Name == foreignKeyOperation.Table)
                            {
                                referenceTableOperation = createTableOperation;
                            }
                            else if (createTableOperation.Name == foreignKeyOperation.PrincipalTable)
                            {
                                principalTableOperation = createTableOperation;
                            }

                            if (referenceTableOperation != null && principalTableOperation != null)
                            {
                                break;
                            }
                        }
                    }

                    if (referenceTableOperation?.FindAnnotation(SnowflakeAnnotationNames.HybridTable) != null &&
                        principalTableOperation?.FindAnnotation(SnowflakeAnnotationNames.HybridTable) != null)
                    {
                        throw new InvalidOperationException(SnowflakeStrings.AddingForeignKeyToAnExitingHybridTable(principalTableOperation.Name));
                    }
                }
                
                switch (operation)

                {
                    case AlterColumnOperation alterColumnOperation:
                        // if only difference is in temporal annotations being removed or history table changed etc - we can ignore this operation
                        if (alterColumnOperation.OldColumn?[SnowflakeAnnotationNames.IsTemporal] as bool? != true
                            || !CanSkipAlterColumnOperation(alterColumnOperation.OldColumn, alterColumnOperation))
                        {
                            operations.Add(operation);
                        }
                        break;
                    case RenameIndexOperation renameIndexOperation:
                    {
                        var table = renameIndexOperation.Table != null
                            ? model?.GetRelationalModel().FindTable(renameIndexOperation.Table, renameIndexOperation.Schema)
                            : null;
                        var index = table?.Indexes.FirstOrDefault(i => i.Name == renameIndexOperation.NewName);
                        if (index != null)
                        {
                            operations.Add(
                                new DropIndexOperation
                                {
                                    Table = renameIndexOperation.Table,
                                    Schema = renameIndexOperation.Schema,
                                    Name = renameIndexOperation.Name
                                });

                            operations.Add(CreateIndexOperation.CreateFrom(index));
                        }
                        else
                        {
                            operations.Add(renameIndexOperation);
                        }

                        break;
                    }
                    default:
                        operations.Add(operation);
                        break;
                        
                    
                }
            }
        }

        return operations;


        static bool CanSkipAlterColumnOperation(ColumnOperation first, ColumnOperation second)
            => ColumnPropertiesAreTheSame(first, second)
               && ColumnOperationsOnlyDifferByTemporalTableAnnotation(first, second)
               && ColumnOperationsOnlyDifferByTemporalTableAnnotation(second, first);

        // don't compare name, table or schema - they are not being set in the model differ (since they should always be the same)
        static bool ColumnPropertiesAreTheSame(ColumnOperation first, ColumnOperation second)
            => first.ClrType == second.ClrType
               && first.Collation == second.Collation
               && first.ColumnType == second.ColumnType
               && first.Comment == second.Comment
               && first.ComputedColumnSql == second.ComputedColumnSql
               && Equals(first.DefaultValue, second.DefaultValue)
               && first.DefaultValueSql == second.DefaultValueSql
               && first.IsDestructiveChange == second.IsDestructiveChange
               && first.IsFixedLength == second.IsFixedLength
               && first.IsNullable == second.IsNullable
               && first.IsReadOnly == second.IsReadOnly
               && first.IsRowVersion == second.IsRowVersion
               && first.IsStored == second.IsStored
               && first.IsUnicode == second.IsUnicode
               && first.MaxLength == second.MaxLength
               && first.Precision == second.Precision
               && first.Scale == second.Scale;

        static bool ColumnOperationsOnlyDifferByTemporalTableAnnotation(ColumnOperation first, ColumnOperation second)
        {
            var unmatched = first.GetAnnotations().ToList();
            foreach (var annotation in second.GetAnnotations())
            {
                var index = unmatched.FindIndex(
                    a => a.Name == annotation.Name
                         && StructuralComparisons.StructuralEqualityComparer.Equals(a.Value, annotation.Value));
                if (index == -1)
                {
                    continue;
                }

                unmatched.RemoveAt(index);
            }

            return unmatched.All(
                a => a.Name is SnowflakeAnnotationNames.IsTemporal);
        }

        static TOperation CopyColumnOperation<TOperation>(ColumnOperation source)
            where TOperation : ColumnOperation, new()
        {
            var result = new TOperation
            {
                ClrType = source.ClrType,
                Collation = source.Collation,
                ColumnType = source.ColumnType,
                Comment = source.Comment,
                ComputedColumnSql = source.ComputedColumnSql,
                DefaultValue = source.DefaultValue,
                DefaultValueSql = source.DefaultValueSql,
                IsDestructiveChange = source.IsDestructiveChange,
                IsFixedLength = source.IsFixedLength,
                IsNullable = source.IsNullable,
                IsRowVersion = source.IsRowVersion,
                IsStored = source.IsStored,
                IsUnicode = source.IsUnicode,
                MaxLength = source.MaxLength,
                Name = source.Name,
                Precision = source.Precision,
                Scale = source.Scale,
                Table = source.Table,
                Schema = source.Schema
            };

            foreach (var annotation in source.GetAnnotations())
            {
                result.AddAnnotation(annotation.Name, annotation.Value);
            }

            return result;
        }
    }
}