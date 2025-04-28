using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Internal;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Snowflake.EntityFrameworkCore.Metadata.Internal;
using Snowflake.EntityFrameworkCore.TestUtilities;

namespace Snowflake.EntityFrameworkCore.FunctionalTests.Migrations;

public class SnowflakeModelDifferTest
{
    private TestHelpers TestHelpers => SnowflakeTestHelpers.Instance;
    
    #region Create Table
    
    [ConditionalFact]
    public void Create_hybrid_table_with_auto_generated_pk()
        => Execute(
            _ => { },
            _ => { },
            builder => builder.Entity("People", e =>
            {
                e.Property<int>("Id");
                e.ToTable(tb => tb.IsHybridTable());
            }),
            upOps =>
            {
                Assert.Equal(1, upOps.Count);
                var createTableOperation = Assert.IsType<CreateTableOperation>(upOps[0]);
                Assert.Single(createTableOperation.GetAnnotations());
                Assert.True(createTableOperation[SnowflakeAnnotationNames.HybridTable] as bool?);
                Assert.NotNull(createTableOperation.PrimaryKey);
                Assert.Equal("Id", createTableOperation.PrimaryKey.Columns[0]);
            },
            downOps =>
            {
                Assert.Equal(1, downOps.Count);
                var dropTableOperation = Assert.IsType<DropTableOperation>(downOps[0]);
                Assert.True(dropTableOperation[SnowflakeAnnotationNames.HybridTable] as bool?);
            });
    
    #endregion

    #region Add Column
    
    [ConditionalFact]
    public void Add_column_computed()
        => Execute(
            builder => builder.Entity(
                "People", e =>
                {
                    e.Property<int>("Id");
                    e.Property<int>("X");
                    e.Property<int>("Y");
                }),
            _ => { },
            builder => builder.Entity("People").Property<string>("Sum")
                .HasComputedColumnSql("\"X\" || \"Y\"", false),
            upOps =>
            {
                Assert.Equal(1, upOps.Count);
                var addColumnOperation = Assert.IsType<AddColumnOperation>(upOps[0]);
                Assert.Equal("\"X\" || \"Y\"", addColumnOperation.ComputedColumnSql);
                Assert.Equal("varchar", addColumnOperation.ColumnType);
            },
            downOps =>
            {
                Assert.Equal(1, downOps.Count);
                var dropColumnOperation = Assert.IsType<DropColumnOperation>(downOps[0]);
                Assert.Equal("Sum", dropColumnOperation.Name);
            });

    [ConditionalFact]
    public void Add_column_with_collation()
    => Execute(
        builder => builder.Entity("People").Property<int>("Id"),
        _ => { },
        builder => builder.Entity("People").Property<string>("Name")
            .UseCollation("en-ci"),
    upOps =>
        {
            Assert.Equal(1, upOps.Count);
            var addColumnOperation = Assert.IsType<AddColumnOperation>(upOps[0]);
            Assert.Equal("en-ci", addColumnOperation.Collation);
            Assert.Equal("varchar", addColumnOperation.ColumnType);
        },
    downOps =>
        {
            Assert.Equal(1, downOps.Count);
            var dropColumnOperation = Assert.IsType<DropColumnOperation>(downOps[0]);
            Assert.Equal("Name", dropColumnOperation.Name);
        });
    
    [ConditionalFact]
    public void Add_column_with_comment()
        => Execute(
            builder => builder.Entity("People").Property<int>("Id"),
            _ => { },
            builder => builder.Entity("People").Property<string>("FullName").HasComment("My comment"),
            upOps =>
            {
                Assert.Equal(1, upOps.Count);
                var addColumnOperation = Assert.IsType<AddColumnOperation>(upOps[0]);
                Assert.Equal("My comment", addColumnOperation.Comment);
                Assert.Equal("varchar", addColumnOperation.ColumnType);
            },
            downOps =>
            {
                Assert.Equal(1, downOps.Count);
                var dropColumnOperation = Assert.IsType<DropColumnOperation>(downOps[0]);
                Assert.Equal("FullName", dropColumnOperation.Name);
            });

