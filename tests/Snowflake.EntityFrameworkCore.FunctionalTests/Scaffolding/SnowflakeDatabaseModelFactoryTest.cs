using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Snowflake.Data.Client;
using Snowflake.EntityFrameworkCore.Diagnostics.Internal;
using Snowflake.EntityFrameworkCore.Internal;
using Snowflake.EntityFrameworkCore.Metadata.Internal;
using Snowflake.EntityFrameworkCore.Scaffolding.Metadata;
using Snowflake.EntityFrameworkCore.TestUtilities;

namespace Snowflake.EntityFrameworkCore.FunctionalTests.Scaffolding;

public class SnowflakeDatabaseModelFactoryTest : IClassFixture<SnowflakeDatabaseModelFactoryTest.SnowflakeDatabaseModelFixture>
{
    protected SnowflakeDatabaseModelFixture Fixture { get; }

    public SnowflakeDatabaseModelFactoryTest(SnowflakeDatabaseModelFixture fixture)
    {
        Fixture = fixture;
        Fixture.OperationReporter.Clear();
    }
    
    public static IEnumerable<object[]> TableTypes =>
        new List<object[]>
        {
            new object[] { "HYBRID " },
            new object[] { "" }
        };
    
    public static IEnumerable<object[]> HybridTableTypesOnly =>
        new List<object[]>
        {
            new object[] { "HYBRID " }
        };

    #region Sequences

    [Fact]
    public void Create_sequences_with_facets()
        => Test(
            @"
CREATE SEQUENCE DefaultFacetsSequence;

CREATE SEQUENCE DB2.CustomFacetsSequence
    START WITH 1
    INCREMENT BY 2;",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var defaultSequence = dbModel.Sequences.First(ds => ds.Name == "DefaultFacetsSequence".ToUpper());
                Assert.Equal("PUBLIC", defaultSequence.Schema);
                Assert.Equal("DefaultFacetsSequence", defaultSequence.Name, StringComparer.InvariantCultureIgnoreCase);
                Assert.Equal("NUMBER(38, 0)", defaultSequence.StoreType);
                Assert.Equal(null, defaultSequence.IsCyclic);
                Assert.Equal(1, defaultSequence.IncrementBy);
                Assert.Equal(1, defaultSequence.StartValue);
                // REVIEW is should have a value for snowflake
                // Assert.Null(defaultSequence.MinValue);
                // Assert.Null(defaultSequence.MaxValue);

                var customSequence = dbModel.Sequences.First(ds => ds.Name == "CustomFacetsSequence".ToUpper());
                Assert.Equal("DB2", customSequence.Schema, StringComparer.InvariantCultureIgnoreCase);
                Assert.Equal("CustomFacetsSequence", customSequence.Name, StringComparer.InvariantCultureIgnoreCase);
                Assert.Equal("NUMBER(38, 0)", customSequence.StoreType);
                Assert.Null(customSequence.IsCyclic);
                Assert.Equal(2, customSequence.IncrementBy);
                Assert.Equal(1, customSequence.StartValue);
            },
            @"
DROP SEQUENCE DefaultFacetsSequence;

DROP SEQUENCE DB2.CustomFacetsSequence;", 2, 2);

    [ConditionalFact]
    public void Sequence_min_max_start_values_are_not_null()
        => Test(
            @"
CREATE SEQUENCE ""MySequence"";",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                Assert.All(
                    dbModel.Sequences,
                    s =>
                    {
                        Assert.NotNull(s.StartValue);
                        Assert.NotNull(s.MinValue);
                        Assert.NotNull(s.MaxValue);
                    });
            },
            @"
DROP SEQUENCE ""MySequence"";");

    
    [ConditionalFact]
    public void Filter_sequences_based_on_schema()
        => Test(
            @"
CREATE SEQUENCE PUBLIC.Sequence;

CREATE SEQUENCE DB2.Sequence;",
            Enumerable.Empty<string>(),
            new[] { "DB2" },
            (dbModel, scaffoldingFactory) =>
            {
                var sequence = Assert.Single(dbModel.Sequences);
                // ReSharper disable once PossibleNullReferenceException
                Assert.Equal("DB2", sequence.Schema);
                Assert.Equal("SEQUENCE", sequence.Name);
                Assert.Equal("NUMBER(38, 0)", sequence.StoreType);
                Assert.Equal(1, sequence.IncrementBy);
            },
            @"
DROP SEQUENCE PUBLIC.Sequence;

DROP SEQUENCE DB2.Sequence;",2,2);

    #endregion

    #region Model

    [ConditionalFact]
    public void Set_default_schema()
        => Test(
            "SELECT 1",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var defaultSchema = Fixture.TestStore.ExecuteScalar<string>("SELECT CURRENT_SCHEMA()");
                Assert.Equal(defaultSchema, dbModel.DefaultSchema);
            },
            null);

    [ConditionalFact]
    public void Create_tables()
        => Test(
            @"
CREATE TABLE PUBLIC.""Everest"" ( id int );

CREATE TABLE PUBLIC.""Denali"" ( id int );",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                Assert.Collection(
                    dbModel.Tables.OrderBy(t => t.Name),
                    d =>
                    {
                        Assert.Equal("PUBLIC", d.Schema);
                        Assert.Equal("Denali", d.Name);
                    },
                    e =>
                    {
                        Assert.Equal("PUBLIC", e.Schema);
                        Assert.Equal("Everest", e.Name);
                    });
            },
            @"
DROP TABLE PUBLIC.""Everest"";

DROP TABLE PUBLIC.""Denali"";",2,2);

    [ConditionalFact]
    [Trait("Category", "UseHybridTables")]
    public void Create_hybrid_tables()
        => Test(
            @"
CREATE HYBRID TABLE PUBLIC.""Everest"" ( id int PRIMARY KEY );

CREATE HYBRID TABLE PUBLIC.""Denali"" ( id int PRIMARY KEY );",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                Assert.Collection(
                    dbModel.Tables.OrderBy(t => t.Name),
                    d =>
                    {
                        Assert.Equal("PUBLIC", d.Schema);
                        Assert.Equal("Denali", d.Name);
                    },
                    e =>
                    {
                        Assert.Equal("PUBLIC", e.Schema);
                        Assert.Equal("Everest", e.Name);
                    });
            },
            @"
DROP TABLE PUBLIC.""Everest"";

DROP TABLE PUBLIC.""Denali"";",2,2);
    
    
    [ConditionalFact]
    public void Create_table_basic()
        => Test(
            @"CREATE TABLE Customer(id int, name string NOT NULL, address string,
CONSTRAINT primaryKeyCustomer PRIMARY KEY (Id));",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                Assert.Collection(
                    dbModel.Tables.OrderBy(t => t.Name),
                    d =>
                    {
                        Assert.Equal("PUBLIC", d.Schema);
                        Assert.Equal("CUSTOMER", d.Name);
                        Assert.IsType<DatabaseTable>(d);
                        Assert.Null(d.FindAnnotation(SnowflakeAnnotationNames.HybridTable));
                    });
            },
            @"
DROP TABLE Customer;",1,1);
    
    [ConditionalFact]
    [Trait("Category", "UseHybridTables")]
    public void Create_table_hybrid()
        => Test(
            @"CREATE HYBRID TABLE Customer(id int, name string NOT NULL, address string,
CONSTRAINT primaryKeyCustomer PRIMARY KEY (Id));",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                Assert.Collection(
                    dbModel.Tables.OrderBy(t => t.Name),
                    d =>
                    {
                        Assert.Equal("PUBLIC", d.Schema);
                        Assert.Equal("CUSTOMER", d.Name);
                        Assert.IsType<DatabaseHybridTable>(d);
                        var annotation = d.FindAnnotation(SnowflakeAnnotationNames.HybridTable);
                        Assert.True(annotation != null);
                        Assert.True((annotation.Value as bool?).Value);
                    });
            },
            @"
DROP TABLE Customer;",1,1);
    
    
    [ConditionalFact]
    public void Create_table_external()
        => Test(
            @"CREATE OR REPLACE FILE FORMAT mycsvformat
   TYPE = 'CSV'
   FIELD_DELIMITER = '|'
   SKIP_HEADER = 1;

CREATE OR REPLACE STAGE my_csv_stage
  FILE_FORMAT = mycsvformat
  URL = 's3://snowflake-docs';

CREATE OR REPLACE EXTERNAL TABLE Contacts (Name string as (value:c2::varchar), LastName string as (value:c3::varchar),
Company string as (value:c4::varchar), email string as (value:c5::varchar) ) 
LOCATION = @my_csv_stage file_format = ( FORMAT_NAME = 'mycsvformat') 
PATTERN = '.*contacts.*\.csv' AUTO_REFRESH = FALSE;",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                Assert.Collection(
                    dbModel.Tables.OrderBy(t => t.Name),
                    d =>
                    {
                        Assert.Equal("PUBLIC", d.Schema);
                        Assert.Equal("CONTACTS", d.Name);
                        Assert.IsType<DatabaseExternalTable>(d);
                    });
            },
            @"
DROP TABLE Contacts;
DROP STAGE my_csv_stage;
DROP FILE FORMAT mycsvformat;",3,3);
    
    
    [ConditionalFact]
    public void Create_dynamic_table()
        => Test(
            @$"CREATE TABLE TableA (column1 int, colum2 string);
CREATE DYNAMIC TABLE DynamicTable WAREHOUSE = {this.Fixture.TestStore.Warehouse} TARGET_LAG = '60 seconds' AS SELECT * FROM TableA",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                Assert.Collection(
                    dbModel.Tables.OrderBy(t => t.Name),
                    d =>
                    {
                        Assert.Equal("PUBLIC", d.Schema);
                        Assert.Equal("DYNAMICTABLE", d.Name);
                        Assert.IsType<DatabaseDynamicTable>(d);
                    },
                    d =>
                    {
                        Assert.Equal("PUBLIC", d.Schema);
                        Assert.Equal("TABLEA", d.Name);
                        Assert.IsType<DatabaseTable>(d);
                    });
            },
            @"
DROP TABLE DYNAMICTABLE;
DROP TABLE TABLEA;",2,2);

    [ConditionalFact]
    public void Expose_join_table_when_interloper_reference()
        => Test(
            @"
CREATE TABLE BBlogs (Id int IDENTITY CONSTRAINT PK_BBlogs PRIMARY KEY);
CREATE TABLE PPosts (Id int IDENTITY CONSTRAINT PK_PPosts PRIMARY KEY);

CREATE TABLE BBlogPPosts (
    BBlogId int NOT NULL CONSTRAINT FK_BBlogPPosts_BBlogs REFERENCES BBlogs(Id),
    PPostId int NOT NULL CONSTRAINT FK_BBlogPPosts_PPosts REFERENCES PPosts(Id),
    CONSTRAINT PK_BBlogPPosts  PRIMARY KEY (BBlogId, PPostId));

CREATE TABLE LinkToBBlogPPosts (
    LinkId1 int NOT NULL,
    LinkId2 int NOT NULL,
    CONSTRAINT PK_LinkToBBlogPPosts PRIMARY KEY (LinkId1, LinkId2),
    CONSTRAINT FK_LinkToBBlogPPosts_BlogPosts FOREIGN KEY (LinkId1, LinkId2) REFERENCES BBlogPPosts(BBlogId, PPostId));
",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                Assert.Collection(
                    dbModel.Tables.OrderBy(t => t.Name),
                    t =>
                    {
                        Assert.Equal("PUBLIC", t.Schema, StringComparer.InvariantCultureIgnoreCase);
                        Assert.Equal("BBlogPPosts", t.Name, StringComparer.InvariantCultureIgnoreCase);
                        Assert.Collection(t.Columns,
                            c => Assert.Equal("BBlogId", c.Name, StringComparer.InvariantCultureIgnoreCase),
                            c => Assert.Equal("PPostId", c.Name, StringComparer.InvariantCultureIgnoreCase));
                        Assert.Collection(t.ForeignKeys,
                            c =>
                            {
                                Assert.Equal("BBlogs", c.PrincipalTable.Name, StringComparer.InvariantCultureIgnoreCase);
                                Assert.Equal("BBlogPPosts", c.Table.Name, StringComparer.InvariantCultureIgnoreCase);
                                Assert.Collection(c.Columns, c => Assert.Equal("BBlogId", c.Name, StringComparer.InvariantCultureIgnoreCase));
                            },
                            c =>
                            {
                                Assert.Equal("PPosts", c.PrincipalTable.Name, StringComparer.InvariantCultureIgnoreCase);
                                Assert.Equal("BBlogPPosts", c.Table.Name, StringComparer.InvariantCultureIgnoreCase);
                                Assert.Collection(c.Columns, c => Assert.Equal("PPostId", c.Name, StringComparer.InvariantCultureIgnoreCase));
                            });
                    },
                    t =>
                    {
                        Assert.Equal("PUBLIC", t.Schema);
                        Assert.Equal("BBlogs", t.Name, StringComparer.InvariantCultureIgnoreCase);
                        Assert.Collection(t.Columns, c => Assert.Equal("Id", c.Name, StringComparer.InvariantCultureIgnoreCase));
                    },
                    t =>
                    {
                        Assert.Equal("PUBLIC", t.Schema);
                        Assert.Equal("LinkToBBlogPPosts", t.Name, StringComparer.InvariantCultureIgnoreCase);
                        Assert.Collection(t.Columns,
                            c => Assert.Equal("LinkId1", c.Name, StringComparer.InvariantCultureIgnoreCase),
                            c => Assert.Equal("LinkId2", c.Name, StringComparer.InvariantCultureIgnoreCase));
                        Assert.Collection(t.ForeignKeys,
                            c =>
                            {
                                Assert.Equal("BBlogPPosts", c.PrincipalTable.Name, StringComparer.InvariantCultureIgnoreCase);
                                Assert.Equal("LinkToBBlogPPosts", c.Table.Name, StringComparer.InvariantCultureIgnoreCase);
                                Assert.Collection(
                                    c.Columns,
                                    c => Assert.Equal("LinkId1", c.Name, StringComparer.InvariantCultureIgnoreCase),
                                    c => Assert.Equal("LinkId2", c.Name, StringComparer.InvariantCultureIgnoreCase));
                            });
                    },
                    t =>
                    {
                        Assert.Equal("PUBLIC", t.Schema);
                        Assert.Equal("PPosts", t.Name, StringComparer.InvariantCultureIgnoreCase);
                        Assert.Collection(t.Columns, c => Assert.Equal("Id", c.Name, StringComparer.InvariantCultureIgnoreCase));
                    });

                var model = scaffoldingFactory.Create(dbModel, new ModelReverseEngineerOptions());

                Assert.Collection(
                    model.GetEntityTypes(),
                    e =>
                    {
                        Assert.Equal("Bblog", e.Name);
                        Assert.Collection(e.GetProperties(), p => Assert.Equal("Id", p.Name));
                        Assert.Empty(e.GetForeignKeys());
                        Assert.Empty(e.GetSkipNavigations());
                        Assert.Collection(e.GetNavigations(), p => Assert.Equal("Bblogpposts", p.Name));
                    },
                    e =>
                    {
                        Assert.Equal("Bblogppost", e.Name);
                        Assert.Collection(e.GetProperties(),
                            p => Assert.Equal("Bblogid", p.Name),
                            p => Assert.Equal("Ppostid", p.Name));
                        Assert.Collection(e.GetForeignKeys(),
                            k =>
                            {
                                Assert.Equal("Bblog", k.PrincipalEntityType.Name);
                                Assert.Equal("Bblogppost", k.DeclaringEntityType.Name);
                                Assert.Collection(k.Properties, p => Assert.Equal("Bblogid", p.Name));
                            },
                            k =>
                            {
                                Assert.Equal("Ppost", k.PrincipalEntityType.Name);
                                Assert.Equal("Bblogppost", k.DeclaringEntityType.Name);
                                Assert.Collection(k.Properties, p => Assert.Equal("Ppostid", p.Name));
                            });
                        Assert.Empty(e.GetSkipNavigations());
                        Assert.Collection(e.GetNavigations(),
                            p => Assert.Equal("Bblog", p.Name),
                            p => Assert.Equal("Linktobblogppost", p.Name),
                            p => Assert.Equal("Ppost", p.Name));
                    },
                    e =>
                    {
                        Assert.Equal("Linktobblogppost", e.Name);
                        Assert.Collection(e.GetProperties(),
                            p => Assert.Equal("Linkid1", p.Name),
                            p => Assert.Equal("Linkid2", p.Name));
                        Assert.Collection(e.GetForeignKeys(),
                            k =>
                            {
                                Assert.Equal("Bblogppost", k.PrincipalEntityType.Name);
                                Assert.Equal("Linktobblogppost", k.DeclaringEntityType.Name);
                                Assert.Collection(k.Properties,
                                    p => Assert.Equal("Linkid1", p.Name),
                                    p => Assert.Equal("Linkid2", p.Name));
                                Assert.Collection(k.PrincipalKey.Properties,
                                    p => Assert.Equal("Bblogid", p.Name),
                                    p => Assert.Equal("Ppostid", p.Name));
                            });
                        Assert.Empty(e.GetSkipNavigations());
                        Assert.Collection(e.GetNavigations(), p => Assert.Equal("Bblogppost", p.Name));
                    },
                    e =>
                    {
                        Assert.Equal("Ppost", e.Name);
                        Assert.Collection(e.GetProperties(), p => Assert.Equal("Id", p.Name));
                        Assert.Empty(e.GetForeignKeys());
                        Assert.Empty(e.GetSkipNavigations());
                        Assert.Collection(e.GetNavigations(), p => Assert.Equal("Bblogpposts", p.Name));
                    });
            },
            @"
