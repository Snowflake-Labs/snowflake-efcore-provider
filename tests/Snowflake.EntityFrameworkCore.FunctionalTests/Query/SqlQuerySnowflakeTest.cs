using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Snowflake.Data.Client;
using Snowflake.Data.Core;
using Snowflake.EntityFrameworkCore.FunctionalTests.TestModels.Northwind;
using Xunit.Abstractions;

namespace Snowflake.EntityFrameworkCore.FunctionalTests.Query;

public class SqlQuerySnowflakeTest : SqlQueryTestBase<NorthwindQuerySnowflakeFixture<NoopModelCustomizer>>
{
    public SqlQuerySnowflakeTest(NorthwindQuerySnowflakeFixture<NoopModelCustomizer> fixture,
        ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
        CreateContext();
    }

    [ConditionalTheory(Skip = "Skipped due SNOW-1850989")]
    [MemberData(nameof(IsAsyncData))]
    public override Task Bad_data_error_handling_invalid_cast(bool async) =>
        Task.CompletedTask;

    [ConditionalTheory(Skip = "Skipped due SNOW-1850989")]
    [MemberData(nameof(IsAsyncData))]
    public override Task Bad_data_error_handling_invalid_cast_key(bool async) =>
        Task.CompletedTask;

    [ConditionalTheory(Skip = "Skipped due SNOW-1850989")]
    [MemberData(nameof(IsAsyncData))]
    public override Task Bad_data_error_handling_invalid_cast_no_tracking(bool async) =>
        Task.CompletedTask;

    [ConditionalTheory(Skip = "Skipped due SNOW-1850989")]
    [MemberData(nameof(IsAsyncData))]
    public override Task Bad_data_error_handling_invalid_cast_projection(bool async) =>
        Task.CompletedTask;

    [ConditionalTheory(Skip = "Duplicated test (SqlQuery_queryable_multiple_composed_with_parameters_and_closure_parameters_interpolated)")]
    [MemberData(nameof(IsAsyncData))]
    public override Task SqlQueryRaw_queryable_multiple_composed_with_parameters_and_closure_parameters(bool async) =>
        Task.CompletedTask;

    [ConditionalTheory(Skip = "Skipped due SNOW-1851096")]
    [MemberData(nameof(IsAsyncData))]
    public override Task SqlQueryRaw_composed_with_common_table_expression(bool async) => Task.CompletedTask;

    [ConditionalTheory(Skip = "Skipped due SNOW-1851041")]
    [MemberData(nameof(IsAsyncData))]
    public override Task SqlQueryRaw_queryable_with_null_parameter(bool async) => Task.CompletedTask;

