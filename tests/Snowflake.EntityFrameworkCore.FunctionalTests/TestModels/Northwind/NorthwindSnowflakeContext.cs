using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;

namespace Snowflake.EntityFrameworkCore.FunctionalTests.TestModels.Northwind;

public class NorthwindSnowflakeContext(DbContextOptions options) : NorthwindRelationalContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Employee>().ToTable(employee => employee.IsHybridTable());

        modelBuilder.Entity<Customer>().ToTable(customer => customer.IsHybridTable());

        modelBuilder.Entity<Product>().ToTable(product => product.IsHybridTable());

        modelBuilder.Entity<Order>().ToTable(order => order.IsHybridTable());

        modelBuilder.Entity<OrderDetail>().ToTable(orderDetail => orderDetail.IsHybridTable());

        modelBuilder.Entity<CustomerQuery>().ToSqlQuery(
            "SELECT\n    \"CustomerID\",\n    \"Address\",\n    \"City\",\n    \"CompanyName\",\n    \"ContactName\",\n    \"ContactTitle\",\n    \"Country\",\n    \"Fax\",\n    \"Phone\",\n    \"PostalCode\",\n    \"Region\"\nFROM\n    \"Customers\"");
    }
}