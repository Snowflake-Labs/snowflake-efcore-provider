using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace EFCore.Tests;

public class TestColumnTypes : EFCoreTestBase
{
    public override void InitializeTableNamesToClean()
    {
        var types = new []
        {
            typeof(Int16), 
            typeof(Int32), 
            typeof(Int64), 
            typeof(Double),
            typeof(Single),
            typeof(string),
        };
        types.ToList().ForEach(t => _tablesToClean.Add(t.ToString()));
    }
    
    [OneTimeSetUp]
    public void BeforeAll()
    {
        DropTables();
    }
    
    [Test]
    [TestCase(1, Int16.MinValue)]
    [TestCase(2, (short)-1)]
    [TestCase(3, (short)10)]
    [TestCase(4, Int16.MaxValue)]
    [TestCase(1, Int32.MinValue)]
    [TestCase(2, Int32.MaxValue)]
    [TestCase(1, Int64.MinValue)]
    [TestCase(2, Int64.MaxValue)]
    // [TestCase(1, Double.MinValue)] // TODO:  Expected: -∞ But was:  -1.7976931348623157E+308d
    // [TestCase(2, Double.MaxValue)] // TODO:  Expected: ∞ But was: ...
    // [TestCase(3, Math.PI)] // TODO: Expected: 3.1415926540000001d  But was:  3.1415926535897931d
    [TestCase(4, -123456789.123456d)]
    [TestCase(5, 123456789.12345d)]
    [TestCase(6, 3.1415926d)]
    [TestCase(1, "")]
    [TestCase(2, "Test")]
    [TestCase(3, "Zażółć gęślą jaźń 🤓")]
    [TestCase(4, -123456789.123456f)]
    [TestCase(5, 123456789.12345f)]
    public void TestDifferentColumnTypesE2E<T>(int testCaseId, T expected)
    {
        EntityWithValue<T> entity = new (testCaseId, expected);
        TypeTestDbContext<T> db = new TypeTestDbContext<T>();
        EnsureTableCreated(db);
        db.Add(entity);
        db.SaveChanges();
        
        db = new TypeTestDbContext<T>();
        var loaded = db.Entity.First(it => it.Id == testCaseId);

        Assert.That(entity.Value, Is.EqualTo(loaded.Value));
    }

    private static void EnsureTableCreated<T>(TypeTestDbContext<T> db)
    {
        try // till we embrace migrations it's the only way
        {
            db.Database.EnsureCreated();
        }
        catch (Exception)
        {
        }
    }
}

class TypeTestDbContext<T> : DbContextBase
{
    public DbSet<EntityWithValue<T>> Entity { get; set; }
    
    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<EntityWithValue<T>>(entity => {
            entity.ToTable(typeof(T).ToString());
        });
    }

}

public class EntityWithValue<T> : IComparable
{
    [Key]
    public int Id { get; set; }
    public T Value { get; set; }

    public EntityWithValue()
    {
    }
    
    public EntityWithValue(int key, T t)
    {
        Id = key;
        Value = t;
    }
        
    public int CompareTo(object? obj)
    {
        if (ReferenceEquals(this, obj)) return 1;
        if (ReferenceEquals(obj, null)) return 0;
        if (ReferenceEquals(this, null)) return 0;
        if (GetType() != obj.GetType()) return 0;
        EntityWithValue<T> that = (EntityWithValue<T>)obj;
        return Equals(Id, that.Id) && Equals(Value, that.Value) ? 1 : 0;   
    }
}
