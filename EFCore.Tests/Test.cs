using System.Reflection;
using log4net;
using Microsoft.EntityFrameworkCore;

namespace EFCore.Tests;

public class Post
{
    public int PostId { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }

    public int BlogId { get; set; }
    public Blog Blog { get; set; }
}

public class Blog
{
    public int BlogId { get; set; }
    public string Url { get; set; }

    public List<Post> Posts { get; } = new();
}

public class Tests : EFCoreTestBase
{
    protected static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);
    
    public class BloggingContext : DbContextBase
    {
        public DbSet<Blog> Blogs { get; set; }
        public DbSet<Post> Posts { get; set; }
        
        public BloggingContext()
        {
        }
    }
    
    [SetUp]
    public void BeforeEach()
    {
        DropTables();
    }
    
    /// <inheritdoc />
    public override void InitializeDatabaseContext()
    {
        _dbContext = new BloggingContext();
    }

    /// <inheritdoc />
    public override void InitializeTableNamesToClean()
    {
        _tablesToClean.Add("Posts");
        _tablesToClean.Add("Blogs");
    }


    [Test]
    public void Test1()
    {
        using var db = new BloggingContext();
        db.Database.EnsureCreated();

        db.Add(new Blog { Url = "http://blogs.msdn.com/adonet", BlogId = 10});
        db.SaveChanges();
    }
    
}