    public override async Task
        SqlQuery_queryable_multiple_composed_with_parameters_and_closure_parameters_interpolated(bool async)
    {
        // Overridden because you cannot use with queries and nameless parameters.
        var city = "London";
        var startDate = new DateTime(1997, 1, 1);
        var endDate = new DateTime(1998, 1, 1);
        var context = Fixture.CreateContext();

        var query = from c in context.Database.SqlQueryRaw<UnmappedCustomer>(
                NormalizeDelimitersInRawString("SELECT * FROM [Customers] WHERE [City] = :city"),
                CreateDbParameter("city", city))
            from o in context.Database.SqlQueryRaw<UnmappedOrder>(
                NormalizeDelimitersInRawString(
                    "SELECT * FROM [Orders] WHERE [OrderDate] BETWEEN :startDate AND :endDate"),
                CreateDbParameter("startDate", startDate),
                CreateDbParameter("endDate", endDate))
            where c.CustomerID == o.CustomerID
            select new { c, o };

        await AssertQuery(
            async,
            _ => query,
            ss => from c in ss.Set<Customer>().Where(x => x.City == city)
                from o in ss.Set<Order>().Where(x => x.OrderDate >= startDate && x.OrderDate <= endDate)
                where c.CustomerID == o.CustomerID
                select new { c = UnmappedCustomer.FromCustomer(c), o = UnmappedOrder.FromOrder(o) },
            elementSorter: e => (e.c.CustomerID, e.o.OrderID),
            elementAsserter: (l, r) =>
            {
                AssertUnmappedCustomers(l.c, r.c);
                AssertUnmappedOrders(l.o, r.o);
            });

        city = "Berlin";
        startDate = new DateTime(1998, 4, 1);
        endDate = new DateTime(1998, 5, 1);

        await AssertQuery(
            async,
            _ => from c in context.Database.SqlQueryRaw<UnmappedCustomer>(
                    NormalizeDelimitersInRawString("SELECT * FROM [Customers] WHERE [City] = :city"),
                    CreateDbParameter("city", city))
                from o in context.Database.SqlQueryRaw<UnmappedOrder>(
                    NormalizeDelimitersInRawString(
                        $"SELECT * FROM [Orders] WHERE [OrderDate] BETWEEN :startDate AND :endDate"),
                    CreateDbParameter("startDate", startDate),
                    CreateDbParameter("endDate", endDate))
                where c.CustomerID == o.CustomerID
                select new { c, o },
            ss => from c in ss.Set<Customer>().Where(x => x.City == city)
                from o in ss.Set<Order>().Where(x => x.OrderDate >= startDate && x.OrderDate <= endDate)
                where c.CustomerID == o.CustomerID
                select new { c = UnmappedCustomer.FromCustomer(c), o = UnmappedOrder.FromOrder(o) },
            elementSorter: e => (e.c.CustomerID, e.o.OrderID),
            elementAsserter: (l, r) =>
            {
                AssertUnmappedCustomers(l.c, r.c);
                AssertUnmappedOrders(l.o, r.o);
            });
    }

    public override async Task Multiple_occurrences_of_SqlQuery_with_db_parameter_adds_parameter_only_once(bool async)
    {
        // Overridden because you cannot combine nameless parameters with named parameters in Snowflake
        await using var context = CreateContext();
        const string city = "Seattle";
        var qqlQuery = context.Database.SqlQueryRaw<UnmappedCustomer>(
            NormalizeDelimitersInRawString(@"SELECT * FROM [Customers] WHERE [City] = :city"),
            CreateDbParameter("city", city));

        var query = qqlQuery.Intersect(qqlQuery);

        var actual = async
            ? await query.ToArrayAsync()
            : query.ToArray();

        Assert.Single(actual);
    }

    public override Task SqlQueryRaw_in_subquery_with_positional_dbParameter_without_name(bool async)
    {
        // Overridden because you cannot combine nameless parameters with named parameters in Snowflake
        var context = Fixture.CreateContext();

        return AssertQuery(
            async,
            _ => context.Database.SqlQueryRaw<UnmappedOrder>(
                NormalizeDelimitersInRawString("SELECT * FROM [Orders]")).Where(
                o => context.Database.SqlQueryRaw<UnmappedCustomer>(
                        NormalizeDelimitersInRawString(@"SELECT * FROM [Customers] WHERE [City] = :city"),
                        CreateDbParameter("city", "London"))
                    .Select(c => c.CustomerID)
                    .Contains(o.CustomerID)),
            ss => ss.Set<Order>().Select(e => UnmappedOrder.FromOrder(e)).Where(
                o => ss.Set<Customer>().Select(e => UnmappedCustomer.FromCustomer(e)).Where(x => x.City == "London")
                    .Select(c => c.CustomerID)
                    .Contains(o.CustomerID)),
            elementSorter: e => e.OrderID,
            elementAsserter: AssertUnmappedOrders);
    }

    public override async Task SqlQueryRaw_queryable_composed_compiled_with_nameless_DbParameter(bool async)
    {
        // Overridden because SnowflakeDbParameter does not support nameless parameters
        await using var context = CreateContext();

        var query = context.Database.SqlQueryRaw<UnmappedCustomer>(
            NormalizeDelimitersInRawString("SELECT * FROM [Customers] WHERE [CustomerID] = :id"),
            CreateDbParameter("id", "CONSH"));

        var result = async
            ? await query.ToArrayAsync()
            : query.ToArray();

        Assert.Single(result);
    }