DROP TABLE PUBLIC.LinkToBBlogPPosts;
DROP TABLE PUBLIC.BBlogPPosts;
DROP TABLE PUBLIC.PPosts;
DROP TABLE PUBLIC.BBlogs;", 4, 4);

    [ConditionalFact]
    [Trait("Category", "UseHybridTables")]
    public void Expose_join_hybrid_table_when_interloper_reference()
        => Test(
            $@"
CREATE HYBRID TABLE BBlogs (Id int IDENTITY CONSTRAINT PK_BBlogs PRIMARY KEY);
CREATE HYBRID TABLE PPosts (Id int IDENTITY CONSTRAINT PK_PPosts PRIMARY KEY);

CREATE HYBRID TABLE BBlogPPosts (
    BBlogId int NOT NULL CONSTRAINT FK_BBlogPPosts_BBlogs REFERENCES BBlogs(Id),
    PPostId int NOT NULL CONSTRAINT FK_BBlogPPosts_PPosts REFERENCES PPosts(Id),
    CONSTRAINT PK_BBlogPPosts  PRIMARY KEY (BBlogId, PPostId));

CREATE HYBRID TABLE LinkToBBlogPPosts (
    LinkId1 int NOT NULL,
    LinkId2 int NOT NULL,
    CONSTRAINT PK_LinkToBBlogPPosts PRIMARY KEY (LinkId1, LinkId2),
    CONSTRAINT FK_LinkToBBlogPPosts_BlogPosts FOREIGN KEY (LinkId1, LinkId2) REFERENCES BBlogPPosts(BBlogId, PPostId));
",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                Assert.Collection(
                    dbModel.Tables.OrderBy(t => t.Name),
                    t =>
                    {
                        Assert.Equal("PUBLIC", t.Schema, StringComparer.InvariantCultureIgnoreCase);
                        Assert.Equal("BBlogPPosts", t.Name, StringComparer.InvariantCultureIgnoreCase);
                        Assert.Collection(t.Columns,
                            c => Assert.Equal("BBlogId", c.Name, StringComparer.InvariantCultureIgnoreCase),
                            c => Assert.Equal("PPostId", c.Name, StringComparer.InvariantCultureIgnoreCase));
                        Assert.Collection(t.ForeignKeys,
                            c =>
                            {
                                Assert.Equal("BBlogs", c.PrincipalTable.Name, StringComparer.InvariantCultureIgnoreCase);
                                Assert.Equal("BBlogPPosts", c.Table.Name, StringComparer.InvariantCultureIgnoreCase);
                                Assert.Collection(c.Columns, c => Assert.Equal("BBlogId", c.Name, StringComparer.InvariantCultureIgnoreCase));
                            },
                            c =>
                            {
                                Assert.Equal("PPosts", c.PrincipalTable.Name, StringComparer.InvariantCultureIgnoreCase);
                                Assert.Equal("BBlogPPosts", c.Table.Name, StringComparer.InvariantCultureIgnoreCase);
                                Assert.Collection(c.Columns, c => Assert.Equal("PPostId", c.Name, StringComparer.InvariantCultureIgnoreCase));
                            });
                    },
                    t =>
                    {
                        Assert.Equal("PUBLIC", t.Schema);
                        Assert.Equal("BBlogs", t.Name, StringComparer.InvariantCultureIgnoreCase);
                        Assert.Collection(t.Columns, c => Assert.Equal("Id", c.Name, StringComparer.InvariantCultureIgnoreCase));
                    },
                    t =>
                    {
                        Assert.Equal("PUBLIC", t.Schema);
                        Assert.Equal("LinkToBBlogPPosts", t.Name, StringComparer.InvariantCultureIgnoreCase);
                        Assert.Collection(t.Columns,
                            c => Assert.Equal("LinkId1", c.Name, StringComparer.InvariantCultureIgnoreCase),
                            c => Assert.Equal("LinkId2", c.Name, StringComparer.InvariantCultureIgnoreCase));
                        Assert.Collection(t.ForeignKeys,
                            c =>
                            {
                                Assert.Equal("BBlogPPosts", c.PrincipalTable.Name, StringComparer.InvariantCultureIgnoreCase);
                                Assert.Equal("LinkToBBlogPPosts", c.Table.Name, StringComparer.InvariantCultureIgnoreCase);
                                Assert.Collection(
                                    c.Columns,
                                    c => Assert.Equal("LinkId1", c.Name, StringComparer.InvariantCultureIgnoreCase),
                                    c => Assert.Equal("LinkId2", c.Name, StringComparer.InvariantCultureIgnoreCase));
                            });
                    },
                    t =>
                    {
                        Assert.Equal("PUBLIC", t.Schema);
                        Assert.Equal("PPosts", t.Name, StringComparer.InvariantCultureIgnoreCase);
                        Assert.Collection(t.Columns, c => Assert.Equal("Id", c.Name, StringComparer.InvariantCultureIgnoreCase));
                    });

                var model = scaffoldingFactory.Create(dbModel, new ModelReverseEngineerOptions());

                Assert.Collection(
                    model.GetEntityTypes(),
                    e =>
                    {
                        Assert.Equal("Bblog", e.Name);
                        Assert.Collection(e.GetProperties(), p => Assert.Equal("Id", p.Name));
                        Assert.Empty(e.GetForeignKeys());
                        Assert.Empty(e.GetSkipNavigations());
                        Assert.Collection(e.GetNavigations(), p => Assert.Equal("Bblogpposts", p.Name));
                    },
                    e =>
                    {
                        Assert.Equal("Bblogppost", e.Name);
                        Assert.Collection(e.GetProperties(),
                            p => Assert.Equal("Bblogid", p.Name),
                            p => Assert.Equal("Ppostid", p.Name));
                        Assert.Collection(e.GetForeignKeys(),
                            k =>
                            {
                                Assert.Equal("Bblog", k.PrincipalEntityType.Name);
                                Assert.Equal("Bblogppost", k.DeclaringEntityType.Name);
                                Assert.Collection(k.Properties, p => Assert.Equal("Bblogid", p.Name));
                            },
                            k =>
                            {
                                Assert.Equal("Ppost", k.PrincipalEntityType.Name);
                                Assert.Equal("Bblogppost", k.DeclaringEntityType.Name);
                                Assert.Collection(k.Properties, p => Assert.Equal("Ppostid", p.Name));
                            });
                        Assert.Empty(e.GetSkipNavigations());
                        Assert.Collection(e.GetNavigations(),
                            p => Assert.Equal("Bblog", p.Name),
                            p => Assert.Equal("Linktobblogppost", p.Name),
                            p => Assert.Equal("Ppost", p.Name));
                    },
                    e =>
                    {
                        Assert.Equal("Linktobblogppost", e.Name);
                        Assert.Collection(e.GetProperties(),
                            p => Assert.Equal("Linkid1", p.Name),
                            p => Assert.Equal("Linkid2", p.Name));
                        Assert.Collection(e.GetForeignKeys(),
                            k =>
                            {
                                Assert.Equal("Bblogppost", k.PrincipalEntityType.Name);
                                Assert.Equal("Linktobblogppost", k.DeclaringEntityType.Name);
                                Assert.Collection(k.Properties,
                                    p => Assert.Equal("Linkid1", p.Name),
                                    p => Assert.Equal("Linkid2", p.Name));
                                Assert.Collection(k.PrincipalKey.Properties,
                                    p => Assert.Equal("Bblogid", p.Name),
                                    p => Assert.Equal("Ppostid", p.Name));
                            });
                        Assert.Empty(e.GetSkipNavigations());
                        Assert.Collection(e.GetNavigations(), p => Assert.Equal("Bblogppost", p.Name));
                    },
                    e =>
                    {
                        Assert.Equal("Ppost", e.Name);
                        Assert.Collection(e.GetProperties(), p => Assert.Equal("Id", p.Name));
                        Assert.Empty(e.GetForeignKeys());
                        Assert.Empty(e.GetSkipNavigations());
                        Assert.Collection(e.GetNavigations(), p => Assert.Equal("Bblogpposts", p.Name));
                    });
            },
            @"