    [ConditionalFact]
    public void Add_column_with_fixed_length()
        => Execute(
            builder => builder.Entity("People").Property<int>("Id"),
            _ => { },
            builder => builder.Entity("People").Property<string>("Name")
                .IsFixedLength()
                .HasMaxLength(100),
            upOps =>
            {
                Assert.Equal(1, upOps.Count);
                var addColumnOperation = Assert.IsType<AddColumnOperation>(upOps[0]);
                Assert.Equal(true, addColumnOperation.IsFixedLength);
                Assert.Equal(100, addColumnOperation.MaxLength);
                Assert.Equal("nchar(100)", addColumnOperation.ColumnType);
            },
            downOps =>
            {
                Assert.Equal(1, downOps.Count);
                var dropColumnOperation = Assert.IsType<DropColumnOperation>(downOps[0]);
                Assert.Equal("Name", dropColumnOperation.Name);
            });
    
    [ConditionalFact]
    public void Add_column_with_max_length()
        => Execute(
            builder => builder.Entity("People").Property<int>("Id"),
            _ => { },
            builder => builder.Entity("People").Property<string>("Name")
                .HasMaxLength(100),
            upOps =>
            {
                Assert.Equal(1, upOps.Count);
                var addColumnOperation = Assert.IsType<AddColumnOperation>(upOps[0]);
                Assert.Equal(100, addColumnOperation.MaxLength);
                Assert.Equal("nvarchar(100)", addColumnOperation.ColumnType);
            },
            downOps =>
            {
                Assert.Equal(1, downOps.Count);
                var dropColumnOperation = Assert.IsType<DropColumnOperation>(downOps[0]);
                Assert.Equal("Name", dropColumnOperation.Name);
            });
    
    [ConditionalFact]
    public void Add_column_non_nullable()
        => Execute(
            builder => builder.Entity("People").Property<int>("Id"),
            _ => { },
            builder => builder.Entity("People").Property<string>("Name").IsRequired(),
            upOps =>
            {
                Assert.Equal(1, upOps.Count);
                var addColumnOperation = Assert.IsType<AddColumnOperation>(upOps[0]);
                Assert.False(addColumnOperation.IsNullable);
                Assert.Equal("varchar", addColumnOperation.ColumnType);
            },
            downOps =>
            {
                Assert.Equal(1, downOps.Count);
                var dropColumnOperation = Assert.IsType<DropColumnOperation>(downOps[0]);
                Assert.Equal("Name", dropColumnOperation.Name);
            });
    
    [ConditionalFact]
    public void Add_column_with_default_value_String()
        => Execute(
            builder => builder.Entity("People").Property<int>("Id"),
            _ => { },
            builder => builder.Entity("People").Property<string>("Name")
                .IsRequired()
                .HasDefaultValue("John Doe"),
            upOps =>
            {
                Assert.Equal(1, upOps.Count);
                var addColumnOperation = Assert.IsType<AddColumnOperation>(upOps[0]);
                Assert.False(addColumnOperation.IsNullable);
                Assert.Equal("varchar", addColumnOperation.ColumnType);
                Assert.Equal("John Doe", addColumnOperation.DefaultValue);
            },
            downOps =>
            {
                Assert.Equal(1, downOps.Count);
                var dropColumnOperation = Assert.IsType<DropColumnOperation>(downOps[0]);
                Assert.Equal("Name", dropColumnOperation.Name);
            });
    
    [ConditionalFact]
    public void Add_column_with_default_value()
        => Execute(
            builder => builder.Entity("People").Property<int>("Id"),
            _ => { },
            builder => builder.Entity("People").Property<int>("Sum")
                .HasDefaultValue("1"),
            upOps =>
            {
                Assert.Equal(1, upOps.Count);
                var addColumnOperation = Assert.IsType<AddColumnOperation>(upOps[0]);
                Assert.False(addColumnOperation.IsNullable);
                Assert.Equal("int", addColumnOperation.ColumnType);
                Assert.Equal(1, addColumnOperation.DefaultValue);
            },
            downOps =>
            {
                Assert.Equal(1, downOps.Count);
                var dropColumnOperation = Assert.IsType<DropColumnOperation>(downOps[0]);
                Assert.Equal("Sum", dropColumnOperation.Name);
            });
    
