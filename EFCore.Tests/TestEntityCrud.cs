using System.Reflection;
using log4net;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.Common;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Snowflake.Data.Client;

namespace EFCore.Tests;

[TestFixture]
public class TestEntityCrud : EFCoreTestBase
{
    private static readonly ILog s_log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);
    private const string _title1 = "English: It works! 🌕";
    private const string _title2 = "Japanese: それは働いています 🐱";
    private const string _title3 = "Chinese: 效果很好 🐺";
    private const string _title4 = "French: Ça marche très bien! 👍";
    private const string _title5 = "Polish: Ależ to działa! 😅";
    private static EntitySample s_entity1 = new (1, _title1, DateTime.Today, Math.PI, 2.7f, true, 987.123456, Guid.NewGuid());
    private static EntitySample s_entity2 = new (2, _title2, DateTime.Now, Math.E, -100.001f, false, 0.0, Guid.NewGuid());
    private static EntitySample s_entity3 = new (3, _title3, DateTime.UtcNow, Math.Tau, 1000.5f, false, 1.00000024, Guid.NewGuid());
    private static EntitySample s_entity4 = new (4, _title4, DateTime.MinValue, Math.Sin(Math.PI / 2.0), float.MinValue, true, 0.0, Guid.NewGuid());
    private static EntitySample s_entity5 = new (5, _title5, DateTime.MaxValue, Math.PI, float.MaxValue, true, -13.0102037, Guid.NewGuid());
    private DbContext _db;
    private List<EntitySample> _entities = new List<EntitySample>(new[] { s_entity1, s_entity2, s_entity3, s_entity4, s_entity5});

    public class DbContext : DbContextBase
    {
        public DbSet<EntitySample> TypesSample { get; set; }

        public void AddAll(List<EntitySample> entities)
        {
            entities.ForEach(it => Add(it));
        }
    }

    public override void InitializeDatabaseContext()
    {
        _dbContext = new DbContext();
        _db = (DbContext)_dbContext;
    }

    public override void InitializeTableNamesToClean()
    {
        _tablesToClean.Add("TypesSample");
    }

    [OneTimeSetUp]
    public void BeforeAll()
    {
        DropTables();
        _db.Database.EnsureCreated();
    }

    [SetUp]
    public void BeforeEach()
    {
        TruncateTables();
    }

    [TearDown]
    public void AfterEach()
    {
    }

    [Test]
    public void TestInsert()
    {
        s_log.Info(MethodBase.GetCurrentMethod()?.ToString());
        s_log.Debug("Running DDL");
        _db.AddAll(_entities);

        s_log.Debug("DML Insert...");
        _dbContext.SaveChanges();

        // TODO: Asserts....
        s_log.Debug("Completed");
    }

    [Test]
    public void TestUpdate()
    {
        s_log.Info(MethodBase.GetCurrentMethod()?.ToString());
        _dbContext.Add(s_entity5);
        _dbContext.SaveChanges();

        s_entity5.StringValue = _title1;

        s_log.Info("DML Update...");
        _dbContext.SaveChanges();

        // TODO: Asserts....
        s_log.Debug("Completed.");
    }

    [Test]
    public void TestSelectTop1()
    {
        s_log.Info(MethodBase.GetCurrentMethod()?.ToString());
        _dbContext.Add(s_entity1);
        _dbContext.SaveChanges();
        
        s_log.Debug("Select");
        var entityFromDb = _db.TypesSample.Single(it => it.IntValue == 1);

        Assert.That(entityFromDb, Is.EqualTo(s_entity1));
        s_log.Debug("Completed.");
    }

    [Test]
    public void TestDelete()
    {
        s_log.Info(MethodBase.GetCurrentMethod()?.ToString());
        _dbContext.Add(s_entity1);
        _dbContext.SaveChanges();

        s_log.Info("DML Delete");
        _dbContext.Remove(s_entity1);
        _dbContext.SaveChanges();

        _db.TypesSample.Load();
        Assert.That(_db.TypesSample.Select(t => t).Count(), Is.EqualTo(0));
        s_log.Debug("Completed.");
    }

    [Test]
    public void TestSelect()
    {
        // Assert.Ignore();
        s_log.Info(MethodBase.GetCurrentMethod()?.ToString());
        InsertData();
        
        _db.TypesSample.Load();
        
        var first = _db.TypesSample
            .Where(it => it.IntValue.Equals(s_entity1.IntValue))
            .ToList()
            .First();
        _db.ChangeTracker.Clear();

        // Assert.That(first, Is.EqualTo(entity1)); cannot be used till double is not saved with a bigger precision
        Assert.That(first.IntValue, Is.EqualTo(s_entity1.IntValue));
        Assert.That(first.StringValue, Is.EqualTo(s_entity1.StringValue));
        Assert.That(first.GuidValue, Is.EqualTo(s_entity1.GuidValue));

        /* TODO:
        1. code {double}       : 3.1415926535897931
        2. Snowflake {float}   : 3.141592654
        3. Loaded back {double}: 3.1415926540000001
        */
        // Assert.That(first.DoubleValue, Is.EqualTo(entity1.DoubleValue));
        s_log.Debug("Completed.");
    }

    private void InsertData()
    {
        using (var conn = new SnowflakeDbConnection(_dbContext.ConnectionString))
        {
            var cmd = conn.CreateCommand();
            cmd.CommandText = @"insert into ""TypesSample"" ( ""IntValue"", ""HexArray"", 
                                ""StringValue"", ""DateTimeValue"", ""DoubleValue"", 
                                ""FloatValue"", ""BooleanValue"", ""GuidValue"") 
                                values (?, ?, ?, ?, ?, ?, ?, ?)";
            CreateParameter(cmd, "1", DbType.Int32, _entities.Select(e => e.IntValue).ToArray());
            CreateParameter(cmd, "2", DbType.Binary, _entities.Select(e => e.HexArray).ToArray());
            CreateParameter(cmd, "3", DbType.String, _entities.Select(e => e.StringValue).ToArray());
            CreateParameter(cmd, "4", DbType.DateTime, _entities.Select(e => e.DateTimeValue).ToArray());
            CreateParameter(cmd, "5", DbType.Double, _entities.Select(e => e.DoubleValue).ToArray());
            CreateParameter(cmd, "6", DbType.Double, _entities.Select(e => e.FloatValue).ToArray());
            CreateParameter(cmd, "7", DbType.Boolean, _entities.Select(e => e.BooleanValue).ToArray());
            CreateParameter(cmd, "8", DbType.Guid, _entities.Select(e => e.GuidValue).ToArray());
            conn.Open();
            cmd.ExecuteNonQuery();
            conn.Close();
        }
    }

    private DbParameter CreateParameter(DbCommand cmd, string paramName, DbType dbType, Object value)
    {
        var parameter1 = cmd.CreateParameter();
        parameter1.ParameterName = paramName;
        parameter1.DbType = dbType;
        parameter1.Value = value;
        cmd.Parameters.Add(parameter1);
        return parameter1;
    }
}

