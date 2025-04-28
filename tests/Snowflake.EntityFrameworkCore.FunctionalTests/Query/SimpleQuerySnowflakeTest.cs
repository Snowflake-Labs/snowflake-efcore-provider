using System.Linq.Expressions;
using System.Security.Cryptography.X509Certificates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Snowflake.EntityFrameworkCore.TestUtilities;

namespace Snowflake.EntityFrameworkCore.FunctionalTests.Query;

[Trait("Category", "UseHybridTables")]
public class SimpleQuerySnowflakeTest : SimpleQueryRelationalTestBase
{
    #region 28196

    public override async Task Hierarchy_query_with_abstract_type_sibling(bool async)
    {
        var contextFactory = await InitializeAsync<SfContext28196>(seed: c => c.Seed());

        await using var context = contextFactory.CreateContext();

        var query = context.Animals.OfType<Pet>().Where(a => "F".StartsWith(a.Species));

        var result = async
            ? await query.ToListAsync()
            : query.ToList();
    }

    public override Task Hierarchy_query_with_abstract_type_sibling_TPC(bool async)
        => Hierarchy_query_with_abstract_type_sibling_helper(
            async,
            GetHybridTableAction("TPC"));

    public override Task Hierarchy_query_with_abstract_type_sibling_TPT(bool async)
        => Hierarchy_query_with_abstract_type_sibling_helper(
            async,
            GetHybridTableAction("TPT"));