    #endregion

    #region Create Sequence

    [ConditionalFact]
    public void Create_sequence()
        => Execute(
            _ => { },
            _ => { },
            builder => builder.HasSequence<int>("TestSequence"),
            upOps =>
            {
                Assert.Equal(1, upOps.Count);
                Assert.IsType<CreateSequenceOperation>(upOps[0]);
            },
            downOps =>
            {
                Assert.Equal(1, downOps.Count);
                var dropColumnOperation = Assert.IsType<DropSequenceOperation>(downOps[0]);
                Assert.Equal("TestSequence", dropColumnOperation.Name);
            });
    
    [ConditionalFact]
    public void Create_sequence_all_settings()
        => Execute(
            _ => { },
            _ => { },
            builder => builder.HasSequence<long>("TestSequence", "dbo2")
                .StartsAt(3)
                .IncrementsBy(2)
                .HasMin(2)
                .HasMax(916),
            upOps =>
            {
                Assert.Equal(2, upOps.Count);
                Assert.IsType<EnsureSchemaOperation>(upOps[0]);
                var createSequenceOperation = Assert.IsType<CreateSequenceOperation>(upOps[1]);
                Assert.Equal(3, createSequenceOperation.StartValue);
                Assert.Equal(2, createSequenceOperation.IncrementBy);
                Assert.Equal(2, createSequenceOperation.MinValue);
                Assert.Equal(916, createSequenceOperation.MaxValue);
            },
            downOps =>
            {
                Assert.Equal(1, downOps.Count);
                var dropColumnOperation = Assert.IsType<DropSequenceOperation>(downOps[0]);
                Assert.Equal("TestSequence", dropColumnOperation.Name);
            });
    
    [ConditionalFact]
    public void Create_sequence_and_dependent_column()
        => Execute(
            builder => builder.Entity("People").Property<int>("Id"),
            _ => { },
            builder =>
            {
                builder.HasSequence<int>("TestSequence");
                builder.Entity("People").Property<int>("SeqProp").HasDefaultValueSql("\"TestSequence\".nextval");
            },
            upOps =>
            {
                Assert.Equal(2, upOps.Count);
                Assert.IsType<CreateSequenceOperation>(upOps[0]);
                var addColumnOperation = Assert.IsType<AddColumnOperation>(upOps[1]);
                Assert.Equal("int", addColumnOperation.ColumnType);
                Assert.Equal("\"TestSequence\".nextval", addColumnOperation.DefaultValueSql);
            },
            downOps =>
            {
                Assert.Equal(2, downOps.Count);
                var dropColumnOperation = Assert.IsType<DropColumnOperation>(downOps[0]);
                Assert.Equal("SeqProp", dropColumnOperation.Name);
                
                var dropSequenceOperation = Assert.IsType<DropSequenceOperation>(downOps[1]);
                Assert.Equal("TestSequence", dropSequenceOperation.Name);
            });

    #endregion

    #region Create Index

    [ConditionalFact]
    public void Create_index_on_table()
        => Execute(
            _ => { },
            source => source.Entity("People", e =>
            {
                e.Property<int>("Id");
                e.Property<int>("age");
                e.ToTable(tb => tb.IsHybridTable());

            }),
            builder => builder.Entity("People", e =>
            {
                e.Property<int>("Id");
                e.Property<int>("age");
                e.ToTable(tb => tb.IsHybridTable());
                e.HasIndex("age");
            }),
            upOps =>
            {
                Assert.Equal(1, upOps.Count);
                var createIndexOperation = Assert.IsType<CreateIndexOperation>(upOps[0]);
                Assert.Equal("People", createIndexOperation.Table);
                Assert.Equal("age", createIndexOperation.Columns[0]);
            },
            downOps =>
            {
                Assert.Equal(1, downOps.Count);
                var dropIndexOperation = Assert.IsType<DropIndexOperation>(downOps[0]);
                Assert.Equal("People", dropIndexOperation.Table);
            });

    #endregion
    
    #region Drop Sequence
    
