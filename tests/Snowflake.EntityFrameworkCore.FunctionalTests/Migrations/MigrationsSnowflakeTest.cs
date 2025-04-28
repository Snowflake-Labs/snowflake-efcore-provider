using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Snowflake.EntityFrameworkCore.Diagnostics;
using Snowflake.EntityFrameworkCore.Internal;
using Snowflake.EntityFrameworkCore.Scaffolding.Internal;
using Snowflake.EntityFrameworkCore.Scaffolding.Metadata;
using Snowflake.EntityFrameworkCore.TestUtilities;
using Xunit.Abstractions;

namespace Snowflake.EntityFrameworkCore.FunctionalTests.Migrations;

public class MigrationsSnowflakeTest : MigrationsTestBase<MigrationsSnowflakeTest.MigrationsSnowflakeFixture>
{
    public MigrationsSnowflakeTest(MigrationsSnowflakeFixture fixture, ITestOutputHelper testOutputHelper) 
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }
    
    /// <inheritdoc />
    protected override string NonDefaultCollation => "en_us";

    // Snowflake doesn't provide the virtual columns values.
    protected override bool AssertComputedColumns => false;

    #region ALTER TABLE

    public override async Task Alter_table_add_comment()
    {
        await base.Alter_table_add_comment();
        AssertSql(
            """
COMMENT IF EXISTS ON TABLE "People" IS 'Table comment';
""");
    }

    public override async Task Alter_table_add_comment_non_default_schema()
    {
        await base.Alter_table_add_comment_non_default_schema();
        AssertSql(
            """
COMMENT IF EXISTS ON TABLE "SomeOtherSchema"."People" IS 'Table comment';
""");
    }

    public override async Task Alter_table_change_comment()
    {
        await base.Alter_table_change_comment();
        AssertSql(
            """
ALTER TABLE "People" UNSET COMMENT;
""",
            """
COMMENT IF EXISTS ON TABLE "People" IS 'Table comment2';
""");
    }

    #endregion
    
    #region ADD COLUMN
    
    [ConditionalFact]
    public override async Task Add_required_primitve_collection_with_custom_default_value_sql_to_existing_table()
    {
        await Add_required_primitve_collection_with_custom_default_value_sql_to_existing_table_core("'[3, 2, 1]'");

        AssertSql(
            """
            ALTER TABLE "Customers" ADD "Numbers" varchar NOT NULL DEFAULT ('[3, 2, 1]');
            """);
    }

    public override async Task Add_column_computed_with_collation(bool stored)
    {
        await Test(
            builder => builder.Entity("People").Property<int>("Id"),
            _ => { },
            builder => builder.Entity("People").Property<string>("Name")
                .HasComputedColumnSql("'hello'", stored)
                .UseCollation(NonDefaultCollation),
            model =>
            {
                var table = Assert.Single(model.Tables);
                Assert.Equal(2, table.Columns.Count);
            });

        AssertSql(
            """
ALTER TABLE "People" ADD "Name" varchar AS ('hello');
""");
    }

    public override async Task Add_column_shared()
    {
        await base.Add_column_shared();

        AssertSql();
    }

    public override async Task Add_column_with_ansi()
    {
        await Test(
            builder => builder.Entity("People").Property<int>("Id"),
            _ => { },
            builder => builder.Entity("People").Property<string>("Name").IsUnicode(false),
            model =>
            {
                var table = Assert.Single(model.Tables);
                var column = Assert.Single(table.Columns, c => c.Name == "Name");
                // TODO: SnowflakeTypeMappingSource needs to be reviewed since all the string types return TEXT
                Assert.Equal("VARCHAR", column.StoreType); 
                Assert.True(column.IsNullable);
            });

        AssertSql(
            """
            ALTER TABLE "People" ADD "Name" varchar NULL;
            """);
    }

    public override async Task Add_column_with_check_constraint()
    {
        await Assert.ThrowsAsync<NotSupportedException>(()=>base.Add_column_with_check_constraint());
    }

    public override async Task Add_column_with_collation()
    {
        await base.Add_column_with_collation();
        
        AssertSql(
            """
ALTER TABLE "People" ADD "Name" varchar COLLATE 'en_us' NULL;
""");
    }

    public override async Task Add_column_with_comment()
    {
        await base.Add_column_with_comment();

        AssertSql(
            """
ALTER TABLE "People" ADD "FullName" varchar NULL COMMENT 'My comment';
""");
    }

    public override async Task Add_column_with_fixed_length()
    {
        await Test(
            builder => builder.Entity("People").Property<int>("Id"),
            builder => { },
            builder => builder.Entity("People").Property<string>("Name")
                .IsFixedLength()
                .HasMaxLength(100),
            model =>
            {
                var table = Assert.Single(model.Tables);
                var column = Assert.Single(table.Columns, c => c.Name == "Name");
                Assert.Equal(
                    // TODO: SnowflakeTypeMappingSource needs to be reviewed since all the string types return TEXT
                    "VARCHAR",
                    column.StoreType);
            });
        
        AssertSql(
            """
            ALTER TABLE "People" ADD "Name" nchar(100) NULL;
            """);
    }

    public override async Task Add_column_with_max_length()
    {
        await Test(
            builder => builder.Entity("People").Property<int>("Id"),
            _ => { },
            builder => builder.Entity("People").Property<string>("Name").HasMaxLength(30),
            model =>
            {
                var table = Assert.Single(model.Tables);
                var column = Assert.Single(table.Columns, c => c.Name == "Name");
                // TODO: SnowflakeTypeMappingSource needs to be reviewed since all the string types return TEXT
                Assert.Equal("VARCHAR", column.StoreType);
            });
        
        AssertSql(
            """
            ALTER TABLE "People" ADD "Name" nvarchar(30) NULL;
            """);
    }

    public override async Task Add_column_with_max_length_on_derived()
    {
        await Test(
            builder =>
            {
                builder.Entity("Person");
                builder.Entity(
                    "SpecialPerson", e =>
                    {
                        e.HasBaseType("Person");
                        e.Property<string>("Name").HasMaxLength(30);
                    });

                builder.Entity("MoreSpecialPerson").HasBaseType("SpecialPerson");
            },
            builder => { },
            builder => builder.Entity("Person").Property<string>("Name").HasMaxLength(30),
            model =>
            {
                var table = Assert.Single(model.Tables, t => t.Name == "Person");
                var column = Assert.Single(table.Columns, c => c.Name == "Name");
                // TODO: SnowflakeTypeMappingSource needs to be reviewed since all the string types return TEXT
                Assert.Equal("VARCHAR", column.StoreType);
            });
        
        Assert.Empty(Fixture.TestSqlLoggerFactory.SqlStatements);
    }

    public override async Task Add_column_with_required()
    {
        await Test(
            builder => builder.Entity("People").Property<int>("Id"),
            _ => { },
            builder => builder.Entity("People").Property<string>("Name").IsRequired(),
            model =>
            {
                var table = Assert.Single(model.Tables);
                var column = Assert.Single(table.Columns, c => c.Name == "Name");
                // TODO: SnowflakeTypeMappingSource needs to be reviewed since all the string types return TEXT
                Assert.Equal("VARCHAR", column.StoreType);
                Assert.False(column.IsNullable);
            });
        AssertSql(
            """
            ALTER TABLE "People" ADD "Name" varchar NOT NULL DEFAULT '';
            """);
    }
    
    /// <inheritdoc />
    public override async Task Add_column_with_unbounded_max_length()
    {
        await Test(
            builder =>
            {
                var entity = builder.Entity("People");
                    entity.Property<int>("Id");
            },
            _ => { },
            builder => builder.Entity("People").Property<string>("Name").HasMaxLength(-1),
            model =>
            {
                var table = Assert.Single(model.Tables);
                Assert.Single(table.Columns, c => c.Name == "Name");
            });
        
        AssertSql(
            """
ALTER TABLE "People" ADD "Name" varchar NULL;
""");
    }

    public override async Task Add_column_with_computedSql(bool? stored)
    {
        await Test(
            builder => builder.Entity(
                "People", e =>
                {
                    e.Property<int>("Id");
                    e.Property<int>("X");
                    e.Property<int>("Y");
                }),
            _ => { },
            builder => builder.Entity("People").Property<string>("Sum")
                .HasComputedColumnSql($"{DelimitIdentifier("X")} || {DelimitIdentifier("Y")}", stored),
            model =>
            {
                var table = Assert.Single(model.Tables);
                Assert.Single(table.Columns, c => c.Name == "Sum");
            });
        
        AssertSql(
            """
            ALTER TABLE "People" ADD "Sum" varchar AS ("X" || "Y");
            """);
    }

    [ConditionalFact]
    public override async Task Add_column_with_defaultValue_datetime()
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await Test(
                builder => builder.Entity("People").Property<int>("Id"),
                _ => { },
                builder => builder.Entity("People").Property<DateTime>("Birthday")
                    .HasDefaultValue(new DateTime(2015, 4, 12, 17, 5, 0)),
                model => { });
        });

        AssertSql(); // No SQL should be generated/run
        Assert.Equal(SnowflakeStrings.DefaultValueInAddColumnNotSupported("timestamp_ntz"), exception.Message);
    }

    public override async Task Add_column_with_defaultValue_string()
    {
        await base.Add_column_with_defaultValue_string();
        
        AssertSql(
            """
            ALTER TABLE "People" ADD "Name" varchar NOT NULL DEFAULT 'John Doe';
            """);
    }

    public override async Task Add_column_with_defaultValueSql()
    {
        await Test(
            builder => builder.Entity("People").Property<int>("Id"),
            _ => { },
            builder => builder.Entity("People").Property<int>("Sum")
                // Snowflake don't support arithmetic operations as default values.
                .HasDefaultValueSql("1"),
            model =>
            {
                var table = Assert.Single(model.Tables);
                Assert.Equal(2, table.Columns.Count);
                var sumColumn = Assert.Single(table.Columns, c => c.Name == "Sum");
                Assert.Contains("1", sumColumn.DefaultValueSql);
            });
        
        AssertSql(
            """
            ALTER TABLE "People" ADD "Sum" int NOT NULL DEFAULT (1);
            """);
    }

    #endregion

    #region DROP COLUMN

    [ConditionalFact]
    public virtual async Task alter_column_drop_column()
    {
        await Test(
            _ => { }
            ,
            builder =>
            {

               builder.Entity(
                "People", e =>
                {
                    e.Property<int>("Id");
                    e.HasKey("Id");
                    e.Property<string>("Name");
                });

            },
            builder =>
            {
                builder.Entity(
                    "People", e =>
                    {
                        e.Property<int>("Id");
                        e.HasKey("Id");
                    });
            },
            model =>
            {
                var table = Assert.Single(model.Tables);
                Assert.Equal(1, table.Columns.Count);
            });

        AssertSql(
            """
ALTER TABLE "People" DROP COLUMN "Name";
""");
    }

    [ConditionalFact]
    public virtual async Task alter_column_drop_column_with_pk()
    {
        await Test(
            _ => { }
            ,
            builder =>
            {

               builder.Entity(
                "People", e =>
                {
                    e.Property<int>("Id");
                    e.HasKey("Id");
                    e.Property<string>("Name");
                });

            },
            builder =>
            {
                builder.Entity(
                    "People", e =>
                    {
                        e.Property<string>("Name");
                    });
            },
            model =>
            {
                var table = Assert.Single(model.Tables);
                Assert.Equal(1, table.Columns.Count);
            });

        AssertSql(
            """
ALTER TABLE "People" DROP CONSTRAINT "PK_People";
""",
            """
ALTER TABLE "People" DROP COLUMN "Id";
""");
    }

    [ConditionalFact]
    public async Task alter_column_drop_column_with_pk_in_hybrid_table()
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await Test(
                _ =>
                {

                },
                builder =>
                {
                    builder.Entity(
                        "People", e =>
                        {
                            e.Property<int>("Id");
                            e.HasKey("Id");
                            e.Property<int>("Name");
                            e.ToTable(tb => tb.IsHybridTable());
                        });
                },
                builder =>
                {
                    builder.Entity(
                        "People", e =>
                        {
                            e.Property<int>("Name");
                            e.ToTable(tb => tb.IsHybridTable());
                        });
                },
                model =>
                {
                });
            AssertSql(); // No SQL should be generated/run
        });
        Assert.Equal("The entity type 'People' marked as a HYBRID TABLE does not have a primary key defined.", exception.Message);

    }


    #endregion

    #region RENAME COLUMN

    [ConditionalFact]
    public async Task alter_column_rename_column_with_pk_in_hybrid_table()
    {
                await Test(
            _ => { }
            ,
            builder =>
            {

               builder.Entity(
                "People", e =>
                {
                    e.Property<int>("Id");
                    e.HasKey("Id");
                    e.Property<string>("Name");
                    e.ToTable(tb => tb.IsHybridTable());
                });

            },
            builder =>
            {
                builder.Entity(
                    "People", e =>
                    {
                        e.Property<int>("Id2");
                        e.HasKey("Id2");
                        e.Property<string>("Name");
                        e.ToTable(tb => tb.IsHybridTable());
                    });
            },
            model =>
            {
                var table = Assert.Single(model.Tables);
                Assert.Equal(2, table.Columns.Count);

                var ordersTable = Assert.Single(model.Tables, t => t.Name == "People");
                Assert.Single(ordersTable.Columns, c => c.Name == "Id2");
            });

        AssertSql(
            """
ALTER TABLE "People" RENAME COLUMN "Id" TO "Id2";
""");
    }

    [ConditionalFact]
    public async Task alter_column_rename_column_with_fk_in_hybrid_table()
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await Test(
            _ => { }
            ,
            builder =>
            {
              builder.Entity(
                        "Customers", e =>
                        {
                            e.Property<int>("Id");
                            e.HasKey("Id");
                            e.ToTable(tb => tb.IsHybridTable());
                        });
                    builder.Entity(
                        "Orders", e =>
                        {
                            e.Property<int>("Id");
                            e.ToTable(tb => tb.IsHybridTable());
                            e.HasOne("Customers").WithMany()
                                .HasForeignKey("CustomerId");
                        });
            },
            builder =>
            {
              builder.Entity(
                        "Customers", e =>
                        {
                            e.Property<int>("Id");
                            e.HasKey("Id");
                            e.ToTable(tb => tb.IsHybridTable());
                        });
                    builder.Entity(
                        "Orders", e =>
                        {
                            e.Property<int>("Id");
                            e.ToTable(tb => tb.IsHybridTable());
                            e.HasOne("Customers").WithMany()
                                .HasForeignKey("CustomerId2");
                        });
            },
            _ => { });
        });

        AssertSql(); // No SQL should be generated/run
        Assert.Contains("""
Adding or dropping constraints is not supported in hybrid tables after table creation. Drop foreign key on 'Orders' is an invalid operation.
""", exception.Message);
    }


    #endregion

    #region CREATE SEQUENCE

    public override async Task Create_sequence()
    {
        await base.Create_sequence();
        
        AssertSql(
            """
CREATE SEQUENCE IF NOT EXISTS "TestSequence" START WITH 1 INCREMENT BY 1 ORDER;
""");
    }
    
    /// <inheritdoc />
    public override async Task Create_sequence_all_settings()
    {
        await Test(
            _ => { },
            builder => builder.HasSequence<long>("TestSequence", "dbo2")
                .StartsAt(3)
                .IncrementsBy(2)
                .HasMin(2)
                .HasMax(916),
            model =>
            {
                var sequence = Assert.Single(model.Sequences);
                Assert.Equal("TestSequence", sequence.Name);
                Assert.Equal("dbo2", sequence.Schema);
                Assert.Equal(3, sequence.StartValue);
                Assert.Equal(2, sequence.IncrementBy);
                // Snowflake already has predefined min and max values for sequences
                // user cannot modify them with the statement.
                Assert.Equal(-9223372036854775808, sequence.MinValue);
                Assert.Equal(9223372036854775807, sequence.MaxValue);
            });
        
        AssertSql(
            """
CREATE SCHEMA IF NOT EXISTS "dbo2";
""",
            """
CREATE SEQUENCE IF NOT EXISTS "dbo2"."TestSequence" START WITH 3 INCREMENT BY 2 ORDER;
""");
    }

    public override async Task Create_sequence_long()
    {
        await base.Create_sequence_long();

        AssertSql(
            """
CREATE SEQUENCE IF NOT EXISTS "TestSequence" START WITH 1 INCREMENT BY 1 ORDER;
""");
    }

    public override async Task Create_sequence_short()
    {
        await base.Create_sequence_short();

        AssertSql(
            """CREATE SEQUENCE IF NOT EXISTS "TestSequence" START WITH 1 INCREMENT BY 1 ORDER;""");
    }
    
    [ConditionalFact]
    public async Task Create_sequence_and_dependent_column()
    {
        await Test(
            builder => builder.Entity("People").Property<int>("Id"),
            _ => { },
            builder =>
            {
                builder.HasSequence<int>("TestSequence");
                builder.Entity("People").Property<int>("SeqProp").HasDefaultValueSql("\"TestSequence\".nextval");
            },
            model =>
            {
                var sequence = Assert.Single(model.Sequences);
                Assert.Equal("TestSequence", sequence.Name);
            });

        AssertSql(
            """
CREATE SEQUENCE IF NOT EXISTS "TestSequence" START WITH 1 INCREMENT BY 1 ORDER;
""",
            //
            """
ALTER TABLE "People" ADD "SeqProp" int NOT NULL DEFAULT ("TestSequence".nextval);
""");
    }
    
    

    #endregion

    #region RENAME SEQUENCE

    [ConditionalFact]
    public override async Task Rename_sequence()
    {
        await base.Rename_sequence();

        AssertSql(
            """
            ALTER SEQUENCE "TestSequence" RENAME TO "testsequence";
            """);
    }

    public override async Task Move_sequence()
    {
        await base.Move_sequence();
        AssertSql(
            """
CREATE SCHEMA IF NOT EXISTS "TestSequenceSchema";
""",
            //
            """
ALTER SEQUENCE PUBLIC."TestSequence" RENAME TO "TestSequenceSchema"."TestSequence";
""");
    }

    #endregion

    #region RENAME TABLE

    public override async Task Move_table()
    {
        await base.Move_table();
        
        AssertSql(
            """
CREATE SCHEMA IF NOT EXISTS "TestTableSchema";
""",
            """
ALTER TABLE PUBLIC."TestTable" RENAME TO "TestTableSchema"."TestTable";
""");
    }

    #endregion

    #region CREATE TABLE
    
    /// <inheritdoc />
    public override async Task Create_table()
    {
        await base.Create_table();
        AssertSql(
            """
CREATE TABLE "People" (
    "Id" int NOT NULL IDENTITY,
    "Name" varchar NULL,
    CONSTRAINT "PK_People" PRIMARY KEY ("Id"));
""");
    }

    public override async Task Create_table_all_settings()
    {
        var char11StoreType = "VARCHAR";

        await Test(
            builder => builder.Entity(
                "Employers", e =>
                {
                    e.Property<int>("Id");
                    e.HasKey("Id");
                }),
            _ => { },
            builder => builder.Entity("People", e =>
            {
                e.ToTable("People" , tb => { tb.HasComment("Table comment"); });
                e.Property<int>("CustomId");
                e.Property<int>("EmployerId")
                    .HasComment("Employer ID comment");
                e.Property<string>("SSN")
                    .HasColumnType(char11StoreType)
                    .UseCollation(NonDefaultCollation)
                    .IsRequired(false);
                e.HasKey("CustomId");
                e.HasAlternateKey("SSN");
                e.HasOne("Employers").WithMany("People").HasForeignKey("EmployerId"); 
            })
            ,
            model =>
            {
                var employersTable = Assert.Single(model.Tables, t => t.Name == "Employers");
                var peopleTable = Assert.Single(model.Tables, t => t.Name == "People");

                Assert.Equal("People", peopleTable.Name);
                if (AssertSchemaNames)
                {
                    Assert.Equal("PUBLIC", peopleTable.Schema);
                }

                Assert.Collection(
                    peopleTable.Columns.OrderBy(c => c.Name),
                    c =>
                    {
                        Assert.Equal("CustomId", c.Name);
                        Assert.False(c.IsNullable);
                        Assert.Equal("NUMBER(38, 0)", c.StoreType);
                        Assert.Null(c.Comment);
                    },
                    c =>
                    {
                        Assert.Equal("EmployerId", c.Name);
                        Assert.False(c.IsNullable);
                        Assert.Equal("NUMBER(38, 0)", c.StoreType);

                        if (AssertComments)
                        {
                            Assert.Equal("Employer ID comment", c.Comment);
                        }
                    },
                    c =>
                    {
                        Assert.Equal("SSN", c.Name);
                        Assert.False(c.IsNullable);
                        Assert.Equal(char11StoreType, c.StoreType);
                        Assert.Null(c.Comment);
                    });

                Assert.Same(
                    peopleTable.Columns.Single(c => c.Name == "CustomId"),
                    Assert.Single(peopleTable.PrimaryKey!.Columns));
                
                var foreignKey = Assert.Single(peopleTable.ForeignKeys);
                Assert.Same(peopleTable, foreignKey.Table);
                Assert.Same(peopleTable.Columns.Single(c => c.Name == "EmployerId"), Assert.Single(foreignKey.Columns));
                Assert.Same(employersTable, foreignKey.PrincipalTable);
                Assert.Same(employersTable.Columns.Single(), Assert.Single(foreignKey.PrincipalColumns));

                if (AssertComments)
                {
                    Assert.Equal("Table comment", peopleTable.Comment);
                }
            });
        
        AssertSql(
            """
CREATE TABLE "People" (
    "CustomId" int NOT NULL IDENTITY,
    "EmployerId" int NOT NULL COMMENT 'Employer ID comment',
    "SSN" VARCHAR COLLATE 'en_us' NOT NULL,
    CONSTRAINT "PK_People" PRIMARY KEY ("CustomId"),
    CONSTRAINT "AK_People_SSN" UNIQUE ("SSN"),
    CONSTRAINT "FK_People_Employers_EmployerId" FOREIGN KEY ("EmployerId") REFERENCES "Employers" ("Id") ON DELETE NO ACTION)
COMMENT='Table comment';
""");
    }

    public override async Task Create_table_no_key()
    {
        await base.Create_table_no_key();
        
        AssertSql(
            """
CREATE TABLE "Anonymous" (
    "SomeColumn" int NOT NULL);
""");
    }

    public override async Task Create_table_with_comments()
    {
        await base.Create_table_with_comments();
        
        AssertSql(
            """
CREATE TABLE "People" (
    "Id" int NOT NULL IDENTITY,
    "Name" varchar NULL COMMENT 'Column comment',
    CONSTRAINT "PK_People" PRIMARY KEY ("Id"))
COMMENT='Table comment';
""");
    }

    public override async Task Create_table_with_complex_type_with_required_properties_on_derived_entity_in_TPH()
    {
        await Test(
            builder => { },
            builder =>
            {
                builder.Entity(
                    "Contact", e =>
                    {
                        e.Property<int>("Id").ValueGeneratedOnAdd();
                        e.HasKey("Id");
                        e.Property<string>("Name");
                        e.ToTable("Contacts");
                    });
                builder.Entity(
                    "Supplier", e =>
                    {
                        e.HasBaseType("Contact");
                        e.Property<int>("Number");
                        e.ComplexProperty<MyComplex>("MyComplex", ct =>
                        {
                            ct.ComplexProperty<MyNestedComplex>("MyNestedComplex").IsRequired();
                        });
                    });
            },
            model =>
            {
                var contactsTable = Assert.Single(model.Tables.Where(t => t.Name == "Contacts"));
                Assert.Collection(
                    contactsTable.Columns,
                    c => Assert.Equal("Id", c.Name),
                    c => Assert.Equal("Discriminator", c.Name),
                    c =>
                    {
                        Assert.Equal("MyComplex_MyNestedComplex_Bar", c.Name);
                        Assert.Equal(true, c.IsNullable);
                    },
                    c =>
                    {
                        Assert.Equal("MyComplex_MyNestedComplex_Foo", c.Name);
                        Assert.Equal(true, c.IsNullable);
                    },
                    c =>
                    {
                        Assert.Equal("MyComplex_Prop", c.Name);
                        Assert.Equal(true, c.IsNullable);
                    },
                    c => Assert.Equal("Name", c.Name),
                    c => Assert.Equal("Number", c.Name));
            });
        
        AssertSql(
            """
CREATE TABLE "Contacts" (
    "Id" int NOT NULL IDENTITY,
    "Discriminator" nvarchar(8) NOT NULL,
    "MyComplex_MyNestedComplex_Bar" timestamp_ntz NULL,
    "MyComplex_MyNestedComplex_Foo" int NULL,
    "MyComplex_Prop" varchar NULL,
    "Name" varchar NULL,
    "Number" int NULL,
    CONSTRAINT "PK_Contacts" PRIMARY KEY ("Id"));
""");
    }
    
    /// <inheritdoc />
    public override async Task Create_table_with_computed_column(bool? stored)
    {
        await Test(
            builder => { },
            builder => builder.Entity(
                "People", e =>
                {
                    e.Property<int>("Id");
                    e.Property<int>("X");
                    e.Property<int>("Y");
                    e.Property<int>("Sum").HasComputedColumnSql(
                        $"{DelimitIdentifier("X")} + {DelimitIdentifier("Y")}",
                        stored);
                }),
            model =>
            {
                var table = Assert.Single(model.Tables);
                var sumColumn = Assert.Single(table.Columns, c => c.Name == "Sum");
                if (AssertComputedColumns)
                {
                    Assert.Contains("X", sumColumn.ComputedColumnSql);
                    Assert.Contains("Y", sumColumn.ComputedColumnSql);
                    if (stored != null)
                    {
                        Assert.Equal(stored, sumColumn.IsStored);
                    }
                }
            });
        
        AssertSql(
            """
CREATE TABLE "People" (
    "Id" int NOT NULL IDENTITY,
    "X" int NOT NULL,
    "Y" int NOT NULL,
    "Sum" int AS ("X" + "Y"),
    CONSTRAINT "PK_People" PRIMARY KEY ("Id"));
""");
    }

    public override async Task Create_table_with_multiline_comments()
    {
        await base.Create_table_with_multiline_comments();
        
        AssertSql(
            """
CREATE TABLE "People" (
    "Id" int NOT NULL IDENTITY,
    "Name" varchar NULL COMMENT 'This is a multi-line
column comment.
More information can
be found in the docs.',
    CONSTRAINT "PK_People" PRIMARY KEY ("Id"))
COMMENT='This is a multi-line
table comment.
More information can
be found in the docs.';
""");
    }

    public override async Task Create_table_with_optional_primitive_collection()
    {
        await base.Create_table_with_optional_primitive_collection();
        
        AssertSql(
            """
CREATE TABLE "Customers" (
    "Id" int NOT NULL IDENTITY,
    "Name" varchar NULL,
    "Numbers" varchar NULL,
    CONSTRAINT "PK_Customers" PRIMARY KEY ("Id"));
""");
    }

    public override async Task Create_table_with_required_primitive_collection()
    {
        await base.Create_table_with_required_primitive_collection();
        
        AssertSql(
            """
CREATE TABLE "Customers" (
    "Id" int NOT NULL IDENTITY,
    "Name" varchar NULL,
    "Numbers" varchar NOT NULL,
    CONSTRAINT "PK_Customers" PRIMARY KEY ("Id"));
""");
    }

    [ConditionalFact]
    public async Task Create_hybrid_table_with_auto_generated_pk()
    {
        // The PK constraint is auto generated.
        await Test(
            _ => { },
            _ => { },
            builder => builder.Entity("People", e =>
            {
                e.Property<int>("Id");
                e.ToTable(tb => tb.IsHybridTable());
            }),
            model => Assert.IsType<DatabaseHybridTable>(Assert.Single(model.Tables)));

        AssertSql(
            """
CREATE HYBRID TABLE "People" (
    "Id" int NOT NULL IDENTITY,
    CONSTRAINT "PK_People" PRIMARY KEY ("Id"));
""");
    }

    [ConditionalFact]
    public async Task Create_hybrid_table_with_no_pk()
    {
        // The PK constraint is not present, SnowflakeHybridTableAttributeConvention
        // throws an exception if the PK is not present, this test validates that.
        
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await Test(
                _ => { },
                _ => { },
                builder => builder.Entity("People", e =>
                {
                    e.Property<int>("col1");
                    e.ToTable(tb => tb.IsHybridTable());
                }),
                _ => { }
            );
        });

        AssertSql(); // No SQL should be generated/run
    }
    
    [ConditionalTheory]
    [InlineData(true)]
    [InlineData(false)]
    public Task Create_table_column_defaultValue_datetime(bool isHybridTable)
        => Test(
            builder => builder.Entity("People", e =>
            {
                e.Property<int>("Id");
                e.Property<DateTime>("Birthday").HasDefaultValue(new DateTime(2025,1, 9, 13, 55, 0));
                if (isHybridTable)
                {
                    e.ToTable(t => t.IsHybridTable());
                }
            }),
            _ => { },
            _ => { },
            model =>
            {
                var table = Assert.Single(model.Tables);
                Assert.Equal(2, table.Columns.Count);
                var birthdayColumn = Assert.Single(table.Columns, c => c.Name == "Birthday");
                Assert.True(birthdayColumn.DefaultValue?.Equals(new DateTime(2025,1, 9, 13, 55, 0)));
                Assert.False(birthdayColumn.IsNullable);
            });
    
    [ConditionalTheory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Create_table_column_defaultValueSql(bool isHybridTable)
    {
        await Test(
            builder => builder.Entity("People", e =>
            {
                e.Property<int>("Id");
                e.Property<int>("Sum").HasDefaultValueSql("1+2");
                if (isHybridTable)
                {
                    e.ToTable(t => t.IsHybridTable());
                }
            }),
            _ => { },
            _ => { },
            model =>
            {
                var table = Assert.Single(model.Tables);
                Assert.Equal(2, table.Columns.Count);
                var sumColumn = Assert.Single(table.Columns, c => c.Name == "Sum");
                Assert.Contains("1", sumColumn.DefaultValueSql);
                Assert.Contains("+", sumColumn.DefaultValueSql);
                Assert.Contains("2", sumColumn.DefaultValueSql);
            });
    }

    #endregion
    
    #region ALTER DATABASE

    [ConditionalFact]
    public async Task Alter_database_with_collation()
    {
        await Test(
            _ => { },
            builder => { builder.UseCollation("en-cs"); },
            builder => { builder.UseCollation(this.NonDefaultCollation); },
            model =>
            {
                Assert.Equal(this.NonDefaultCollation, model.Collation);
            });

        AssertSql(
            $"""
BEGIN
    LET db_name := '"' || REPLACE(CURRENT_DATABASE(), '"', '""') || '"';
    ALTER DATABASE IDENTIFIER(:db_name) SET DEFAULT_DDL_COLLATION = '{this.NonDefaultCollation}';
END;
""");
    }

    #endregion
    
    # region ENSURE SCHEMA

    public override async Task Create_schema()
    {
        await base.Create_schema();
        
        AssertSql(
            """
CREATE SCHEMA IF NOT EXISTS "SomeOtherSchema";
""",
            //
            """
CREATE TABLE "SomeOtherSchema"."People" (
    "Id" int NOT NULL IDENTITY,
    CONSTRAINT "PK_People" PRIMARY KEY ("Id"));
""");
    }

    # endregion

    # region ALTER COLUMN

    public override async Task Alter_column_add_comment()
    {
        await base.Alter_column_add_comment();

        AssertSql(
            """
COMMENT IF EXISTS ON COLUMN "People"."Id" IS 'Some comment';
""");
    }

    public override async Task Alter_column_change_comment()
    {
        await base.Alter_column_change_comment();

        AssertSql(
            """
ALTER TABLE "People" ALTER COLUMN "Id" UNSET COMMENT;
""",
            //
            """
COMMENT IF EXISTS ON COLUMN "People"."Id" IS 'Some comment2';
""");
    }

    public override async Task Alter_column_change_computed()
    {
        await base.Alter_column_change_computed();

        AssertSql(
            """
ALTER TABLE "People" DROP COLUMN "Sum";
""",
            """
ALTER TABLE "People" ADD "Sum" int AS ("X" - "Y");
""");
    }

    public override async Task Alter_column_change_computed_recreates_indexes()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(() => base.Alter_column_change_computed_recreates_indexes());
    }

    public override async Task Alter_column_change_type()
    {
        await Test(
            builder => builder.Entity("People").Property<int>("Id"),
            builder => builder.Entity("People").Property<int>("SomeColumn"),
            builder => builder.Entity("People").Property<long>("SomeColumn"),
            model =>
            {
                var table = Assert.Single(model.Tables);
                var column = Assert.Single(table.Columns, c => c.Name == "SomeColumn");
                Assert.Equal("NUMBER(38, 0)", column.StoreType);
            });

        AssertSql(
            """
ALTER TABLE "People" ALTER COLUMN "SomeColumn" SET DATA TYPE bigint;
""");
    }

    public override async Task Alter_column_make_computed(bool? stored)
    {
        await base.Alter_column_make_computed(stored);

        AssertSql(
                """
ALTER TABLE "People" DROP COLUMN "Sum";
""",
                """
ALTER TABLE "People" ADD "Sum" int AS ("X" + "Y");
""");

    }

    public override async Task Alter_column_make_non_computed()
    {
        await base.Alter_column_make_non_computed();

        AssertSql(
            """
ALTER TABLE "People" DROP COLUMN "Sum";
""",
            //
            """
ALTER TABLE "People" ADD "Sum" int NOT NULL;
""");
    }

    public override async Task Alter_column_make_required()
    {
        await base.Alter_column_make_required();

        AssertSql(
            """
UPDATE "People" SET "SomeColumn" = '' WHERE "SomeColumn" IS NULL;
""",
            //
            """
ALTER TABLE "People" ALTER COLUMN "SomeColumn" SET NOT NULL;
""");
    }

    [ConditionalFact]
    public async Task AlterTable_RenameTo()
    {
        await Test(
            builder => builder.Entity("People", b =>
            {
                b.Property<int>("test");
                b.HasKey("test").HasName("PK_test");
                
            }),
            builder => { },
        builder => builder.Entity("People").ToTable("Person"),
            model =>
            {
                var table = Assert.Single(model.Tables);
                Assert.Equal("Person", table.Name);
            });
        AssertSql(
            """
ALTER TABLE "People" RENAME TO "Person";
""");
    }

    [ConditionalFact]
    public async Task AlterTable_RenameTo_same_name()
    {
        await Test(
            builder => builder.Entity("People", b =>
            {
                b.Property<int>("test");
                b.HasKey("test").HasName("PK_test");
                
            }),
            builder => { },
            builder => builder.Entity("People").ToTable("People"),
            model =>
            {
                var table = Assert.Single(model.Tables);
                Assert.Equal("People", table.Name);
            });
        AssertSql();
    }
    
    [ConditionalFact]
    public async Task AlterTable_RenameTo_Hybrid()
    {
        await Test(
            builder => builder.Entity("People", b =>
            {
                b.Property<int>("test");
                b.HasKey("test").HasName("PK_test");
                b.ToTable(tb => tb.IsHybridTable());
            }),
            builder => { },
            builder => builder.Entity("People").ToTable("Person"),
            model =>
            {
                var table = Assert.Single(model.Tables);
                Assert.Equal("Person", table.Name);
            });
        AssertSql(
            """
            ALTER TABLE "People" RENAME TO "Person";
            """);
    }

    public override async Task Alter_column_make_required_with_composite_index()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(() => base.Alter_column_make_required_with_composite_index());
    }

    public override async Task Alter_column_make_required_with_index()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(() => base.Alter_column_make_required_with_index());
    }

    [ConditionalFact]
    public async Task Alter_column_add_identity()
    {
        var ex = await TestThrows<NotSupportedException>(
            builder => builder.Entity(
                "People", e =>
                {
                    e.Property<string>("Id");
                    e.Property<int>("IdentityColumn");
                }),
            builder => builder.Entity("People").Property<int>("IdentityColumn").UseIdentityColumn());

        Assert.Equal(SnowflakeStrings.AlterIdentityColumn, ex.Message);
    }

    [ConditionalFact]
    public async Task Alter_column_drop_identity()
    {
        await Test(
            builder => builder.Entity(
                "People", e =>
                {
                    e.Property<string>("Id");
                    e.Property<int>("IdentityColumn").UseIdentityColumn();
                }),
            builder => builder.Entity(
                "People", e =>
                {
                    e.Property<string>("Id");
                    e.Property<int>("IdentityColumn");
                }),
            model =>
            {
                var table = Assert.Single(model.Tables);
                var column = Assert.Single(table.Columns, c => c.Name == "IdentityColumn");
                Assert.Null(column.ValueGenerated);
            });

        AssertSql(
            """
ALTER TABLE "People" ALTER COLUMN "IdentityColumn" DROP DEFAULT;
""");
    }

    [ConditionalFact(Skip = "Test would be addressed in SNOW-1769856")]
    public override async Task Alter_column_make_required_with_null_data()
    {
        await base.Alter_column_make_required_with_null_data();
    }

    public override async Task Alter_column_remove_comment()
    {
        await base.Alter_column_remove_comment();

        AssertSql(
            """
ALTER TABLE "People" ALTER COLUMN "Id" UNSET COMMENT;
""");
    }

    public override async Task Alter_column_reset_collation()
    {
        await Assert.ThrowsAsync<NotSupportedException>(() => base.Alter_column_reset_collation());
    }

    public override async Task Alter_column_set_collation()
    {
        await Assert.ThrowsAsync<NotSupportedException>(() => base.Alter_column_set_collation());
    }

    public override async Task Alter_computed_column_add_comment()
    {
        await base.Alter_computed_column_add_comment();

        AssertSql(
            """
COMMENT IF EXISTS ON COLUMN "People"."SomeColumn" IS 'Some comment';
""");
    }

    # endregion

    #region DROP TABLE

    public override async Task Drop_table()
    {
        await base.Drop_table();

        AssertSql(
            """
DROP TABLE IF EXISTS "People" RESTRICT;
""");
    }

    #endregion

    #region CREATE INDEX

    [ConditionalFact]
    public async Task Create_index_on_table()
    {
        // The PK constraint is auto generated.
        await Test(
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
            model => Assert.Equal(2, model.Tables[0].Indexes.Count)
        );

        AssertSql(
            """
            CREATE INDEX "IX_People_age" ON "People" ("age");
            """);
    }


    [ConditionalFact]
    public override async Task Create_index()
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => base.Create_index());
        AssertSql(); // No SQL should be generated/run
        Assert.Contains("""
Snowflake only support indexes within Hybrid Tables. The index 'IX_People_FirstName' on the table 'People' is invalid.
""", exception.Message);
    }

    public override async Task Create_index_descending()
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => base.Create_index_descending());
        AssertSql(); // No SQL should be generated/run
        Assert.Contains("""
                        Snowflake only support indexes within Hybrid Tables. The index 'IX_People_X' on the table 'People' is invalid.
                        """, exception.Message);
    }

    public override async Task Create_index_descending_mixed()
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => base.Create_index_descending_mixed());
        AssertSql(); // No SQL should be generated/run
        Assert.Contains("""
                        Snowflake only support indexes within Hybrid Tables. The index 'IX_People_X_Y_Z' on the table 'People' is invalid.
                        """, exception.Message);
    }

    public override async Task Create_index_unique()
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => base.Create_index_unique());
        AssertSql(); // No SQL should be generated/run
        Assert.Contains("""
                        Snowflake only support indexes within Hybrid Tables. The index 'IX_People_FirstName_LastName' on the table 'People' is invalid.
                        """, exception.Message);
    }

    public override async Task Create_index_with_filter()
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => base.Create_index_with_filter());
        AssertSql(); // No SQL should be generated/run
        Assert.Contains("""
                        Snowflake only support indexes within Hybrid Tables. The index 'IX_People_Name' on the table 'People' is invalid.
                        """, exception.Message);
    }

    public override async Task Create_unique_index_with_filter()
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => base.Create_unique_index_with_filter());
        AssertSql(); // No SQL should be generated/run
        Assert.Contains("""
                        Snowflake only support indexes within Hybrid Tables. The index 'IX_People_Name' on the table 'People' is invalid.
                        """, exception.Message);
    }

    [ConditionalFact]
    public async Task Create_index_with_include_option()
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await Test(
            _ => { },
            source => source.Entity("People", e =>
            {
                e.Property<int>("Id");
                e.Property<int>("age");
                e.Property<int>("name");

            }),
            builder => builder.Entity("People", e =>
            {
                e.Property<int>("Id");
                e.Property<int>("age");
                e.Property<int>("name");
                e.HasIndex("age").IncludeProperties("name");
            }),
            _ => { }
        );
        });

        AssertSql(); // No SQL should be generated/run
        Assert.Contains("""
Snowflake only support indexes within Hybrid Tables. The index 'IX_People_age' on the table 'People' is invalid.
""", exception.Message);
    }

    [ConditionalFact]
    public async Task Create_index_with_unsupported_options()
    {
        await Test(
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
                e.HasIndex("age").IsUnique().IsDescending();
            }),
            _ => { }
        );

        Assert.True(
            Fixture.TestSqlLoggerFactory.Log.Exists(item => item.Id == SnowflakeEventId.MigrationOperationOptionNotSupported)
        );


        AssertSql("CREATE INDEX \"IX_People_age\" ON \"People\" (\"age\");");
    }

    public override async Task Alter_index_change_sort_order()
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => base.Alter_index_change_sort_order());
        Assert.Equal(SnowflakeStrings.IndexOnlySupportedWithinHybridTables("IX_People_X_Y_Z", "People"), exception.Message);
    }
    
    /// <inheritdoc />
    public override async Task Alter_index_make_unique()
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => base.Alter_index_change_sort_order());
        Assert.Equal(SnowflakeStrings.IndexOnlySupportedWithinHybridTables("IX_People_X_Y_Z", "People"), exception.Message);
    }

    public override async Task Rename_index()
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => base.Rename_index());

        Assert.Equal(SnowflakeStrings.IndexOnlySupportedWithinHybridTables("Foo", "People"), exception.Message);
    }
    
    [ConditionalFact]
    public async Task Rename_index_hybridtable()
    {
        await Test(
            builder => builder.Entity(
                "People", e =>
                {
                    e.Property<int>("Id");
                    e.Property<int>("FirstName");
                    e.ToTable(tb => tb.IsHybridTable());
                }),
            builder => builder.Entity("People").HasIndex(new[] { "FirstName" }, "Foo"),
            builder => builder.Entity("People").HasIndex(new[] { "FirstName" }, "foo"),
            model =>
            {
                var table = Assert.Single(model.Tables);
                var index = Assert.Single(table.Indexes, i => i.Name != "SYS_INDEX_People_PRIMARY");
                Assert.Equal("foo", index.Name);
            });

        AssertSql(
            """
DROP INDEX IF EXISTS "People"."Foo";
""",
            //
            """
CREATE INDEX "foo" ON "People" ("FirstName");
""");
    }

    #endregion

    #region DROP INDEX

    public override async Task Drop_index()
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => base.Drop_index());
        AssertSql();
        Assert.Contains(SnowflakeStrings.IndexOnlySupportedWithinHybridTables("IX_People_SomeField", "People"), exception.Message);
    }

    [ConditionalFact]
    public async Task Drop_index_on_table()
    {
        // The PK constraint is auto generated.
        await Test(
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
            model => Assert.Equal(1, model.Tables[0].Indexes.Count)
        );

        AssertSql(
            """
DROP INDEX IF EXISTS "People"."IX_People_age";
""");
    }

    #endregion

    # region PRIMARY KEY OPERATIONS

    public override async Task Add_primary_key_int()
    {
        var exception = await Assert.ThrowsAsync<NotSupportedException>(() => base.Add_primary_key_int());

        Assert.Equal(SnowflakeStrings.AlterIdentityColumn, exception.Message);
    }

    public override async Task Add_primary_key_string()
    {
        await Test(
            builder => builder.Entity("People").Property<string>("SomeField").HasMaxLength(450).IsRequired(),
            builder => { },
            builder => builder.Entity("People").HasKey("SomeField"),
            model =>
            {
                var table = Assert.Single(model.Tables);
                var primaryKey = table.PrimaryKey;
                Assert.NotNull(primaryKey);
                Assert.Same(table, primaryKey!.Table);
                Assert.Same(table.Columns.Single(), Assert.Single(primaryKey.Columns));
                if (AssertConstraintNames)
                {
                    Assert.Equal("PK_People", primaryKey.Name);
                }
            });

        AssertSql(
            """
ALTER TABLE "People" ADD CONSTRAINT "PK_People" PRIMARY KEY ("SomeField");
""");
    }

    public override async Task Add_primary_key_with_name()
    {
        await Test(
            builder => builder.Entity("People").Property<string>("SomeField").HasMaxLength(450),
            builder => { },
            builder => builder.Entity("People").HasKey("SomeField").HasName("PK_Foo"),
            model =>
            {
                var table = Assert.Single(model.Tables);
                var primaryKey = table.PrimaryKey;
                Assert.NotNull(primaryKey);
                Assert.Same(table, primaryKey!.Table);
                Assert.Same(table.Columns.Single(), Assert.Single(primaryKey.Columns));
                if (AssertConstraintNames)
                {
                    Assert.Equal("PK_Foo", primaryKey.Name);
                }
            });

        AssertSql(
            """
UPDATE "People" SET "SomeField" = '' WHERE "SomeField" IS NULL;
""",
            //
            """
ALTER TABLE "People" ALTER COLUMN "SomeField" SET NOT NULL;
""",
            //
            """
ALTER TABLE "People" ADD CONSTRAINT "PK_Foo" PRIMARY KEY ("SomeField");
""");
    }

    public override async Task Add_primary_key_composite_with_name()
    {
        await base.Add_primary_key_composite_with_name();

        AssertSql(
            """
ALTER TABLE "People" ADD CONSTRAINT "PK_Foo" PRIMARY KEY ("SomeField1", "SomeField2");
""");
    }

    [ConditionalFact]
    public virtual async Task Change_primary_key_int_standardtable()
    {
        await Test(
            builder =>
            {
                builder.Entity("People").Property<int>("SomeField");
            },
            builder => { builder.Entity("People").HasKey("SomeField"); },
            builder =>
            {
                builder.Entity("People").Property<int>("SomeField2");
                builder.Entity("People").HasKey("SomeField2");
            },
            model =>
            {
                var table = Assert.Single(model.Tables);
                var primaryKey = table.PrimaryKey;
                Assert.NotNull(primaryKey);
                Assert.Same(table, primaryKey!.Table);
                Assert.Same(table.Columns.FirstOrDefault(c => c.Name == "SomeField2"), Assert.Single(primaryKey.Columns));
                if (AssertConstraintNames)
                {
                    Assert.Equal("PK_People", primaryKey.Name);
                }
            }
        );

        AssertSql(
            """
            ALTER TABLE "People" DROP CONSTRAINT "PK_People";
            """,
            //
            """
            ALTER TABLE "People" ALTER COLUMN "SomeField" DROP DEFAULT;
            """,
            //
            """
            ALTER TABLE "People" ADD "SomeField2" int NOT NULL IDENTITY;
            """,
            //
            """
            ALTER TABLE "People" ADD CONSTRAINT "PK_People" PRIMARY KEY ("SomeField2");
            """);
    }

    [ConditionalFact]
    public virtual async Task Add_primary_key_int_standardtable()
    {
        await Test(
            builder =>
            {
                builder.Entity("People").Property<int>("SomeField");
            },
            _ => {},
            builder =>
            {
                builder.Entity("People").Property<int>("SomeField2");
                builder.Entity("People").HasKey("SomeField2");
            },
            model =>
            {
                var table = Assert.Single(model.Tables);
                var primaryKey = table.PrimaryKey;
                Assert.NotNull(primaryKey);
                Assert.Same(table, primaryKey!.Table);
                Assert.Same(table.Columns.FirstOrDefault(c => c.Name == "SomeField2"), Assert.Single(primaryKey.Columns));
                if (AssertConstraintNames)
                {
                    Assert.Equal("PK_People", primaryKey.Name);
                }
            }
        );

        AssertSql(
            """
ALTER TABLE "People" ADD "SomeField2" int NOT NULL IDENTITY;
""",
            //
            """
ALTER TABLE "People" ADD CONSTRAINT "PK_People" PRIMARY KEY ("SomeField2");
""");
    }

    [ConditionalFact]
    public virtual async Task Change_primary_key_int_hybrid()
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () => await Test(
            builder =>
            {
                builder.Entity("People").Property<int>("SomeField");
                builder.Entity("People", t => t.ToTable(h => h.IsHybridTable()));
            },
            builder => { builder.Entity("People").HasKey("SomeField"); },
            builder =>
            {
                builder.Entity("People").Property<int>("SomeField2");
                builder.Entity("People").HasKey("SomeField2");
            },
            _ => { }));

        Assert.Equal(@"Adding or dropping constraints is not supported in hybrid tables after table creation. Drop primary key on 'People' is an invalid operation.", exception.Message);
    }

    [ConditionalFact]
    public virtual async Task Drop_primary_key_int_hybrid()
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () => await Test(
            builder =>
            {
                builder.Entity("People").Property<int>("SomeField");
                builder.Entity("People", t => t.ToTable(h => h.IsHybridTable()));
            },
            builder => { builder.Entity("People").HasKey("SomeField"); },
            builder =>
            {
                builder.Entity("People").Property<int>("SomeField2");
            },
            _ => { }));
        Assert.Equal(@"The entity type 'People' marked as a HYBRID TABLE does not have a primary key defined.", exception.Message);
    }

    public override async Task Drop_primary_key_int()
    {
        await base.Drop_primary_key_int();


        AssertSql(
            """
ALTER TABLE "People" DROP CONSTRAINT "PK_People";
""",
            //
            """
ALTER TABLE "People" ALTER COLUMN "SomeField" DROP DEFAULT;
""");
    }

    public override async Task Drop_primary_key_string()
    {
        await Test(
            builder => builder.Entity("People").Property<string>("SomeField").HasMaxLength(450).IsRequired(),
            builder => builder.Entity("People").HasKey("SomeField"),
            builder => { },
            model => Assert.Null(Assert.Single(model.Tables).PrimaryKey));

        AssertSql(
            """
            ALTER TABLE "People" DROP CONSTRAINT "PK_People";
            """);
    }

        [ConditionalFact]
    public async Task drop_primary_key_to_hybrid_table()
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await Test(
                _ =>
                {

                },
                builder =>
                {
                    builder.Entity(
                        "Customers", e =>
                        {
                            e.Property<int>("Id");
                            e.Property<int>("Name");
                            e.HasKey("Id");
                            e.ToTable(tb => tb.IsHybridTable());
                        });
                },
                builder =>
                {
                    builder.Entity(
                        "Customers", e =>
                        {
                            e.Property<int>("Id");
                            e.Property<int>("Name");
                            e.HasKey("Name");
                            e.ToTable(tb => tb.IsHybridTable());
                        });
                },
                model =>
                {
                });
            AssertSql(); // No SQL should be generated/run
        });

        Assert.Equal(@"Adding or dropping constraints is not supported in hybrid tables after table creation. Drop primary key on 'Customers' is an invalid operation.", exception.Message);

    }

    #endregion

    # region ADD FOREIGN KEY

    [ConditionalFact]
    public override Task Add_foreign_key()
        => Test(
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
            builder => { },
            builder =>

    builder.Entity("Orders").HasOne("Customers").WithMany()
                .HasForeignKey("CustomerId"),
            model =>
            {
                var customersTable = Assert.Single(model.Tables, t => t.Name == "Customers");
                var ordersTable = Assert.Single(model.Tables, t => t.Name == "Orders");
                var foreignKey = ordersTable.ForeignKeys.Single();
                if (AssertConstraintNames)
                {
                    Assert.Equal("FK_Orders_Customers_CustomerId", foreignKey.Name);
                }

                Assert.Equal(ReferentialAction.NoAction, foreignKey.OnDelete);
                Assert.Same(customersTable, foreignKey.PrincipalTable);
                Assert.Same(customersTable.Columns.Single(), Assert.Single(foreignKey.PrincipalColumns));
                Assert.Equal("CustomerId", Assert.Single(foreignKey.Columns).Name);
            });

    [ConditionalFact]
    public override async Task Add_foreign_key_with_name()
    {
        await base.Add_foreign_key_with_name();
        AssertSql(
            """
            ALTER TABLE "Orders" ADD CONSTRAINT "FK_Foo" FOREIGN KEY ("CustomerId") REFERENCES "Customers" ("Id") ON DELETE NO ACTION;
            """);
    }

    [ConditionalTheory]
    [InlineData(DeleteBehavior.Cascade)]
    [InlineData(DeleteBehavior.Restrict)]
    [InlineData(DeleteBehavior.SetNull)]
    public async Task Add_foreign_key_with_delete_behavior(DeleteBehavior deleteBehavior)
    {
        await Test(
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
            builder => { },
            builder => builder.Entity("Orders").HasOne("Customers").WithMany()
                .HasForeignKey("CustomerId").OnDelete(deleteBehavior),
            model =>
            {
                var ordersTable = Assert.Single(model.Tables, t => t.Name == "Orders");
                var foreignKey = ordersTable.ForeignKeys.Single();
                Assert.Equal(ReferentialAction.NoAction, foreignKey.OnDelete);
            });
        
        Assert.True(
            Fixture.TestSqlLoggerFactory.Log.Exists(item => item.Id == SnowflakeEventId.MigrationOperationOptionNotSupported)
        );
        AssertSql(
            """
            ALTER TABLE "Orders" ADD CONSTRAINT "FK_Orders_Customers_CustomerId" FOREIGN KEY ("CustomerId") REFERENCES "Customers" ("Id") ON DELETE NO ACTION;
            """);
    }

    [ConditionalFact]
    public async Task Add_foreign_key_to_hybrid_table()
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await Test(
                builder =>
                {
                    builder.Entity(
                        "Customers", e =>
                        {
                            e.Property<int>("Id");
                            e.HasKey("Id");
                            e.ToTable(tb => tb.IsHybridTable());
                        });
                    builder.Entity(
                        "Orders", e =>
                        {
                            e.Property<int>("Id");
                            e.Property<int>("CustomerId");
                            e.ToTable(tb => tb.IsHybridTable());
                        });
                },
                _ => { },
                builder => builder.Entity("Orders").HasOne("Customers").WithMany()
                    .HasForeignKey("CustomerId"),
                model =>
                {
                });
            AssertSql(); // No SQL should be generated/run
        });

        Assert.Equal(@"Adding or dropping constraints is not supported in hybrid tables after table creation. Add foreign key on 'Orders' is an invalid operation.", exception.Message);

    }


    # endregion

    # region DROP FOREIGN KEY

    public override Task Drop_foreign_key()
        => Test(
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
            builder => builder.Entity("Orders").HasOne("Customers").WithMany().HasForeignKey("CustomerId"),
            builder => { },
            model =>
            {
                var customersTable = Assert.Single(model.Tables, t => t.Name == "Customers");
                Assert.Empty(customersTable.ForeignKeys);
                AssertSql("ALTER TABLE \"Orders\" DROP CONSTRAINT \"FK_Orders_Customers_CustomerId\";");
            });

    [ConditionalFact]
    public async Task Drop_foreign_key_to_hybrid_table()
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await Test(
                _ =>
                {

                },
                builder =>
                {
                    builder.Entity(
                        "Customers", e =>
                        {
                            e.Property<int>("Id");
                            e.HasKey("Id");
                            e.ToTable(tb => tb.IsHybridTable());
                        });
                    builder.Entity(
                        "Orders", e =>
                        {
                            e.Property<int>("Id");
                            e.Property<int>("CustomerId");
                            e.ToTable(tb => tb.IsHybridTable());
                            e.HasOne("Customers").WithMany()
                                .HasForeignKey("CustomerId");
                        });
                },
                builder =>
                {
                    builder.Entity(
                        "Customers", e =>
                        {
                            e.Property<int>("Id");
                            e.HasKey("Id");
                            e.ToTable(tb => tb.IsHybridTable());
                        });
                    builder.Entity(
                        "Orders", e =>
                        {
                            e.Property<int>("Id");
                            e.Property<int>("CustomerId");
                            e.ToTable(tb => tb.IsHybridTable());
                        });
                },
                model =>
                {
                });
            AssertSql(); // No SQL should be generated/run
        });

        Assert.Equal(@"Adding or dropping constraints is not supported in hybrid tables after table creation. Drop foreign key on 'Orders' is an invalid operation.", exception.Message);

    }

    # endregion

    # region SQL OPERATIONS

    [ConditionalFact]
    public override async Task SqlOperation()
    {
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
        {
            await Test(
                builder => { },
                new SqlOperation() { Sql = "-- I <3 DDL" },
                model =>
                {
                    Assert.Empty(model.Tables);
                    Assert.Empty(model.Sequences);
                });
        });

        AssertSql("-- I <3 DDL");
    }

    [ConditionalFact]
    public async Task SqlOperation_dml()
    {
        await Test(
            builder => { },
            new SqlOperation() { Sql = "SELECT 123 FROM DUAL" },
            model =>{});

        AssertSql("SELECT 123 FROM DUAL");
    }

    [ConditionalFact(Skip = "Disable due to test fails randomly during test class execution.")]
    public async Task SqlOperation_multiple_statements()
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await Test(
                builder => { },
                new SqlOperation() { Sql = "SELECT 123 FROM DUAL;\nSELECT 456 FROM DUAL;" },
                model =>{});
        });

        AssertSql("SELECT 123 FROM DUAL;\nSELECT 456 FROM DUAL;");
        Assert.Equal("Multiple statements cannot be executed in a single SqlOperation. Please separate each statement into a different SqlOperation.", exception.Message);
    }

    # endregion

    #region ADD UNIQUE CONTRAINT

    [ConditionalFact]
    public override Task Add_unique_constraint()
        => Test(
            builder => builder.Entity(
                "People", e =>
                {
                    e.Property<int>("Id");
                    e.Property<int>("AlternateKeyColumn");
                }),
            builder => { },
            builder => builder.Entity("People").HasAlternateKey("AlternateKeyColumn"),
            model =>
            {
                Assert.True(
                    Fixture.TestSqlLoggerFactory.Log.Exists(item => item.Message.Equals(SnowflakeStrings.UniqueConstraintNotCreated))
                );
                
                AssertSql(
                    """
                    ALTER TABLE "People" ADD CONSTRAINT "AK_People_AlternateKeyColumn" UNIQUE ("AlternateKeyColumn");
                    """
                );
            });

    [ConditionalFact]
    public async Task Add_unique_constraint_hybrid_table()
    {
        var exception = await Assert.ThrowsAsync<NotSupportedException>(async () =>
        {
            await Test(
                builder => builder.Entity(
                    "Table", e =>
                    {
                        e.Property<int>("Id");
                        e.Property<int>("AlternateKeyColumn");
                        e.ToTable(t => t.IsHybridTable());
                    }),
                builder => { },
                builder => builder.Entity("Table").HasAlternateKey("AlternateKeyColumn"),
                model =>
                {
                    
                });
        });

        AssertSql(); // No SQL should be generated/run
        Assert.Equal(SnowflakeStrings.ConstraintOperationNotSupported(nameof(AddUniqueConstraintOperation), "Table"), exception.Message);
    }
    
    [ConditionalFact]
    public override Task Add_unique_constraint_composite_with_name()
        => Test(
            builder => builder.Entity(
                "People", e =>
                {
                    e.Property<int>("Id");
                    e.Property<int>("AlternateKeyColumn1");
                    e.Property<int>("AlternateKeyColumn2");
                }),
            builder => { },
            builder => builder.Entity("People").HasAlternateKey("AlternateKeyColumn1", "AlternateKeyColumn2").HasName("AK_Foo"),
            model =>
            {
                Assert.True(
                    Fixture.TestSqlLoggerFactory.Log.Exists(item => item.Message.Equals(SnowflakeStrings.UniqueConstraintNotCreated))
                );
                
                AssertSql(
                    """
            ALTER TABLE "People" ADD CONSTRAINT "AK_Foo" UNIQUE ("AlternateKeyColumn1", "AlternateKeyColumn2");
            """);
            });

    #endregion

    #region RESTART SEQUENCE
 
    public override async Task Alter_sequence_restart_with()
    {
        var exception = await Assert.ThrowsAsync<NotSupportedException>(async () => await base.Alter_sequence_restart_with());
        AssertSql(); // No SQL should be generated/run
        Assert.Equal(SnowflakeStrings.RestartSequenceNotSupported, exception.Message);
    }

    public override async Task Alter_sequence_all_settings()
    {
        var exception = await Assert.ThrowsAsync<NotSupportedException>(async () => await base.Alter_sequence_all_settings());
        AssertSql(); // No SQL should be generated/run
        Assert.Equal(SnowflakeStrings.RestartSequenceNotSupported, exception.Message);
    }

    #endregion

    #region DROP UNIQUE CONSTRAINT

    public override Task Drop_unique_constraint()
        => Test(
            builder => builder.Entity(
                "People", e =>
                {
                    e.Property<int>("Id");
                    e.Property<int>("AlternateKeyColumn");
                }),
            builder => builder.Entity("People").HasAlternateKey("AlternateKeyColumn"),
            builder => { },
            model =>
            {
                Assert.Empty(Assert.Single(model.Tables).UniqueConstraints);
                AssertSql("ALTER TABLE \"People\" DROP CONSTRAINT \"AK_People_AlternateKeyColumn\";");
            });

    #endregion

    #region INSERT

    public override async Task InsertDataOperation()
    {
        await base.InsertDataOperation();
        
        AssertSql(
            """
INSERT INTO "Person" ("Id", "Name")
VALUES (1, 'Daenerys Targaryen');
""",
            //
            """
INSERT INTO "Person" ("Id", "Name")
VALUES (2, 'John Snow');
""",
            //
            """
INSERT INTO "Person" ("Id", "Name")
VALUES (3, 'Arya Stark');
""",
            //
            """
INSERT INTO "Person" ("Id", "Name")
VALUES (4, 'Harry Strickland');
""",
            //
            """
INSERT INTO "Person" ("Id", "Name")
VALUES (5, NULL);
""");
    }

    #endregion
    
    #region DELETE

    public override async Task DeleteDataOperation_composite_key()
    {
        await base.DeleteDataOperation_composite_key();
        
        AssertSql(
            """
DELETE FROM "Person"
WHERE "AnotherId" = 12 AND "Id" = 2;
""");
    }

    public override async Task DeleteDataOperation_simple_key()
    {
        await base.DeleteDataOperation_simple_key();
        AssertSql(
            """
DELETE FROM "Person"
WHERE "Id" = 2;
""");
    }

    #endregion

    #region UPDATE DATA

    [ConditionalFact]
    public override async Task UpdateDataOperation_simple_key()
    {
        await base.UpdateDataOperation_simple_key();
        AssertSql("""
UPDATE "Person" SET "Name" = 'Another John Snow'
WHERE "Id" = 2;
""");
    }
    
    [ConditionalFact]
    public virtual async Task UpdateDataOperation_simple_key_hybrid_table()
    {
        await Test(
            builder => builder.Entity(
                "Person", e =>
                {
                    e.Property<int>("Id");
                    e.Property<string>("Name");
                    e.HasKey("Id");
                    e.HasData(new Person { Id = 1, Name = "Daenerys Targaryen" });
                    e.ToTable(tb => tb.IsHybridTable());
                }),
            builder => builder.Entity("Person").HasData(new Person { Id = 2, Name = "John Snow" }),
            builder => builder.Entity("Person").HasData(new Person { Id = 2, Name = "Another John Snow" }),
            _ => { });
        AssertSql("""
UPDATE "Person" SET "Name" = 'Another John Snow'
WHERE "Id" = 2;
""");
    }

    [ConditionalFact]
    public override async Task UpdateDataOperation_composite_key()
    {
        await base.UpdateDataOperation_composite_key();
        AssertSql("""
UPDATE "Person" SET "Name" = 'Another John Snow'
WHERE "AnotherId" = 11 AND "Id" = 2;
""");
    }
    
    [ConditionalFact]
    public virtual async Task UpdateDataOperation_composite_key_hybrid_table()
    {
        await Test(
            builder => builder.Entity(
                "Person", e =>
                {
                    e.Property<int>("Id");
                    e.Property<int>("AnotherId");
                    e.HasKey("Id", "AnotherId");
                    e.Property<string>("Name");
                    e.HasData(
                        new Person
                        {
                            Id = 1,
                            AnotherId = 11,
                            Name = "Daenerys Targaryen"
                        });
                    e.ToTable(tb => tb.IsHybridTable());
                }),
            builder => builder.Entity("Person").HasData(
                new Person
                {
                    Id = 2,
                    AnotherId = 11,
                    Name = "John Snow"
                }),
            builder => builder.Entity("Person").HasData(
                new Person
                {
                    Id = 2,
                    AnotherId = 11,
                    Name = "Another John Snow"
                }),
            model => { });
        AssertSql(
            """
UPDATE "Person" SET "Name" = 'Another John Snow'
WHERE "AnotherId" = 11 AND "Id" = 2;
""");
    }

    [ConditionalFact]
    public override async Task UpdateDataOperation_multiple_columns()
    {
        await base.UpdateDataOperation_multiple_columns();
        AssertSql("""
UPDATE "Person" SET "Age" = 21, "Name" = 'Another John Snow'
WHERE "Id" = 2;
""");
    }
    
    [ConditionalFact]
    public virtual async Task UpdateDataOperation_multiple_columns_hybrid_table()
    {
        await Test(
            builder => builder.Entity(
                "Person", e =>
                {
                    e.Property<int>("Id");
                    e.Property<string>("Name");
                    e.Property<int>("Age");
                    e.HasKey("Id");
                    e.HasData(
                        new Person
                        {
                            Id = 1,
                            Name = "Daenerys Targaryen",
                            Age = 18
                        });
                    e.ToTable(tb => tb.IsHybridTable());
                }),
            builder => builder.Entity("Person").HasData(
                new Person
                {
                    Id = 2,
                    Name = "John Snow",
                    Age = 20
                }),
            builder => builder.Entity("Person").HasData(
                new Person
                {
                    Id = 2,
                    Name = "Another John Snow",
                    Age = 21
                }),
            model => { });
        AssertSql("""
UPDATE "Person" SET "Age" = 21, "Name" = 'Another John Snow'
WHERE "Id" = 2;
""");
    }
    #endregion

    #region CHECK CONSTRAINT

    public override async Task Add_check_constraint_with_name()
    {
        var exception = await Assert.ThrowsAsync<NotSupportedException>(() => base.Add_check_constraint_with_name());
        
        Assert.Equal(SnowflakeStrings.AddCheckConstraintNotSupported("CK_People_Foo", "People"), exception.Message);
    }

    public override async Task Alter_check_constraint()
    {
        var exception = await Assert.ThrowsAsync<NotSupportedException>(() => base.Alter_check_constraint());
        Assert.Equal(SnowflakeStrings.AddCheckConstraintNotSupported("CK_People_Foo", "People"), exception.Message);
    }

    public override async Task Drop_check_constraint()
    {
        var exception = await Assert.ThrowsAsync<NotSupportedException>(() => base.Drop_check_constraint());
        
        Assert.Equal(SnowflakeStrings.AddCheckConstraintNotSupported("CK_People_Foo", "People"), exception.Message);
    }

    #endregion
    
    public class MigrationsSnowflakeFixture : MigrationsFixtureBase
    {
        protected override string StoreName
            => nameof(MigrationsSnowflakeTest);

        protected override ITestStoreFactory TestStoreFactory
            => SnowflakeTestStoreFactory.Instance;

        public override RelationalTestHelpers TestHelpers
            => SnowflakeTestHelpers.Instance;

        protected override IServiceCollection AddServices(IServiceCollection serviceCollection)
            => base.AddServices(serviceCollection)
                .AddScoped<IDatabaseModelFactory, SnowflakeDatabaseModelFactory>();

        protected override bool ShouldLogCategory(string logCategory)
        => true;
    }
}