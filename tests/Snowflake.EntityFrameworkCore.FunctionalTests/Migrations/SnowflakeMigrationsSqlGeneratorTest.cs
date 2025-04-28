using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.Extensions.DependencyInjection;
using Snowflake.EntityFrameworkCore.Infrastructure;
using Snowflake.EntityFrameworkCore.Internal;
using Snowflake.EntityFrameworkCore.Metadata;
using Snowflake.EntityFrameworkCore.Metadata.Internal;
using Snowflake.EntityFrameworkCore.Migrations.Operations;
using Snowflake.EntityFrameworkCore.TestUtilities;

namespace Snowflake.EntityFrameworkCore.FunctionalTests.Migrations;

public class SnowflakeMigrationsSqlGeneratorTest : MigrationsSqlGeneratorTestBase
{
    /// <inheritdoc />
    public SnowflakeMigrationsSqlGeneratorTest()
        : base(
            SnowflakeTestHelpers.Instance,
            new ServiceCollection(),
            SnowflakeTestHelpers.Instance.AddProviderOptions(
                ((IRelationalDbContextOptionsBuilderInfrastructure)
                    new SnowflakeDbContextOptionsBuilder(new DbContextOptionsBuilder()))
                .OptionsBuilder).Options)
    {
    }
    
    #region ALTER TABLE ADD FOREIGN KEY

    [ConditionalFact]
    public override void AddForeignKeyOperation_without_principal_columns()
    {
        base.AddForeignKeyOperation_without_principal_columns();
        AssertSql(
            """
            ALTER TABLE "People" ADD FOREIGN KEY ("SpouseId") REFERENCES "People";
            """);
    }
    
    [ConditionalTheory]
    [InlineData(ReferentialAction.Cascade)]
    [InlineData(ReferentialAction.Restrict)]
    [InlineData(ReferentialAction.SetNull)]
    [InlineData(ReferentialAction.SetDefault)]
    public virtual void AddForeignKeyOperation_with_on_delete(ReferentialAction referentialAction)
    {
        Generate(
            new AddForeignKeyOperation
            {
                Table = "People",
                Columns = new[] { "SpouseId" },
                PrincipalTable = "People",
                OnDelete = referentialAction,
            });
        
        AssertSql(
            """
            ALTER TABLE "People" ADD FOREIGN KEY ("SpouseId") REFERENCES "People" ON DELETE NO ACTION;
            """);
    }
    
    [ConditionalTheory]
    [InlineData(ReferentialAction.Cascade)]
    [InlineData(ReferentialAction.Restrict)]
    [InlineData(ReferentialAction.SetNull)]
    [InlineData(ReferentialAction.SetDefault)]
    public virtual void AddForeignKeyOperation_with_on_update(ReferentialAction referentialAction)
    {
        Generate(
            new AddForeignKeyOperation
            {
                Table = "People",
                Columns = new[] { "SpouseId" },
                PrincipalTable = "People",
                OnUpdate = referentialAction,
            });
        
        AssertSql(
            """
            ALTER TABLE "People" ADD FOREIGN KEY ("SpouseId") REFERENCES "People" ON UPDATE NO ACTION;
            """);
    }

    #endregion

    #region ALTER COLUMN

    [ConditionalFact (Skip = "Test would be addressed on SNOW-1769830")]
    public override void AlterColumnOperation_without_column_type()
    {
        // TODO
    }

    #endregion

    #region RENAME TABLE

    public override void RenameTableOperation()
    {
        base.RenameTableOperation();
        AssertSql(
            """
            ALTER TABLE "dbo"."People" RENAME TO "dbo"."Person";
            """);
    }

    public override void RenameTableOperation_legacy()
    {
        base.RenameTableOperation_legacy();
        AssertSql(
            """
            ALTER TABLE "dbo"."People" RENAME TO "dbo"."Person";
            """
            );
    }

    #endregion
    
    #region SQL OPERATIONS

    [ConditionalFact]
    public override void SqlOperation()
    {
        base.SqlOperation();
        AssertSql(
            """
            -- I <3 DDL
            """);
    }

    #endregion

    #region INSERT
    
