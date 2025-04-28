using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using log4net;
using Microsoft.EntityFrameworkCore;

namespace EFCore.Tests;

public class DateAndTimeTypes
{
    public int DateAndTimeTypesId { get; set; }

    // DateTime
    
    public DateTime? DT_UNTYPPED { get; set; }
    
    [Column(TypeName="DATE")]
    public DateTime? DT_DATE { get; set; }

    [Column(TypeName="TIME")]
    public DateTime? DT_TIME_DEF { get; set; }
    
    [Column(TypeName="TIME(1)")]
    public DateTime? DT_TIME_1 { get; set; }

    [Column(TypeName="TIME(6)")]
    public DateTime? DT_TIME_6 { get; set; }
    
    [Column(TypeName="TIMESTAMP_NTZ")]
    public DateTime? DT_TS_NTZ_DEF { get; set; }

    [Column(TypeName="TIMESTAMP_NTZ(1)")]
    public DateTime? DT_TS_NTZ_1 { get; set; }

    [Column(TypeName="TIMESTAMP_NTZ(6)")]
    public DateTime? DT_TS_NTZ_6 { get; set; }

    // DateTimeOffset
    
    public DateTimeOffset? DTO_UNTYPPED { get; set; }

    [Column(TypeName = "TIMESTAMP_TZ")] 
    public DateTimeOffset? DTO_TS_TZ_DEF { get; set; }

    [Column(TypeName = "TIMESTAMP_TZ(1)")] 
    public DateTimeOffset? DTO_TS_TZ_1 { get; set; }

    [Column(TypeName = "TIMESTAMP_TZ(6)")] 
    public DateTimeOffset? DTO_TS_TZ_6 { get; set; }

    [Column(TypeName="TIMESTAMP_LTZ")]
    public DateTimeOffset? DTO_TS_LTZ_DEF { get; set; }

    [Column(TypeName="TIMESTAMP_LTZ(1)")]
    public DateTimeOffset? DTO_TS_LTZ_1 { get; set; }

    [Column(TypeName="TIMESTAMP_LTZ(6)")]
    public DateTimeOffset? DTO_TS_LTZ_6 { get; set; }
}

public class TestDateAndTimeTypes : EFCoreTestBase
{
    protected static readonly ILog s_log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);
    
    public class DbContext : DbContextBase
    {
        public DbSet<DateAndTimeTypes> DateAndTimeTypes { get; set; }
        
        public DbContext()
        {
        }
    }

    public TestDateAndTimeTypes()
    {
    }

    public override void InitializeDatabaseContext()
    {
        _dbContext = new DbContext();
    }

    public override void InitializeTableNamesToClean()
    {
        _tablesToClean.Add("DateAndTimeTypes");
    }

    [SetUp]
    public void SetUp()
    {
        DropTables();
    }

    [Test]
    public void TestTypes()
    {
        var timestampToTest = DateTimeOffset.Parse("2019-01-01 12:12:12.1234567 -0500");
        var timeToTest = DateTime.UnixEpoch.AddTicks(timestampToTest.TimeOfDay.Ticks);
        var ticksCorrectionForPrecision1 = -timeToTest.Ticks % (TimeSpan.TicksPerMillisecond * 100);
        var ticksCorrectionForPrecision6 = -timeToTest.Ticks % (TimeSpan.TicksPerMillisecond / 1000);

        var db = new DbContext();
        db.Database.EnsureCreated();

        var expectedValues = new DateAndTimeTypes
        {
            DateAndTimeTypesId = 1,
            DT_UNTYPPED = timestampToTest.DateTime,
            DT_DATE = timestampToTest.Date,
            DT_TIME_DEF = timeToTest,
            DT_TIME_1 = timeToTest,
            DT_TIME_6 = timeToTest,
            DT_TS_NTZ_DEF = timestampToTest.DateTime,
            DT_TS_NTZ_1 = timestampToTest.DateTime,
            DT_TS_NTZ_6 = timestampToTest.DateTime,
            DTO_UNTYPPED = timestampToTest,
            DTO_TS_LTZ_DEF = timestampToTest,
            DTO_TS_LTZ_1 = timestampToTest,
            DTO_TS_LTZ_6 = timestampToTest,
            DTO_TS_TZ_DEF = timestampToTest,
            DTO_TS_TZ_6 = timestampToTest,
            DTO_TS_TZ_1 = timestampToTest
        };

        db.Add(expectedValues);
        db.SaveChanges();

        // renew the context
        db = new DbContext();
        
        var loadedValues = db.DateAndTimeTypes.First();

        Assert.That(loadedValues.DT_UNTYPPED, Is.EqualTo(timestampToTest.DateTime));
        Assert.That(loadedValues.DT_DATE, Is.EqualTo(timestampToTest.Date));
        Assert.That(loadedValues.DT_TIME_DEF, Is.EqualTo(timeToTest));
        Assert.That(loadedValues.DT_TIME_1, Is.EqualTo(timeToTest.AddTicks(ticksCorrectionForPrecision1)));
        Assert.That(loadedValues.DT_TIME_6, Is.EqualTo(timeToTest.AddTicks(ticksCorrectionForPrecision6)));
        Assert.That(loadedValues.DT_TS_NTZ_DEF, Is.EqualTo(timestampToTest.DateTime));
        Assert.That(loadedValues.DT_TS_NTZ_1, Is.EqualTo(timestampToTest.AddTicks(ticksCorrectionForPrecision1).DateTime));
        Assert.That(loadedValues.DT_TS_NTZ_6, Is.EqualTo(timestampToTest.AddTicks(ticksCorrectionForPrecision6).DateTime));
        Assert.That(loadedValues.DTO_UNTYPPED, Is.EqualTo(timestampToTest));
        Assert.That(loadedValues.DTO_TS_LTZ_DEF, Is.EqualTo(timestampToTest));
        Assert.That(loadedValues.DTO_TS_LTZ_1, Is.EqualTo(timestampToTest.AddTicks(ticksCorrectionForPrecision1)));
        Assert.That(loadedValues.DTO_TS_LTZ_6, Is.EqualTo(timestampToTest.AddTicks(ticksCorrectionForPrecision6)));
        Assert.That(loadedValues.DTO_TS_TZ_DEF, Is.EqualTo(timestampToTest));
        Assert.That(loadedValues.DTO_TS_TZ_1, Is.EqualTo(timestampToTest.AddTicks(ticksCorrectionForPrecision1)));
        Assert.That(loadedValues.DTO_TS_TZ_6, Is.EqualTo(timestampToTest.AddTicks(ticksCorrectionForPrecision6)));
    }

}