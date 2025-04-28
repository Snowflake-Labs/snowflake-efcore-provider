using System.Collections;
using System.Reflection;
using log4net;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Snowflake.Data.Client;
using Snowflake.EntityFrameworkCore.Metadata.Internal;

namespace EFCore.Tests;

[TestFixture]
public class TestHtapPerf : EFCoreTestBase
{
    private static readonly ILog s_log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);
    private const string _title1 = "English: It works! 🌕";
    private const string _title2 = "Japanese: それは働いています 🐱";
    private const string _title3 = "Chinese: 效果很好 🐺";
    private const string _title4 = "French: Ça marche très bien! 👍";
    private const string _title5 = "Polish: Ależ to działa! 😅";
    private static Entity s_entity1 = new (1, _title1, DateTime.Today, Math.PI, 2.7f, true, 987.123456, Guid.NewGuid());
    private static Entity s_entity2 = new (2, _title2, DateTime.Now, Math.E, -100.001f, false, 0.0, Guid.NewGuid());
    private static Entity s_entity3 = new (3, _title3, DateTime.UtcNow, Math.Tau, 1000.5f, false, 1.00000024, Guid.NewGuid());
    private static Entity s_entity4 = new (4, _title4, DateTime.MinValue, Math.Sin(Math.PI / 2.0), float.MinValue, true, 0.0, Guid.NewGuid());
    private static Entity s_entity5 = new (5, _title5, DateTime.MaxValue, Math.PI, float.MaxValue, true, -13.0102037, Guid.NewGuid());
    private DbContext _db;
    private List<Entity> _entities = new (new[] { s_entity1, s_entity2, s_entity3, s_entity4, s_entity5});

    public class DbContext : DbContextBase
    {
        public DbSet<Entity> Entity { get; set; }
        public DbSet<HtapPerfTest> BigTable { get; set; }

        public void AddAll(List<Entity> entities)
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
        _tablesToClean.Add("Entity");
    }

    [OneTimeSetUp]
    public void BeforeAll()
    {
        DropTables();
    }

    [SetUp]
    public void BeforeEach()
    {
        // _dbContext.Database.EnsureCreated(); // it will fail on existing BigTable which got created by hand; comment line 36 to run this
    }

    [TearDown]
    public void AfterEach()
    {
    }
    
    [Test]
    [Ignore("Manual performance testing")]
    public void TestUpdateQps()
    {
        s_log.Info(MethodBase.GetCurrentMethod()?.ToString());
        _dbContext.Add(s_entity5);
        _dbContext.SaveChanges();
        var newTitle = s_entity5.StringValue.Substring(0, s_entity5.StringValue.Length - 1);
        var i = 0;
        var queryCount = 10;
        
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        while (++i < queryCount)
        {
            s_entity5.StringValue = newTitle + i;
        }
        _dbContext.SaveChanges();
        stopwatch.Stop();

        s_log.Info("DML "+queryCount+"x updates: "  + stopwatch.ElapsedMilliseconds/1000.0 + "s");
        s_log.Info("QPS: " + Math.Round(queryCount/(stopwatch.ElapsedMilliseconds/1000.0), 2));
        
        s_log.Debug("Completed.");
    }

    [Test]
    [Ignore("Manual performance testing")]
    public void TestInsertMoreRows()
    {
        s_log.Info(MethodBase.GetCurrentMethod()?.ToString());

        ArrayList entities = new ArrayList();
        int size = 1;
        for (var j = 1; j <= size; j++)
        {
            Entity entity = new(j, "Title" + j, DateTime.Now.Subtract(TimeSpan.FromMinutes(j)), j / 10.0,
                j / 10.0f, true, j, Guid.NewGuid());
            _log.Debug($"{j}. {entity}");
            entities.Add(entity);
            _dbContext.Add(entity);
        }
        _dbContext.SaveChanges();
        foreach (var entity in entities)
        {
            Entity e = (Entity)entity;
            e.StringValue = "test";
            // _dbContext.Remove(entity);
        }

        var stopwatch = new Stopwatch();
        stopwatch.Start();
        _dbContext.SaveChanges();
        stopwatch.Stop();
        
        s_log.Info($"DML {size} inserts: "  + stopwatch.ElapsedMilliseconds/1000.0 + "s");
        s_log.Info("QPS: " + Math.Round(size/(stopwatch.ElapsedMilliseconds/1000.0), 2));
        
        s_log.Debug("Completed.");
    }
    
    [Test]
    [Ignore("Manual performance testing")]
    public void TestDeleteRows()
    {
        s_log.Info(MethodBase.GetCurrentMethod()?.ToString());

        ArrayList entities = new ArrayList();
        int size = 1;
        for (var j = 1; j <= size; j++)
        {
            Entity entity = new(j, "Title" + j, DateTime.Now.Subtract(TimeSpan.FromMinutes(j)), j / 10.0,
                j / 10.0f, true, j, Guid.NewGuid());
            _log.Debug($"{j}. {entity}");
            entities.Add(entity);
            _dbContext.Add(entity);
        }
        _dbContext.SaveChanges();
        
        foreach (var entity in entities)
        {
            _dbContext.Remove(entity);
        }

        var stopwatch = new Stopwatch();
        stopwatch.Start();
        _dbContext.SaveChanges();
        stopwatch.Stop();
        
        s_log.Info($"DML {size} deletes: "  + stopwatch.ElapsedMilliseconds/1000.0 + "s");
        s_log.Info("QPS: " + Math.Round(size/(stopwatch.ElapsedMilliseconds/1000.0), 2));
        
        s_log.Debug("Completed.");
    }

    [Test]
    [Ignore("Manual performance testing")]
    public void TestLinqMilion()
    {
        var stopwatch = new Stopwatch();

        stopwatch.Start();
        var htapPerfTests = _db.BigTable.Where(i=>i.Id <= 1000000).ToList();
        stopwatch.Stop();
        s_log.Debug($"Load {htapPerfTests.Count}: ---> ETA {stopwatch.ElapsedMilliseconds/1000.0}s");
    }
        
    [Test]
    [Ignore("Manual performance testing")]
    public void TestUpdatePartOfHugeSet()
    {
        var stopwatch = new Stopwatch();

        stopwatch.Start();
        var htapPerfTests = _db.BigTable.Where(i=>i.Id <= 1000000).ToList();
        stopwatch.Stop();
        s_log.Debug($"Load {htapPerfTests.Count}: ---> ETA {stopwatch.ElapsedMilliseconds/1000.0}s");
        
        stopwatch.Reset();
        stopwatch.Start();
        foreach (var row in htapPerfTests)
        {
            if (row.Id % 10000 == 0)
            {
                row.String1 = "New value";
                // row.Boolean = !row.Boolean;
                row.Double1 *= 2;
            }
        }
        stopwatch.Stop();
        s_log.Debug($"Update {htapPerfTests.Count}: ---> ETA {stopwatch.ElapsedMilliseconds/1000.0}s");

        stopwatch.Reset();
        stopwatch.Start();
        _db.SaveChanges();
        stopwatch.Stop();
        s_log.Debug($"Save {htapPerfTests.Count}: ---> ETA {stopwatch.ElapsedMilliseconds/1000.0}s");
    }
    
    [Test]
    [Ignore("Manual performance testing")]
    public void TestLinqHalfMilionRows()
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        var htapPerfTests = _db.BigTable.Where(i=>i.Id <= 500000).ToList();
        stopwatch.Stop();
        s_log.Debug($"{htapPerfTests.Count}: ---> ETA {stopwatch.ElapsedMilliseconds/1000.0}s");
    }
       
    [Test]
    [Ignore("Manual performance testing")]
    public void TestLinqInClause()
    {
        var id = 15000;
        var include = new int[id];
        while (--id >= 0) include[id] = id;
        
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        var query = from perf in _db.BigTable
            where include.Any(id => perf.Id == id)
            select perf;
        var htapPerfTests = query.ToList();
        stopwatch.Stop();
        s_log.Debug($"{htapPerfTests.Count}: ---> ETA {stopwatch.ElapsedMilliseconds/1000.0}s");
    }
    
    [Test]
    [Ignore("Manual performance testing")]
    public void TestLoadMoreRows()
    {
        s_log.Info(MethodBase.GetCurrentMethod()?.ToString());

        TestInsertMoreRows();
        
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        List<Entity> allRows = ((DbContext)_dbContext).Entity.ToList();
        stopwatch.Stop();

        int size = allRows.Count;
        s_log.Info($"DML {size} select: "  + stopwatch.ElapsedMilliseconds/1000.0 + "s");
        s_log.Info("QPS: " + Math.Round(size/(stopwatch.ElapsedMilliseconds/1000.0), 2));
        
        s_log.Debug("Completed.");
    }
}