    /// <inheritdoc />
    public override void InsertDataOperation_all_args_spatial()
    {
        // TODO: geospatial types should be supported
        Assert.Throws<InvalidOperationException>(() => base.InsertDataOperation_all_args_spatial());
    }
    
    /// <inheritdoc />
    public override void InsertDataOperation_required_args()
    {
        base.InsertDataOperation_required_args();
        AssertSql("INSERT INTO \"dbo\".\"People\" (\"First Name\")\nVALUES ('John');\n");
    }
    
    /// <inheritdoc />
    public override void InsertDataOperation_required_args_composite()
    {
        base.InsertDataOperation_required_args_composite();
        AssertSql("INSERT INTO \"dbo\".\"People\" (\"First Name\", \"Last Name\")\nVALUES ('John', 'Snow');\n") ;
    }
    
    /// <inheritdoc />
    public override void InsertDataOperation_required_args_multiple_rows()
    {
        base.InsertDataOperation_required_args_multiple_rows();
        AssertSql("""
                  INSERT INTO "dbo"."People" ("First Name")
                  VALUES ('John');
                  
                  INSERT INTO "dbo"."People" ("First Name")
                  VALUES ('Daenerys');
                  
                  """);
    }

    #endregion
    
    #region DELETE
    
    /// <inheritdoc />
    public override void DeleteDataOperation_all_args()
    {
        base.DeleteDataOperation_all_args();
        AssertSql("""
                  DELETE FROM "People"
                  WHERE "First Name" = 'Hodor';
                  DELETE FROM "People"
                  WHERE "First Name" = 'Daenerys';
                  DELETE FROM "People"
                  WHERE "First Name" = 'John';
                  DELETE FROM "People"
                  WHERE "First Name" = 'Arya';
                  DELETE FROM "People"
                  WHERE "First Name" = 'Harry';
                  
                  """);
    }
    
    /// <inheritdoc />
    public override void DeleteDataOperation_all_args_composite()
    {
        base.DeleteDataOperation_all_args_composite();
        AssertSql("""
                  DELETE FROM "People"
                  WHERE "First Name" = 'Hodor' AND "Last Name" IS NULL;
                  DELETE FROM "People"
                  WHERE "First Name" = 'Daenerys' AND "Last Name" = 'Targaryen';
                  DELETE FROM "People"
                  WHERE "First Name" = 'John' AND "Last Name" = 'Snow';
                  DELETE FROM "People"
                  WHERE "First Name" = 'Arya' AND "Last Name" = 'Stark';
                  DELETE FROM "People"
                  WHERE "First Name" = 'Harry' AND "Last Name" = 'Strickland';
                  
                  """);
    }
    
    /// <inheritdoc />
    public override void DeleteDataOperation_required_args()
    {
        base.DeleteDataOperation_required_args();
        AssertSql("""
                  DELETE FROM "People"
                  WHERE "Last Name" = 'Snow';
                  """);
    }
    
    /// <inheritdoc />
    public override void DeleteDataOperation_required_args_composite()
    {
        base.DeleteDataOperation_required_args_composite();
        AssertSql("""
                  DELETE FROM "People"
                  WHERE "First Name" = 'John' AND "Last Name" = 'Snow';
                  """);
    }

    #endregion

    #region UPDATE

    [ConditionalFact]
    public override void UpdateDataOperation_all_args()
    {
        base.UpdateDataOperation_all_args();
        AssertSql("""
UPDATE "People" SET "Birthplace" = 'Winterfell', "House Allegiance" = 'Stark', "Culture" = 'Northmen'
WHERE "First Name" = 'Hodor';
UPDATE "People" SET "Birthplace" = 'Dragonstone', "House Allegiance" = 'Targaryen', "Culture" = 'Valyrian'
WHERE "First Name" = 'Daenerys';
""");
    }

    [ConditionalFact]
    public override void UpdateDataOperation_all_args_composite()
    {
        base.UpdateDataOperation_all_args_composite();
        AssertSql("""
UPDATE "People" SET "House Allegiance" = 'Stark'
WHERE "First Name" = 'Hodor' AND "Last Name" IS NULL;
UPDATE "People" SET "House Allegiance" = 'Targaryen'
WHERE "First Name" = 'Daenerys' AND "Last Name" = 'Targaryen';
""");
    }

