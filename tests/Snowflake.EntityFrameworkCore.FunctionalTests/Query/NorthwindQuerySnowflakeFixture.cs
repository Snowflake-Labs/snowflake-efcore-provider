using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Snowflake.EntityFrameworkCore.FunctionalTests.TestModels.Northwind;
using Snowflake.EntityFrameworkCore.FunctionalTests.TestUtilities;

namespace Snowflake.EntityFrameworkCore.FunctionalTests.Query;

public class NorthwindQuerySnowflakeFixture<TModelCustomizer> : NorthwindQueryRelationalFixture<TModelCustomizer>
    where TModelCustomizer : IModelCustomizer, new()
{
    protected override ITestStoreFactory TestStoreFactory
        => SnowflakeNorthwindTestStoreFactory.Instance;

    protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
    {
        base.OnModelCreating(modelBuilder, context);

        modelBuilder.Entity<Customer>(
            b =>
            {
                b.Property(customer => customer.CustomerID).HasColumnType("nchar(5)");
                b.Property(customer => customer.CompanyName).HasMaxLength(40);
                b.Property(customer => customer.ContactName).HasMaxLength(30);
                b.Property(customer => customer.ContactTitle).HasColumnType("national character varying(30)");
            });

        modelBuilder.Entity<Employee>(
            b =>
            {
                b.Property(employee => employee.EmployeeID).HasColumnType("int");
                b.Property(employee => employee.ReportsTo).HasColumnType("int");
            });

        modelBuilder.Entity<Order>(
            b =>
            {
                b.Property(order => order.EmployeeID).HasColumnType("int");
                b.Property(order => order.OrderDate).HasColumnType("datetime");
            });

        modelBuilder.Entity<OrderDetail>()
            .Property(orderDetailed => orderDetailed.UnitPrice)
            .HasColumnType("money");

        modelBuilder.Entity<Product>(
            b =>
            {
                b.Property(product => product.UnitPrice).HasColumnType("money");
                b.Property(product => product.UnitsInStock).HasColumnType("smallint");
                b.Property(product => product.ProductName).HasMaxLength(40);
            });

        modelBuilder.Entity<MostExpensiveProduct>()
            .Property(mostExpensiveProduct => mostExpensiveProduct.UnitPrice)
            .HasColumnType("money");
    }

    protected override Type ContextType
        => typeof(NorthwindSnowflakeContext);
}