DROP TABLE PUBLIC.LinkToBBlogPPosts;
DROP TABLE PUBLIC.BBlogPPosts;
DROP TABLE PUBLIC.PPosts;
DROP TABLE PUBLIC.BBlogs;", 4, 4);

    [ConditionalFact]
    public void Default_database_collation_is_not_scaffolded()
        => Test(
            @"",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) => Assert.Null(dbModel.Collation),
            @"");

    #endregion

    #region FilteringSchemaTable

    [ConditionalTheory]
    [MemberData(nameof(TableTypes))]
    [Trait("Category", "UseHybridTables")]
    public void Filter_schemas(string tableType)
        => Test(
            $@"
CREATE {tableType}TABLE DB2.K2 ( Id  int PRIMARY KEY, A varchar, UNIQUE (A ) );

CREATE {tableType}TABLE PUBLIC.Kilimanjaro ( Id int PRIMARY KEY, B varchar, UNIQUE (B));",
            Enumerable.Empty<string>(),
            new[] { "DB2" },
            (dbModel, scaffoldingFactory) =>
            {
                var table = Assert.Single(dbModel.Tables);
                // ReSharper disable once PossibleNullReferenceException
                Assert.Equal("K2", table.Name);
                Assert.Equal(2, table.Columns.Count);
                Assert.Equal(tableType == string.Empty ? 0 : 1, table.UniqueConstraints.Count);
                Assert.NotNull(table.PrimaryKey);
                Assert.Empty(table.ForeignKeys);
            },
            @"
DROP TABLE PUBLIC.Kilimanjaro;

DROP TABLE DB2.K2;", 2, 2);

    [ConditionalTheory]
    [MemberData(nameof(TableTypes))]
    [Trait("Category", "UseHybridTables")]
    public void Filter_tables(string tableType)
        => Test(
            $@"
CREATE {tableType}TABLE DB2.K2 ( Id  int PRIMARY KEY, A varchar, UNIQUE (A ) );

CREATE {tableType}TABLE PUBLIC.Kilimanjaro ( Id int PRIMARY KEY, B varchar, UNIQUE (B));",
            new[] { "K2" },
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var table = Assert.Single(dbModel.Tables);
                // ReSharper disable once PossibleNullReferenceException
                Assert.Equal("K2", table.Name);
                Assert.Equal(2, table.Columns.Count);
                Assert.Equal(tableType == string.Empty ? 0 : 1, table.UniqueConstraints.Count);
                Assert.Empty(table.ForeignKeys);
            },
            @"
DROP TABLE PUBLIC.Kilimanjaro;

DROP TABLE DB2.K2;", 2, 2);

    [ConditionalFact]
    [Trait("Category", "UseHybridTables")]
    public void Filter_tables_with_quote_in_name()
        => Test(
            @"
CREATE HYBRID TABLE PUBLIC.""K2'"" ( Id int PRIMARY KEY, A varchar, UNIQUE (A ) );

CREATE HYBRID TABLE PUBLIC.Kilimanjaro ( Id int PRIMARY KEY, B varchar, UNIQUE (B), FOREIGN KEY (B) REFERENCES ""K2'"" (A) );",
            new[] { "K2\\'" },
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var table = Assert.Single(dbModel.Tables);
                // ReSharper disable once PossibleNullReferenceException
                Assert.Equal("K2'", table.Name);
                Assert.Equal(2, table.Columns.Count);
                Assert.Equal(1, table.UniqueConstraints.Count);
                Assert.Empty(table.ForeignKeys);
            },
            @"
DROP TABLE PUBLIC.Kilimanjaro;

DROP TABLE PUBLIC.""K2'""", 2, 2);

    [ConditionalFact]
    [Trait("Category", "UseHybridTables")]
    public void Filter_tables_with_qualified_name()
        => Test(
            @"
CREATE HYBRID TABLE PUBLIC.""K.2"" ( Id int PRIMARY KEY, A varchar, UNIQUE (A ) );

CREATE HYBRID TABLE PUBLIC.Kilimanjaro ( Id int PRIMARY KEY, B varchar, UNIQUE (B) );",
            new[] { @"""K.2""" },
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var table = Assert.Single(dbModel.Tables);
                // ReSharper disable once PossibleNullReferenceException
                Assert.Equal(@"K.2", table.Name);
                Assert.Equal(2, table.Columns.Count);
                Assert.Equal(1, table.UniqueConstraints.Count);
                Assert.Empty(table.ForeignKeys);
            },
            @"
DROP TABLE PUBLIC.Kilimanjaro;

DROP TABLE PUBLIC.""K.2"";",2,2);

    [ConditionalFact]
    [Trait("Category", "UseHybridTables")]
    public void Filter_tables_with_schema_qualified_name1()
        => Test(
            @"
CREATE HYBRID TABLE PUBLIC.K2 ( Id int PRIMARY KEY, A varchar, UNIQUE (A ) );

CREATE HYBRID TABLE DB2.K2 ( Id int PRIMARY KEY, A varchar, UNIQUE (A ) );

CREATE HYBRID TABLE PUBLIC.Kilimanjaro ( Id int PRIMARY KEY, B varchar, UNIQUE (B) );",
            new[] { "PUBLIC.K2" },
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var table = Assert.Single(dbModel.Tables);
                // ReSharper disable once PossibleNullReferenceException
                Assert.Equal("K2", table.Name);
                Assert.Equal(2, table.Columns.Count);
                Assert.Equal(1, table.UniqueConstraints.Count);
                Assert.Empty(table.ForeignKeys);
            },
            @"
DROP TABLE PUBLIC.Kilimanjaro;

DROP TABLE PUBLIC.K2;

DROP TABLE DB2.K2;", 3, 3);

    [ConditionalFact]
    [Trait("Category", "UseHybridTables")]
    public void Filter_tables_with_schema_qualified_name2()
        => Test(
            @"
CREATE HYBRID TABLE PUBLIC.""K.2"" ( Id int PRIMARY KEY, A varchar, UNIQUE (A ) );

CREATE HYBRID TABLE ""db.2"".""K.2"" ( Id int PRIMARY KEY , A varchar, UNIQUE (A ) );

CREATE HYBRID TABLE ""db.2"".Kilimanjaro ( Id int PRIMARY KEY , B varchar, UNIQUE (B) );",
            new[] { @"""db.2"".""K.2""" },
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var table = Assert.Single(dbModel.Tables);
                // ReSharper disable once PossibleNullReferenceException
                Assert.Equal("K.2", table.Name);
                Assert.Equal(2, table.Columns.Count);
                Assert.Equal(1, table.UniqueConstraints.Count);
                Assert.Empty(table.ForeignKeys);
            },
            @"
DROP TABLE ""db.2"".Kilimanjaro;

DROP TABLE PUBLIC.""K.2"";

DROP TABLE ""db.2"".""K.2"";", 3, 3);

    [ConditionalFact]
    [Trait("Category", "UseHybridTables")]
    public void Filter_tables_with_schema_qualified_name3()
        => Test(
            @"
CREATE HYBRID TABLE PUBLIC.""K.2"" ( Id int PRIMARY KEY, A varchar, UNIQUE (A ) );

CREATE HYBRID TABLE DB2.""K.2"" ( Id int PRIMARY KEY, A varchar, UNIQUE (A ) );

CREATE HYBRID TABLE PUBLIC.Kilimanjaro ( Id int PRIMARY KEY, B varchar, UNIQUE (B) );",
            new[] { @"PUBLIC.""K.2""" },
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var table = Assert.Single(dbModel.Tables);
                // ReSharper disable once PossibleNullReferenceException
                Assert.Equal("K.2", table.Name);
                Assert.Equal(2, table.Columns.Count);
                Assert.Equal(1, table.UniqueConstraints.Count);
                Assert.Empty(table.ForeignKeys);
            },
            @"
DROP TABLE PUBLIC.Kilimanjaro;

DROP TABLE PUBLIC.""K.2"";

DROP TABLE DB2.""K.2"";", 3, 3);

    [ConditionalFact]
    [Trait("Category", "UseHybridTables")]
    public void Filter_tables_with_schema_qualified_name4()
        => Test(
            @"
CREATE HYBRID TABLE PUBLIC.K2 ( Id int PRIMARY KEY, A varchar, UNIQUE (A ) );

CREATE HYBRID TABLE ""db.2"".K2 ( Id int PRIMARY KEY, A varchar, UNIQUE (A ) );

CREATE HYBRID TABLE ""db.2"".Kilimanjaro ( Id int PRIMARY KEY, B varchar, UNIQUE (B) );",
            new[] { @"""db.2"".K2" },
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var table = Assert.Single(dbModel.Tables);
                // ReSharper disable once PossibleNullReferenceException
                Assert.Equal("K2", table.Name);
                Assert.Equal(2, table.Columns.Count);
                Assert.Equal(1, table.UniqueConstraints.Count);
                Assert.Empty(table.ForeignKeys);
            },
            @"
DROP TABLE ""db.2"".Kilimanjaro;

DROP TABLE PUBLIC.K2;

DROP TABLE ""db.2"".K2;", 3, 3);

    [ConditionalFact]
    [Trait("Category", "UseHybridTables")]
    public void Complex_filtering_validation()
        => Test(
            @"
CREATE SEQUENCE PUBLIC.SEQUENCE;
CREATE SEQUENCE DB2.SEQUENCE;

CREATE HYBRID TABLE ""db.2"".""QuotedTableName"" ( Id int PRIMARY KEY );
CREATE HYBRID TABLE ""db.2"".""Table.With.Dot"" ( Id int PRIMARY KEY );
CREATE HYBRID TABLE ""db.2"".SIMPLETABLENAME ( Id int PRIMARY KEY );
CREATE HYBRID TABLE ""db.2"".JUSTTABLENAME ( Id int PRIMARY KEY );

CREATE HYBRID TABLE PUBLIC.""QuotedTableName"" ( Id int PRIMARY KEY );
CREATE HYBRID TABLE PUBLIC.""Table.With.Dot"" ( Id int PRIMARY KEY );
CREATE HYBRID TABLE PUBLIC.SIMPLETABLENAME ( Id int PRIMARY KEY );
CREATE HYBRID TABLE PUBLIC.JUSTTABLENAME ( Id int PRIMARY KEY );

CREATE HYBRID TABLE DB2.""QuotedTableName"" ( Id int PRIMARY KEY );
CREATE HYBRID TABLE DB2.""Table.With.Dot"" ( Id int PRIMARY KEY );
CREATE HYBRID TABLE DB2.SIMPLETABLENAME ( Id int PRIMARY KEY );
CREATE HYBRID TABLE DB2.JUSTTABLENAME ( Id int PRIMARY KEY );

CREATE HYBRID TABLE DB2.""PrincipalTable"" (
    Id int PRIMARY KEY,
    UC1 nvarchar(450),
    UC2 int,
    Index1 int,
    Index2 bigint,
    CONSTRAINT UX UNIQUE (UC1, UC2)
);

CREATE INDEX IX_COMPOSITE ON DB2.""PrincipalTable"" ( Index2, Index1 );

CREATE HYBRID TABLE DB2.""DependentTable"" (
    Id int PRIMARY KEY,
    ForeignKeyId1 nvarchar(450),
    ForeignKeyId2 int,
    FOREIGN KEY (ForeignKeyId1, ForeignKeyId2) REFERENCES DB2.""PrincipalTable""(UC1, UC2) ON DELETE NO ACTION
);",
            new[] { @"""db.2"".""QuotedTableName""", @"""db.2"".SIMPLETABLENAME", @"PUBLIC.""Table.With.Dot""", "PUBLIC.SIMPLETABLENAME", "JUSTTABLENAME" },
            new[] { "DB2" },
            (dbModel, scaffoldingFactory) =>
            {
                var sequence = Assert.Single(dbModel.Sequences);
                // ReSharper disable once PossibleNullReferenceException
                Assert.Equal("DB2", sequence.Schema);

                Assert.Single(dbModel.Tables.Where(t => t is { Schema: "db.2", Name: "QuotedTableName" }));
                Assert.Empty(dbModel.Tables.Where(t => t is { Schema: "db.2", Name: "Table.With.Dot" }));
                Assert.Single(dbModel.Tables.Where(t => t is { Schema: "db.2", Name: "SIMPLETABLENAME" }));
                Assert.Single(dbModel.Tables.Where(t => t is { Schema: "db.2", Name: "JUSTTABLENAME" }));

                Assert.Empty(dbModel.Tables.Where(t => t is { Schema: "PUBLIC", Name: "QuotedTableName" }));
                Assert.Single(dbModel.Tables.Where(t => t is { Schema: "PUBLIC", Name: "Table.With.Dot" }));
                Assert.Single(dbModel.Tables.Where(t => t is { Schema: "PUBLIC", Name: "SIMPLETABLENAME" }));
                Assert.Single(dbModel.Tables.Where(t => t is { Schema: "PUBLIC", Name: "JUSTTABLENAME" }));

                Assert.Single(dbModel.Tables.Where(t => t is { Schema: "DB2", Name: "QuotedTableName" }));
                Assert.Single(dbModel.Tables.Where(t => t is { Schema: "DB2", Name: "Table.With.Dot" }));
                Assert.Single(dbModel.Tables.Where(t => t is { Schema: "DB2", Name: "SIMPLETABLENAME" }));
                Assert.Single(dbModel.Tables.Where(t => t is { Schema: "DB2", Name: "JUSTTABLENAME" }));

                var principalTable = Assert.Single(dbModel.Tables.Where(t => t is { Schema: "DB2", Name: "PrincipalTable" }));
                // ReSharper disable once PossibleNullReferenceException
                Assert.NotNull(principalTable.PrimaryKey);
                Assert.Single(principalTable.UniqueConstraints);
                Assert.Single(principalTable.Indexes);

                var dependentTable = Assert.Single(dbModel.Tables.Where(t => t is { Schema: "DB2", Name: "DependentTable" }));
                // ReSharper disable once PossibleNullReferenceException
                Assert.Single(dependentTable.ForeignKeys);
            },
            @"
DROP SEQUENCE PUBLIC.SEQUENCE;
DROP SEQUENCE DB2.SEQUENCE;

DROP TABLE ""db.2"".""QuotedTableName"";
DROP TABLE ""db.2"".""Table.With.Dot"";
DROP TABLE ""db.2"".SIMPLETABLENAME;
DROP TABLE ""db.2"".JUSTTABLENAME;

DROP TABLE PUBLIC.""QuotedTableName"";
DROP TABLE PUBLIC.""Table.With.Dot"";
DROP TABLE PUBLIC.SIMPLETABLENAME;
DROP TABLE PUBLIC.JUSTTABLENAME;

DROP TABLE DB2.""QuotedTableName"";
DROP TABLE DB2.""Table.With.Dot"";
DROP TABLE DB2.SIMPLETABLENAME;
DROP TABLE DB2.JUSTTABLENAME;
DROP TABLE DB2.""DependentTable"";
DROP TABLE DB2.""PrincipalTable"";", 17, 16);

    #endregion

    #region Table

    [ConditionalTheory]
    [MemberData(nameof(TableTypes))]
    [Trait("Category", "UseHybridTables")]
    public void Create_columns(string tableType)
        => Test(
            $@"
CREATE {tableType}TABLE PUBLIC.""Blogs"" (
    Id int PRIMARY KEY COMMENT 'Blog.Id column comment.',
    Name nvarchar(100) NOT NULL
) COMMENT = 'Blog table comment.
On multiple lines.';
",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var table = dbModel.Tables.Single();

                Assert.Equal(2, table.Columns.Count);
                Assert.All(
                    table.Columns, c =>
                    {
                        Assert.Equal("PUBLIC", c.Table.Schema);
                        Assert.Equal("Blogs", c.Table.Name);
                        Assert.Equal(
                            @"Blog table comment.
On multiple lines.", c.Table.Comment);
                    });

                Assert.Single(table.Columns.Where(c => c.Name == "ID"));
                Assert.Single(table.Columns.Where(c => c.Name == "NAME"));
                Assert.Single(table.Columns.Where(c => c.Comment == "Blog.Id column comment."));
                Assert.Single(table.Columns.Where(c => c.Comment != null));
            },
            @"DROP TABLE PUBLIC.""Blogs""");

    [ConditionalFact]
    public void Create_view_columns()
        => Test(
            @"
CREATE VIEW PUBLIC.""BlogsView""
 AS
SELECT
 CAST(100 AS int) AS Id,
 CAST('' AS nvarchar(100)) AS Name;",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var table = Assert.IsType<DatabaseView>(dbModel.Tables.Single());

                Assert.Equal(2, table.Columns.Count);
                Assert.Null(table.PrimaryKey);
                Assert.All(
                    table.Columns, c =>
                    {
                        Assert.Equal("PUBLIC", c.Table.Schema);
                        Assert.Equal("BlogsView", c.Table.Name);
                    });

                Assert.Single(table.Columns.Where(c => c.Name == "ID"));
                Assert.Single(table.Columns.Where(c => c.Name == "NAME"));
            },
            @"DROP VIEW PUBLIC.""BlogsView"";");

    [ConditionalFact]
    public void Create_primary_key()
        => Test(
            @"
CREATE TABLE PrimaryKeyTable (
    ID int PRIMARY KEY
);",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var pk = dbModel.Tables.Single().PrimaryKey;

                Assert.Equal("PUBLIC", pk!.Table!.Schema);
                Assert.Equal("PrimaryKeyTable", pk.Table.Name, StringComparer.InvariantCultureIgnoreCase);
                Assert.StartsWith("SYS_CONSTRAINT", pk.Name);
                Assert.Equal(
                    new List<string> { "ID" }, pk.Columns.Select(ic => ic.Name).ToList());
            },
            "DROP TABLE PrimaryKeyTable;");
    
    [ConditionalFact]
    [Trait("Category", "UseHybridTables")]
    public void Create_primary_key_hybrid()
        => Test(
            @"
CREATE HYBRID TABLE PrimaryKeyTable (
    ID int PRIMARY KEY
);",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var pk = dbModel.Tables.Single().PrimaryKey;

                Assert.Equal("PUBLIC", pk!.Table!.Schema);
                Assert.Equal("PrimaryKeyTable", pk.Table.Name, StringComparer.InvariantCultureIgnoreCase);
                Assert.StartsWith("SYS_INDEX_PRIMARYKEYTABLE_PRIMARY", pk.Name);
                Assert.Equal(
                    new List<string> { "ID" }, pk.Columns.Select(ic => ic.Name).ToList());
            },
            "DROP TABLE PrimaryKeyTable;");

    [ConditionalFact]
    [Trait("Category", "UseHybridTables")]
    public void Create_unique_constraints()
        => Test(
            @"
CREATE HYBRID TABLE UniqueConstraint (
    Id int PRIMARY KEY,
    Name int Unique,
    IndexProperty int
);

CREATE INDEX IX_INDEX on UniqueConstraint ( IndexProperty );",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var uniqueConstraint = Assert.Single(dbModel.Tables.Single().UniqueConstraints);

                // ReSharper disable once PossibleNullReferenceException
                Assert.Equal("PUBLIC", uniqueConstraint.Table.Schema);
                Assert.Equal("UniqueConstraint", uniqueConstraint.Table.Name, StringComparer.InvariantCultureIgnoreCase);
                Assert.StartsWith("SYS_INDEX_UNIQUECONSTRAINT_UNIQUE_NAME", uniqueConstraint.Name);
                Assert.Equal(
                    new List<string> { "NAME" }, uniqueConstraint.Columns.Select(ic => ic.Name).ToList());
            },
            "DROP TABLE UniqueConstraint;",2);

    [ConditionalFact]
    public void Create_unique_constraints_standard()
        => Test(
            @"
CREATE TABLE UniqueConstraint (
    Id int PRIMARY KEY,
    Name int Unique,
    IndexProperty int
);",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                Assert.Empty(dbModel.Tables.Single().UniqueConstraints);
            },
            "DROP TABLE UniqueConstraint;");

    [ConditionalFact]
    [Trait("Category", "UseHybridTables")]
    public void Create_indexes()
        => Test(
            @"
CREATE HYBRID TABLE IndexTable (
    Id int PRIMARY KEY,
    Name int,
    IndexProperty int,
    INDEX IX_NAME ( Name )
);

CREATE INDEX IX_INDEX on IndexTable ( IndexProperty );",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var table = dbModel.Tables.Single();

                Assert.Equal(2, table.Indexes.Count);
                Assert.All(
                    table.Indexes, c =>
                    {
                        Assert.Equal("PUBLIC", c.Table!.Schema);
                        Assert.Equal("IndexTable", c.Table.Name, StringComparer.InvariantCultureIgnoreCase);
                    });

                Assert.Single(table.Indexes.Where(c => c.Name == "IX_NAME"));
                Assert.Single(table.Indexes.Where(c => c.Name == "IX_INDEX"));
            },
            "DROP TABLE IndexTable;", 2);

    [ConditionalFact]
    [Trait("Category", "UseHybridTables")]
    public void Create_multiple_indexes_on_same_column()
        => Test(
            @"
CREATE HYBRID TABLE IndexTable (
    Id int PRIMARY KEY,
    IndexProperty int,
    INDEX IX_One ( IndexProperty )
);

CREATE INDEX IX_Two on IndexTable ( IndexProperty );",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var table = dbModel.Tables.Single();

                Assert.Equal(2, table.Indexes.Count);
                Assert.All(
                    table.Indexes, c =>
                    {
                        Assert.Equal("PUBLIC", c.Table!.Schema);
                        Assert.Equal("INDEXTABLE", c.Table.Name);
                    });

                Assert.Collection(
                    table.Indexes.OrderBy(i => i.Name),
                    index =>
                    {
                        Assert.Equal("IX_ONE", index.Name);
                    },
                    index =>
                    {
                        Assert.Equal("IX_TWO", index.Name);
                    });
            },
            "DROP TABLE IndexTable;", 2);

    [ConditionalFact]
    [Trait("Category", "UseHybridTables")]
    public void Create_foreign_keys()
        => Test(
            @"
CREATE HYBRID TABLE ""PrincipalTable"" (
    Id int PRIMARY KEY
);

CREATE HYBRID TABLE ""FirstDependent"" (
    Id int PRIMARY KEY,
    ForeignKeyId int,
    FOREIGN KEY (ForeignKeyId) REFERENCES ""PrincipalTable""(Id) ON DELETE RESTRICT
);

CREATE HYBRID TABLE ""SecondDependent"" (
    Id int PRIMARY KEY,
    FOREIGN KEY (Id) REFERENCES ""PrincipalTable""(Id) ON DELETE NO ACTION
);",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var firstFk = Assert.Single(dbModel.Tables.Single(t => t.Name == "FirstDependent").ForeignKeys);

                // ReSharper disable once PossibleNullReferenceException
                Assert.Equal("PUBLIC", firstFk.Table.Schema);
                Assert.Equal("FirstDependent", firstFk.Table.Name);
                Assert.Equal("PUBLIC", firstFk.PrincipalTable.Schema);
                Assert.Equal("PrincipalTable", firstFk.PrincipalTable.Name);
                Assert.Equal(
                    new List<string> { "FOREIGNKEYID" }, firstFk.Columns.Select(ic => ic.Name).ToList());
                Assert.Equal(
                    new List<string> { "ID" }, firstFk.PrincipalColumns.Select(ic => ic.Name).ToList());
                Assert.Equal(ReferentialAction.Restrict, firstFk.OnDelete);

                var secondFk = Assert.Single(dbModel.Tables.Single(t => t.Name == "SecondDependent").ForeignKeys);

                // ReSharper disable once PossibleNullReferenceException
                Assert.Equal("PUBLIC", secondFk.Table.Schema);
                Assert.Equal("SecondDependent", secondFk.Table.Name);
                Assert.Equal("PUBLIC", secondFk.PrincipalTable.Schema);
                Assert.Equal("PrincipalTable", secondFk.PrincipalTable.Name);
                Assert.Equal(
                    new List<string> { "ID" }, secondFk.Columns.Select(ic => ic.Name).ToList());
                Assert.Equal(
                    new List<string> { "ID" }, secondFk.PrincipalColumns.Select(ic => ic.Name).ToList());
                Assert.Equal(ReferentialAction.NoAction, secondFk.OnDelete);
            },
            @"