    [ConditionalFact]
    public override void UpdateDataOperation_all_args_composite_multi()
    {
        base.UpdateDataOperation_all_args_composite_multi();
        AssertSql("""
UPDATE "People" SET "Birthplace" = 'Winterfell', "House Allegiance" = 'Stark', "Culture" = 'Northmen'
WHERE "First Name" = 'Hodor' AND "Last Name" IS NULL;
UPDATE "People" SET "Birthplace" = 'Dragonstone', "House Allegiance" = 'Targaryen', "Culture" = 'Valyrian'
WHERE "First Name" = 'Daenerys' AND "Last Name" = 'Targaryen';
""");
    }

    [ConditionalFact]
    public override void UpdateDataOperation_all_args_multi()
    {
        base.UpdateDataOperation_all_args_multi();
        AssertSql("""
UPDATE "People" SET "Birthplace" = 'Dragonstone', "House Allegiance" = 'Targaryen', "Culture" = 'Valyrian'
WHERE "First Name" = 'Daenerys';
""");
    }

    [ConditionalFact]
    public override void UpdateDataOperation_required_args()
    {
        base.UpdateDataOperation_required_args();
        AssertSql("""
UPDATE "People" SET "House Allegiance" = 'Targaryen'
WHERE "First Name" = 'Daenerys';
""");
    }

    [ConditionalFact]
    public override void UpdateDataOperation_required_args_composite()
    {
        base.UpdateDataOperation_all_args_composite();
        AssertSql("""
UPDATE "People" SET "House Allegiance" = 'Stark'
WHERE "First Name" = 'Hodor' AND "Last Name" IS NULL;
UPDATE "People" SET "House Allegiance" = 'Targaryen'
WHERE "First Name" = 'Daenerys' AND "Last Name" = 'Targaryen';
""");
    }

    [ConditionalFact]
    public override void UpdateDataOperation_required_args_composite_multi()
    {
        base.UpdateDataOperation_required_args_composite_multi();
        AssertSql("""
UPDATE "People" SET "Birthplace" = 'Dragonstone', "House Allegiance" = 'Targaryen', "Culture" = 'Valyrian'
WHERE "First Name" = 'Daenerys' AND "Last Name" = 'Targaryen';
""");
    }

    [ConditionalFact]
    public override void UpdateDataOperation_required_args_multi()
    {
        base.UpdateDataOperation_required_args_multi();
        AssertSql("""
UPDATE "People" SET "Birthplace" = 'Dragonstone', "House Allegiance" = 'Targaryen', "Culture" = 'Valyrian'
WHERE "First Name" = 'Daenerys';
""");
    }

    [ConditionalFact]
    public override void UpdateDataOperation_required_args_multiple_rows()
    {
        base.UpdateDataOperation_required_args_multiple_rows();
        AssertSql("""
UPDATE "People" SET "House Allegiance" = 'Stark'
WHERE "First Name" = 'Hodor';
UPDATE "People" SET "House Allegiance" = 'Targaryen'
WHERE "First Name" = 'Daenerys';
""");
    }

    #endregion

    #region DEFAULT VALUE

    [ConditionalTheory (Skip = "Test would be addressed on SNOW-1775591")]
    [InlineData(false)]
    [InlineData(true)]
    public override void DefaultValue_with_line_breaks(bool isUnicode)
    {
        // TODO
    }

    [ConditionalTheory (Skip = "Test would be addressed on SNOW-1775591")]
    [InlineData(false)]
    [InlineData(true)]
    public override void DefaultValue_with_line_breaks_2(bool isUnicode)
    {
        // TODO
    }

    #endregion

    #region RESTART SEQUENCE
    
    [InlineData(3L)]
    [InlineData(null)]
    public override void Sequence_restart_operation(long? startsAt)
    {
        var exception = Assert.Throws<NotSupportedException>(() => base.Sequence_restart_operation(startsAt));
        Assert.Equal(SnowflakeStrings.RestartSequenceNotSupported, exception.Message);
    }

    #endregion

    #region RENAME SEQUENCE

    [ConditionalFact]
    public virtual void RenameSequence()
    {
        Generate(
            new RenameSequenceOperation()
            {
                Name = "Seq1",
                Schema = "demo",
                NewName = "Seq2",
                NewSchema = "demo2"
            }
        );

        AssertSql(
            """
        ALTER SEQUENCE "demo"."Seq1" RENAME TO "demo2"."Seq2";
        """);
    }

