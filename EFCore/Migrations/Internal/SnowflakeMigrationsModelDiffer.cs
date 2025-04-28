namespace Snowflake.EntityFrameworkCore.Migrations.Internal;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Update.Internal;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

/// <inheritdoc />
public class SnowflakeMigrationsModelDiffer : MigrationsModelDiffer
{
    public  SnowflakeMigrationsModelDiffer(
        IRelationalTypeMappingSource typeMappingSource, 
        IMigrationsAnnotationProvider migrationsAnnotationProvider,
        IRowIdentityMapFactory rowIdentityMapFactory,
        CommandBatchPreparerDependencies commandBatchPreparerDependencies) 
        : base(typeMappingSource, migrationsAnnotationProvider, rowIdentityMapFactory, commandBatchPreparerDependencies)
    {
    }

    /// <inheritdoc />
    protected override IEnumerable<MigrationOperation> Add(
        ITable target,
        DiffContext diffContext)
    {
        
        if (target.IsExcludedFromMigrations)
        {
            yield break;
        }

        var createTableOperation = new CreateTableOperation
        {
            Schema = target.Schema,
            Name = target.Name,
            Comment = target.Comment
        };
        
        createTableOperation.AddAnnotations(target.GetAnnotations());

        // We need to reorder the columns in case they are virtual/computed. Default behavior is add them in alphabetic order.
        var columnsGraph = new SnowflakeColumnGraph(target);
        
        createTableOperation.Columns.AddRange(
            columnsGraph.TopologicalSort(target).SelectMany(p => Add(p, diffContext, inline: true)).Cast<AddColumnOperation>());

        var primaryKey = target.PrimaryKey;
        
        if (primaryKey != null)
        {
            createTableOperation.PrimaryKey = Add(primaryKey, diffContext).Cast<AddPrimaryKeyOperation>().Single();
        }

        createTableOperation.UniqueConstraints.AddRange(
            target.UniqueConstraints.Where(c => !c.GetIsPrimaryKey()).SelectMany(c => Add(c, diffContext))
                .Cast<AddUniqueConstraintOperation>());
        createTableOperation.CheckConstraints.AddRange(
            target.CheckConstraints.SelectMany(c => Add(c, diffContext))
                .Cast<AddCheckConstraintOperation>());

        diffContext.AddCreate(target, createTableOperation);

        yield return createTableOperation;

        foreach (var operation in target.Indexes.SelectMany(i => Add(i, diffContext)))
        {
            yield return operation;
        }
    }
}