    protected class SfContext28196(DbContextOptions options) : Context28196(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Animal>().ToTable(t => t.IsHybridTable());
            modelBuilder.Entity<Pet>().ToTable(t => t.IsHybridTable());
            modelBuilder.Entity<Cat>().ToTable(t => t.IsHybridTable());
            modelBuilder.Entity<Dog>().ToTable(t => t.IsHybridTable());
            modelBuilder.Entity<FarmAnimal>().ToTable(t => t.IsHybridTable());
        }
    }

    private static Action<ModelBuilder> GetHybridTableAction(string mappingStrategy) =>
        modelBuilder =>
        {
            if (mappingStrategy == "TPC")
            {
                modelBuilder.Entity<Animal>().UseTpcMappingStrategy();
            }
            else
            {
                modelBuilder.Entity<Animal>().UseTptMappingStrategy();
            }

            modelBuilder.Entity<Animal>().ToTable(t => t.IsHybridTable());
            modelBuilder.Entity<Pet>().ToTable(t => t.IsHybridTable());
            modelBuilder.Entity<Cat>().ToTable(t => t.IsHybridTable());
            modelBuilder.Entity<Dog>().ToTable(t => t.IsHybridTable());
            modelBuilder.Entity<FarmAnimal>().ToTable(t => t.IsHybridTable());
        };

    #endregion

    #region 23981

    public override async Task Multiple_different_entity_type_from_different_namespaces(bool async)
    {
        var contextFactory = await InitializeAsync<Context23981>();
        await using var context = contextFactory.CreateContext();
        var bad = context.Set<NameSpace1.TestQuery>().FromSqlRaw(@"SELECT cast(null as int) AS MyValue")
            .ToList(); // Exception
    }

    #endregion

    #region 27954

    public override async Task StoreType_for_UDF_used(bool async)
    {
        var contextFactory = await InitializeAsync<SfContext27954>();
        await using var context = contextFactory.CreateContext();

        var date = new DateTime(2012, 12, 12);
        var query1 = context.Set<MyEntity>().Where(x => x.SomeDate == date);
        var query2 = context.Set<MyEntity>().Where(x => MyEntity.Modify(x.SomeDate) == date);

        if (async)
        {
            await query1.ToListAsync();
            await Assert.ThrowsAnyAsync<Exception>(() => query2.ToListAsync());
        }
        else
        {
            var myEntities = query1.ToList();
            Assert.ThrowsAny<Exception>(() => query2.ToList());
        }
    }

    protected class SfContext27954(DbContextOptions options) : Context27954(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<MyEntity>().ToTable(t => t.IsHybridTable());
        }
    }

    #endregion

    #region 27954

    public override async Task Unwrap_convert_node_over_projection_when_translating_contains_over_subquery(bool async)
    {
        // // Review SNOW-1852407
        var contextFactory = await InitializeAsync<SfContext26593>(seed: c => c.Seed());
        await using var context = contextFactory.CreateContext();

        const int currentUserId = 1;

        var currentUserGroupIds = context.Memberships
            .Where(m => m.UserId == currentUserId)
            .Select(m => m.GroupId);

        var hasMembership = context.Memberships
            .Where(m => currentUserGroupIds.Contains(m.GroupId))
            .Select(m => m.User);

        var query = context.Users
            .Select(u => new { HasAccess = hasMembership.Contains(u) });

        var users = async
            ? await query.ToListAsync()
            : query.ToList();
    }

    public override async Task Unwrap_convert_node_over_projection_when_translating_contains_over_subquery_2(bool async)
    {
        // Review SNOW-1852407
        var contextFactory = await InitializeAsync<SfContext26593>(seed: c => c.Seed());
        await using var context = contextFactory.CreateContext();

        const int currentUserId = 1;

        var currentUserGroupIds = context.Memberships
            .Where(m => m.UserId == currentUserId)
            .Select(m => m.Group);

        var hasMembership = context.Memberships
            .Where(m => currentUserGroupIds.Contains(m.Group))
            .Select(m => m.User);

        var query = context.Users
            .Select(u => new { HasAccess = hasMembership.Contains(u) });

        var users = async
            ? await query.ToListAsync()
            : query.ToList();
    }

    public override async Task Unwrap_convert_node_over_projection_when_translating_contains_over_subquery_3(bool async)
    {
        // Review SNOW-1852407
        var contextFactory = await InitializeAsync<SfContext26593>(seed: c => c.Seed());
        await using var context = contextFactory.CreateContext();

        const int currentUserId = 1;

        var currentUserGroupIds = context.Memberships
            .Where(m => m.UserId == currentUserId)
            .Select(m => m.GroupId);

        var hasMembership = context.Memberships
            .Where(m => currentUserGroupIds.Contains(m.GroupId))
            .Select(m => m.User);

        var query = context.Users
            .Select(u => new { HasAccess = hasMembership.Any(e => e == u) });

        var users = async
            ? await query.ToListAsync()
            : query.ToList();
    }

    protected class SfContext26593(DbContextOptions options) : Context26593(options)
    {
        public new void Seed()
        {
            var user = new User { Id = 1, Memberships = [] };
            var group = new Group { Id = 1, Memberships = [] };
            var membership = new Membership { Id = 1, UserId = 1, GroupId = 1, Group = group, User = user };
            AddRange(user, group, membership);

            SaveChanges();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<User>().ToTable(t => t.IsHybridTable());
            modelBuilder.Entity<Group>().ToTable(t => t.IsHybridTable());
            modelBuilder.Entity<Membership>().ToTable(t => t.IsHybridTable());
        }
    }

    #endregion

    #region 24368

    [ConditionalTheory(Skip = "Disable due SNOW-1825765")]
    [MemberData(nameof(IsAsyncData))]
    public override async Task Multiple_nested_reference_navigations(bool async)
    {
        var contextFactory = await InitializeAsync<SfContext24368>();
        await using var context = contextFactory.CreateContext();
        const int id = 1;
        var staff = await context.Staff.FindAsync(3);

        Assert.Equal(1, staff?.ManagerId);

        var query = context.Appraisals
            .Include(ap => ap.Staff).ThenInclude(s => s.Manager)
            .Include(ap => ap.Staff).ThenInclude(s => s.SecondaryManager)
            .Where(ap => ap.Id == id);

        var appraisal = async
            ? await query.SingleOrDefaultAsync()
            : query.SingleOrDefault();

        Assert.Null(staff?.ManagerId);

        Assert.NotNull(appraisal);
        Assert.Same(staff, appraisal.Staff);
        Assert.NotNull(appraisal.Staff.SecondaryManager);
        Assert.Equal(2, appraisal.Staff.SecondaryManagerId);
    }

    protected class SfContext24368(DbContextOptions options) : Context24368(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Appraisal>().ToTable(t => t.IsHybridTable());
            modelBuilder.Entity<Staff>().ToTable(t => t.IsHybridTable());
        }
    }

    #endregion

    #region 26587

    public override async Task GroupBy_aggregate_on_right_side_of_join(bool async)
    {
        var contextFactory = await InitializeAsync<SfContext26587>();
        await using var context = contextFactory.CreateContext();

        const int orderId = 123456;

        var orderItems = context.OrderItems.Where(o => o.OrderId == orderId);
        var items = orderItems
            .GroupBy(
                o => o.OrderId,
                (o, g) => new
                {
                    Key = o,
                    IsPending = g.Max(y => y.ShippingDate == null && y.CancellationDate == null ? o : (o - 10000000))
                })
            .OrderBy(e => e.Key);

        var query = orderItems
            .Join(items, x => x.OrderId, x => x.Key, (x, y) => x)
            .OrderBy(x => x.OrderId);

        var users = async
            ? await query.ToListAsync()
            : query.ToList();
    }

    protected class SfContext26587(DbContextOptions options) : Context26587(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<OrderItem>().ToTable(t => t.IsHybridTable());
        }
    }

    #endregion

    #region 26472

    [ConditionalTheory(Skip = "Snowflake does not support this feature (Unsupported subquery)")]
    [MemberData(nameof(IsAsyncData))]
    public override async Task Enum_with_value_converter_matching_take_value(bool async)
    {
        var contextFactory = await InitializeAsync<SfContext26472>();
        await using var context = contextFactory.CreateContext();
        var orderItemType = OrderItemType.MyType1;
        var query = context.Orders.Where(x => x.Items.Any()).OrderBy(e => e.Id).Take(1)
            .Select(e => e.Id)
            .Join(context.Orders, o => o, i => i.Id, (o, i) => i)
            .Select(
                entity => new
                {
                    entity.Id,
                    SpecialSum = entity.Items.Where(x => x.Type == orderItemType)
                        .Select(x => x.Price)
                        .FirstOrDefault()
                });

        var result = async
            ? await query.ToListAsync()
            : query.ToList();
    }

    protected class SfContext26472(DbContextOptions options) : Context26472(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Order>().ToTable(t => t.IsHybridTable());
            modelBuilder.Entity<Customer>().ToTable(t => t.IsHybridTable());
            modelBuilder.Entity<Project>().ToTable(t => t.IsHybridTable());
            modelBuilder.Entity<Order26472>().ToTable(t => t.IsHybridTable());
            modelBuilder.Entity<OrderItem26472>().ToTable(t => t.IsHybridTable());
        }
    }

    #endregion

    #region 27083

    public override async Task GroupBy_Aggregate_over_navigations_repeated(bool async)
    {
        // Review SNOW-1852407
        var contextFactory = await InitializeAsync<SfContext27083>(seed: c => c.Seed());
        await using var context = contextFactory.CreateContext();

        var query = context
            .Set<TimeSheet>()
            .Where(x => x.OrderId != null)
            .GroupBy(x => x.OrderId)
            .Select(
                x => new
                {
                    HourlyRate = x.Min(f => f.Order.HourlyRate),
                    CustomerId = x.Min(f => f.Project.Customer.Id),
                    CustomerName = x.Min(f => f.Project.Customer.Name),
                });

        var timeSheets = async
            ? await query.ToListAsync()
            : query.ToList();

        Assert.Equal(2, timeSheets.Count);
    }

    public override async Task Aggregate_over_subquery_in_group_by_projection(bool async)
    {
        // Review SNOW-1852407
        var contextFactory = await InitializeAsync<SfContext27083>(seed: c => c.Seed());
        await using var context = contextFactory.CreateContext();

        Expression<Func<Order, bool>> someFilterFromOutside = x => x.Number != "A1";

        var query = context
            .Set<Order>()
            .Where(someFilterFromOutside)
            .GroupBy(x => new { x.CustomerId, x.Number })
            .Select(
                x => new
                {
                    x.Key.CustomerId,
                    CustomerMinHourlyRate = context.Set<Order>().Where(n => n.CustomerId == x.Key.CustomerId)
                        .Min(h => h.HourlyRate),
                    HourlyRate = x.Min(f => f.HourlyRate),
                    Count = x.Count()
                });

        var orders = async
            ? await query.ToListAsync()
            : query.ToList();

        Assert.Collection(
            orders.OrderBy(x => x.CustomerId),
            t =>
            {
                Assert.Equal(1, t.CustomerId);
                Assert.Equal(10, t.CustomerMinHourlyRate);
                Assert.Equal(11, t.HourlyRate);
                Assert.Equal(1, t.Count);
            },
            t =>
            {
                Assert.Equal(2, t.CustomerId);
                Assert.Equal(20, t.CustomerMinHourlyRate);
                Assert.Equal(20, t.HourlyRate);
                Assert.Equal(1, t.Count);
            });
    }

    protected class SfContext27083(DbContextOptions options) : Context27083(options)
    {
        public new void Seed()
        {
            var customerA = new Customer { Id = 1, Name = "Customer A" };
            var customerB = new Customer { Id = 2, Name = "Customer B" };

            var projectA = new Project { Id = 1, Customer = customerA };
            var projectB = new Project { Id = 2, Customer = customerB };

            var orderA1 = new Order
            {
                Id = 1,
                Number = "A1",
                Customer = customerA,
                HourlyRate = 10
            };
            var orderA2 = new Order
            {
                Id = 2,
                Number = "A2",
                Customer = customerA,
                HourlyRate = 11
            };
            var orderB1 = new Order
            {
                Id = 3,
                Number = "B1",
                Customer = customerB,
                HourlyRate = 20
            };

            var timeSheetA = new TimeSheet { Id = 1, Order = orderA1, Project = projectA };
            var timeSheetB = new TimeSheet { Id = 2, Order = orderB1, Project = projectB };

            AddRange(customerA, customerB);
            AddRange(projectA, projectB);
            AddRange(orderA1, orderA2, orderB1);
            AddRange(timeSheetA, timeSheetB);
            SaveChanges();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Customer>().ToTable(t => t.IsHybridTable());
            modelBuilder.Entity<TimeSheet>().ToTable(t => t.IsHybridTable());
            modelBuilder.Entity<Order>().ToTable(t => t.IsHybridTable());
            modelBuilder.Entity<Project>().ToTable(t => t.IsHybridTable());
        }
    }

    #endregion

    #region 27094

    public override async Task Aggregate_over_subquery_in_group_by_projection_2(bool async)
    {
        var contextFactory = await InitializeAsync<SfContext27094>();
        await using var context = contextFactory.CreateContext();

        var query = from t in context.Table
            group t.Id by t.Value
            into tg
            select new
            {
                A = tg.Key, B = context.Table.Where(t => t.Value == tg.Max() * 6).Max(t => (int?)t.Id),
            };

        var orders = async
            ? await query.ToListAsync()
            : query.ToList();
    }

    [ConditionalTheory(Skip = "Snowflake does not support this feature (Unsupported subquery)")]
    [MemberData(nameof(IsAsyncData))]
    public override async Task Group_by_aggregate_in_subquery_projection_after_group_by(bool async)
    {
        var contextFactory = await InitializeAsync<SfContext27094>();
        await using var context = contextFactory.CreateContext();

        var query = from t in context.Table
            group t.Id by t.Value
            into tg
            select new
            {
                A = tg.Key,
                B = tg.Sum(),
                C = (from t in context.Table
                        group t.Id by t.Value
                        into tg2
                        select tg.Sum() + tg2.Sum()
                    ).OrderBy(e => 1).FirstOrDefault()
            };

        var orders = async
            ? await query.ToListAsync()
            : query.ToList();
    }

    protected class SfContext27094(DbContextOptions options) : Context27094(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Table>().ToTable(t => t.IsHybridTable());
        }
    }

    #endregion

    #region 27163

    public override async Task Group_by_multiple_aggregate_joining_different_tables(bool async)
    {
        var contextFactory = await InitializeAsync<SfContext27163>();
        await using var context = contextFactory.CreateContext();

        var query = context.Parents
            .GroupBy(x => new { })
            .Select(
                g => new
                {
                    Test1 = g
                        .Select(x => x.Child1.Value1)
                        .Distinct()
                        .Count(),
                    Test2 = g
                        .Select(x => x.Child2.Value2)
                        .Distinct()
                        .Count()
                });

        var orders = async
            ? await query.ToListAsync()
            : query.ToList();
    }

    public override async Task Group_by_multiple_aggregate_joining_different_tables_with_query_filter(bool async)
    {
        var contextFactory = await InitializeAsync<SfContext27163>();
        await using var context = contextFactory.CreateContext();

        var query = context.Parents
            .GroupBy(x => new { })
            .Select(
                g => new
                {
                    Test1 = g
                        .Select(x => x.ChildFilter1.Value1)
                        .Distinct()
                        .Count(),
                    Test2 = g
                        .Select(x => x.ChildFilter2.Value2)
                        .Distinct()
                        .Count()
                });

        var orders = async
            ? await query.ToListAsync()
            : query.ToList();
    }

    protected class SfContext27163(DbContextOptions options) : Context27163(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Parent>().ToTable(t => t.IsHybridTable());
            modelBuilder.Entity<Child1>().ToTable(t => t.IsHybridTable());
            modelBuilder.Entity<Child2>().ToTable(t => t.IsHybridTable());
            modelBuilder.Entity<ChildFilter1>().ToTable(t => t.IsHybridTable());
            modelBuilder.Entity<ChildFilter2>().ToTable(t => t.IsHybridTable());
        }
    }

    #endregion

    #region 26744

    [ConditionalTheory(Skip = "Snowflake does not support this feature (Unsupported subquery)")]
    [MemberData(nameof(IsAsyncData))]
    public override async Task Subquery_first_member_compared_to_null(bool async)
    {
        var contextFactory = await InitializeAsync<SfContext26744>(seed: c => c.Seed());
        await using var context = contextFactory.CreateContext();

        var query = context.Parents
            .Where(
                p => p.Children.Any(c => c.SomeNullableDateTime == null)
                     && p.Children.Where(c => c.SomeNullableDateTime == null)
                         .OrderBy(c => c.SomeInteger)
                         .First().SomeOtherNullableDateTime
                     != null)
            .Select(
                p => p.Children.Where(c => c.SomeNullableDateTime == null)
                    .OrderBy(c => c.SomeInteger)
                    .First().SomeOtherNullableDateTime);

        var result = async
            ? await query.ToListAsync()
            : query.ToList();

        Assert.Single(result);
    }

    public override async Task SelectMany_where_Select(bool async)
    {
        // Review SNOW-1852407
        var contextFactory = await InitializeAsync<SfContext26744>(seed: c => c.Seed());
        await using var context = contextFactory.CreateContext();

        var query = context.Parents
            .SelectMany(
                p => p.Children
                    .Where(c => c.SomeNullableDateTime == null)
                    .OrderBy(c => c.SomeInteger)
                    .Take(1))
            .Where(c => c.SomeOtherNullableDateTime != null)
            .Select(c => c.SomeNullableDateTime);

        var result = async
            ? await query.ToListAsync()
            : query.ToList();

        Assert.Single(result);
    }

    protected class SfContext26744(DbContextOptions options) : Context26744(options)
    {
        public new void Seed()
        {
            Add(
                new Parent26744
                {
                    Id = 1,
                    Children =
                    [
                        new Child26744
                            { Id = 1, SomeInteger = 1, SomeOtherNullableDateTime = new DateTime(2000, 11, 18) }
                    ]
                });

            Add(new Parent26744 { Id = 2, Children = [new Child26744 { Id = 2, SomeInteger = 1 }] });

            SaveChanges();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Parent26744>().ToTable(t => t.IsHybridTable());
            modelBuilder.Entity<Child26744>().ToTable(t => t.IsHybridTable());
        }
    }

    #endregion

    #region 27343

    public override async Task Flattened_GroupJoin_on_interface_generic(bool async)
    {
        var contextFactory = await InitializeAsync<SfContext27343>(seed: c => c.Seed());
        await using var context = contextFactory.CreateContext();

        var entitySet = context.Parents.AsQueryable<IDocumentType27343>();

        var query = from p in entitySet
            join c in context.Set<Child27343>()
                on p.Id equals c.Id into grouping
            from c in grouping.DefaultIfEmpty()
            select c;

        var result = async
            ? await query.ToListAsync()
            : query.ToList();

        Assert.Empty(result);
    }

    protected class SfContext27343(DbContextOptions options) : Context27343(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Parent27343>().ToTable(t => t.IsHybridTable());
            modelBuilder.Entity<Child27343>().ToTable(t => t.IsHybridTable());
        }
    }

    #endregion

    #region 28039

    public override async Task Pushdown_does_not_add_grouping_key_to_projection_when_distinct_is_applied(bool async)
    {
        var contextFactory = await InitializeAsync<SfContext28039>();
        await using var db = contextFactory.CreateContext();

        var queryResults = (from i in db.IndexData.Where(a => a.Parcel == "some condition")
                .Select(a => new SearchResult { ParcelNumber = a.Parcel, RowId = a.RowId })
            group i by new { i.ParcelNumber, i.RowId }
            into grp
            where grp.Count() == 1
            select grp.Key.ParcelNumber).Distinct();

        var jsonLookup = (from dcv in db.TableData.Where(a => a.TableId == 123)
            join wos in queryResults
                on dcv.ParcelNumber equals wos
            orderby dcv.ParcelNumber
            select dcv.JSON).Take(123456);

        var result = async
            ? await jsonLookup.ToListAsync()
            : jsonLookup.ToList();
    }

    protected class SfContext28039(DbContextOptions options) : Context28039(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<IndexData>().ToTable(t => t.IsHybridTable());
            modelBuilder.Entity<TableData>().ToTable(t => t.IsHybridTable());
        }
    }

    protected class SearchResult
    {
        public string ParcelNumber { get; set; }
        public int RowId { get; set; }
        public string DistinctValue { get; set; }
    }

    #endregion

    #region 31961

    public override async Task Filter_on_nested_DTO_with_interface_gets_simplified_correctly(bool async)
    {
        var contextFactory = await InitializeAsync<SfContext31961>();
        await using var context = contextFactory.CreateContext();

        var query = await context.Customers
            .Select(m => new CustomerDto31961()
            {
                Id = m.Id,
                CompanyId = m.CompanyId,
                Company = m.Company != null
                    ? new CompanyDto31961()
                    {
                        Id = m.Company.Id,
                        CompanyName = m.Company.CompanyName,
                        CountryId = m.Company.CountryId,
                        Country = new CountryDto31961()
                        {
                            Id = m.Company.Country.Id,
                            CountryName = m.Company.Country.CountryName,
                        },
                    }
                    : null,
            })
            .Where(m => m.Company.Country.CountryName == "COUNTRY")
            .ToListAsync();
    }

    protected class SfContext31961(DbContextOptions options) : Context31961(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Customer31961>().ToTable(t => t.IsHybridTable());
            modelBuilder.Entity<Company31961>().ToTable(t => t.IsHybridTable());
            modelBuilder.Entity<Country31961>().ToTable(t => t.IsHybridTable());
        }
    }

    #endregion

    #region 24657

    public override async Task Bool_discriminator_column_works(bool async)
    {
        // Review SNOW-1852407
        var contextFactory = await InitializeAsync<SfContext24657>(seed: c => c.Seed());
        await using var context = contextFactory.CreateContext();

        var query = context.Authors.Include(e => e.Blog);

        var authors = async
            ? await query.ToListAsync()
            : query.ToList();

        Assert.Equal(2, authors.Count);
    }

    protected class SfContext24657(DbContextOptions options) : Context24657(options)
    {
        public new void Seed()
        {
            Add(new Author { Id = 1, Blog = new DevBlog { Id = 1, Title = "Dev Blog", } });
            Add(new Author { Id = 2, Blog = new PhotoBlog { Id = 2, Title = "Photo Blog", } });

            SaveChanges();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Author>().ToTable(t => t.IsHybridTable());
            modelBuilder.Entity<Blog>().ToTable(t => t.IsHybridTable());
            modelBuilder.Entity<DevBlog>().ToTable(t => t.IsHybridTable());
            modelBuilder.Entity<PhotoBlog>().ToTable(t => t.IsHybridTable());
            base.OnModelCreating(modelBuilder);
        }
    }

    #endregion

    #region 21770

    [ConditionalTheory(Skip = "Disable due SNOW-1825765")]
    [MemberData(nameof(IsAsyncData))]
    public override async Task Comparing_enum_casted_to_byte_with_int_parameter(bool async)
    {
        var contextFactory = await InitializeAsync<SfContext21770>();
        await using var context = contextFactory.CreateContext();
        var bitterTaste = Taste.Bitter;
        var query = context.IceCreams.Where(i => i.Taste == (byte)bitterTaste);

        var bitterIceCreams = async
            ? await query.ToListAsync()
            : query.ToList();

        Assert.Single(bitterIceCreams);
    }

    [ConditionalTheory(Skip = "Disable due SNOW-1825765")]
    [MemberData(nameof(IsAsyncData))]
    public override async Task Comparing_enum_casted_to_byte_with_int_constant(bool async)
    {
        var contextFactory = await InitializeAsync<SfContext21770>();
        await using var context = contextFactory.CreateContext();
        var query = context.IceCreams.Where(i => i.Taste == (byte)Taste.Bitter);

        var bitterIceCreams = async
            ? await query.ToListAsync()
            : query.ToList();

        Assert.Single(bitterIceCreams);
    }

    [ConditionalTheory(Skip = "Disable due SNOW-1825765")]
    [MemberData(nameof(IsAsyncData))]
    public override async Task Comparing_byte_column_to_enum_in_vb_creating_double_cast(bool async)
    {
        var contextFactory = await InitializeAsync<SfContext21770>();
        await using var context = contextFactory.CreateContext();
        Expression<Func<Food, byte?>> memberAccess = i => i.Taste;
        var predicate = Expression.Lambda<Func<Food, bool>>(
            Expression.Equal(
                Expression.Convert(memberAccess.Body, typeof(int?)),
                Expression.Convert(
                    Expression.Convert(Expression.Constant(Taste.Bitter, typeof(Taste)), typeof(int)),
                    typeof(int?))),
            memberAccess.Parameters);
        var query = context.Food.Where(predicate);

        var bitterFood = async
            ? await query.ToListAsync()
            : query.ToList();
    }

    [ConditionalTheory(Skip = "Disable due SNOW-1825765")]
    [MemberData(nameof(IsAsyncData))]
    public override async Task Null_check_removal_in_ternary_maintain_appropriate_cast(bool async)
    {
        var contextFactory = await InitializeAsync<SfContext21770>();
        await using var context = contextFactory.CreateContext();

        var query = from f in context.Food
            select new { Bar = f.Taste != null ? (Taste)f.Taste : (Taste?)null };

        var bitterFood = async
            ? await query.ToListAsync()
            : query.ToList();
    }

    protected class SfContext21770(DbContextOptions options) : Context21770(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<IceCream>().ToTable(t => t.IsHybridTable());
            modelBuilder.Entity<Food>().ToTable(t => t.IsHybridTable());
            base.OnModelCreating(modelBuilder);
        }
    }

    #endregion

    #region 26433

    public override async Task Count_member_over_IReadOnlyCollection_works(bool async)
    {
        // Review SNOW-1852407
        var contextFactory = await InitializeAsync<SfContext26433>(seed: c => c.Seed());
        await using var context = contextFactory.CreateContext();

        var query = context.Authors
            .Select(a => new { BooksCount = a.Books.Count });

        var authors = async
            ? await query.ToListAsync()
            : query.ToList();

        Assert.Equal(3, Assert.Single(authors).BooksCount);
    }

    protected class SfContext26433(DbContextOptions options) : Context26433(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Author26433>().ToTable(t => t.IsHybridTable());
            modelBuilder.Entity<Book26433>().ToTable(t => t.IsHybridTable());
        }

        public new void Seed()
        {
            base.Add(
                new Author26433
                {
                    AuthorId = 1,
                    FirstName = "William",
                    LastName = "Shakespeare",
                    Books = new List<Book26433>
                    {
                        new() { BookId = 1, Title = "Hamlet" },
                        new() { BookId = 2, Title = "Othello" },
                        new() { BookId = 3, Title = "MacBeth" }
                    }
                });

            SaveChanges();
        }
    }

    #endregion

    #region 26428

    public override async Task IsDeleted_query_filter_with_conversion_to_int_works(bool async)
    {
        // Review SNOW-1852407
        var contextFactory = await InitializeAsync<SfContext26428>(seed: c => c.Seed());
        await using var context = contextFactory.CreateContext();

        var query = context.Suppliers.Include(s => s.Location).OrderBy(s => s.Name);

        var suppliers = async
            ? await query.ToListAsync()
            : query.ToList();

        Assert.Equal(4, suppliers.Count);
        Assert.Single(suppliers.Where(e => e.Location != null));
    }

    protected class SfContext26428(DbContextOptions options) : Context26428(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Supplier>().ToTable(t => t.IsHybridTable());
            modelBuilder.Entity<Location>().ToTable(t => t.IsHybridTable());
        }

        public new void Seed()
        {
            var activeAddress = new Location { LocationId = new Guid(), Address = "Active address", IsDeleted = false };
            var deletedAddress = new Location
                { LocationId = new Guid(), Address = "Deleted address", IsDeleted = true };

            var activeSupplier1 = new Supplier
            {
                SupplierId = new Guid(),
                Name = "Active supplier 1",
                IsDeleted = false,
                Location = activeAddress
            };
            var activeSupplier2 = new Supplier
            {
                SupplierId = new Guid(),
                Name = "Active supplier 2",
                IsDeleted = false,
                Location = deletedAddress
            };
            var activeSupplier3 = new Supplier
            {
                SupplierId = new Guid(),
                Name = "Active supplier 3",
                IsDeleted = false,
                Location = deletedAddress
            };
            var deletedSupplier = new Supplier
            {
                SupplierId = new Guid(),
                Name = "Deleted supplier",
                IsDeleted = false,
                Location = deletedAddress
            };

            AddRange(activeAddress, deletedAddress);
            AddRange(activeSupplier1, activeSupplier2, activeSupplier3, deletedSupplier);

            SaveChanges();
        }
    }

    #endregion
    
    #region CustomCases

    [ConditionalFact]
    public virtual async Task InsertDataWithIdentityAndDateOnlyValues()
    {
        var contextFactory = await InitializeAsync<SfContextCustomCases>(seed: c => c.Seed());
        await using var context = contextFactory.CreateContext();

        var query = context.EntitiesWithIdentityAndDate;

        var entities =  await query.ToListAsync();

        var x = Assert.Single(entities);
        Assert.Equal(new DateTime(2015,1,1), x.Value);
    }
    
    [ConditionalFact]
    public virtual async Task InsertDataWithIdentityAndDateTimesValues()
    {
        var contextFactory = await InitializeAsync<SfContextCustomCases>(seed: c => c.Seed());
        await using var context = contextFactory.CreateContext();

        var query = context.EntitiesWithIdentityAndDateTime;

        var entities =  await query.ToListAsync();

        var x = Assert.Single(entities);
        Assert.Equal(new DateTime(2015,1,1,11,15,34), x.Value);
    }
    
    [ConditionalFact]
    public virtual async Task InsertDataWithIdentityAndDateTimeOffsetValues()
    {
        var contextFactory = await InitializeAsync<SfContextCustomCases>(seed: c => c.Seed());
        await using var context = contextFactory.CreateContext();

        var query = context.EntitiesWithIdentityAndDateTimeOffset;

        var entities =  await query.ToListAsync();

        var x = Assert.Single(entities);
        Assert.Equal(new DateTimeOffset(2015,1,1,11,15,34, new TimeSpan(6, 0, 0)), x.Value);
    }

    protected class SfContextCustomCases(DbContextOptions options) : DbContext(options)
    {
        public DbSet<EntityWithIdentityAndDate> EntitiesWithIdentityAndDate { get; set; }
        public DbSet<EntityWithIdentityAndDateTime> EntitiesWithIdentityAndDateTime { get; set; }
        public DbSet<EntityWithIdentityAndDateTimeOffset> EntitiesWithIdentityAndDateTimeOffset { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<EntityWithIdentityAndDate>(entity =>
            {
                entity.ToTable("TABLE_WITH_IDENTITY_AND_DATE", t => t.IsHybridTable());
                entity.Property(x => x.Id).HasColumnName("TABLE_ID").UseIdentityColumn(500).ValueGeneratedOnAdd();
                entity.Property(x => x.Value).HasColumnName("TABLE_VALUE").HasColumnType("DATE");
                entity.Property(x => x.Name).HasColumnName("NAME").HasMaxLength(200);
            });
            modelBuilder.Entity<EntityWithIdentityAndDateTime>(entity =>
            {
                entity.ToTable("TABLE_WITH_IDENTITY_AND_DATETIME", t => t.IsHybridTable());
                entity.Property(x => x.Id).HasColumnName("TABLE_ID").UseIdentityColumn(500).ValueGeneratedOnAdd();
                entity.Property(x => x.Value).HasColumnName("TABLE_VALUE").HasColumnType("TIMESTAMP_NTZ");
                entity.Property(x => x.Name).HasColumnName("NAME").HasMaxLength(200);
            });
            modelBuilder.Entity<EntityWithIdentityAndDateTimeOffset>(entity =>
            {
                entity.ToTable("TABLE_WITH_IDENTITY_AND_DATETIMEOFFSET", t => t.IsHybridTable());
                entity.Property(x => x.Id).HasColumnName("TABLE_ID").UseIdentityColumn(500).ValueGeneratedOnAdd();
                entity.Property(x => x.Value).HasColumnName("TABLE_VALUE").HasColumnType("TIMESTAMP_TZ");
                entity.Property(x => x.Name).HasColumnName("NAME").HasMaxLength(200);
            });
        }
        
        public new void Seed()
        {
            var entityDateTime = new EntityWithIdentityAndDateTime() { Name = "TestEntity1", Value = new DateTime(2015,1,1,11,15,34) };
            var entityDateOnly = new EntityWithIdentityAndDate() { Name = "TestEntity1", Value = new DateTime(2015,1,1) };
            var entityDateTimeOffset = new EntityWithIdentityAndDateTimeOffset() { Name = "TestEntity1", Value = new DateTimeOffset(2015,1,1,11,15,34, new TimeSpan(6, 0,0)) };
            AddRange(entityDateOnly);
            AddRange(entityDateTime);
            AddRange(entityDateTimeOffset);
            SaveChanges();
        }
        
    }
    
    protected class EntityWithIdentityAndSpecialType<T>
    {
        public int Id { get; set; }
        public T? Value { get; set; }
        public string? Name { get; set; }
    }
    
    protected class EntityWithIdentityAndDateTime : EntityWithIdentityAndSpecialType<DateTime>
    {
    }
    
    protected class EntityWithIdentityAndDateTimeOffset : EntityWithIdentityAndSpecialType<DateTimeOffset>
    {
    }
    
    protected class EntityWithIdentityAndDate : EntityWithIdentityAndSpecialType<DateTime>
    {
    }


    #endregion

    protected override ITestStoreFactory TestStoreFactory
        => SnowflakeTestStoreFactory.Instance;
}