    [ConditionalFact]
    public virtual void RenameSequence_with_no_newSchema()
    {
        Generate(
            new RenameSequenceOperation()
            {
                Name = "Seq1",
                Schema = "demo",
                NewName = "Seq2",
            }
        );

        AssertSql(
            """
        ALTER SEQUENCE "demo"."Seq1" RENAME TO "demo"."Seq2";
        """);
    }

    [ConditionalFact]
    public virtual void RenameSequence_with_no_newName()
    {
        Generate(
            new RenameSequenceOperation()
            {
                Name = "Seq1",
                Schema = "demo",
                NewSchema = "demo2",
            }
        );

        AssertSql(
            """
        ALTER SEQUENCE "demo"."Seq1" RENAME TO "demo2"."Seq1";
        """);
    }

    [ConditionalFact]
    public async Task RenameSequence_with_no_newName_and_no_newSchema()
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                    Generate(
                    new RenameSequenceOperation()
                    {
                        Name = "Seq1",
                        Schema = "demo",
                    }
                );

            }
            );

        Assert.Equal(@"The operation requires a new name or schema.", exception.Message);

    }

    [ConditionalFact]
    public  async Task RenameSequence_with_same_parameters()
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                    Generate(
                        new RenameSequenceOperation()
                        {
                            Name = "Seq1",
                            Schema = "demo",
                            NewName = "Seq1",
                            NewSchema = "demo",
                        }
                );

            }
            );

        Assert.Equal(@"The operation requires a new name or schema.", exception.Message);

    }


    #endregion

    #region ALTER TABLE ADD COLUMN

    [ConditionalFact]
    public virtual void AddColumnOperation_identity_legacy()
    {
        Generate(
            new AddColumnOperation
            {
                Table = "People",
                Name = "Id",
                ClrType = typeof(int),
                ColumnType = "int",
                DefaultValue = 0,
                IsNullable = false,
                [SnowflakeAnnotationNames.ValueGenerationStrategy] =
                    SnowflakeValueGenerationStrategy.IdentityColumn
            });
        
        AssertSql(
            """
        ALTER TABLE "People" ADD "Id" int NOT NULL IDENTITY;
        """);
    }

    public override void AddColumnOperation_without_column_type()
    {
        base.AddColumnOperation_without_column_type();

        AssertSql(
            """
            ALTER TABLE "People" ADD "Alias" varchar NOT NULL;
            """);
    }

    public override void AddColumnOperation_with_unicode_no_model()
    {
        base.AddColumnOperation_with_unicode_no_model();

        AssertSql(
            """
ALTER TABLE "Person" ADD "Name" varchar NULL;
""");
    }

    public override void AddColumnOperation_with_fixed_length_no_model()
    {
        base.AddColumnOperation_with_fixed_length_no_model();

        AssertSql(
            """
ALTER TABLE "Person" ADD "Name" char(100) NULL;
""");
    }

    public override void AddColumnOperation_with_maxLength_no_model()
    {
        base.AddColumnOperation_with_maxLength_no_model();

        AssertSql(
            """
ALTER TABLE "Person" ADD "Name" nvarchar(30) NULL;
""");
    }

    public override void AddColumnOperation_with_maxLength_overridden()
    {
        base.AddColumnOperation_with_maxLength_overridden();

        AssertSql(
            """
ALTER TABLE "Person" ADD "Name" nvarchar(32) NULL;
""");
    }

    public override void AddColumnOperation_with_precision_and_scale_overridden()
    {
        base.AddColumnOperation_with_precision_and_scale_overridden();

        AssertSql(
            """
ALTER TABLE "Person" ADD "Pi" decimal(15,10) NOT NULL;
""");
    }

    public override void AddColumnOperation_with_precision_and_scale_no_model()
    {
        base.AddColumnOperation_with_precision_and_scale_no_model();

        AssertSql(
            """
ALTER TABLE "Person" ADD "Pi" decimal(20,7) NOT NULL;
""");
    }

    public override void AddColumnOperation_with_unicode_overridden()
    {
        base.AddColumnOperation_with_unicode_overridden();

        AssertSql(
            """
ALTER TABLE "Person" ADD "Name" varchar NULL;
""");
    }

    [ConditionalFact]
    public virtual void AddColumnOperation_with_rowversion_overridden()
    {
        Generate(
            modelBuilder => modelBuilder.Entity<Person>().Property<byte[]>("RowVersion"),
            new AddColumnOperation
            {
                Table = "Person",
                Name = "RowVersion",
                ClrType = typeof(byte[]),
                IsRowVersion = true,
                IsNullable = true
            });

        AssertSql(
            """
ALTER TABLE "Person" ADD "RowVersion" varbinary NULL;
""");
    }

    [ConditionalFact]
    public virtual void AddColumnOperation_with_rowversion_no_model()
    {
        Generate(
            new AddColumnOperation
            {
                Table = "Person",
                Name = "RowVersion",
                ClrType = typeof(byte[]),
                IsRowVersion = true,
                IsNullable = true
            });

        AssertSql(
            """
            ALTER TABLE "Person" ADD "RowVersion" varbinary NULL;
            """);
    }
    
    #endregion

    #region PrimaryKeyOperations
    
    [ConditionalFact]
    public virtual void AddPrimaryKeyOperation()
    {
        Generate(
            new AddPrimaryKeyOperation()
            {
                Table = "Person",
                Name = "PkCol",
                Columns = ["Col1"]
            });

        AssertSql(
            """
            ALTER TABLE "Person" ADD CONSTRAINT "PkCol" PRIMARY KEY ("Col1");
            """);
    }
    
    [ConditionalFact]
    public virtual void DropPrimaryKeyOperation()
    {
        Generate(
            new DropPrimaryKeyOperation()
            {
                Table = "Person",
                Name = "PkCol"
            });

        AssertSql(
            """
            ALTER TABLE "Person" DROP CONSTRAINT "PkCol";
            """);
    }

    #endregion

    #region ALTER TABLE DROP COLUMN

    [ConditionalFact]
    public virtual void DropColumnOperation()
    {
        Generate(
            new DropColumnOperation()
            {
                Table = "People",
                Name = "Id"
            });

        AssertSql(
            """
        ALTER TABLE "People" DROP COLUMN "Id";
        """);
    }

    [ConditionalFact]
    public virtual void DropColumnOperationWithSchema()
    {
        Generate(
            new DropColumnOperation()
            {
                Table = "People",
                Schema = "Schema",
                Name = "Id"
            });

        AssertSql(
            """
        ALTER TABLE "Schema"."People" DROP COLUMN "Id";
        """);
    }

    #endregion

    #region ALTER TABLE DROP COLUMN

    [ConditionalFact]
    public virtual void RenameColumnOperation()
    {
        Generate(
            new RenameColumnOperation()
            {
                Table = "People",
                Name = "Id",
                NewName = "Id2"
            });

        AssertSql(
            """
        ALTER TABLE "People" RENAME COLUMN "Id" TO "Id2";
        """);
    }

    [ConditionalFact]
    public virtual void RenameColumnOperationWithSchema()
    {
        Generate(
            new RenameColumnOperation()
            {
                Table = "People",
                Name = "Id",
                Schema = "schma",
                NewName = "Id2"
            });

        AssertSql(
            """
        ALTER TABLE "schma"."People" RENAME COLUMN "Id" TO "Id2";
        """);
    }

    #endregion

    #region ALTER TABLE ADD CHECK CONSTRAINT

    [ConditionalFact]
    public async Task AlterTable_AddCheckConstraint()
    {
        var exception = await Assert.ThrowsAsync<NotSupportedException>(() =>
        {
            Generate(
                new AddCheckConstraintOperation
                {
                    Name = "constraintName",
                    Table = "People",
                    Sql = "age >= 0"
                }
            );
            return Task.CompletedTask;
        });

        Assert.Contains(
            SnowflakeStrings.AddCheckConstraintNotSupported("constraintName", "People"),
            exception.Message);
    }
    
    [ConditionalFact]
    public async Task DropCheckConstraintTable()
    {
        var exception = await Assert.ThrowsAsync<NotSupportedException>(() =>
        {
            Generate(
                new DropCheckConstraintOperation()
                {
                    Name = "constraintToDropName",
                    Table = "People",
                }
            );
            return Task.CompletedTask;
        });

        Assert.Contains(
            SnowflakeStrings.DropCheckConstraintNotSupported("constraintToDropName", "People"),
            exception.Message);
    }

    #endregion
    
    #region CREATE TABLE

    [ConditionalFact]
    public virtual void CreateTable_basic()
    {
        // Regular standard table
        Generate(
            new CreateTableOperation
            {
                Name = "People",
                Columns =
                {
                    new AddColumnOperation
                    {
                        Name = "Id",
                        ClrType = typeof(int),
                        ColumnType = "int",
                        IsNullable = false,
                        [SnowflakeAnnotationNames.ValueGenerationStrategy] = SnowflakeValueGenerationStrategy.IdentityColumn
                    },
                    new AddColumnOperation
                    {
                        Name = "EmployerId",
                        ClrType = typeof(int),
                        ColumnType = "int",
                        IsNullable = true
                    },
                    new AddColumnOperation
                    {
                        Name = "Name",
                        ClrType = typeof(string),
                        ColumnType = "nvarchar(4000)",
                        IsNullable = true
                    }
                }
            });

        AssertSql("""
                  CREATE TABLE "People" (
                      "Id" int NOT NULL IDENTITY,
                      "EmployerId" int NULL,
                      "Name" nvarchar(4000) NULL);
                  """);
    }

    [ConditionalFact]
    public virtual void CreateHybridTable_with_pk()
    {
        Generate(new CreateTableOperation
        {
            Name = "People",
            Columns =
            {
                new AddColumnOperation()
                {
                    Name = "Id",
                    ClrType = typeof(int),
                    ColumnType = "int",
                    IsNullable = false,
                    [SnowflakeAnnotationNames.ValueGenerationStrategy] = SnowflakeValueGenerationStrategy.IdentityColumn
                }
            },
            PrimaryKey = new AddPrimaryKeyOperation
            {
                Columns = ["Id"],
                Name = "PK_People"
            },
            [SnowflakeAnnotationNames.HybridTable] = true,
        });

        AssertSql("""
                  CREATE HYBRID TABLE "People" (
                      "Id" int NOT NULL IDENTITY,
                      CONSTRAINT "PK_People" PRIMARY KEY ("Id"));
                  """);
    }
    
    #endregion

    #region ALTER DATABASE

    [ConditionalFact]
    public virtual void AlterDatabaseOperation_with_collation()
    {
        Generate(
            new AlterDatabaseOperation { Collation = "en-ci" }
        );
        AssertSql("""
                  BEGIN
                      LET db_name := '"' || REPLACE(CURRENT_DATABASE(), '"', '""') || '"';
                      ALTER DATABASE IDENTIFIER(:db_name) SET DEFAULT_DDL_COLLATION = 'en-ci';
                  END;
                  """);
    }

    #endregion
    
    # region CREATE DATABASE
    
    [ConditionalFact]
    public virtual void CreateDatabaseOperation()
    {
        Generate(
            new SnowflakeCreateDatabaseOperation() { Name = "Northwind" });

        AssertSql(
            "CREATE DATABASE Northwind;");
    }

    [ConditionalFact]
    public virtual void CreateDatabaseOperation_with_collation()
    {
        Generate(
            new SnowflakeCreateDatabaseOperation { Name = "Northwind", Collation = "en-ci" });

        AssertSql(
            """
        CREATE DATABASE Northwind
        DEFAULT_DDL_COLLATION = 'en-ci';
        """);
    }
    
    [ConditionalFact]
    public virtual void DropDatabaseOperation()
    {
        Generate(
            new SnowflakeDropDatabaseOperation() { Name = "Northwind", Cascade = false });

        AssertSql(
            "DROP DATABASE IF EXISTS Northwind;");
    }
    
    [ConditionalFact]
    public virtual void DropDatabaseOperation_with_cascade()
    {
        Generate(
            new SnowflakeDropDatabaseOperation { Name = "Northwind", Cascade = true });

        AssertSql(
            "DROP DATABASE IF EXISTS Northwind CASCADE;");
    }
    
    # endregion
    
    #region CREATE INDEX
    
    [ConditionalFact]
    public virtual void CreateAlterIndexOperation()
    {
        Generate(
            modelBuilder => modelBuilder.Entity<Person>().HasAnnotation(SnowflakeAnnotationNames.HybridTable, true),
            new CreateIndexOperation
            {
                Table = nameof(Person),
                Name = "Id",
                Columns = new[] { "Column1", "Column2" }
            }
        );
        
        AssertSql(
            """
        CREATE INDEX "Id" ON "Person" ("Column1", "Column2");
        """);
    }
    
    [ConditionalFact]
    public virtual void CreateAlterIndexOperationWithSchema()
    {
        Generate(
            modelBuilder => modelBuilder.Entity<Person>().
                HasAnnotation(SnowflakeAnnotationNames.HybridTable, true).Metadata.SetSchema("schm1")
            ,
            new CreateIndexOperation
            {
                Table = nameof(Person),
                Schema = "schm1",
                Name = "Id",
                Columns = new[] { "Column1", "Column2" }
            }
        );
        
        AssertSql(
            """
        CREATE INDEX "Id" ON "schm1"."Person" ("Column1", "Column2");
        """);
    }

     [ConditionalFact]
    public async Task Create_index_without_table_name()
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            Generate(
            modelBuilder => modelBuilder.Entity<Person>().
                HasAnnotation(SnowflakeAnnotationNames.HybridTable, true).Metadata.SetSchema("schm1")
            ,
            new CreateIndexOperation
            {
                Schema = "schm1",
                Name = "Id",
                Columns = new[] { "Column1", "Column2" }
            }
        );
        });

        Assert.Contains("""
Snowflake requires the table name to be specified for index operations. Specify table name in calls to 'MigrationBuilder.RenameIndex' and 'DropIndex'.
""", exception.Message);
    }
    
    [ConditionalFact]
    public virtual void CreateAlterIndexOperationWithUnsupportedOptions()
    {
        Generate(
            modelBuilder => modelBuilder.Entity<Person>().
                HasAnnotation(SnowflakeAnnotationNames.HybridTable, true).Metadata.SetSchema("schm1"),
            new CreateIndexOperation
            {
                Table = nameof(Person),
                Schema = "schm1",
                Name = "Index",
                Columns = new[] { "Column1", "Column2" },
                IsUnique = true,
                IsDescending = new[] { true, false },
            }
        );
        
        AssertSql(
            """
        CREATE INDEX "Index" ON "schm1"."Person" ("Column1", "Column2");
        """);
    }

    [ConditionalFact]
    public virtual async void RenameIndexOperations_notsupported()
    {
        var exception = await Assert.ThrowsAsync<NotSupportedException>(async () =>
        {
            Generate(
                modelBuilder => modelBuilder.Entity<Person>().HasAnnotation(SnowflakeAnnotationNames.HybridTable, true)
                    .Metadata.SetSchema("schm1"),
                new RenameIndexOperation()
                {
                    Table = nameof(Person),
                    Schema = "schm1",
                    Name = "IX_OldIndex",
                    NewName = "IX_NewIndex"
                }
            );
        });

        Assert.Contains("SQL generation for the operation 'RenameIndexOperation' is not supported by the current database provider. Database providers must implement the appropriate method in 'MigrationsSqlGenerator' to support this operation.", exception.Message);
        
    }
    
    
    #endregion
    
    #region DROP INDEX
    
    [ConditionalFact]
    public virtual void DropIndexOnTable()
    {
        Generate(
            modelBuilder => modelBuilder.Entity<Person>().HasAnnotation(SnowflakeAnnotationNames.HybridTable, true),
            new DropIndexOperation()
            {
                Table = nameof(Person),
                Name = "Id",
                
            }
        );
        
        AssertSql("DROP INDEX IF EXISTS \"Person\".\"Id\";");
    }
    
    [ConditionalFact]
    public virtual void DropIndexOnTableWithSchema()
    {
        Generate(
            modelBuilder => modelBuilder.Entity<Person>().ToTable(t=>t.IsHybridTable()).Metadata.SetSchema("schm"),
            new DropIndexOperation()
            {
                Table = nameof(Person),
                Schema = "schm",
                Name = "Id",
                
            }
        );
        
        AssertSql("DROP INDEX IF EXISTS \"schm\".\"Person\".\"Id\";");
    }
    
    #endregion

    # region ADD PRIMARY KEY

    [ConditionalFact]
    public async Task add_primary_key_to_hybrid_table()
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            Generate(
            modelBuilder => modelBuilder.Entity<Person>().HasAnnotation(SnowflakeAnnotationNames.HybridTable, true),
            new AddPrimaryKeyOperation()
            {
                Table = nameof(Person),
                Columns = ["id"]

            }
        );

        });

        Assert.Equal(@"Adding or dropping constraints is not supported in hybrid tables after table creation. Add primary key on 'Person' is an invalid operation.", exception.Message);

    }

    # endregion

    #region DROP SCHEMA

    [ConditionalFact]
    public virtual void DropSchemaOperation()
    {
        Generate(
            new DropSchemaOperation { Name = "Northwind" });

        AssertSql("DROP SCHEMA IF EXISTS Northwind RESTRICT;");
    }

    #endregion

    #region DROP TABLE

    [ConditionalFact]
    public virtual void DropTableOperation()
    {
        Generate(
            new DropTableOperation { Name = "Northwind" });

        AssertSql(
            "DROP TABLE IF EXISTS \"Northwind\" RESTRICT;");
    }

    #endregion
    
    #region DROP FOREIGN KEY

    [ConditionalFact]
    public virtual void DropForeignKeyOperation()
    {
        Generate(
            new DropForeignKeyOperation
            {
                Table = "People",
                Name = "FK_People_Companies"
            });

        AssertSql(
            "ALTER TABLE \"People\" DROP CONSTRAINT \"FK_People_Companies\";");
    }

    [ConditionalFact]
    public virtual void DropForeignKeyOperationOnHybridTable()
    {
        var exception = Assert.Throws<InvalidOperationException>(() =>
        {
            Generate(
                modelBuilder => modelBuilder.Entity<Person>().ToTable(t => t.IsHybridTable()),
                new DropForeignKeyOperation
                {
                    Table = "Person",
                    Name = "FK_People_Companies"
                });
        });

        Assert.Equal("Adding or dropping constraints is not supported in hybrid tables after table creation. Drop foreign key on 'Person' is an invalid operation.", exception.Message);
    }

    #endregion

    #region DROP UNIQUE CONSTRAINT

    [ConditionalFact]
    public virtual void DropUniqueConstraintOperation()
    {
        Generate(
            new DropUniqueConstraintOperation
            {
                Table = "People",
                Name = "UC_People_Name"
            });

        AssertSql(
            "ALTER TABLE \"People\" DROP CONSTRAINT \"UC_People_Name\";");
    }

    [ConditionalFact]
    public virtual void DropUniqueConstraintOperationOnHybridTable()
    {
        Generate(
            modelBuilder => modelBuilder.Entity<Person>().ToTable(t => t.IsHybridTable()),
            new DropUniqueConstraintOperation
            {
                Table = "Person",
                Name = "UC_People_Name"
            });

        AssertSql(
            "ALTER TABLE \"Person\" DROP CONSTRAINT \"UC_People_Name\";");
    }

    #endregion
    
    /// <inheritdoc />
    protected override string GetGeometryCollectionStoreType() => "geometry";
    
    /// <inheritdoc />
    protected override void Generate(
        Action<ModelBuilder> buildAction,
        MigrationOperation[] operation,
        MigrationsSqlGenerationOptions options)
    {
        IModel model = null;
        if (buildAction != null)
        {
            var modelBuilder = TestHelpers.CreateConventionBuilder();
            modelBuilder.Model.RemoveAnnotation(CoreAnnotationNames.ProductVersion);
            buildAction(modelBuilder);

            model = modelBuilder.FinalizeModel(designTime: true, skipValidation: true);
        }

        var services = ContextOptions != null
            ? TestHelpers.CreateContextServices(CustomServices, ContextOptions)
            : TestHelpers.CreateContextServices(CustomServices);
        var batch = services.GetRequiredService<IMigrationsSqlGenerator>().Generate(operation, model, options);

        Sql = string.Join(EOL, batch.Select(b => b.CommandText));
    }
}