[Table("HTAP_PERF_TEST")]
public class HtapPerfTest
{
    [Key] 
    public int Id { get; set; }
    public string? String1   { get; set; }
    public string? String2   { get; set; }
    public string? String3   { get; set; }
    public string? String4   { get; set; }
    public DateTime? DateTime{ get; set; }
    public double? Double1   { get; set; }
    public double? Double2   { get; set; }
    public bool? Boolean     { get; set; }
    public Guid? Guid        { get; set; }
}

public class Entity : IComparable
{
    [Key] public int IntValue { get; set; }
    // public byte[]? HexArray { get; set; } // TODO: fails when DB-side identity used (multistatements)
    public string StringValue { get; set; }

    // public DateTime? DateTimeValue { get; set; } // TODO: fails when DB-side identity used (multistatements)

    // public TimeSpan TimeSpanValue { get; set; } // TODO: failed to convert timestamp to time
    public double? DoubleValue { get; set; }
    public float? FloatValue { get; set; }
    // public bool? BooleanValue { get; set; }

    // [Precision(14, 6)]
    // public double NumericValue { get; set; }
    public Guid GuidValue { get; set; }
    
    public Entity()
    {
    }

    public Entity(int intValue, string stringValue, DateTime dateTimeValue, double doubleValue, float floatValue,
        bool booleanValue, double numericValue, Guid guidValue)
    {
        IntValue = intValue;
        // HexArray = Encoding.Default.GetBytes(stringValue);
        StringValue = stringValue;
        // DateTimeValue = dateTimeValue;
        DoubleValue = doubleValue;
        FloatValue = floatValue;
        // BooleanValue = booleanValue;
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
        Entity y = (Entity)yy;
        return x.IntValue == y.IntValue && 
               // Equals(x.HexArray, y.HexArray) && 
               x.StringValue == y.StringValue && 
               // Nullable.Equals(x.DateTimeValue, y.DateTimeValue) && 
               Nullable.Equals(x.DoubleValue, y.DoubleValue) && 
               Nullable.Equals(x.FloatValue, y.FloatValue) && 
               // x.BooleanValue == y.BooleanValue &&
               x.GuidValue.Equals(y.GuidValue) ? 1 : 0;
    }

    public override string ToString()
    {
        return String.Format($"Entity({IntValue}; {StringValue}; {DoubleValue}; {GuidValue})");
    }
}