    [ConditionalFact]
    public void Drop_index_on_table()
        => Execute(
            _ => { },
            source => source.Entity("People", e =>
            {
                e.Property<int>("Id");
                e.Property<int>("age");
                e.ToTable(tb => tb.IsHybridTable());
                e.HasIndex("age");
            }),
            builder => builder.Entity("People", e =>
            {
                e.Property<int>("Id");
                e.Property<int>("age");
                e.ToTable(tb => tb.IsHybridTable());
            }),
            upOps =>
            {
                Assert.Equal(1, upOps.Count);
                var dropIndexOperation = Assert.IsType<DropIndexOperation>(upOps[0]);
                Assert.Equal("People", dropIndexOperation.Table);
            },
            downOps =>
            {
                Assert.Equal(1, downOps.Count);
                var dropIndexOperation = Assert.IsType<CreateIndexOperation>(downOps[0]);
                Assert.Equal("People", dropIndexOperation.Table);
                Assert.Equal("age", dropIndexOperation.Columns[0]);
            });

    #endregion

    #region Add Constraint

    [ConditionalFact]
    public void Add_primary_key()
        => Execute(
            builder => builder.Entity("People").Property<string>("SomeField").HasMaxLength(450).IsRequired(),
            _ => { },
            builder => builder.Entity("People").HasKey("SomeField"),
            upOps =>
            {
                Assert.Equal(1, upOps.Count);
                var addPrimaryKeyOperation = Assert.IsType<AddPrimaryKeyOperation>(upOps[0]);
                Assert.Equal("People", addPrimaryKeyOperation.Table);
                Assert.Equal("SomeField", addPrimaryKeyOperation.Columns[0]);
            },
            downOps =>
            {
                Assert.Equal(1, downOps.Count);
                var dropPrimaryKeyOperation = Assert.IsType<DropPrimaryKeyOperation>(downOps[0]);
                Assert.Equal("People", dropPrimaryKeyOperation.Table);
            });
    
    [ConditionalFact]
    public void Drop_primary_key()
        => Execute(
            builder => builder.Entity("People").Property<int>("SomeField"),
            builder => builder.Entity("People").HasKey("SomeField"),
            _ => { },
            upOps =>
            {
                Assert.Equal(2, upOps.Count);
                var dropConstraintOperation = Assert.IsType<DropPrimaryKeyOperation>(upOps[0]);
                Assert.Equal("People", dropConstraintOperation.Table);
                Assert.IsType<AlterColumnOperation>(upOps[1]);
            },
            downOps =>
            {
                Assert.Equal(2, downOps.Count);
                Assert.IsType<AlterColumnOperation>(downOps[0]);
                var addPrimaryKeyOperation = Assert.IsType<AddPrimaryKeyOperation>(downOps[1]);
                Assert.Equal("People", addPrimaryKeyOperation.Table);
                Assert.Equal("SomeField", addPrimaryKeyOperation.Columns[0]);
            });
    
    [ConditionalFact]
    public void Add_foreign_key()
        => Execute(
            builder =>
            {
                builder.Entity(
                    "Customers", e =>
                    {
                        e.Property<int>("Id");
                        e.HasKey("Id");
                    });
                builder.Entity(
                    "Orders", e =>
                    {
                        e.Property<int>("Id");
                        e.Property<int>("CustomerId");
                    });
            },
            _ => { },
            builder => 
                builder.Entity("Orders").HasOne("Customers").WithMany()
                    .HasForeignKey("CustomerId"),
            upOps =>
            {
                Assert.Equal(1, upOps.Count);
                var foreignKeyOperation = Assert.IsType<AddForeignKeyOperation>(upOps[0]);
                Assert.Equal("Customers", foreignKeyOperation.PrincipalTable);
                Assert.Equal("Orders", foreignKeyOperation.Table);
                Assert.Equal("Id", foreignKeyOperation.PrincipalColumns[0]);
                Assert.Equal("CustomerId", foreignKeyOperation.Columns[0]);
                
            },
            downOps =>
            {
                Assert.Equal(1, downOps.Count);
                var dropForeignKeyOperation = Assert.IsType<DropForeignKeyOperation>(downOps[0]);
                Assert.Equal("Orders", dropForeignKeyOperation.Table);
            });