DROP TABLE ""SecondDependent"";
DROP TABLE ""FirstDependent"";
DROP TABLE ""PrincipalTable"";", 3, 3);

    [ConditionalFact]
    public void Create_foreign_keys_standard()
        => Test(
            @"
CREATE TABLE ""PrincipalTable"" (
    Id int PRIMARY KEY
);

CREATE TABLE ""FirstDependent"" (
    Id int PRIMARY KEY,
    ForeignKeyId int,
    FOREIGN KEY (ForeignKeyId) REFERENCES ""PrincipalTable""(Id)
);

CREATE TABLE ""SecondDependent"" (
    Id int PRIMARY KEY,
    FOREIGN KEY (Id) REFERENCES ""PrincipalTable""(Id)
);",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var firstFk = Assert.Single(dbModel.Tables.Single(t => t.Name == "FirstDependent").ForeignKeys);
                
                Assert.Equal("PUBLIC", firstFk.Table.Schema);
                Assert.Equal("FirstDependent", firstFk.Table.Name);
                Assert.Equal("PUBLIC", firstFk.PrincipalTable.Schema);
                Assert.Equal("PrincipalTable", firstFk.PrincipalTable.Name);
                Assert.Equal(
                    new List<string> { "FOREIGNKEYID" }, firstFk.Columns.Select(ic => ic.Name).ToList());
                Assert.Equal(
                    new List<string> { "ID" }, firstFk.PrincipalColumns.Select(ic => ic.Name).ToList());
                Assert.Equal(ReferentialAction.NoAction, firstFk.OnDelete);

                var secondFk = Assert.Single(dbModel.Tables.Single(t => t.Name == "SecondDependent").ForeignKeys);
                
                Assert.Equal("PUBLIC", secondFk.Table.Schema);
                Assert.Equal("SecondDependent", secondFk.Table.Name);
                Assert.Equal("PUBLIC", secondFk.PrincipalTable.Schema);
                Assert.Equal("PrincipalTable", secondFk.PrincipalTable.Name);
                Assert.Equal(
                    new List<string> { "ID" }, secondFk.Columns.Select(ic => ic.Name).ToList());
                Assert.Equal(
                    new List<string> { "ID" }, secondFk.PrincipalColumns.Select(ic => ic.Name).ToList());
                Assert.Equal(ReferentialAction.NoAction, secondFk.OnDelete);
            },
            @"
DROP TABLE ""SecondDependent"";
DROP TABLE ""FirstDependent"";
DROP TABLE ""PrincipalTable"";", 3, 3);

    #endregion

    #region ColumnFacets

    [ConditionalFact]
    public void Decimal_numeric_types_have_precision_scale()
        => Test(
            @"
CREATE TABLE NumericColumns (
    ""Id"" int,

    ""number152Column"" number(15, 2) NOT NULL
);",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var columns = dbModel.Tables.Single().Columns;
                Assert.Equal("NUMBER(15, 2)", columns.Single(c => string.Equals(c.Name, "number152Column", StringComparison.InvariantCultureIgnoreCase)).StoreType);
            },
            "DROP TABLE NumericColumns;");

    [ConditionalFact]
    public void Default_length_of_string_and_binary_translate_to_store_type()
        => Test(
            @"
CREATE TABLE DefaultLenColumns (
    Id int,
    stringDefaultLenColumn string NULL,
    textDefaultLenColumn text NULL,
    varcharDefaultLenColumn varchar NULL,
    nvarcharDefaultLenColumn nvarchar NULL,
    nvarchar2DefaultLenColumn nvarchar2 NULL,
    charVaryingDefaultLenColumn char varying NULL,
    nCharVaryingDefaultLenColumn nchar varying NULL,
    characterVaryingDefaultLenColumn character varying NULL,
    binaryDefaultLenColumn binary NULL,
    varbinaryDefaultLenColumn varbinary NULL,
    charDefaultLenColumn char NULL,
    characterDefaultLenColumn character NULL,
    nationalCharDefaultLenColumn nchar NULL
);",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var columns = dbModel.Tables.Single().Columns;

                Assert.Equal("VARCHAR", columns.Single(c => string.Equals(c.Name, "stringDefaultLenColumn", StringComparison.InvariantCultureIgnoreCase)).StoreType);
                Assert.Equal("VARCHAR", columns.Single(c => string.Equals(c.Name, "textDefaultLenColumn", StringComparison.InvariantCultureIgnoreCase)).StoreType);
                Assert.Equal("VARCHAR", columns.Single(c => string.Equals(c.Name, "varcharDefaultLenColumn", StringComparison.InvariantCultureIgnoreCase)).StoreType);
                Assert.Equal("VARCHAR", columns.Single(c => string.Equals(c.Name, "nvarcharDefaultLenColumn", StringComparison.InvariantCultureIgnoreCase)).StoreType);
                Assert.Equal("VARCHAR", columns.Single(c => string.Equals(c.Name, "nvarchar2DefaultLenColumn", StringComparison.InvariantCultureIgnoreCase)).StoreType);
                Assert.Equal("VARCHAR", columns.Single(c => string.Equals(c.Name, "charVaryingDefaultLenColumn", StringComparison.InvariantCultureIgnoreCase)).StoreType);
                Assert.Equal("VARCHAR", columns.Single(c => string.Equals(c.Name, "nCharVaryingDefaultLenColumn", StringComparison.InvariantCultureIgnoreCase)).StoreType);
                Assert.Equal("VARCHAR", columns.Single(c => string.Equals(c.Name, "characterVaryingDefaultLenColumn", StringComparison.InvariantCultureIgnoreCase)).StoreType);
                Assert.Equal("BINARY", columns.Single(c => string.Equals(c.Name, "binaryDefaultLenColumn", StringComparison.InvariantCultureIgnoreCase)).StoreType);
                Assert.Equal("BINARY", columns.Single(c => string.Equals(c.Name, "varbinaryDefaultLenColumn", StringComparison.InvariantCultureIgnoreCase)).StoreType);
                Assert.Equal("VARCHAR", columns.Single(c => string.Equals(c.Name, "charDefaultLenColumn", StringComparison.InvariantCultureIgnoreCase)).StoreType);
                Assert.Equal("VARCHAR", columns.Single(c => string.Equals(c.Name, "characterDefaultLenColumn", StringComparison.InvariantCultureIgnoreCase)).StoreType);
                Assert.Equal("VARCHAR", columns.Single(c => string.Equals(c.Name, "nationalCharDefaultLenColumn", StringComparison.InvariantCultureIgnoreCase)).StoreType);
            },
            "DROP TABLE DefaultLenColumns;");

    [ConditionalFact]
    public void Specific_max_length_are_add_to_store_type()
        => Test(
            @"
CREATE TABLE LengthColumns (
    Id int,
    string1Column string(1) NULL,
    text5Column text(5) NULL,
    char10Column char(10) NULL,
    varchar66Column varchar(66) NULL,
    nvarchar77Column nvarchar(77) NULL,
    nvarchar288Column nvarchar2(88) NULL,
    nchar99Column nchar(99) NULL,
    charVarying110Column char varying(110) NULL,
    nCharVarying115Column nchar varying(115) NULL,
    nvarchar121Column nvarchar(121) NULL,
    characterVarying133Column character varying(133) NULL,
    binary137Column binary(137) NULL,
    varbinary143Column varbinary(143) NULL,
    character155Column character(155) NULL
);",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var columns = dbModel.Tables.Single().Columns;

                Assert.Equal("VARCHAR", columns.Single(c => string.Equals(c.Name, "string1Column", StringComparison.InvariantCultureIgnoreCase)).StoreType);
                Assert.Equal("VARCHAR", columns.Single(c => string.Equals(c.Name, "text5Column", StringComparison.InvariantCultureIgnoreCase)).StoreType);
                Assert.Equal("VARCHAR", columns.Single(c => string.Equals(c.Name, "char10Column", StringComparison.InvariantCultureIgnoreCase)).StoreType);
                Assert.Equal("VARCHAR", columns.Single(c => string.Equals(c.Name, "varchar66Column", StringComparison.InvariantCultureIgnoreCase)).StoreType);
                Assert.Equal("VARCHAR", columns.Single(c => string.Equals(c.Name, "nvarchar77Column", StringComparison.InvariantCultureIgnoreCase)).StoreType);
                Assert.Equal("VARCHAR", columns.Single(c => string.Equals(c.Name, "nvarchar288Column", StringComparison.InvariantCultureIgnoreCase)).StoreType);
                Assert.Equal("VARCHAR", columns.Single(c => string.Equals(c.Name, "nchar99Column", StringComparison.InvariantCultureIgnoreCase)).StoreType);
                Assert.Equal("VARCHAR", columns.Single(c => string.Equals(c.Name, "charVarying110Column", StringComparison.InvariantCultureIgnoreCase)).StoreType);
                Assert.Equal("VARCHAR", columns.Single(c => string.Equals(c.Name, "nCharVarying115Column", StringComparison.InvariantCultureIgnoreCase)).StoreType);
                Assert.Equal("VARCHAR", columns.Single(c => string.Equals(c.Name, "nvarchar121Column", StringComparison.InvariantCultureIgnoreCase)).StoreType);
                Assert.Equal("VARCHAR", columns.Single(c => string.Equals(c.Name, "characterVarying133Column", StringComparison.InvariantCultureIgnoreCase)).StoreType);
                Assert.Equal("BINARY", columns.Single(c => string.Equals(c.Name, "binary137Column", StringComparison.InvariantCultureIgnoreCase)).StoreType);
                Assert.Equal("BINARY", columns.Single(c => string.Equals(c.Name, "varbinary143Column", StringComparison.InvariantCultureIgnoreCase)).StoreType);
                Assert.Equal("VARCHAR", columns.Single(c => string.Equals(c.Name, "character155Column", StringComparison.InvariantCultureIgnoreCase)).StoreType);
            },
            "DROP TABLE LengthColumns;");

    [ConditionalFact]
    // TODO CHECK if length is required in EF Core
    public void Default_max_length_are_added_to_binary_varbinary()
        => Test(
            @"
CREATE TABLE DefaultRequiredLengthBinaryColumns (
    Id int,
    binaryColumn binary(8000),
    binaryVaryingColumn binary varying(8000),
    varbinaryColumn varbinary(8000)
);",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var columns = dbModel.Tables.Single().Columns;

                Assert.Equal("BINARY", columns.Single(c => string.Equals(c.Name, "binaryColumn", StringComparison.InvariantCultureIgnoreCase)).StoreType);
                Assert.Equal("BINARY", columns.Single(c => string.Equals(c.Name, "binaryVaryingColumn", StringComparison.InvariantCultureIgnoreCase)).StoreType);
                Assert.Equal("BINARY", columns.Single(c => string.Equals(c.Name, "varbinaryColumn", StringComparison.InvariantCultureIgnoreCase)).StoreType);
            },
            "DROP TABLE DefaultRequiredLengthBinaryColumns;");

    [ConditionalFact]
    public void Default_max_length_are_added_to_char_1()
        => Test(
            @"
CREATE TABLE DefaultRequiredLengthCharColumns (
    Id int,
    charColumn char(8000)
);",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var columns = dbModel.Tables.Single().Columns;

                Assert.Equal("VARCHAR", columns.Single(c => string.Equals(c.Name, "charColumn", StringComparison.InvariantCultureIgnoreCase)).StoreType);
            },
            "DROP TABLE DefaultRequiredLengthCharColumns;");

    [ConditionalFact]
    public void Default_max_length_are_added_to_char_2()
        => Test(
            @"
CREATE TABLE DefaultRequiredLengthCharColumns (
    Id int,
    characterColumn character(8000)
);",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var columns = dbModel.Tables.Single().Columns;

                Assert.Equal("VARCHAR", columns.Single(c => string.Equals(c.Name, "characterColumn", StringComparison.InvariantCultureIgnoreCase)).StoreType);
            },
            "DROP TABLE DefaultRequiredLengthCharColumns;");

    [ConditionalFact]
    public void Default_max_length_are_added_to_varchar()
        => Test(
            @"
CREATE TABLE DefaultRequiredLengthVarcharColumns (
    Id int,
    charVaryingColumn char varying(8000),
    characterVaryingColumn character varying(8000),
    varcharColumn varchar(8000)
);",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var columns = dbModel.Tables.Single().Columns;

                Assert.Equal("VARCHAR", columns.Single(c => string.Equals(c.Name, "charVaryingColumn", StringComparison.InvariantCultureIgnoreCase)).StoreType);
                Assert.Equal("VARCHAR", columns.Single(c => string.Equals(c.Name, "characterVaryingColumn", StringComparison.InvariantCultureIgnoreCase)).StoreType);
                Assert.Equal("VARCHAR", columns.Single(c => string.Equals(c.Name, "varcharColumn", StringComparison.InvariantCultureIgnoreCase)).StoreType);
            },
            "DROP TABLE DefaultRequiredLengthVarcharColumns;");

    [ConditionalFact]
    public void Default_max_length_are_added_to_nchar_1()
        => Test(
            @"
CREATE TABLE DefaultRequiredLengthNcharColumns (
    Id int,
    nationalCharColumn nchar(16777216)
);",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var columns = dbModel.Tables.Single().Columns;

                Assert.Equal("VARCHAR", columns.Single(c => string.Equals(c.Name, "nationalCharColumn", StringComparison.InvariantCultureIgnoreCase)).StoreType);
            },
            "DROP TABLE DefaultRequiredLengthNcharColumns;");

    [ConditionalFact]
    public void Default_max_length_are_added_to_nvarchar()
        => Test(
            @"
CREATE TABLE DefaultRequiredLengthNvarcharColumns (
    Id int,
    nationalCharVaryingColumn nchar varying(16777216),
    nvarcharColumn nvarchar(16777216)
);",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var columns = dbModel.Tables.Single().Columns;

                Assert.Equal("VARCHAR", columns.Single(c => string.Equals(c.Name, "nationalCharVaryingColumn", StringComparison.InvariantCultureIgnoreCase)).StoreType);
                Assert.Equal("VARCHAR", columns.Single(c => string.Equals(c.Name, "nvarcharColumn", StringComparison.InvariantCultureIgnoreCase)).StoreType);
            },
            "DROP TABLE DefaultRequiredLengthNvarcharColumns;");

    [ConditionalFact]
    public void Datetime_types_have_precision_if_non_null_scale()
        => Test(
            @"
CREATE TABLE LengthColumns (
    Id int,
    time4Column time(4) NULL,
    datetime4Column datetime(4) NULL,
    timezonetz5Column timestamp_tz(5) NULL,
    timezoneltz6Column timestamp_ltz(6) NULL,
    timezonentz7Column timestamp_ntz(7) NULL
);",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var columns = dbModel.Tables.Single().Columns;

                Assert.Equal("TIME", columns.Single(c => string.Equals(c.Name, "time4Column", StringComparison.InvariantCultureIgnoreCase)).StoreType);
                Assert.Equal("TIMESTAMP_NTZ", columns.Single(c => string.Equals(c.Name, "datetime4Column", StringComparison.InvariantCultureIgnoreCase)).StoreType);
                Assert.Equal("TIMESTAMP_TZ", columns.Single(c => string.Equals(c.Name, "timezonetz5Column", StringComparison.InvariantCultureIgnoreCase)).StoreType);
                Assert.Equal("TIMESTAMP_LTZ", columns.Single(c => string.Equals(c.Name, "timezoneltz6Column", StringComparison.InvariantCultureIgnoreCase)).StoreType);
                Assert.Equal("TIMESTAMP_NTZ", columns.Single(c => string.Equals(c.Name, "timezonentz7Column", StringComparison.InvariantCultureIgnoreCase)).StoreType);
            },
            "DROP TABLE LengthColumns;");

    [ConditionalFact]
    public void Types_with_required_length_uses_length_of_one()
        => Test(
            @"
CREATE TABLE OneLengthColumns (
    Id int,
    characterColumn character NULL,
    charColumn char NULL,
    ncharColumn nchar NULL
);",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var columns = dbModel.Tables.Single().Columns;

                Assert.Equal("VARCHAR", columns.Single(c => string.Equals(c.Name, "characterColumn", StringComparison.InvariantCultureIgnoreCase)).StoreType);
                Assert.Equal("VARCHAR", columns.Single(c => string.Equals(c.Name, "charColumn", StringComparison.InvariantCultureIgnoreCase)).StoreType);
                Assert.Equal("VARCHAR", columns.Single(c => string.Equals(c.Name, "ncharColumn", StringComparison.InvariantCultureIgnoreCase)).StoreType);
            },
            "DROP TABLE OneLengthColumns;");

    [ConditionalFact]
    public void Store_types_without_any_facets()
        => Test(
            @"
CREATE TABLE NoFacetTypes (
    Id int,
    numberColumn number NOT NULL,
    decimalColumn decimal NOT NULL,
    numericColumn numeric NOT NULL,
    intColumn int NOT NULL,
    integerColumn integer NOT NULL,
    bigintColumn bigint NOT NULL,
    smallintColumn smallint NOT NULL,
    tinyintColumn tinyint NOT NULL,
    byteintColumn byteint NOT NULL,
    floatColumn float NOT NULL,
    float4Column float4 NOT NULL,
    float8Column float8 NOT NULL,
    doubleColumn double NOT NULL,
    doublePrecisionColumn double precision NOT NULL,
    realColumn real NOT NULL,
    varcharColumn varchar NOT NULL,
    charColumn char NOT NULL,
    characterColumn character NOT NULL,
    stringColumn string NOT NULL,
    textColumn text NOT NULL,
    binaryColumn binary NOT NULL,
    varbinaryColumn varbinary NOT NULL,
    booleanColumn boolean NOT NULL,
    dateColumn date NOT NULL,
    datetimeColumn datetime NOT NULL,
    timeColumn time NOT NULL,
    timestampColumn timestamp NOT NULL,
    timestamp_ltzColumn timestamp_ltz NOT NULL,
    timestamp_ntzColumn timestamp_ntz NOT NULL,
    timestamp_tzColumn timestamp_tz NOT NULL,
    variantColumn variant NOT NULL,
    objectColumn object NOT NULL,
    arrayColumn array NOT NULL,
    geographyColumn geography NOT NULL,
    geometryColumn geometry NOT NULL
);",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var columns = dbModel.Tables.Single(t =>string.Equals(t.Name , "NoFacetTypes", StringComparison.InvariantCultureIgnoreCase)).Columns;

                Assert.Equal("NUMBER(38, 0)", columns.Single(c => string.Equals(c.Name, "numberColumn", StringComparison.InvariantCultureIgnoreCase)).StoreType);
                Assert.Equal("NUMBER(38, 0)", columns.Single(c => string.Equals(c.Name, "decimalColumn", StringComparison.InvariantCultureIgnoreCase)).StoreType);
                Assert.Equal("NUMBER(38, 0)", columns.Single(c => string.Equals(c.Name, "numericColumn", StringComparison.InvariantCultureIgnoreCase)).StoreType);
                Assert.Equal("NUMBER(38, 0)", columns.Single(c => string.Equals(c.Name, "intColumn", StringComparison.InvariantCultureIgnoreCase)).StoreType);
                Assert.Equal("NUMBER(38, 0)", columns.Single(c => string.Equals(c.Name, "integerColumn", StringComparison.InvariantCultureIgnoreCase)).StoreType);
                Assert.Equal("NUMBER(38, 0)", columns.Single(c => string.Equals(c.Name, "bigintColumn", StringComparison.InvariantCultureIgnoreCase)).StoreType);
                Assert.Equal("NUMBER(38, 0)", columns.Single(c => string.Equals(c.Name, "smallintColumn", StringComparison.InvariantCultureIgnoreCase)).StoreType);
                Assert.Equal("NUMBER(38, 0)", columns.Single(c => string.Equals(c.Name, "tinyintColumn", StringComparison.InvariantCultureIgnoreCase)).StoreType);
                Assert.Equal("NUMBER(38, 0)", columns.Single(c => string.Equals(c.Name, "byteintColumn", StringComparison.InvariantCultureIgnoreCase)).StoreType);
                Assert.Equal("FLOAT", columns.Single(c => string.Equals(c.Name, "floatColumn", StringComparison.InvariantCultureIgnoreCase)).StoreType);
                Assert.Equal("FLOAT", columns.Single(c => string.Equals(c.Name, "float4Column", StringComparison.InvariantCultureIgnoreCase)).StoreType);
                Assert.Equal("FLOAT", columns.Single(c => string.Equals(c.Name, "float8Column", StringComparison.InvariantCultureIgnoreCase)).StoreType);
                Assert.Equal("FLOAT", columns.Single(c => string.Equals(c.Name, "doubleColumn", StringComparison.InvariantCultureIgnoreCase)).StoreType);
                Assert.Equal("FLOAT", columns.Single(c => string.Equals(c.Name, "doublePrecisionColumn", StringComparison.InvariantCultureIgnoreCase)).StoreType);
                Assert.Equal("FLOAT", columns.Single(c => string.Equals(c.Name, "realColumn", StringComparison.InvariantCultureIgnoreCase)).StoreType);
                Assert.Equal("VARCHAR", columns.Single(c => string.Equals(c.Name, "varcharColumn", StringComparison.InvariantCultureIgnoreCase)).StoreType);
                Assert.Equal("VARCHAR", columns.Single(c => string.Equals(c.Name, "charColumn", StringComparison.InvariantCultureIgnoreCase)).StoreType);
                Assert.Equal("VARCHAR", columns.Single(c => string.Equals(c.Name, "characterColumn", StringComparison.InvariantCultureIgnoreCase)).StoreType);
                Assert.Equal("VARCHAR", columns.Single(c => string.Equals(c.Name, "stringColumn", StringComparison.InvariantCultureIgnoreCase)).StoreType);
                Assert.Equal("VARCHAR", columns.Single(c => string.Equals(c.Name, "textColumn", StringComparison.InvariantCultureIgnoreCase)).StoreType);
                Assert.Equal("BINARY", columns.Single(c => string.Equals(c.Name, "binaryColumn", StringComparison.InvariantCultureIgnoreCase)).StoreType);
                Assert.Equal("BINARY", columns.Single(c => string.Equals(c.Name, "varbinaryColumn", StringComparison.InvariantCultureIgnoreCase)).StoreType);
                Assert.Equal("BOOLEAN", columns.Single(c => string.Equals(c.Name, "booleanColumn", StringComparison.InvariantCultureIgnoreCase)).StoreType);
                Assert.Equal("DATE", columns.Single(c => string.Equals(c.Name, "dateColumn", StringComparison.InvariantCultureIgnoreCase)).StoreType);
                Assert.Equal("TIMESTAMP_NTZ", columns.Single(c => string.Equals(c.Name, "datetimeColumn", StringComparison.InvariantCultureIgnoreCase)).StoreType);
                Assert.Equal("TIME", columns.Single(c => string.Equals(c.Name, "timeColumn", StringComparison.InvariantCultureIgnoreCase)).StoreType);
                Assert.Equal("TIMESTAMP_NTZ", columns.Single(c => string.Equals(c.Name, "timestampColumn", StringComparison.InvariantCultureIgnoreCase)).StoreType);
                Assert.Equal("TIMESTAMP_LTZ", columns.Single(c => string.Equals(c.Name, "timestamp_ltzColumn", StringComparison.InvariantCultureIgnoreCase)).StoreType);
                Assert.Equal("TIMESTAMP_NTZ", columns.Single(c => string.Equals(c.Name, "timestamp_ntzColumn", StringComparison.InvariantCultureIgnoreCase)).StoreType);
                Assert.Equal("TIMESTAMP_TZ", columns.Single(c => string.Equals(c.Name, "timestamp_tzColumn", StringComparison.InvariantCultureIgnoreCase)).StoreType);
                Assert.Equal("VARIANT", columns.Single(c => string.Equals(c.Name, "variantColumn", StringComparison.InvariantCultureIgnoreCase)).StoreType);
                Assert.Equal("OBJECT", columns.Single(c => string.Equals(c.Name, "objectColumn", StringComparison.InvariantCultureIgnoreCase)).StoreType);
                Assert.Equal("ARRAY", columns.Single(c => string.Equals(c.Name, "arrayColumn", StringComparison.InvariantCultureIgnoreCase)).StoreType);
                Assert.Equal("GEOGRAPHY", columns.Single(c => string.Equals(c.Name, "geographyColumn", StringComparison.InvariantCultureIgnoreCase)).StoreType);
                Assert.Equal("GEOMETRY", columns.Single(c => string.Equals(c.Name, "geometryColumn", StringComparison.InvariantCultureIgnoreCase)).StoreType);
            },
            @"
