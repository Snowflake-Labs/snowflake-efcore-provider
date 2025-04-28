using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;

namespace Snowflake.EntityFrameworkCore.TestUtilities;

public class SnowflakeTestStoreFactory : RelationalTestStoreFactory
{
    public static SnowflakeTestStoreFactory Instance { get; } = new();

    protected SnowflakeTestStoreFactory()
    {
    }

    public override TestStore Create(string storeName)
        => SnowflakeTestStore.Create(storeName);

    public override TestStore GetOrCreate(string storeName)
        => SnowflakeTestStore.GetOrCreate(storeName);

    public override IServiceCollection AddProviderServices(IServiceCollection serviceCollection)
        => serviceCollection.AddEntityFrameworkSnowflake();
}
