using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Snowflake.EntityFrameworkCore.FunctionalTests.TestModels.ComplexTypeData;
using Snowflake.EntityFrameworkCore.TestUtilities;

namespace Snowflake.EntityFrameworkCore.FunctionalTests.Query;

public class ComplexTypeQueryStandardTableSnowflakeTest(
    ComplexTypeQueryStandardTableSnowflakeTest.ComplexTypeQueryStandardTableSnowflakeFixture fixture)
    : ComplexTypeQueryRelationalSnowflakeBaseTest<
        ComplexTypeQueryStandardTableSnowflakeTest.ComplexTypeQueryStandardTableSnowflakeFixture>(fixture)
{

    public class ComplexTypeQueryStandardTableSnowflakeFixture : ComplexTypeQueryRelationalFixtureBase
    {
        protected override string StoreName => "ComplexTypeStandardTableQueryTest";
        
        protected override ITestStoreFactory TestStoreFactory
            => SnowflakeTestStoreFactory.Instance;
    }
}