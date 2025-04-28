using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Snowflake.Data.Client;
using Snowflake.EntityFrameworkCore.Diagnostics.Internal;

namespace Snowflake.EntityFrameworkCore.TestUtilities;

public class SnowflakeTestHelpers : RelationalTestHelpers
{
    protected SnowflakeTestHelpers()
    {
    }

    public static SnowflakeTestHelpers Instance { get; } = new();

    public override IServiceCollection AddProviderServices(IServiceCollection services)
        => services.AddEntityFrameworkSnowflake();

    public override DbContextOptionsBuilder UseProviderOptions(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSnowflake(new SnowflakeDbConnection("Database=DummyDatabase"));

    public override LoggingDefinitions LoggingDefinitions { get; } = new SnowflakeLoggingDefinitions();
}
