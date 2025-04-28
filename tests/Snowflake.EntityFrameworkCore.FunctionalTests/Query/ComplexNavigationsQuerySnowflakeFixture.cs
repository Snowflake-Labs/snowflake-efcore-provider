using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Snowflake.EntityFrameworkCore.TestUtilities;

namespace Snowflake.EntityFrameworkCore.FunctionalTests.Query;

/// <inheritdoc />
public class ComplexNavigationsQuerySnowflakeFixture : ComplexNavigationsQueryRelationalFixtureBase
{
  protected override ITestStoreFactory TestStoreFactory => SnowflakeTestStoreFactory.Instance;
}