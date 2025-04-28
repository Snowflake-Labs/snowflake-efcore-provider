using EFCore.Tests.Common;

namespace Snowflake.EntityFrameworkCore.TestUtilities;

public static class TestEnvironment
{

    // TODO LOAD FROM parameters.json
    public static string DefaultConnection { get; } = TestUtil.GetConnectionStringFromParameters("parameters.json");
    public static bool IsConfigured { get; } = !string.IsNullOrEmpty(DefaultConnection);
}