DROP TABLE NoFacetTypes;");

    [ConditionalFact]
    public void Default_and_computed_values_are_stored()
        => Test(
            @"
CREATE TABLE DefaultComputedValues (
    Id int,
    FixedDefaultValue timestamp_ntz NOT NULL DEFAULT to_timestamp_ntz('October 20, 2015 11am'),
    dateToUseInDefault datetime,
    ValueCallFunctionBaseInAnotherColumn int DEFAULT NEXT_DAY(dateToUseInDefault, 'Friday '),
    A int NOT NULL,
    B int NOT NULL,
    SumOfAAndB int DEFAULT A + B);",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var columns = dbModel.Tables.Single().Columns;

                var fixedDefaultValue = columns.Single(c => string.Equals(c.Name,"FixedDefaultValue", StringComparison.InvariantCultureIgnoreCase ));
                Assert.Equal("CAST('October 20, 2015 11am' AS TIMESTAMP_NTZ(9))", fixedDefaultValue.DefaultValueSql);
                Assert.Null(fixedDefaultValue.ComputedColumnSql);

                var defaultValueFromFunctionUsingColumn = columns.Single(c => string.Equals(c.Name,"ValueCallFunctionBaseInAnotherColumn", StringComparison.InvariantCultureIgnoreCase));
                Assert.Equal("NEXT_DAY(CAST(DATETOUSEINDEFAULT AS DATE), 'Friday ')", defaultValueFromFunctionUsingColumn.DefaultValueSql);
                Assert.Null(defaultValueFromFunctionUsingColumn.ComputedColumnSql);

                var sumOfAAndB = columns.Single(c => string.Equals(c.Name, "SumOfAAndB", StringComparison.InvariantCultureIgnoreCase));
                Assert.Equal("A + B", sumOfAAndB.DefaultValueSql);
                Assert.Null(sumOfAAndB.ComputedColumnSql);

            },
            "DROP TABLE DefaultComputedValues;");

    [ConditionalFact]
    public void Non_literal_bool_default_values_are_passed_through()
        => Test(
            @"
CREATE TABLE MyTable (
    Id int,
    A boolean DEFAULT (TO_BOOLEAN(Id))
);",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var columns = dbModel.Tables.Single().Columns;

                var column = columns.Single(c => c.Name == "A");
                Assert.Equal("CAST(ID <> 0 AS BOOLEAN)", column.DefaultValueSql);
                Assert.Null(column.FindAnnotation(RelationalAnnotationNames.DefaultValue));
            },
            "DROP TABLE MyTable;");

    [ConditionalFact]
    public void Simple_int_literals_are_parsed_for_HasDefaultValue()
        => Test(
            """
            CREATE TABLE MyTable (
                ID INT,
                A INT DEFAULT -1,
                B INT DEFAULT 0,
                C INT DEFAULT (0),
                D INT DEFAULT (-2),
                E INT DEFAULT (2),
                F INT DEFAULT (3),
                G INT DEFAULT ((4)),
                H INT DEFAULT CAST(6 AS INTEGER),
                I INT DEFAULT TO_NUMBER('-7'),
                J INT DEFAULT CAST(((-8)) AS INTEGER)
            );
            """,
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var columns = dbModel.Tables.Single().Columns;

                var column = columns.Single(c => c.Name == "A");
                Assert.Equal("-1", column.DefaultValueSql);
                Assert.Equal((decimal)-1, column.DefaultValue);

                column = columns.Single(c => c.Name == "B");
                Assert.Equal("0", column.DefaultValueSql);
                Assert.Equal((decimal)0, column.DefaultValue);

                column = columns.Single(c => c.Name == "C");
                Assert.Equal("0", column.DefaultValueSql);
                Assert.Equal((decimal)0, column.DefaultValue);

                column = columns.Single(c => c.Name == "D");
                Assert.Equal("-2", column.DefaultValueSql);
                Assert.Equal((decimal)-2, column.DefaultValue);

                column = columns.Single(c => c.Name == "E");
                Assert.Equal("2", column.DefaultValueSql);
                Assert.Equal((decimal)2, column.DefaultValue);

                column = columns.Single(c => c.Name == "F");
                Assert.Equal("3", column.DefaultValueSql);
                Assert.Equal((decimal)3, column.DefaultValue);

                column = columns.Single(c => c.Name == "G");
                Assert.Equal("4", column.DefaultValueSql);
                Assert.Equal((decimal)4, column.DefaultValue);

                column = columns.Single(c => c.Name == "H");
                Assert.Equal("CAST(6 AS NUMBER(38,0))", column.DefaultValueSql);
                Assert.Equal((decimal)6, column.DefaultValue);

                column = columns.Single(c => c.Name == "I");
                Assert.Equal("TO_NUMBER('-7')", column.DefaultValueSql);
                Assert.Null(column.DefaultValue);

                column = columns.Single(c => c.Name == "J");
                Assert.Equal("CAST(-8 AS NUMBER(38,0))", column.DefaultValueSql);
                Assert.Equal((decimal)-8, column.DefaultValue);
            },
            "DROP TABLE MyTable;");

    [ConditionalFact]
    public void Simple_short_literals_are_parsed_for_HasDefaultValue()
        => Test(
            """
            CREATE TABLE MyTable (
                ID INT,
                A SMALLINT DEFAULT -1,
                B SMALLINT DEFAULT (0)
            );
            """,
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var columns = dbModel.Tables.Single().Columns;

                var column = columns.Single(c => c.Name == "A");
                Assert.Equal("-1", column.DefaultValueSql);
                Assert.Equal((decimal)-1, column.DefaultValue);

                column = columns.Single(c => c.Name == "B");
                Assert.Equal("0", column.DefaultValueSql);
                Assert.Equal((decimal)0, column.DefaultValue);
            },
            "DROP TABLE MyTable;");

    [ConditionalFact]
    public void Simple_long_literals_are_parsed_for_HasDefaultValue()
        => Test(
            """
            CREATE TABLE MyTable (
                ID INT,
                A BIGINT DEFAULT -1,
                B BIGINT DEFAULT (0),
                C BIGINT DEFAULT ((CAST(((-7)) AS BIGINT)))
            );
            """,
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var columns = dbModel.Tables.Single().Columns;

                var column = columns.Single(c => c.Name == "A");
                Assert.Equal("-1", column.DefaultValueSql);
                Assert.Equal((decimal)-1.0, column.DefaultValue);

                column = columns.Single(c => c.Name == "B");
                Assert.Equal("0", column.DefaultValueSql);
                Assert.Equal((decimal)0, column.DefaultValue);

                column = columns.Single(c => c.Name == "C");
                Assert.Equal("CAST(-7 AS NUMBER(38,0))", column.DefaultValueSql);
                Assert.Equal((decimal)-7, column.DefaultValue);
            },
            "DROP TABLE MyTable;");

    [ConditionalFact]
    public void Simple_byte_literals_are_parsed_for_HasDefaultValue()
        => Test(
            """
            CREATE TABLE MyTable (
                ID INT,
                A TINYINT DEFAULT 1,
                B TINYINT DEFAULT (0)
            );
            """,
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var columns = dbModel.Tables.Single().Columns;

                var column = columns.Single(c => c.Name == "A");
                Assert.Equal("1", column.DefaultValueSql);
                Assert.Equal((decimal)1.0, column.DefaultValue);

                column = columns.Single(c => c.Name == "B");
                Assert.Equal("0", column.DefaultValueSql);
                Assert.Equal((decimal)0, column.DefaultValue);
            },
            "DROP TABLE MyTable;");
    
    [ConditionalFact]
    public void Non_literal_int_default_values_are_passed_through()
        => Test(
            """
            CREATE TABLE MyTable (
                ID INTEGER,
                A INTEGER DEFAULT CASE
                    1
                    WHEN 1 THEN 0
                    WHEN 2 THEN 1
                    WHEN 3 THEN 2
                    ELSE NULL
                END,
                B INTEGER DEFAULT TO_NUMBER(
                    CASE
                        1
                        WHEN 1 THEN 0
                        WHEN 2 THEN 1
                        WHEN 3 THEN 2
                        ELSE NULL
                    END
                )
            );
            """,
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var columns = dbModel.Tables.Single().Columns;

                var column = columns.Single(c => c.Name == "A");
                Assert.Equal("CASE 1 WHEN 1 THEN 0 WHEN 2 THEN 1 WHEN 3 THEN 2 ELSE SYSTEM$NULL_TO_FIXED(null) END", column.DefaultValueSql);
                Assert.Null(column.FindAnnotation(RelationalAnnotationNames.DefaultValue));

                column = columns.Single(c => c.Name == "B");
                Assert.Equal("CASE 1 WHEN 1 THEN 0 WHEN 2 THEN 1 WHEN 3 THEN 2 ELSE SYSTEM$NULL_TO_FIXED(null) END", column.DefaultValueSql);
                Assert.Null(column.FindAnnotation(RelationalAnnotationNames.DefaultValue));
            },
            "DROP TABLE MyTable;");

    [ConditionalFact]
    public void Simple_double_literals_are_parsed_for_HasDefaultValue()
        => Test(
            """
            CREATE TABLE MyTable (
                ID INT,
                A FLOAT DEFAULT -1.1111,
                B FLOAT DEFAULT (0.0),
                C FLOAT DEFAULT (1.1000000000000001E+000),
                D FLOAT DEFAULT ((CAST(((1.1234)) AS FLOAT)))
            );
            """,
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var columns = dbModel.Tables.Single().Columns;

                var column = columns.Single(c => c.Name == "A");
                Assert.Equal("-1.1111", column.DefaultValueSql);
                Assert.Equal(-1.1111, column.DefaultValue);

                column = columns.Single(c => c.Name == "B");
                Assert.Equal("0", column.DefaultValueSql);
                Assert.Equal((double)0, column.DefaultValue);

                column = columns.Single(c => c.Name == "C");
                Assert.Equal("1.1000000000000001", column.DefaultValueSql);
                Assert.Equal(1.1000000000000001e+000, column.DefaultValue);

                column = columns.Single(c => c.Name == "D");
                Assert.Equal("CAST(1.1234 AS FLOAT)", column.DefaultValueSql);
                Assert.Equal(1.1234, column.DefaultValue);
            },
            "DROP TABLE MyTable;");

    [ConditionalFact]
    public void Simple_float_literals_are_parsed_for_HasDefaultValue()
        => Test(
            """
            CREATE TABLE MyTable (
                ID INT,
                A REAL DEFAULT -1.1111,
                B REAL DEFAULT (0.0),
                C REAL DEFAULT (1.1000000000000001E+000),
                D REAL DEFAULT ((CAST(((1.1234)) AS REAL)))
            );
            """,
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var columns = dbModel.Tables.Single().Columns;

                var column = columns.Single(c => c.Name == "A");
                Assert.Equal("-1.1111", column.DefaultValueSql);
                Assert.Equal(-1.1111, column.DefaultValue);

                column = columns.Single(c => c.Name == "B");
                Assert.Equal("0", column.DefaultValueSql);
                Assert.Equal((double)0, column.DefaultValue);

                column = columns.Single(c => c.Name == "C");
                Assert.Equal("1.1000000000000001", column.DefaultValueSql);
                Assert.Equal(1.1000000000000001e+000, column.DefaultValue);

                column = columns.Single(c => c.Name == "D");
                Assert.Equal("CAST(1.1234 AS FLOAT)", column.DefaultValueSql);
                Assert.Equal(1.1234, column.DefaultValue);
            },
            "DROP TABLE MyTable;");

    [ConditionalFact]
    public void Simple_decimal_literals_are_parsed_for_HasDefaultValue()
        => Test(
            """
            CREATE TABLE MyTable (
                ID INT,
                A DECIMAL DEFAULT -1.1111,
                B DECIMAL DEFAULT (0.0),
                C DECIMAL DEFAULT (0),
                D DECIMAL DEFAULT ((CAST(((1.1234)) AS DECIMAL)))
            );
            """,
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var columns = dbModel.Tables.Single().Columns;

                var column = columns.Single(c => c.Name == "A");
                Assert.Equal("-1.1111", column.DefaultValueSql);
                Assert.Equal((decimal)-1.1111, column.DefaultValue);

                column = columns.Single(c => c.Name == "B");
                Assert.Equal("0", column.DefaultValueSql);
                Assert.Equal((decimal)0, column.DefaultValue);

                column = columns.Single(c => c.Name == "C");
                Assert.Equal("0", column.DefaultValueSql);
                Assert.Equal((decimal)0, column.DefaultValue);

                column = columns.Single(c => c.Name == "D");
                Assert.Equal("CAST(1.1234 AS NUMBER(38,0))", column.DefaultValueSql);
                Assert.Equal((decimal)1.1234, column.DefaultValue);
            },
            "DROP TABLE MyTable;");

    [ConditionalFact]
    public void Simple_bool_literals_are_parsed_for_HasDefaultValue()
        => Test(
            """
            CREATE TABLE MyTable (
                ID INT,
                A BOOLEAN DEFAULT 0::BOOLEAN,
                B BOOLEAN DEFAULT 1::BOOLEAN,
                C BOOLEAN DEFAULT (0)::BOOLEAN,
                D BOOLEAN DEFAULT (1)::BOOLEAN,
                E BOOLEAN DEFAULT ('FaLse')::BOOLEAN,
                F BOOLEAN DEFAULT ('tRuE')::BOOLEAN,
                G BOOLEAN DEFAULT ((CAST((('tRUE')) AS BOOLEAN))),
                H BOOLEAN DEFAULT ((CAST(1 <> 1 AS BOOLEAN))),
                I BOOLEAN DEFAULT ((CAST(A <> 0  AS BOOLEAN)))
            );
            """,
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var columns = dbModel.Tables.Single().Columns;

                var column = columns.Single(c => c.Name == "A");
                Assert.Equal("CAST(0 <> 0 AS BOOLEAN)", column.DefaultValueSql);
                Assert.Equal(false, column.DefaultValue);

                column = columns.Single(c => c.Name == "B");
                Assert.Equal("CAST(1 <> 0 AS BOOLEAN)", column.DefaultValueSql);
                Assert.Equal(true, column.DefaultValue);
                
                column = columns.Single(c => c.Name == "C");
                Assert.Equal("CAST(0 <> 0 AS BOOLEAN)", column.DefaultValueSql);
                Assert.Equal(false, column.DefaultValue);
                
                column = columns.Single(c => c.Name == "D");
                Assert.Equal("CAST(1 <> 0 AS BOOLEAN)", column.DefaultValueSql);
                Assert.Equal(true, column.DefaultValue);
                
                column = columns.Single(c => c.Name == "E");
                Assert.Equal("CAST('FaLse' AS BOOLEAN)", column.DefaultValueSql);
                Assert.Equal(false, column.DefaultValue);
                
                column = columns.Single(c => c.Name == "F");
                Assert.Equal("CAST('tRuE' AS BOOLEAN)", column.DefaultValueSql);
                Assert.Equal(true, column.DefaultValue);
                
                column = columns.Single(c => c.Name == "G");
                Assert.Equal("CAST('tRUE' AS BOOLEAN)", column.DefaultValueSql);
                Assert.Equal(true, column.DefaultValue);

                column = columns.Single(c => c.Name == "H");
                Assert.Equal("CAST(1 <> 1 AS BOOLEAN)", column.DefaultValueSql);
                Assert.Null(column.DefaultValue);

                column = columns.Single(c => c.Name == "I");
                Assert.Equal("CAST(A <> (CAST(0 AS BOOLEAN)) AS BOOLEAN)", column.DefaultValueSql);
                Assert.Null(column.DefaultValue);
            },
            "DROP TABLE MyTable;");

    [ConditionalFact]
    public void Simple_DateTime_literals_are_parsed_for_HasDefaultValue()
        => Test(
            """
            CREATE TABLE MyTable (
                ID INT,
                A DATETIME DEFAULT '1973-09-03T12:00:01.0020000'::DATETIME,
                B DATETIME DEFAULT ('1968-10-23')::DATETIME,
                C DATETIME DEFAULT CAST('1973-09-03T01:02:03' AS DATETIME),
                D DATETIME DEFAULT (CAST('12:12:12' AS DATETIME))
            );
            """,
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var columns = dbModel.Tables.Single().Columns;

                var column = columns.Single(c => c.Name == "A");
                Assert.Equal("CAST('1973-09-03T12:00:01.0020000' AS TIMESTAMP_NTZ(9))", column.DefaultValueSql);
                Assert.Equal(new DateTime(1973, 9, 3, 12, 0, 1, 2, DateTimeKind.Unspecified), column.DefaultValue);

                column = columns.Single(c => c.Name == "B");
                Assert.Equal("CAST('1968-10-23' AS TIMESTAMP_NTZ(9))", column.DefaultValueSql);
                Assert.Equal(new DateTime(1968, 10, 23, 0, 0, 0, 0, DateTimeKind.Unspecified), column.DefaultValue);

                column = columns.Single(c => c.Name == "C");
                Assert.Equal("CAST('1973-09-03T01:02:03' AS TIMESTAMP_NTZ(9))", column.DefaultValueSql);
                Assert.Equal(new DateTime(1973, 9, 3, 1, 2, 3, 0, DateTimeKind.Unspecified), column.DefaultValue);

                column = columns.Single(c => c.Name == "D");
                Assert.Equal("CAST('12:12:12' AS TIMESTAMP_NTZ(9))", column.DefaultValueSql);
                Assert.Equal(12, ((DateTime)column.DefaultValue!).Hour);
                Assert.Equal(12, ((DateTime)column.DefaultValue!).Minute);
                Assert.Equal(12, ((DateTime)column.DefaultValue!).Second);
            },
            "DROP TABLE MyTable;");

    [ConditionalFact]
    public void Non_literal_or_non_parsable_DateTime_default_values_are_passed_through()
        => Test(
            """
            CREATE TABLE MyTable (
                ID INT,
                A DATETIME DEFAULT (CAST(GETDATE() AS DATETIME)),
                B DATETIME DEFAULT GETDATE(),
                C DATETIME DEFAULT ((CAST('12-01-16 12:32' AS DATETIME)))
            );
            """,
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var columns = dbModel.Tables.Single().Columns;

                var column = columns.Single(c => c.Name == "A");
                Assert.Equal("CAST(CURRENT_TIMESTAMP() AS TIMESTAMP_NTZ(9))", column.DefaultValueSql);
                Assert.Null(column.FindAnnotation(RelationalAnnotationNames.DefaultValue));

                column = columns.Single(c => c.Name == "B");
                Assert.Equal("CURRENT_TIMESTAMP()", column.DefaultValueSql);
                Assert.Null(column.FindAnnotation(RelationalAnnotationNames.DefaultValue));

                column = columns.Single(c => c.Name == "C");
                Assert.Equal("CAST('12-01-16 12:32' AS TIMESTAMP_NTZ(9))", column.DefaultValueSql);
                Assert.Null(column.FindAnnotation(RelationalAnnotationNames.DefaultValue));
            },
            "DROP TABLE MyTable;");

    [ConditionalFact]
    public void Simple_DateOnly_literals_are_parsed_for_HasDefaultValue()
        => Test(
            """
            CREATE TABLE MyTable (
                ID INT,
                A DATE DEFAULT ('1968-10-23')::DATE,
                B DATE DEFAULT (CAST('1973-09-03T01:02:03' AS DATE))
            );
            """,
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var columns = dbModel.Tables.Single().Columns;

                var column = columns.Single(c => c.Name == "A");
                Assert.Equal("CAST('1968-10-23' AS DATE)", column.DefaultValueSql);
                Assert.Equal(new DateTime(1968, 10, 23), column.DefaultValue);

                column = columns.Single(c => c.Name == "B");
                Assert.Equal("CAST('1973-09-03T01:02:03' AS DATE)", column.DefaultValueSql);
                Assert.Equal(new DateTime(1973, 9, 3, 1, 2,3), column.DefaultValue);
            },
            "DROP TABLE MyTable;");

    [ConditionalFact]
    public void Simple_TimeOnly_literals_are_parsed_for_HasDefaultValue()
        => Test(
            """
            CREATE TABLE MyTable (
                ID INT,
                A TIME DEFAULT ('12:00:01.0020000')::TIME,
                B TIME DEFAULT (CAST('01:02:03' AS TIME))
            );
            """,
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var columns = dbModel.Tables.Single().Columns;

                var column = columns.Single(c => c.Name == "A");
                Assert.Equal("CAST('12:00:01.0020000' AS TIME(9))", column.DefaultValueSql);
                Assert.Equal(new TimeOnly(12, 0, 1, 2), TimeOnly.FromDateTime((DateTime)column.DefaultValue!));

                column = columns.Single(c => c.Name == "B");
                Assert.Equal("CAST('01:02:03' AS TIME(9))", column.DefaultValueSql);
                Assert.Equal(new TimeOnly(1, 2, 3), TimeOnly.FromDateTime((DateTime)column.DefaultValue!));
            },
            "DROP TABLE MyTable;");

    [ConditionalFact]
    public void Simple_DateTimeOffset_literals_are_parsed_for_HasDefaultValue()
        => Test(
            """
            CREATE TABLE MyTable (
                ID INT,
                A TIMESTAMP_TZ DEFAULT ('1973-09-03T12:00:01.0000000+10:00')::TIMESTAMP_TZ,
                B TIMESTAMP_TZ DEFAULT (CAST('1973-09-03T01:02:03' AS TIMESTAMP_TZ))
            );
            """,
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var columns = dbModel.Tables.Single().Columns;

                var column = columns.Single(c => c.Name == "A");
                Assert.Equal("CAST('1973-09-03T12:00:01.0000000+10:00' AS TIMESTAMP_TZ(9))", column.DefaultValueSql);
                Assert.Equal(
                    new DateTimeOffset(new DateTime(1973, 9, 3, 12, 0, 1, 0, DateTimeKind.Unspecified), new TimeSpan(0, 10, 0, 0, 0)),
                    column.DefaultValue);

                column = columns.Single(c => c.Name == "B");
                Assert.Equal("CAST('1973-09-03T01:02:03' AS TIMESTAMP_TZ(9))", column.DefaultValueSql);
                Assert.Equal(
                    new DateTime(1973, 9, 3, 1, 2, 3, 0, DateTimeKind.Unspecified),
                    ((DateTimeOffset)column.DefaultValue!).DateTime);
            },
            "DROP TABLE MyTable;");

    [ConditionalFact]
    public void Simple_Guid_literals_are_parsed_for_HasDefaultValue()
        => Test(
            """
            CREATE TABLE MyTable (
                ID INT,
                A VARCHAR DEFAULT ('0E984725-C51C-4BF4-9960-E1C80E27ABA0'),
                B VARCHAR DEFAULT (CAST('0E984725-C51C-4BF4-9960-E1C80E27ABA0' AS VARCHAR))
            );
            """,
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var columns = dbModel.Tables.Single().Columns;

                var column = columns.Single(c => c.Name == "A");
                Assert.Equal("'0E984725-C51C-4BF4-9960-E1C80E27ABA0'", column.DefaultValueSql);
                Assert.Equal("0E984725-C51C-4BF4-9960-E1C80E27ABA0", column.DefaultValue);

                column = columns.Single(c => c.Name == "B");
                Assert.Equal("'0E984725-C51C-4BF4-9960-E1C80E27ABA0'", column.DefaultValueSql);
                Assert.Equal("0E984725-C51C-4BF4-9960-E1C80E27ABA0", column.DefaultValue);
            },
            "DROP TABLE MyTable;");

    
    [ConditionalFact]
    public void Simple_string_literals_are_parsed_for_HasDefaultValue()
        => Test(
            """
            CREATE TABLE MyTable (
                ID INT,
                A NVARCHAR DEFAULT 'Hot',
                B VARCHAR DEFAULT ('Buttered'),
                C CHARACTER(100) DEFAULT (''),
                D TEXT DEFAULT (''),
                E NVARCHAR(100) DEFAULT (' Toast! '),
                F NVARCHAR(20) DEFAULT (CAST(('Scones') AS VARCHAR)),
                G VARCHAR DEFAULT (CAST(('Toasted teacakes') AS CHAR VARYING))
            );
            """,
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var columns = dbModel.Tables.Single().Columns;

                var column = columns.Single(c => c.Name == "A");
                Assert.Equal("'Hot'", column.DefaultValueSql);
                Assert.Equal("Hot", column.DefaultValue);

                column = columns.Single(c => c.Name == "B");
                Assert.Equal("'Buttered'", column.DefaultValueSql);
                Assert.Equal("Buttered", column.DefaultValue);

                column = columns.Single(c => c.Name == "C");
                Assert.Equal("''", column.DefaultValueSql);
                Assert.Equal("", column.DefaultValue);

                column = columns.Single(c => c.Name == "D");
                Assert.Equal("''", column.DefaultValueSql);
                Assert.Equal("", column.DefaultValue);

                column = columns.Single(c => c.Name == "E");
                Assert.Equal("' Toast! '", column.DefaultValueSql);
                Assert.Equal(" Toast! ", column.DefaultValue);

                column = columns.Single(c => c.Name == "F");
                Assert.Equal("'Scones'", column.DefaultValueSql);
                Assert.Equal("Scones", column.DefaultValue);

                column = columns.Single(c => c.Name == "G");
                Assert.Equal("'Toasted teacakes'", column.DefaultValueSql);
                Assert.Equal("Toasted teacakes", column.DefaultValue);
            },
            "DROP TABLE MyTable;");

    [ConditionalFact]
    public void ValueGenerated_is_set_for_identity_and_computed_column()
        => Test(
            """
            CREATE TABLE ValueGeneratedProperties (
                ID INT IDENTITY(1, 1),
                NO_VALUE_GENERATION_COLUMN NVARCHAR,
                FIXED_DEFAULT_VALUE DATETIME NOT NULL DEFAULT ('October 20, 2015 11am')::DATETIME,
                COMPUTED_VALUE DATETIME DEFAULT GETDATE()
            );
            """,
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var columns = dbModel.Tables.Single().Columns;

                Assert.Equal(ValueGenerated.OnAdd, columns.Single(c => c.Name == "ID").ValueGenerated);
                Assert.Null(columns.Single(c => string.Equals(c.Name, "NO_VALUE_GENERATION_COLUMN", StringComparison.InvariantCultureIgnoreCase)).ValueGenerated);
                Assert.Null(columns.Single(c => c.Name == "FIXED_DEFAULT_VALUE").ValueGenerated);
                Assert.Null(columns.Single(c => c.Name == "COMPUTED_VALUE").ValueGenerated);
            },
            "DROP TABLE ValueGeneratedProperties;");

    [ConditionalFact]
    public void Column_nullability_is_set()
        => Test(
            """
            CREATE TABLE NullableColumns (
                ID INT,
                NULLABLE_INT INT NULL,
                NON_NULL_STRING NVARCHAR NOT NULL
            );
            """,
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var columns = dbModel.Tables.Single().Columns;

                Assert.True(columns.Single(c => string.Equals(c.Name, "NULLABLE_INT", StringComparison.InvariantCultureIgnoreCase)).IsNullable);
                Assert.False(columns.Single(c => string.Equals(c.Name, "NON_NULL_STRING", StringComparison.InvariantCultureIgnoreCase)).IsNullable);
            },
            "DROP TABLE NullableColumns;");

    #endregion

    #region PrimaryKeyFacets

    [ConditionalFact]
    public void Create_composite_primary_key()
        => Test(
            """
            CREATE TABLE CompositePrimaryKeyTable (
                ID1 INT,
                ID2 INT,
                PRIMARY KEY (ID2, ID1)
            );
            """,
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var pk = dbModel.Tables.Single().PrimaryKey;

                Assert.Equal("PUBLIC", pk!.Table!.Schema);
                Assert.Equal("COMPOSITEPRIMARYKEYTABLE", pk.Table.Name);
                Assert.StartsWith("SYS_CONSTRAINT_", pk.Name);
                Assert.Equal(
                    ["ID1", "ID2"], pk.Columns.Select(ic => ic.Name).OrderBy(ic=> ic).ToList());
            },
            "DROP TABLE CompositePrimaryKeyTable;");

    [ConditionalFact]
    [Trait("Category", "UseHybridTables")]
    public void Create_composite_primary_key_hybrid()
        => Test(
            """
            CREATE HYBRID TABLE CompositePrimaryKeyTable (
                ID1 INT,
                ID2 INT,
                PRIMARY KEY (ID2, ID1)
            );
            """,
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var pk = dbModel.Tables.Single().PrimaryKey;

                Assert.Equal("PUBLIC", pk!.Table!.Schema);
                Assert.Equal("COMPOSITEPRIMARYKEYTABLE", pk.Table.Name);
                Assert.StartsWith("SYS_INDEX_COMPOSITEPRIMARYKEYTABLE_PRIMARY", pk.Name);
                Assert.Equal(
                    ["ID1", "ID2"], pk.Columns.Select(ic => ic.Name).OrderBy(ic=> ic).ToList());
            },
            "DROP TABLE CompositePrimaryKeyTable;");

    [ConditionalTheory]
    [MemberData(nameof(TableTypes))]
    [Trait("Category", "UseHybridTables")]
    public void Set_primary_key_name_from_index(string tableType)
        => Test(
            $"""
            CREATE {tableType}TABLE PrimaryKeyName (
                ID1 INT,
                ID2 INT,
                CONSTRAINT MYPK PRIMARY KEY (ID2)
            );
            """,
            [],
            [],
            (dbModel, scaffoldingFactory) =>
            {
                var pk = dbModel.Tables.Single().PrimaryKey;

                Assert.Equal("PUBLIC", pk!.Table!.Schema);
                Assert.Equal("PRIMARYKEYNAME", pk.Table.Name);
                Assert.Contains(tableType == string.Empty ? "MYPK" : "SYS_INDEX_PRIMARYKEYNAME_PRIMARY", pk.Name);
                Assert.Equal(
                    ["ID2"], pk.Columns.Select(ic => ic.Name).ToList());
            },
            "DROP TABLE PrimaryKeyName;");
    
    [ConditionalFact]
    [Trait("Category", "UseHybridTables")]
    public void Create_primary_with_autoincrement()
        => Test(
            """
            CREATE OR REPLACE HYBRID TABLE SAMPLE_TABLE (
            	ID NUMBER CONSTRAINT PK_SAMPLE_TABLE PRIMARY KEY NOT NULL AUTOINCREMENT START 1 INCREMENT 1 NOORDER,
            	OTHER_COLUMN VARCHAR NOT NULL
            );
            """,
            [],
            [],
            (dbModel, _) =>
            {
                var pk = dbModel.Tables.Single().PrimaryKey;

                Assert.Equal("PUBLIC", pk!.Table!.Schema);
                Assert.Equal("SAMPLE_TABLE", pk.Table.Name);
                Assert.StartsWith("SYS_INDEX_SAMPLE_TABLE_PRIMARY", pk.Name);
                Assert.Equal(
                    ["ID"], pk.Columns.Select(ic => ic.Name));
            },
            "DROP TABLE SAMPLE_TABLE;");
    

    [ConditionalFact]
    [Trait("Category", "UseHybridTables")]
    public void Create_primary_with_autoincrement_with_dependant()
        => Test(
            """
            CREATE OR REPLACE HYBRID TABLE SECONDARY_TABLE (
                ID NUMBER NOT NULL AUTOINCREMENT START 1 INCREMENT 1 NOORDER,
                CONSTRAINT PK_SECONDARY_TABLE PRIMARY KEY (ID)
            );
            
            CREATE OR REPLACE HYBRID TABLE PRIMARY_TABLE (
                ID NUMBER NOT NULL,
                SECONDARY_TABLE_ID NUMBER NOT NULL,
                CONSTRAINT PK_PRIMARY_TABLE PRIMARY KEY (ID),
                CONSTRAINT FK_PRIMARY_TABLE_SECONDARY_TABLE_ID FOREIGN KEY (ID) REFERENCES SECONDARY_TABLE(ID)
            );
            """,
            [],
            [],
            (dbModel, _) =>
            {
                var primaryTablePk = dbModel.Tables.Single(t => t.Name == "PRIMARY_TABLE").PrimaryKey;
                var secondaryTablePk = dbModel.Tables.Single(t => t.Name == "SECONDARY_TABLE").PrimaryKey;

                Assert.Equal("SYS_INDEX_PRIMARY_TABLE_PRIMARY", primaryTablePk!.Name);
                Assert.Equal("SYS_INDEX_SECONDARY_TABLE_PRIMARY", secondaryTablePk!.Name);
                Assert.Equal(["ID"], primaryTablePk.Columns.Select(ic => ic.Name));
                Assert.Equal(["ID"], secondaryTablePk.Columns.Select(ic => ic.Name));
            },
            "DROP TABLE PRIMARY_TABLE; DROP TABLE SECONDARY_TABLE;", 
            2, 
            2);

    #endregion

    #region UniqueConstraintFacets

    [ConditionalFact]
    [Trait("Category", "UseHybridTables")]
    public void Create_composite_unique_constraint()
        => Test(
            """
            CREATE HYBRID TABLE CompositeUniqueConstraintTable (
                ID1 INT PRIMARY KEY,
                ID2 INT,
                CONSTRAINT UX UNIQUE (ID2, ID1)
            );
            """,
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var uniqueConstraint = Assert.Single(dbModel.Tables.Single().UniqueConstraints);

                // ReSharper disable once PossibleNullReferenceException
                Assert.Equal("PUBLIC", uniqueConstraint.Table.Schema);
                Assert.Equal("COMPOSITEUNIQUECONSTRAINTTABLE", uniqueConstraint.Table.Name);
                Assert.Equal("UX", uniqueConstraint.Name);
                Assert.Equal(
                    ["ID1", "ID2"], uniqueConstraint.Columns.Select(ic => ic.Name).OrderBy(ic=> ic).ToList());
            },
            "DROP TABLE CompositeUniqueConstraintTable;");

    [ConditionalFact]
    [Trait("Category", "UseHybridTables")]
    public void Set_unique_constraint_name_from_index()
        => Test(
            """
            CREATE HYBRID TABLE UniqueConstraintName (
                ID1 INT PRIMARY KEY,
                ID2 INT,
                CONSTRAINT MYUC UNIQUE (ID2)
            );
            """,
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var uniqueConstraint = Assert.Single(dbModel.Tables.Single().UniqueConstraints);

                // ReSharper disable once PossibleNullReferenceException
                Assert.Equal("PUBLIC", uniqueConstraint.Table.Schema);
                Assert.Equal("UNIQUECONSTRAINTNAME", uniqueConstraint.Table.Name);
                Assert.Equal("MYUC", uniqueConstraint.Name);
                Assert.Equal(
                    ["ID2"], uniqueConstraint.Columns.Select(ic => ic.Name).ToList());
            },
            "DROP TABLE UniqueConstraintName;");

    #endregion

    #region IndexFacets

    [ConditionalFact]
    [Trait("Category", "UseHybridTables")]
    public void Create_composite_index()
        => Test(
            """
            CREATE HYBRID TABLE CompositeIndexTable (ID1 INT PRIMARY KEY, ID2 INT);
            CREATE INDEX IX_COMPOSITE ON CompositeIndexTable (ID2, ID1);
            """,
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var index = Assert.Single(dbModel.Tables.Single().Indexes);

                // ReSharper disable once PossibleNullReferenceException
                Assert.Equal("PUBLIC", index.Table!.Schema);
                Assert.Equal("COMPOSITEINDEXTABLE", index.Table.Name);
                Assert.Equal("IX_COMPOSITE", index.Name);
                Assert.Equal(
                    ["ID1", "ID2"], index.Columns.Select(ic => ic.Name).OrderBy(ic=> ic).ToList());
            },
            """
            DROP INDEX CompositeIndexTable.IX_COMPOSITE;
            DROP TABLE CompositeIndexTable;
            """,
            2,
            2);

    #endregion

    #region ForeignKeyFacets

    [ConditionalTheory]
    [MemberData(nameof(HybridTableTypesOnly))]
    // TODO: Check support for standard tables when available, ON DELETE {ACTION} not supported.
    [Trait("Category", "UseHybridTables")]
    public void Create_composite_foreign_key(string tableType)
        => Test(
            $"""
            CREATE {tableType}TABLE Principal_Table (
                ID1 INT,
                ID2 INT,
                PRIMARY KEY (ID1, ID2)
            );

            CREATE {tableType}TABLE Dependent_Table (
                ID INT PRIMARY KEY,
                FOREIGN_KEY_ID1 INT,
                FOREIGN_KEY_ID2 INT,
                FOREIGN KEY (FOREIGN_KEY_ID1, FOREIGN_KEY_ID2) REFERENCES Principal_Table(ID1, ID2) ON DELETE RESTRICT
            );
            """,
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var fk = Assert.Single(dbModel.Tables.Single(t => t.Name == "DEPENDENT_TABLE").ForeignKeys);

                // ReSharper disable once PossibleNullReferenceException
                Assert.Equal("PUBLIC", fk.Table.Schema);
                Assert.Equal("DEPENDENT_TABLE", fk.Table.Name);
                Assert.Equal("PUBLIC", fk.PrincipalTable.Schema);
                Assert.Equal("PRINCIPAL_TABLE", fk.PrincipalTable.Name);
                Assert.Equal(
                    ["FOREIGN_KEY_ID1", "FOREIGN_KEY_ID2"], fk.Columns.Select(ic => ic.Name).ToList());
                Assert.Equal(
                    ["ID1", "ID2"], fk.PrincipalColumns.Select(ic => ic.Name).ToList());
                Assert.Equal(ReferentialAction.Restrict, fk.OnDelete);
            },
            """
            DROP TABLE Dependent_Table;
            DROP TABLE Principal_Table;
            """,
            2,
            2);

    [ConditionalTheory]
    // TODO: Check support for standard tables when available, ON DELETE RESTRICT not supported.
    [MemberData(nameof(HybridTableTypesOnly))]
    [Trait("Category", "UseHybridTables")]
    public void Create_multiple_foreign_key_in_same_table(string tableType)
        => Test(
            $"""
            CREATE {tableType}TABLE PRINCIPAL_TABLE (ID INT PRIMARY KEY);

            CREATE {tableType}TABLE ANOTHER_PRINCIPAL_TABLE (ID INT PRIMARY KEY);

            CREATE {tableType}TABLE DEPENDENT_TABLE (
                ID INT PRIMARY KEY,
                FOREIGN_KEY_ID1 INT,
                FOREIGN_KEY_ID2 INT,
                FOREIGN KEY (FOREIGN_KEY_ID1) REFERENCES PRINCIPAL_TABLE(ID) ON DELETE RESTRICT,
                FOREIGN KEY (FOREIGN_KEY_ID2) REFERENCES ANOTHER_PRINCIPAL_TABLE(ID) ON DELETE RESTRICT
            );
            """,
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var foreignKeys = dbModel.Tables.Single(t => t.Name == "DEPENDENT_TABLE").ForeignKeys;

                Assert.Equal(2, foreignKeys.Count);

                var principalFk = Assert.Single(foreignKeys.Where(f => f.PrincipalTable.Name == "PRINCIPAL_TABLE"));

                // ReSharper disable once PossibleNullReferenceException
                Assert.Equal("PUBLIC", principalFk.Table.Schema);
                Assert.Equal("DEPENDENT_TABLE", principalFk.Table.Name);
                Assert.Equal("PUBLIC", principalFk.PrincipalTable.Schema);
                Assert.Equal("PRINCIPAL_TABLE", principalFk.PrincipalTable.Name);
                Assert.Equal(
                    ["FOREIGN_KEY_ID1"], principalFk.Columns.Select(ic => ic.Name).ToList());
                Assert.Equal(
                    ["ID"], principalFk.PrincipalColumns.Select(ic => ic.Name).ToList());
                Assert.Equal(ReferentialAction.Restrict, principalFk.OnDelete);

                var anotherPrincipalFk = Assert.Single(foreignKeys.Where(f => f.PrincipalTable.Name == "ANOTHER_PRINCIPAL_TABLE"));

                // ReSharper disable once PossibleNullReferenceException
                Assert.Equal("PUBLIC", anotherPrincipalFk.Table.Schema);
                Assert.Equal("DEPENDENT_TABLE", anotherPrincipalFk.Table.Name);
                Assert.Equal("PUBLIC", anotherPrincipalFk.PrincipalTable.Schema);
                Assert.Equal("ANOTHER_PRINCIPAL_TABLE", anotherPrincipalFk.PrincipalTable.Name);
                Assert.Equal(
                    ["FOREIGN_KEY_ID2"], anotherPrincipalFk.Columns.Select(ic => ic.Name).ToList());
                Assert.Equal(
                    ["ID"], anotherPrincipalFk.PrincipalColumns.Select(ic => ic.Name).ToList());
                Assert.Equal(ReferentialAction.Restrict, anotherPrincipalFk.OnDelete);
            },
            """
            DROP TABLE DEPENDENT_TABLE;
            DROP TABLE ANOTHER_PRINCIPAL_TABLE;
            DROP TABLE PRINCIPAL_TABLE;
            """,
            3,
            3);

    [ConditionalFact]
    [Trait("Category", "UseHybridTables")]
    public void Create_foreign_key_referencing_unique_constraint()
        => Test(
            """
            CREATE HYBRID TABLE PRINCIPAL_TABLE (ID1 INT PRIMARY KEY, ID2 INT UNIQUE);

            CREATE HYBRID TABLE DEPENDENT_TABLE (
                ID INT PRIMARY KEY,
                FOREIGN_KEY_ID INT,
                FOREIGN KEY (FOREIGN_KEY_ID) REFERENCES PRINCIPAL_TABLE(ID2) ON DELETE RESTRICT
            );
            """,
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var fk = Assert.Single(dbModel.Tables.Single(t => t.Name == "DEPENDENT_TABLE").ForeignKeys);

                // ReSharper disable once PossibleNullReferenceException
                Assert.Equal("PUBLIC", fk.Table.Schema);
                Assert.Equal("DEPENDENT_TABLE", fk.Table.Name);
                Assert.Equal("PUBLIC", fk.PrincipalTable.Schema);
                Assert.Equal("PRINCIPAL_TABLE", fk.PrincipalTable.Name);
                Assert.Equal(
                    ["FOREIGN_KEY_ID"], fk.Columns.Select(ic => ic.Name).ToList());
                Assert.Equal(
                    ["ID2"], fk.PrincipalColumns.Select(ic => ic.Name).ToList());
                Assert.Equal(ReferentialAction.Restrict, fk.OnDelete);
            },
            """
            DROP TABLE DEPENDENT_TABLE;
            DROP TABLE PRINCIPAL_TABLE;
            """,
            2,
            2);

    [ConditionalTheory]
    // TODO: Check support for standard tables when available, ON DELETE RESTRICT not supported.
    [MemberData(nameof(HybridTableTypesOnly))]
    [Trait("Category", "UseHybridTables")]
    public void Set_name_for_foreign_key(string tableType)
        => Test(
            $"""
            CREATE {tableType}TABLE PRINCIPAL_TABLE (ID INT PRIMARY KEY);

            CREATE {tableType}TABLE DEPENDENT_TABLE (
                ID INT PRIMARY KEY,
                FOREIGN_KEY_ID INT,
                CONSTRAINT MYFK FOREIGN KEY (FOREIGN_KEY_ID) REFERENCES PRINCIPAL_TABLE(ID) ON DELETE RESTRICT
            );
            """,
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var fk = Assert.Single(dbModel.Tables.Single(t => t.Name == "DEPENDENT_TABLE").ForeignKeys);

                // ReSharper disable once PossibleNullReferenceException
                Assert.Equal("PUBLIC", fk.Table.Schema);
                Assert.Equal("DEPENDENT_TABLE", fk.Table.Name);
                Assert.Equal("PUBLIC", fk.PrincipalTable.Schema);
                Assert.Equal("PRINCIPAL_TABLE", fk.PrincipalTable.Name);
                Assert.Equal(
                    ["FOREIGN_KEY_ID"], fk.Columns.Select(ic => ic.Name).ToList());
                Assert.Equal(
                    ["ID"], fk.PrincipalColumns.Select(ic => ic.Name).ToList());
                Assert.Equal(ReferentialAction.Restrict, fk.OnDelete);
                Assert.Equal("MYFK", fk.Name);
            },
            """
            DROP TABLE DEPENDENT_TABLE;
            DROP TABLE PRINCIPAL_TABLE;
            """,
            2,
            2);

    [ConditionalFact]
    [Trait("Category", "UseHybridTables")]
    public void Set_referential_action_for_foreign_key()
        => Test(
            """
            CREATE HYBRID TABLE PRINCIPAL_TABLE (ID INT PRIMARY KEY);

            CREATE HYBRID TABLE DEPENDENT_TABLE (
                ID INT PRIMARY KEY,
                FOREIGN_KEY_ID INT,
                FOREIGN KEY (FOREIGN_KEY_ID) REFERENCES PRINCIPAL_TABLE(ID) ON DELETE NO ACTION
            );
            """,
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var fk = Assert.Single(dbModel.Tables.Single(t => t.Name == "DEPENDENT_TABLE").ForeignKeys);

                // ReSharper disable once PossibleNullReferenceException
                Assert.Equal("PUBLIC", fk.Table.Schema);
                Assert.Equal("DEPENDENT_TABLE", fk.Table.Name);
                Assert.Equal("PUBLIC", fk.PrincipalTable.Schema);
                Assert.Equal("PRINCIPAL_TABLE", fk.PrincipalTable.Name);
                Assert.Equal(
                    ["FOREIGN_KEY_ID"], fk.Columns.Select(ic => ic.Name).ToList());
                Assert.Equal(
                    ["ID"], fk.PrincipalColumns.Select(ic => ic.Name).ToList());
                Assert.Equal(ReferentialAction.NoAction, fk.OnDelete);
            },
            """
            DROP TABLE DEPENDENT_TABLE;
            DROP TABLE PRINCIPAL_TABLE;
            """,
            2,
            2);

    #endregion

    #region Warnings

    [ConditionalFact]
    public void Warn_missing_schema()
        => Test(
            "CREATE TABLE BLANK (ID INT);",
            Enumerable.Empty<string>(),
            new[] { "MySchema" },
            (dbModel, scaffoldingFactory) =>
            {
                Assert.Empty(dbModel.Tables);

                var message = Fixture.OperationReporter.Messages.Single(m => m.Level == LogLevel.Warning).Message;

                Assert.Equal(
                    SnowflakeResources.LogMissingSchema(new TestLogger<SnowflakeLoggingDefinitions>()).GenerateMessage("MySchema"),
                    message);
            },
            "DROP TABLE BLANK;");

    [ConditionalFact]
    public void Warn_standard_table_pk()
        => Test(
            "CREATE TABLE BLANK (ID INT PRIMARY KEY);",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                Assert.True(Fixture.OperationReporter.Messages.Any(m => m.Message.StartsWith(@"Found primary key on Standard table 'PUBLIC.BLANK'")));
            },
            "DROP TABLE BLANK;");

    [ConditionalFact]
    public void Warn_standard_table_fk()
        => Test(
            @"CREATE TABLE PRINCIPAL_TABLE (ID INT PRIMARY KEY);
CREATE TABLE DEPENDECY_TABLE (
    ID INT PRIMARY KEY, 
    FOREIGN_KEY_ID INT, 
    FOREIGN KEY (FOREIGN_KEY_ID) REFERENCES PRINCIPAL_TABLE(ID));",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                Assert.True(Fixture.OperationReporter.Messages.Any(m => m.Message.StartsWith(@"Found foreign key on Standard table")));
            },
            @"DROP TABLE PRINCIPAL_TABLE;
