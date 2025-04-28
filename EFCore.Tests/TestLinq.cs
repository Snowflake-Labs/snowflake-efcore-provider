using System.Reflection;
using log4net;
using Microsoft.EntityFrameworkCore;

namespace EFCore.Tests;

[TestFixture]
public class TestLinq : EFCoreTestBase
{
    private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);
    private static Animal catLucy = new(1, "Lucy", "cat", 8);
    private static Animal dogBarry = new(2, "Barry", "dog", 2);
    private static Animal catTom = new(3, "Tom", "cat", 5);
    private static PetOwner McCormick = new(1, "Mary", "McCormick", new List<Animal> { catLucy, catTom });
    private static PetOwner Woznicky = new(2, "Kate", "Woznicky", new List<Animal> { dogBarry });
    private static VetClinic clinicFourPaws = new(1, "Four Paws", new List<Animal> { catLucy, dogBarry });
    private static VetClinic clinicPetLovers = new(2, "Pet Lovers", new List<Animal> { dogBarry });
    private static Parent vader = new(1, "Darth Vader");
    private static Child leia = new(1, "Leia Organa", 1);
    private static Child luke = new(2, "Luke Skywalker", 1);

    public class PetsDbContext : DbContextBase
    {
        public DbSet<VetClinic> VetClinic { get; set; }
        public DbSet<PetOwner> PetOwner { get; set; }
        public DbSet<Animal> Animal { get; set; }
        
        public DbSet<Parent> Parent { get; set; }
        
        public DbSet<Child> Child { get; set; }
    }

    private PetsDbContext db = new PetsDbContext();

    public override void InitializeTableNamesToClean()
    {
        _tablesToClean.Add("AnimalVetClinic");
        _tablesToClean.Add("Animal");
        _tablesToClean.Add("VetClinic");
        _tablesToClean.Add("PetOwner");
        _tablesToClean.Add("Parent");
        _tablesToClean.Add("Child");
    }

    [OneTimeSetUp]
    public void BeforeAll()
    {
        DropTables();
        WriteTestData();
    }

    private void WriteTestData()
    {
        db.Add(McCormick);
        db.Add(Woznicky);
        db.Add(catLucy);
        db.Add(dogBarry);
        db.Add(clinicFourPaws);
        db.Add(clinicPetLovers);
        db.Add(catTom);
        db.Add(vader);
        db.Add(leia);
        db.Add(luke);
        db.Database.EnsureCreated();
        db.SaveChanges();
    }

    /*
     * 
     * https://michaelscodingspot.com/when-to-use-c-linq-with-query-syntax-over-method-syntax/
     * https://stackoverflow.com/questions/17890729/how-can-i-write-take1-in-query-syntax
     * 
     * Where,Select,SelectMany,Join,GroupJoin,OrderBy,OrderByDescending,ThenBy,ThenByDescending,GroupBy,Cast
     * 
     * select, where, count, take, skip, any, all, union, join, distinct, except, orderby, groupby, contains, let keyword
     * multiple orderby, groupby
     */

    [Test]
    public void TestSelectQuerySyntax()
    {
        var query = from clinic in db.VetClinic
            select clinic.Name;

        var result = query.ToList();

        Assert.That(result.Count, Is.EqualTo(2));
        Assert.That(result.Contains(clinicFourPaws.Name));
        Assert.That(result.Contains(clinicPetLovers.Name));
    }

    [Test]
    public void TestSelectFluentSyntax()
    {
        db.PetOwner.Include(i => i.Animals);
        var query = db.VetClinic
            .Include(i => i.Animals);

        var result = query.Include(i => i.Animals).ToList();

        Assert.That(result.Count, Is.EqualTo(2));
        Assert.That(result.Contains(clinicFourPaws));
        Assert.That(result.Contains(clinicPetLovers));
    }

    [Test]
    public void TestWhereQuerySyntax()
    {
        db.PetOwner.Include(i => i.Animals);
        var owners = from petOwner
                in db.PetOwner
            where petOwner.Animals.Count >= 1
            select petOwner;

        Assert.That(owners.Contains(McCormick));
        Assert.That(owners.Contains(Woznicky));
        Assert.That(owners.Count, Is.EqualTo(2));
    }

    [Test]
    public void TestWhereFluentSyntax()
    {
        db.PetOwner.Include(i => i.Animals);
        db.Animal.Include(i => i.Clinics);
        var query = db.PetOwner
            .Include(i => i.Animals)
            .Where(i => i.Animals.Count >= 1);

        var result = query.ToList();

        Assert.That(result.Contains(McCormick));
        Assert.That(result.Contains(Woznicky));
        Assert.That(result.Count, Is.EqualTo(2));
    }

    [Test]
    public void TestCountQuerySyntax()
    {
        var query = (from animal in db.Animal select animal)
            .Count(); // no equivalent in query syntax for Count
        
        Assert.That(query, Is.EqualTo(3));
    }

    [Test]
    public void TestCountFluentSyntax()
    {
        var query = db.Animal.Count();

        Assert.That(query, Is.EqualTo(3));
    }

    [Test]
    public void TestTakeFluentSyntax() // no query syntax equiv
    {
        var result = db.Animal
            .OrderBy(i=>i.Breed)
            .Take(2)
            .ToList();
        
        Assert.That(result.Count, Is.EqualTo(2));
        Assert.That(result.Contains(catLucy));
        Assert.That(result.Contains(catTom));
    }

    [Test]
    public void TestAny()
    {
        var query = db.PetOwner
            .Include(i=>i.Animals)
            .Any(i => i.Animals.Count > 1);
        
        Assert.That(query, Is.EqualTo(true));
    }

    [Test]
    public void TestAll()
    {
        var query = db.VetClinic.All(i => i.Animals.Count > 0);
        
        Assert.That(query, Is.EqualTo(true));
    }
    
    [Test]
    public void TestUnion()
    {
        var query = db.Animal.Select(i => new { i.AnimalId, i.Name, i.Breed } );
        var query2 = db.Animal.Select(i => new { i.AnimalId, i.Name, i.Breed } );

        var result = query.Concat(query2).ToList();
        
        Assert.That(result.Count, Is.EqualTo(6));
    }

    [Test]
    public void TestDistinctQuerySyntax()
    {

        var query = (from animal in db.Animal
            from clinic in animal.Clinics 
            select clinic).Distinct(); // no query syntax for Distinct

        var result = query.ToList();
        
        Assert.That(result.Count, Is.EqualTo(2));
    }

    [Test]
    public void TestDistinctFluentSyntax()
    {
        var query = db.Animal
            .SelectMany(i => i.Clinics)
            .Distinct();

        var result = query.ToList();

        Assert.That(result.Count(), Is.EqualTo(2));
    }

    [Test]
    public void TestExceptQuerySyntax()
    {
        var exceptions = from animal in db.Animal where animal.Breed == "dog" select animal;
        var query = (from animal in db.Animal select animal).Except(exceptions);

        var result = query.ToList();

        Assert.That(result.Contains(catTom), Is.EqualTo(true));
        Assert.That(result.Contains(catLucy), Is.EqualTo(true));
        Assert.That(result.Contains(dogBarry), Is.EqualTo(false));
    }

    [Test]
    public void TestExceptFluentSyntax()
    {
        var exceptions = new[] { catTom };
        var query = db.Animal
            .AsEnumerable()
            .Except(exceptions);

        var result = query.ToList();

        Assert.That(result.Contains(catTom), Is.EqualTo(false));
        Assert.That(result.Contains(catLucy), Is.EqualTo(true));
        Assert.That(result.Contains(dogBarry), Is.EqualTo(true));
    }

    [Test]
    public void TestOrderByQuerySyntax()
    {
        var query = from animal in db.Animal
            orderby animal.Age 
            select animal;

        var result = query.ToList();

        Assert.That(result[0].Age, Is.EqualTo(dogBarry.Age));
        Assert.That(result[1].Age, Is.EqualTo(catTom.Age));
        Assert.That(result[2].Age, Is.EqualTo(catLucy.Age));
    }

    [Test]
    public void TestOrderByFluentSyntax()
    {
        var query = db.Animal
            .Where(i => i.Age >= 1)
            .OrderBy(i => i.Age);

        var result = query.ToList();

        Assert.That(result[0].Age, Is.EqualTo(dogBarry.Age));
        Assert.That(result[1].Age, Is.EqualTo(catTom.Age));
        Assert.That(result[2].Age, Is.EqualTo(catLucy.Age));
    }

    [Test]
    public void TestOrderByDescendingQuerySyntax()
    {
        var query = from animal in db.Animal
            orderby animal.Age descending 
            select animal;
        
        var result = query.ToList();

        Assert.That(result[0].Age, Is.EqualTo(catLucy.Age));
        Assert.That(result[1].Age, Is.EqualTo(catTom.Age));
        Assert.That(result[2].Age, Is.EqualTo(dogBarry.Age));
    }

    [Test]
    public void TestOrderByDescendingFluentSyntax()
    {
        var query = db.Animal
            .Where(i => i.Age >= 1)
            .OrderByDescending(i => i.Age);

        var result = query.ToList();

        Assert.That(result[0].Age, Is.EqualTo(catLucy.Age));
        Assert.That(result[1].Age, Is.EqualTo(catTom.Age));
        Assert.That(result[2].Age, Is.EqualTo(dogBarry.Age));
    }

    [Test]
    public void TestGroupByQuerySyntax()
    {
        var query = from animal in db.Animal
            group animal by animal.Breed
            into newGroup
            select new { breed = newGroup.Key, count = newGroup.Count() };

        var result = query.ToList();

        Assert.That(result.Count, Is.EqualTo(2));
        Assert.That(result.Contains(new { breed = "cat", count = 2 }));
        Assert.That(result.Contains(new { breed = "dog", count = 1 }));
    }

    [Test]
    public void TestGroupByFluentSyntax()
    {
        var query = db.Animal
            .GroupBy(i => i.Breed)
            .Select(i => new { breed = i.Key!, count = i.Count() });

        var result = query.ToList();

        Assert.That(result.Count, Is.EqualTo(2));
        Assert.That(result.Contains(new { breed = "cat", count = 2 }));
        Assert.That(result.Contains(new { breed = "dog", count = 1 }));
    }

    [Test]
    public void TestContainsQuerySyntax()
    {
        db.PetOwner.Include(i => i.Animals);
        var query = from petOwner in db.PetOwner
            where petOwner.PetOwnerId == McCormick.PetOwnerId
            select petOwner;

        Assert.That(query.Contains(McCormick));
    }

    [Test]
    public void TestContainsFluentSyntax()
    {
        var query = db.PetOwner
            .Include(i=>i.Animals)
            .Where(i => i.PetOwnerId == McCormick.PetOwnerId);

        Assert.That(query.Contains(McCormick));
    }

    [Test]
    public void TestInClauseQuerySyntax()
    {
        var include = new[] { catLucy.Name, catTom.Name };
        var query = from animal in db.Animal
            where include.Any(s => s == animal.Name)
            select animal;

        var result = query.ToList();

        Assert.That(result.Count, Is.EqualTo(2));
    }
    
    
    [Test] 
    [Ignore("Should be working fine in Query syntax; requires further investigation")]
    public void TestSkipQuerySyntax()
    {
        // TODO:
        var result = db.Animal
            .SkipLast(1)
            .ToList();
        // throws: .SkipLast(__p_0)' could not be translated. Either rewrite the query in a form that can be translated, or switch to client evaluation

        Assert.That(result.Count, Is.EqualTo(2));
    }

    [Test] 
    public void TestSkipFluentSyntax()
    {
        var result = db.Animal
            .ToList()
            .AsEnumerable()
            .SkipLast(1);

        Assert.That(result.Count, Is.EqualTo(2));
    }
    
    [Test]
    [Ignore("Should be working fine in Query syntax; requires further investigation on replacing concatenation with || instead of +")]
    public void TestLetQuerySyntax()
    {
        var query = from animal in db.Animal
            let x = animal.Breed + " " + animal.Age + " years old"
            select new {animal.Name, Age = x};
        // TODO: ((COALESCE("a"."Breed", '') + ' ') + COALESCE(CAST("a"."Age" AS varchar), '')) + ' years old' AS "Age"
        
        foreach (var a in query.ToList())
        {
            log.Debug($"{a.Name}, {a.Age}");
        }
        // TODO: add asserts...
    }

    [Test]
    public void TestJoinQuerySyntax()
    {
        var query = from parent in db.Parent
            join child in db.Child
                on parent.ParentId equals child.ParentId
            select new { Parent = parent.Name, Kid = child.Name };

        var result = query.ToList();
        
        Assert.That(result.Count, Is.EqualTo(2));
        Assert.That(result.Contains(new { Parent = vader.Name, Kid = leia.Name }));
        Assert.That(result.Contains(new { Parent = vader.Name, Kid = luke.Name }));
    }

    [Test]
    public void TestJoinFluentSyntax()
    {
        var result = db.Parent
            .Join(db.Child, 
                parent => parent.ParentId, 
                child => child.ParentId, 
                (parent, child) => new {Parent = parent.Name, Kid = child.Name})
            .ToList();
        
        Assert.That(result.Count, Is.EqualTo(2));
        Assert.That(result.Contains(new { Parent = vader.Name, Kid = leia.Name }));
        Assert.That(result.Contains(new { Parent = vader.Name, Kid = luke.Name }));
    }
}