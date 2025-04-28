using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Snowflake.EntityFrameworkCore.TestUtilities;

public static class SnowflakeDatabaseFacadeExtensions
{
    public static void EnsureClean(this DatabaseFacade databaseFacade)
        => databaseFacade.CreateExecutionStrategy()
            .Execute(databaseFacade, database => new SnowflakeDatabaseCleaner().Clean(database));
}