    public override async Task SqlQueryRaw_queryable_composed_compiled_with_parameter(bool async)
    {
        // Overridden because SnowflakeDbParameter does not support nameless parameters
        await using var context = CreateContext();

        var query = context.Database.SqlQueryRaw<UnmappedCustomer>(
            NormalizeDelimitersInRawString("SELECT * FROM [Customers] WHERE [CustomerID] = {0}"),
            "CONSH");

        var result = async
            ? await query.ToArrayAsync()
            : query.ToArray();

        Assert.Single(result);
    }

    public override Task SqlQueryRaw_in_subquery_with_positional_dbParameter_with_name(bool async)
    {
        // Overridden because SnowflakeDbParameter does not support nameless parameters
        var context = Fixture.CreateContext();

        return AssertQuery(
            async,
            _ => context.Database.SqlQueryRaw<UnmappedOrder>(
                NormalizeDelimitersInRawString("SELECT * FROM [Orders]")).Where(
                o => context.Database.SqlQueryRaw<UnmappedCustomer>(
                        NormalizeDelimitersInRawString(@"SELECT * FROM [Customers] WHERE [City] = :city"),
                        CreateDbParameter("@city", "London"))
                    .Select(c => c.CustomerID)
                    .Contains(o.CustomerID)),
            ss => ss.Set<Order>().Select(e => UnmappedOrder.FromOrder(e)).Where(
                o => ss.Set<Customer>().Select(e => UnmappedCustomer.FromCustomer(e)).Where(x => x.City == "London")
                    .Select(c => c.CustomerID)
                    .Contains(o.CustomerID)),
            elementSorter: e => e.OrderID,
            elementAsserter: AssertUnmappedOrders);
    }

    private static void AssertUnmappedOrders(UnmappedOrder l, UnmappedOrder r)
    {
        Assert.Equal(l.OrderID, r.OrderID);
        Assert.Equal(l.CustomerID, r.CustomerID);
        Assert.Equal(l.EmployeeID, r.EmployeeID);
        Assert.Equal(l.OrderDate, r.OrderDate);
        Assert.Equal(l.RequiredDate, r.RequiredDate);
        Assert.Equal(l.ShippedDate, r.ShippedDate);
        Assert.Equal(l.ShipVia, r.ShipVia);
        Assert.Equal(l.Freight, r.Freight);
        Assert.Equal(l.ShipName, r.ShipName);
        Assert.Equal(l.ShipAddress, r.ShipAddress);
        Assert.Equal(l.ShipRegion, r.ShipRegion);
        Assert.Equal(l.ShipPostalCode, r.ShipPostalCode);
        Assert.Equal(l.ShipCountry, r.ShipCountry);
    }

    private static void AssertUnmappedCustomers(UnmappedCustomer l, UnmappedCustomer r)
    {
        Assert.Equal(l.CustomerID, r.CustomerID);
        Assert.Equal(l.CompanyName, r.CompanyName);
        Assert.Equal(l.ContactName, r.ContactName);
        Assert.Equal(l.ContactTitle, r.ContactTitle);
        Assert.Equal(l.City, r.City);
        Assert.Equal(l.Region, r.Region);
        Assert.Equal(l.Zip, r.Zip);
        Assert.Equal(l.Country, r.Country);
        Assert.Equal(l.Phone, r.Phone);
        Assert.Equal(l.Fax, r.Fax);
    }

    protected override DbParameter CreateDbParameter(string name, object value)
        => new SnowflakeDbParameter { ParameterName = name.Replace("@", ""), Value = value };

    protected new NorthwindSnowflakeContext CreateContext()
        => (NorthwindSnowflakeContext)Fixture.CreateContext();
}