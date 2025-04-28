using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestModels.ComplexTypeModel;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Snowflake.EntityFrameworkCore.FunctionalTests.TestModels.ComplexTypeData;
using Snowflake.EntityFrameworkCore.TestUtilities;

namespace Snowflake.EntityFrameworkCore.FunctionalTests.Query;

public class ComplexTypeQueryHybridTablesSnowflakeTest : ComplexTypeQueryRelationalSnowflakeBaseTest<ComplexTypeQueryHybridTablesSnowflakeTest.ComplexTypeQueryRelationalSnowflakeFixture>
{

    [ConditionalTheory(Skip = "Skip due not supported optional foreign keys on Snowflake")]
    [MemberData("IsAsyncData", new object[] {})]
    public override async Task
        Filter_on_required_property_inside_required_struct_complex_type_on_required_navigation(bool async)
    {
    }
    
    [ConditionalTheory(Skip = "Skip due not supported optional foreign keys on Snowflake")]
    [MemberData("IsAsyncData", new object[] {})]
    public override async Task Filter_on_required_property_inside_required_struct_complex_type_on_optional_navigation(
        bool async)
    {
    }

    public class ComplexTypeQueryRelationalSnowflakeFixture : ComplexTypeQueryRelationalFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => SnowflakeTestStoreFactory.Instance;

        private SnowflakeComplexTypeData? _expectedData;

        public ComplexTypeQueryRelationalSnowflakeFixture()
        {
            this.OverrideGetExpectedData();
        }
        
        protected override void Seed(PoolableDbContext context)
        {
            SnowflakeComplexTypeData.Seed(context);
        }

        public void OverrideGetExpectedData()
        {
            var baseData = base.GetExpectedData() as ComplexTypeData;
            var customerGroup = baseData.Set<CustomerGroup>().FirstOrDefault(c => c.Id == 3);
            customerGroup.OptionalCustomer = baseData.Set<Customer>().FirstOrDefault(c => c.Id == 1);
        }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            modelBuilder.Entity<Customer>(
                cb =>
                {
                    cb.ToTable(t => t.IsHybridTable());
                });

            modelBuilder.Entity<CustomerGroup>(
                cgb =>
                {
                    cgb.ToTable(t => t.IsHybridTable());
                });

            modelBuilder.Entity<ValuedCustomer>(
                cb =>
                {
                    cb.ToTable(t => t.IsHybridTable());
                });

            modelBuilder.Entity<ValuedCustomerGroup>(
                cgb =>
                {
                    cgb.ToTable(t => t.IsHybridTable());
                });
            base.OnModelCreating(modelBuilder, context);
        }
    }

    public ComplexTypeQueryHybridTablesSnowflakeTest(ComplexTypeQueryRelationalSnowflakeFixture fixture) : base(fixture)
    {
    }
}