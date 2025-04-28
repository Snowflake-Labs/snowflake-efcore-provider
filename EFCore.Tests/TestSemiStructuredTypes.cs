using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Reflection;
using log4net;
using Microsoft.EntityFrameworkCore;
using Snowflake.Data.Client;

namespace EFCore.Tests;

public class SemiStructuredTypes
{
    public int SemiStructuredTypesId { get; set; }

    public String? SS_UNTYPPED { get; set; }
    
    [Column(TypeName="VARIANT")]
    public String? SS_VARIANT { get; set; }

    [Column(TypeName="ARRAY")]
    public String? SS_ARRAY { get; set; }

    [Column(TypeName="OBJECT")]
    public String? SS_OBJECT { get; set; }
}

public class TestSemiStructuredTypes : EFCoreTestBase
{
    protected static readonly ILog s_log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);
    
    public class DbContext : DbContextBase
    {
        public DbSet<SemiStructuredTypes> SemiStructuredTypes { get; set; }
        
        public DbContext()
        {
        }
    }

    public TestSemiStructuredTypes()
    {
    }

    public override void InitializeDatabaseContext()
    {
        _dbContext = new DbContext();
    }

    public override void InitializeTableNamesToClean()
    {
        _tablesToClean.Add("SemiStructuredTypes");
    }

    [SetUp]
    public void SetUp()
    {
        DropTables();
    }

    [Test]
    public void TestTypes()
    {

        var db = new DbContext();
        db.Database.EnsureCreated();

        var expectedValues = new SemiStructuredTypes
        {
            SemiStructuredTypesId = 1,
            SS_UNTYPPED = "test",
            SS_VARIANT = "14",
            SS_ARRAY = "[\"some\",\"array\",\"values\"]",
            SS_OBJECT = "{\"key\":10}",
        };

        db.Add(expectedValues);
        // Variant parameter binding is not supported by Snowflake
        Assert.Throws<DbUpdateException>(() => db.SaveChanges());

        using (var conn = new SnowflakeDbConnection(_dbContext.ConnectionString))
        {
            var cmd = conn.CreateCommand();
            cmd.CommandText = $"insert into \"SemiStructuredTypes\" " +
                              $" (\"SemiStructuredTypesId\", \"SS_UNTYPPED\", \"SS_VARIANT\", \"SS_ARRAY\", \"SS_OBJECT\") " +
                              $"select {expectedValues.SemiStructuredTypesId}, " +
                              $" '{expectedValues.SS_UNTYPPED}', " +
                              $" {expectedValues.SS_VARIANT}::VARIANT, " +
                              $" STRTOK_TO_ARRAY('some,array,values', ',')," +
                              $" PARSE_JSON('{expectedValues.SS_OBJECT}')::OBJECT";
            conn.Open();
            cmd.ExecuteNonQuery();
        }
        
        // renew the context
        db = new DbContext();
        var loadedValues = db.SemiStructuredTypes.First();

        Assert.That(loadedValues.SS_UNTYPPED, Is.EqualTo(expectedValues.SS_UNTYPPED));
        Assert.That(loadedValues.SS_VARIANT, Is.EqualTo(expectedValues.SS_VARIANT));
        Assert.That(NormalizeOutput(loadedValues.SS_ARRAY), Is.EqualTo(expectedValues.SS_ARRAY));
        Assert.That(NormalizeOutput(loadedValues.SS_OBJECT), Is.EqualTo(expectedValues.SS_OBJECT));
    } 
    
    string? NormalizeOutput(string? value) =>
        value?
            .Replace("\n", "")
            .Replace(" ", "");
}