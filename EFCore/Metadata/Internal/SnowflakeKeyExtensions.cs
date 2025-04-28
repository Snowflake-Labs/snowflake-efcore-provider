namespace Snowflake.EntityFrameworkCore.Metadata.Internal;

using Microsoft.EntityFrameworkCore.Metadata;

public static class SnowflakeKeyExtensions
{   
    public static bool AreCompatibleForSnowflake(
        this IReadOnlyKey key,
        IReadOnlyKey duplicateKey,
        in StoreObjectIdentifier storeObject,
        bool shouldThrow)
    {
        return true;
    }

}