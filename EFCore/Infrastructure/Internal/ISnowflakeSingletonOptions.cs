namespace Snowflake.EntityFrameworkCore.Infrastructure.Internal;

using Microsoft.EntityFrameworkCore.Infrastructure;

public interface ISnowflakeSingletonOptions : ISingletonOptions
{
    
    int CompatibilityLevel { get; }

    
    int? CompatibilityLevelWithoutDefault { get; }

    /// <summary>
    ///     Whether reverse null ordering is enabled.
    /// </summary>
    bool ReverseNullOrderingEnabled { get; }
}