    #endregion
    
    private void Execute(
        Action<ModelBuilder> buildSourceAction,
        Action<ModelBuilder> buildTargetAction,
        Action<IReadOnlyList<MigrationOperation>> assertAction,
        bool skipSourceConventions = false)
        => Execute(m => { }, buildSourceAction, buildTargetAction, assertAction, null, skipSourceConventions);

    private void Execute(
        Action<ModelBuilder> buildCommonAction,
        Action<ModelBuilder> buildSourceAction,
        Action<ModelBuilder> buildTargetAction,
        Action<IReadOnlyList<MigrationOperation>> assertAction,
        bool skipSourceConventions = false)
        => Execute(buildCommonAction, buildSourceAction, buildTargetAction, assertAction, null, skipSourceConventions);

    private void Execute(
        Action<ModelBuilder> buildCommonAction,
        Action<ModelBuilder> buildSourceAction,
        Action<ModelBuilder> buildTargetAction,
        Action<IReadOnlyList<MigrationOperation>> assertActionUp,
        Action<IReadOnlyList<MigrationOperation>> assertActionDown,
        bool skipSourceConventions = false)
        => Execute(
            buildCommonAction, buildSourceAction, buildTargetAction, assertActionUp, assertActionDown, null, skipSourceConventions);

    private void Execute(
        Action<ModelBuilder> buildCommonAction,
        Action<ModelBuilder> buildSourceAction,
        Action<ModelBuilder> buildTargetAction,
        Action<IReadOnlyList<MigrationOperation>> assertActionUp,
        Action<IReadOnlyList<MigrationOperation>> assertActionDown,
        Action<DbContextOptionsBuilder> builderOptionsAction,
        bool skipSourceConventions = false,
        bool enableSensitiveLogging = true)
    {
        var sourceModelBuilder = CreateModelBuilder(skipSourceConventions);
        buildCommonAction(sourceModelBuilder);
        buildSourceAction(sourceModelBuilder);

        var targetModelBuilder = CreateModelBuilder(skipConventions: false);
        buildCommonAction(targetModelBuilder);
        buildTargetAction(targetModelBuilder);

        var sourceModel = sourceModelBuilder.FinalizeModel(designTime: true, skipValidation: true);
        var targetModel = targetModelBuilder.FinalizeModel(designTime: true, skipValidation: true);

        var targetOptionsBuilder = TestHelpers
            .AddProviderOptions(new DbContextOptionsBuilder())
            .UseModel(targetModel);

        if (enableSensitiveLogging)
        {
            targetOptionsBuilder = targetOptionsBuilder.EnableSensitiveDataLogging();
        }

        if (builderOptionsAction != null)
        {
            builderOptionsAction(targetOptionsBuilder);
        }

        var modelDiffer = CreateModelDiffer(targetOptionsBuilder.Options);

        var operationsUp = modelDiffer.GetDifferences(sourceModel.GetRelationalModel(), targetModel.GetRelationalModel());
        assertActionUp(operationsUp);

        if (assertActionDown != null)
        {
            var operationsDown = modelDiffer.GetDifferences(targetModel.GetRelationalModel(), sourceModel.GetRelationalModel());
            assertActionDown(operationsDown);
        }

        var noopOperations = modelDiffer.GetDifferences(sourceModel.GetRelationalModel(), sourceModel.GetRelationalModel());
        Assert.Empty(noopOperations);

        noopOperations = modelDiffer.GetDifferences(targetModel.GetRelationalModel(), targetModel.GetRelationalModel());
        Assert.Empty(noopOperations);
    }
    
    private MigrationsModelDiffer CreateModelDiffer(DbContextOptions options)
    => (MigrationsModelDiffer)TestHelpers.CreateContext(options).GetService<IMigrationsModelDiffer>();
    
    private TestHelpers.TestModelBuilder CreateModelBuilder(bool skipConventions)
        => TestHelpers.CreateConventionBuilder(configureConventions: skipConventions ? c => c.RemoveAllConventions() : null);
}