DROP TABLE DEPENDECY_TABLE;", 2, 2);

    [ConditionalFact]
    public void Warn_missing_table()
        => Test(
            "CREATE TABLE BLANK (ID INT);",
            ["MyTable"],
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                Assert.Empty(dbModel.Tables);

                var message = Fixture.OperationReporter.Messages.Single(m => m.Level == LogLevel.Warning).Message;

                Assert.Equal(
                    SnowflakeResources.LogMissingTable(new TestLogger<SnowflakeLoggingDefinitions>()).GenerateMessage("MyTable"),
                    message);
            },
            "DROP TABLE BLANK;");

    [ConditionalFact]
    [Trait("Category", "UseHybridTables")]
    public void Warn_missing_principal_table_for_foreign_key()
        => Test(
            """
            CREATE HYBRID TABLE PRINCIPAL_TABLE (
                ID INT PRIMARY KEY
            );

            CREATE HYBRID TABLE DEPENDENT_TABLE (
                ID INT PRIMARY KEY,
                FOREIGN_KEY_ID INT,
                CONSTRAINT MY_FK FOREIGN KEY (FOREIGN_KEY_ID) REFERENCES PRINCIPAL_TABLE(ID) ON DELETE RESTRICT
            );
            """,
            ["DEPENDENT_TABLE"],
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var message = Fixture.OperationReporter.Messages.Single(m => m.Level == LogLevel.Warning).Message;

                Assert.Equal(
                    SnowflakeResources.LogPrincipalTableNotInSelectionSet(new TestLogger<SnowflakeLoggingDefinitions>())
                        .GenerateMessage(
                            "MY_FK", "PUBLIC.DEPENDENT_TABLE", "PUBLIC.PRINCIPAL_TABLE"), message);
            },
            """
            DROP TABLE DEPENDENT_TABLE;
            DROP TABLE PRINCIPAL_TABLE;
            """,
            2,
            2);

    [ConditionalFact]
    public void Skip_reflexive_foreign_key()
        => Test(
            """
            CREATE TABLE PRINCIPAL_TABLE (
                ID INT PRIMARY KEY,
                CONSTRAINT MY_FK FOREIGN KEY (ID) REFERENCES PRINCIPAL_TABLE(ID)
            );
            """,
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var level = Fixture.OperationReporter.Messages
                    .Single(
                        m => m.Message
                            == SnowflakeResources.LogReflexiveConstraintIgnored(new TestLogger<SnowflakeLoggingDefinitions>())
                                .GenerateMessage("MY_FK", "PUBLIC.PRINCIPAL_TABLE")).Level;

                Assert.Equal(LogLevel.Debug, level);

                var table = Assert.Single(dbModel.Tables);
                Assert.Empty(table.ForeignKeys);
            },
            "DROP TABLE PRINCIPAL_TABLE;");

    [ConditionalFact]
    public void Skip_duplicate_foreign_key()
        => Test(
            """
            CREATE TABLE PRINCIPAL_TABLE (
                ID INT PRIMARY KEY,
                VALUE1 VARCHAR,
                VALUE2 VARCHAR,
                CONSTRAINT UNIQUE_VALUE1 UNIQUE (VALUE1),
                CONSTRAINT UNIQUE_VALUE2 UNIQUE (VALUE2)
            );

            CREATE TABLE OTHER_PRINCIPAL_TABLE (ID INT PRIMARY KEY);

            CREATE TABLE DEPENDENT_TABLE (
                ID INT PRIMARY KEY,
                FOREIGN_KEY_ID INT,
                VALUE_KEY VARCHAR,
                CONSTRAINT MY_FK1 FOREIGN KEY (FOREIGN_KEY_ID) REFERENCES PRINCIPAL_TABLE(ID),
                CONSTRAINT MY_FK2 FOREIGN KEY (FOREIGN_KEY_ID) REFERENCES PRINCIPAL_TABLE(ID),
                CONSTRAINT MY_FK3 FOREIGN KEY (FOREIGN_KEY_ID) REFERENCES OTHER_PRINCIPAL_TABLE(ID),
                CONSTRAINT MY_FK4 FOREIGN KEY (VALUE_KEY) REFERENCES PRINCIPAL_TABLE(VALUE1),
                CONSTRAINT MY_FK5 FOREIGN KEY (VALUE_KEY) REFERENCES PRINCIPAL_TABLE(VALUE2)
            );
            """,
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var level = Fixture.OperationReporter.Messages
                    .Single(
                        m => m.Message
                             == SnowflakeResources
                                 .LogDuplicateForeignKeyConstraintIgnored(new TestLogger<SnowflakeLoggingDefinitions>())
                                 .GenerateMessage("MY_FK2", "PUBLIC.DEPENDENT_TABLE", "MY_FK1")).Level;

                Assert.Equal(LogLevel.Warning, level);

                var table = dbModel.Tables.Single(t => t.Name == "DEPENDENT_TABLE");
                Assert.Equal(4, table.ForeignKeys.Count);
            },
            """
            DROP TABLE DEPENDENT_TABLE;
            DROP TABLE PRINCIPAL_TABLE;
            DROP TABLE OTHER_PRINCIPAL_TABLE;
            """,
            3,
            3);

    [ConditionalFact]
    public void No_warning_missing_view_definition()
        => Test(
            "CREATE TABLE TEST_VIEW_DEFINITION (ID INT PRIMARY KEY);",
            Enumerable.Empty<string>(),
            Enumerable.Empty<string>(),
            (dbModel, scaffoldingFactory) =>
            {
                var message = Fixture.OperationReporter.Messages
                    .SingleOrDefault(
                        m => m.Message
                             == SnowflakeResources
                                 .LogMissingViewDefinitionRights(new TestLogger<SnowflakeLoggingDefinitions>())
                                 .GenerateMessage()).Message;

                Assert.Null(message);
            },
            "DROP TABLE TEST_VIEW_DEFINITION;");

    #endregion

    private void Test(
        string? createSql,
        IEnumerable<string> tables,
        IEnumerable<string> schemas,
        Action<DatabaseModel, IScaffoldingModelFactory> asserter,
        string? cleanupSql,
        int createMultiStatementsCount = 1,
        int cleanMultiStatementCount = 1)
        => Test(
            string.IsNullOrEmpty(createSql) ? Array.Empty<string>() : new[]
            {
                createSql
            },
            tables,
            schemas,
            asserter,
            cleanupSql,
            createMultiStatementsCount,
            cleanMultiStatementCount);

    private void Test(
        string[] createSqls,
        IEnumerable<string> tables,
        IEnumerable<string> schemas,
        Action<DatabaseModel, IScaffoldingModelFactory> asserter,
        string? cleanupSql,
        int createMultiStatementsCount = 1,
        int cleanMultiStatementCount = 1)
    {
        foreach (var createSql in createSqls)
        {
            Fixture.TestStore.ExecuteNonQuery(createSql, createMultiStatementsCount);
        }

        try
        {
            var serviceProvider = SnowflakeTestHelpers.Instance.CreateDesignServiceProvider(reporter: Fixture.OperationReporter)
                .CreateScope().ServiceProvider;

            var databaseModelFactory = serviceProvider.GetRequiredService<IDatabaseModelFactory>();

            var databaseModel = databaseModelFactory.Create(
                Fixture.TestStore.ConnectionString,
                new DatabaseModelFactoryOptions(tables, schemas));

            Assert.NotNull(databaseModel);

            asserter(databaseModel, serviceProvider.GetRequiredService<IScaffoldingModelFactory>());
        }
        finally
        {
            if (!string.IsNullOrEmpty(cleanupSql))
            {
                Fixture.TestStore.ExecuteNonQuery(cleanupSql, cleanMultiStatementCount);
            }
        }
    }

    public class SnowflakeDatabaseModelFixture : SharedStoreFixtureBase<PoolableDbContext>
    {
        protected override string StoreName
            => nameof(SnowflakeDatabaseModelFactoryTest).ToUpper();

        protected override ITestStoreFactory TestStoreFactory
            => SnowflakeTestStoreFactory.Instance;

        public new SnowflakeTestStore TestStore
            => (SnowflakeTestStore)base.TestStore;

        public TestOperationReporter OperationReporter { get; } = new();

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            await TestStore.ExecuteNonQueryAsync("CREATE SCHEMA DB2");
            await TestStore.ExecuteNonQueryAsync(@"CREATE SCHEMA ""db.2""");
            SnowflakeDbConnectionPool.ClearAllPools();
        }

        protected override bool ShouldLogCategory(string logCategory)
            => logCategory == DbLoggerCategory.Scaffolding.Name;
    }
}
