using Microsoft.EntityFrameworkCore.TestUtilities;
using Snowflake.EntityFrameworkCore.TestUtilities;

namespace Snowflake.EntityFrameworkCore.FunctionalTests.TestUtilities;

public class SnowflakeNorthwindTestStoreFactory : SnowflakeTestStoreFactory
{
    public const string Name = "Northwind";
    public static readonly string NorthwindConnectionString = SnowflakeTestStore.CreateConnectionString(Name);
    public new static SnowflakeNorthwindTestStoreFactory Instance { get; } = new();

    protected SnowflakeNorthwindTestStoreFactory()
    {
    }

    public override TestStore GetOrCreate(string storeName)
        => SnowflakeTestStore.GetOrCreateWithScriptPath(storeName, "Northwind.sql");
}