public class EntitySample : IComparable
{
    [Key] public int IntValue { get; set; }
    public byte[]? HexArray { get; set; }
    public string StringValue { get; set; }

    public DateTime? DateTimeValue { get; set; }

    // public TimeSpan TimeSpanValue { get; set; } // TODO: failed to convert timestamp to time
    public double? DoubleValue { get; set; }
    public float? FloatValue { get; set; }
    public bool? BooleanValue { get; set; }

    [Precision(14, 6)]
    // public double NumericValue { get; set; }
    public Guid GuidValue { get; set; }

    public EntitySample()
    {
    }

    public EntitySample(int intValue, string stringValue, DateTime dateTimeValue, double doubleValue, float floatValue,
        bool booleanValue, double numericValue, Guid guidValue)
    {
        IntValue = intValue;
        HexArray = Encoding.Default.GetBytes(stringValue);
        StringValue = stringValue;
        DateTimeValue = dateTimeValue;
        DoubleValue = doubleValue;
        FloatValue = floatValue;
        BooleanValue = booleanValue;
        // NumericValue = numericValue;
        GuidValue = guidValue;
    }
    
    public int CompareTo(object? obj)
    {
        var x = this;
        var yy = obj;
        if (ReferenceEquals(x, yy)) return 1;
        if (ReferenceEquals(x, null)) return 0;
        if (ReferenceEquals(yy, null)) return 0;
        if (x.GetType() != yy.GetType()) return 0;
        EntitySample y = (EntitySample)yy;
        return x.IntValue == y.IntValue && 
               Equals(x.HexArray, y.HexArray) && 
               x.StringValue == y.StringValue && 
               Nullable.Equals(x.DateTimeValue, y.DateTimeValue) && 
               Nullable.Equals(x.DoubleValue, y.DoubleValue) && 
               Nullable.Equals(x.FloatValue, y.FloatValue) && 
               x.BooleanValue == y.BooleanValue &&
               x.GuidValue.Equals(y.GuidValue) ? 1 : 0;
    }

    public override string ToString()
    {
        return IntValue + " " + StringValue;
    }
}
