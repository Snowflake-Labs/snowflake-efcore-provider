using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Reflection;
using log4net;
using Microsoft.EntityFrameworkCore;
using Snowflake.Data.Client;
using Snowflake.EntityFrameworkCore;
using Snowflake.EntityFrameworkCore.Metadata.Internal;
using Xunit;

namespace EFCore.Tests;

public class IdentityIssues
{
    [Key]
    public int IdentityIssuesId { get; set; }

    public int DT_INT { get; set; }
}

[Index(nameof(DT_INT))]
public class SequenceIssues
{
    public int SequenceIssuesId { get; set; }

    public int DT_INT { get; set; }
    
}

public class TestPKIssues : EFCoreTestBase
{
    protected static readonly ILog s_log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);
    
    public class DbContext : DbContextBase
    {
        public DbSet<IdentityIssues> IdentityIssues { get; set; }
        public DbSet<SequenceIssues> SequenceIssues { get; set; }
        
        protected override void OnModelCreating(ModelBuilder builder)
        {
            // create sequence
            builder.HasSequence("SequenceIssuesSeq");
            // set the sequence.nextval as default value
            builder.Entity<SequenceIssues>().ToTable(t => t.IsHybridTable()).Property(p => p.SequenceIssuesId).HasDefaultValueSql("\"SequenceIssuesSeq\".nextval");
        }

        public DbContext()
        {
        }
    }

    public TestPKIssues()
    {
    }

    public override void InitializeDatabaseContext()
    {
        _dbContext = new DbContext();
    }

    public override void InitializeTableNamesToClean()
    {
        _tablesToClean.Add("IdentityIssues");
        _tablesToClean.Add("SequenceIssues");
        _tablesToClean.Add("HT_TEST");
    }

    [SetUp]
    public void SetUp()
    {
        DropTables();
    }

    [Test]
    [Trait("Category", "UseHybridTables")]
    public void TestIdentityAsPrimaryKey()
    {
        var db = new DbContext();
        db.Database.EnsureCreated();

        var expectedValues1 = new IdentityIssues { DT_INT = 10 };
        var expectedValues2 = new IdentityIssues { DT_INT = 20 };

        db.Add(expectedValues1);
        db.Add(expectedValues2);
        
        Assert.That(expectedValues1.IdentityIssuesId, Is.EqualTo(0));
        Assert.That(expectedValues2.IdentityIssuesId, Is.EqualTo(0));
        db.SaveChanges();
        Assert.That(expectedValues1.IdentityIssuesId, Is.GreaterThan(0));
        Assert.That(expectedValues2.IdentityIssuesId, Is.GreaterThan(0));

        // renew the context
        db = new DbContext();

        var loadedValues = db.IdentityIssues.OrderBy(a => a.DT_INT).ToArray();
        Assert.That(loadedValues[0].IdentityIssuesId, Is.EqualTo(expectedValues1.IdentityIssuesId));
        Assert.That(loadedValues[1].IdentityIssuesId, Is.EqualTo(expectedValues2.IdentityIssuesId));
    }
    
    [Test]
    [Trait("Category", "UseHybridTables")]
    public void TestPrimaryKeyViolationThrowsExceptionForHtapTable()
    {
        var db = new DbContext();
        db.Database.EnsureCreated();
        var expectedValues1 = new SequenceIssues { SequenceIssuesId = 1, DT_INT = 10 };
        db.Add(expectedValues1);
        db.SaveChanges();
        
        // renew the context
        db = new DbContext();
        var expectedValues2 = new SequenceIssues { SequenceIssuesId = 1, DT_INT = 10 };
        db.Add(expectedValues2);

        var updateException = Assert.Throws<DbUpdateException>(()=>db.SaveChanges());
        var snowflakeDbException = updateException.InnerException;
        Assert.That(snowflakeDbException.Message.Contains("A primary key already exists. SqlState: 22000, VendorCode: 200001"));
    }
    
    [Test]
    [Trait("Category", "UseHybridTables")]
    public void TestIdentityWithPredefinedValues()
    {
        var db = new DbContext();
        db.Database.EnsureCreated();

        var expectedValues1 = new IdentityIssues { IdentityIssuesId = 1, DT_INT = 10 };
        var expectedValues2 = new IdentityIssues { IdentityIssuesId = 2, DT_INT = 20 };

        db.Add(expectedValues1);
        db.Add(expectedValues2);
        
        Assert.That(expectedValues1.IdentityIssuesId, Is.EqualTo(1));
        Assert.That(expectedValues2.IdentityIssuesId, Is.EqualTo(2));
        db.SaveChanges();
        Assert.That(expectedValues1.IdentityIssuesId, Is.EqualTo(1));
        Assert.That(expectedValues2.IdentityIssuesId, Is.EqualTo(2));

        // renew the context
        db = new DbContext();

        var loadedValues = db.IdentityIssues.OrderBy(a => a.DT_INT).ToArray();
        Assert.That(loadedValues[0].IdentityIssuesId, Is.EqualTo(expectedValues1.IdentityIssuesId));
        Assert.That(loadedValues[1].IdentityIssuesId, Is.EqualTo(expectedValues2.IdentityIssuesId));
    }
    
    [Test]
    [Trait("Category", "UseHybridTables")]
    public void TestSequenceAsPrimaryKey()
    {
        var db = new DbContext();
        db.Database.EnsureCreated();

        var expectedValues1 = new SequenceIssues { DT_INT = 10 };
        var expectedValues2 = new SequenceIssues { DT_INT = 20 };

        db.Add(expectedValues1);
        db.Add(expectedValues2);
        
        Assert.That(expectedValues1.SequenceIssuesId, Is.EqualTo(0));
        Assert.That(expectedValues2.SequenceIssuesId, Is.EqualTo(0));
        db.SaveChanges();
        Assert.That(expectedValues1.SequenceIssuesId, Is.GreaterThan(0));
        Assert.That(expectedValues2.SequenceIssuesId, Is.GreaterThan(0));

        // renew the context
        db = new DbContext();

        var loadedValues = db.SequenceIssues.OrderBy(a => a.DT_INT).ToArray();
        Assert.That(loadedValues[0].SequenceIssuesId, Is.EqualTo(expectedValues1.SequenceIssuesId));
        Assert.That(loadedValues[1].SequenceIssuesId, Is.EqualTo(expectedValues2.SequenceIssuesId));
    }
    
    [Test]
    [Trait("Category", "UseHybridTables")]
    public void TestSequenceWithPredefinedValues()
    {
        var db = new DbContext();
        db.Database.EnsureCreated();

        var expectedValues1 = new SequenceIssues { SequenceIssuesId = 20, DT_INT = 10 };
        var expectedValues2 = new SequenceIssues { SequenceIssuesId = 30, DT_INT = 20 };

        db.Add(expectedValues1);
        db.Add(expectedValues2);
        
        Assert.That(expectedValues1.SequenceIssuesId, Is.EqualTo(20));
        Assert.That(expectedValues2.SequenceIssuesId, Is.EqualTo(30));
        db.SaveChanges();
        Assert.That(expectedValues1.SequenceIssuesId, Is.EqualTo(20));
        Assert.That(expectedValues2.SequenceIssuesId, Is.EqualTo(30));

        // renew the context
        db = new DbContext();

        var loadedValues = db.SequenceIssues.OrderBy(a => a.DT_INT).ToArray();
        Assert.That(loadedValues[0].SequenceIssuesId, Is.EqualTo(expectedValues1.SequenceIssuesId));
        Assert.That(loadedValues[1].SequenceIssuesId, Is.EqualTo(expectedValues2.SequenceIssuesId));
    }

    [Test]
    [Trait("Category", "UseHybridTables")]
    public void TestFailureForUnnamedBindingsInCodeBlock()
    {
        using (var conn = new SnowflakeDbConnection(_dbContext.ConnectionString))
        {
            var cmd = conn.CreateCommand();
            conn.Open();
            cmd.CommandText = "create or replace hybrid table HT_TEST (id int identity primary key, a int, b time)";
            cmd.ExecuteNonQuery();

            cmd.CommandText = $"begin insert into HT_TEST (a, b) values (?, ?); end;";

            var par = cmd.CreateParameter();
            par.ParameterName = "1";
            par.DbType = DbType.Int32;
            par.Value = 10;
            cmd.Parameters.Add(par);

            par = cmd.CreateParameter();
            par.ParameterName = "2";
            par.DbType = DbType.DateTime;
            par.Value = DateTime.Now;
            cmd.Parameters.Add(par);
            
            var exception = Assert.Throws<SnowflakeDbException>(() => cmd.ExecuteNonQuery());
            Assert.That(exception.ErrorCode, Is.EqualTo(92240));
        }
    } 

    [Test]
    [Trait("Category", "UseHybridTables")]
    public void TestFailureForNamedBindingsInCodeBlock()
    {
        using (var conn = new SnowflakeDbConnection(_dbContext.ConnectionString))
        {
            var cmd = conn.CreateCommand();
            conn.Open();
            cmd.CommandText = "create or replace hybrid table HT_TEST (id int identity primary key, a int, b time)";
            cmd.ExecuteNonQuery();

            cmd.CommandText = $"begin insert into HT_TEST (a, b) values (:p1, :p2); end;";

            var par = cmd.CreateParameter();
            par.ParameterName = "p1";
            par.DbType = DbType.Int32;
            par.Value = 10;
            cmd.Parameters.Add(par);

            par = cmd.CreateParameter();
            par.ParameterName = "p2";
            par.DbType = DbType.DateTime;
            par.Value = DateTime.Now;
            cmd.Parameters.Add(par);
            
            var exception = Assert.Throws<SnowflakeDbException>(() => cmd.ExecuteNonQuery());
            Assert.That(exception.ErrorCode, Is.EqualTo(904));
        }
    } 
    
    [Test]
    [Trait("Category", "UseHybridTables")]
    public void TestUnnamedBindingsInMultiStatementWithoutTimeColumn()
    {
        using (var conn = new SnowflakeDbConnection(_dbContext.ConnectionString))
        {
            var cmd = conn.CreateCommand();
            conn.Open();
            cmd.CommandText = "create or replace hybrid table HT_TEST (id int identity primary key, a int)";
            cmd.ExecuteNonQuery();

            cmd.CommandText = $"insert into HT_TEST (a) values (?);select 1;";

            var par = cmd.CreateParameter();
            par.ParameterName = "1";
            par.DbType = DbType.Int32;
            par.Value = 10;
            cmd.Parameters.Add(par);

            par = cmd.CreateParameter();
            par.ParameterName = "MULTI_STATEMENT_COUNT";
            par.DbType = DbType.Int32;
            par.Value = 0;
            cmd.Parameters.Add(par);
            
            Assert.DoesNotThrow(() => cmd.ExecuteNonQuery());
        }
    } 
    
    [Test]
    [Trait("Category", "UseHybridTables")]
    public void TestFailureForUnnamedBindingsInMultiStatementWithNullValues()
    {
        using (var conn = new SnowflakeDbConnection(_dbContext.ConnectionString))
        {
            var cmd = conn.CreateCommand();
            conn.Open();
            cmd.CommandText = "create or replace hybrid table HT_TEST (id int identity primary key, a int null)";
            cmd.ExecuteNonQuery();

            cmd.CommandText = $"insert into HT_TEST (a) values (:p1); select 1;";

            var par = cmd.CreateParameter();
            par.ParameterName = "p1";
            par.DbType = DbType.Int32;
            par.Value = DBNull.Value;
            cmd.Parameters.Add(par);

            par = cmd.CreateParameter();
            par.ParameterName = "MULTI_STATEMENT_COUNT";
            par.DbType = DbType.Int32;
            par.Value = 0;
            cmd.Parameters.Add(par);
            
            var exception = Assert.Throws<SnowflakeDbException>(() => cmd.ExecuteNonQuery());
            Assert.That(exception.ErrorCode, Is.EqualTo(100132));
        }
    } 
    
    [Test]
    [Trait("Category", "UseHybridTables")]
    public void TestFailureForNamedBindingsInMultiStatementWithoutTimeColumn()
    {
        using (var conn = new SnowflakeDbConnection(_dbContext.ConnectionString))
        {
            var cmd = conn.CreateCommand();
            conn.Open();
            cmd.CommandText = "create or replace hybrid table HT_TEST (id int identity primary key, a int)";
            cmd.ExecuteNonQuery();

            cmd.CommandText = $"insert into HT_TEST (a) values (:p1);select 1;";

            var par = cmd.CreateParameter();
            par.ParameterName = "p1";
            par.DbType = DbType.Int32;
            par.Value = 10;
            cmd.Parameters.Add(par);

            par = cmd.CreateParameter();
            par.ParameterName = "MULTI_STATEMENT_COUNT";
            par.DbType = DbType.Int32;
            par.Value = 0;
            cmd.Parameters.Add(par);
            
            var exception = Assert.Throws<SnowflakeDbException>(() => cmd.ExecuteNonQuery());
            Assert.That(exception.ErrorCode, Is.EqualTo(100132));
        }
    } 
    
    [Test]
    [Trait("Category", "UseHybridTables")]
    public void TestFailureForUnnamedBindingsInMultiStatementWithTimeColumn()
    {
        using (var conn = new SnowflakeDbConnection(_dbContext.ConnectionString))
        {
            var cmd = conn.CreateCommand();
            conn.Open();
            cmd.CommandText = "create or replace hybrid table HT_TEST (id int identity primary key, a int, b time)";
            cmd.ExecuteNonQuery();

            cmd.CommandText = $"insert into HT_TEST (a, b) values (?, ?);select 1;";

            var par = cmd.CreateParameter();
            par.ParameterName = "1";
            par.DbType = DbType.Int32;
            par.Value = 10;
            cmd.Parameters.Add(par);

            par = cmd.CreateParameter();
            par.ParameterName = "2";
            par.DbType = DbType.DateTime;
            par.Value = DateTime.Now;
            cmd.Parameters.Add(par);

            par = cmd.CreateParameter();
            par.ParameterName = "MULTI_STATEMENT_COUNT";
            par.DbType = DbType.Int32;
            par.Value = 0;
            cmd.Parameters.Add(par);
            
            var exception = Assert.Throws<SnowflakeDbException>(() => cmd.ExecuteNonQuery());
            Assert.That(exception.ErrorCode, Is.EqualTo(100132));
        }
    } 
    
    [Test]
    [Trait("Category", "UseHybridTables")]
    public void TestFailureForPrimaryKeyCollision()
    {
        using (var conn = new SnowflakeDbConnection(_dbContext.ConnectionString))
        {
            var cmd = conn.CreateCommand();
            conn.Open();
            cmd.CommandText = "create or replace hybrid table PK_DUPLICATE (id int identity primary key, a int, b time)";
            cmd.ExecuteNonQuery();

            cmd.CommandText = $"begin insert into HT_TEST (a, b) values (?, ?); end;";

            var par = cmd.CreateParameter();
            par.ParameterName = "1";
            par.DbType = DbType.Int32;
            par.Value = 10;
            cmd.Parameters.Add(par);

            par = cmd.CreateParameter();
            par.ParameterName = "2";
            par.DbType = DbType.DateTime;
            par.Value = DateTime.Now;
            cmd.Parameters.Add(par);
            
            var exception = Assert.Throws<SnowflakeDbException>(() => cmd.ExecuteNonQuery());
            Assert.That(exception.ErrorCode, Is.EqualTo(92240));
        }
    } 
}