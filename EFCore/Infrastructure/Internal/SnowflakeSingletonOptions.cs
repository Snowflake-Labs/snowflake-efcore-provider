namespace Snowflake.EntityFrameworkCore.Infrastructure.Internal;

using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;

public class SnowflakeSingletonOptions : ISnowflakeSingletonOptions
{
    
    public virtual int CompatibilityLevel { get; private set; } = 1;

    
    public virtual int? CompatibilityLevelWithoutDefault { get; private set; }


    /// <inheritdoc />
    public virtual bool ReverseNullOrderingEnabled { get;  private set; }

    
    public virtual void Initialize(IDbContextOptions options)
    {
        var snowflakeOptions = GetOrCreateExtension(options);
        ReverseNullOrderingEnabled = snowflakeOptions.ReverseNullOrdering;

        // var snowflakeOptions = options.FindExtension<SnowflakeOptionsExtension>();
        // if (snowflakeOptions != null)
        // {
        //     CompatibilityLevel = 1;
        //     CompatibilityLevelWithoutDefault = 1;
        // }
    }

    
    public virtual void Validate(IDbContextOptions options)
    {
        var snowflakeOptions = GetOrCreateExtension(options);
        if (ReverseNullOrderingEnabled != snowflakeOptions.ReverseNullOrdering)
        {
            throw new InvalidOperationException(
                CoreStrings.SingletonOptionChanged(
                    nameof(SnowflakeDbContextOptionsBuilder.ReverseNullOrdering),
                    nameof(DbContextOptionsBuilder.UseInternalServiceProvider)));
        }
        // var snowflakeOptions = options.FindExtension<SnowflakeOptionsExtension>();
        //
        // if (snowflakeOptions != null
        //     && (CompatibilityLevelWithoutDefault != snowflakeOptions.CompatibilityLevelWithoutDefault
        //         || CompatibilityLevel != snowflakeOptions.CompatibilityLevel))
        // {
        //     throw new InvalidOperationException(
        //         CoreStrings.SingletonOptionChanged(
        //             nameof(SqlServerDbContextOptionsExtensions.UseSqlServer),
        //             nameof(DbContextOptionsBuilder.UseInternalServiceProvider)));
        // }
    }
    
    /// <summary>
    /// Gets the extension that stores the options for this context.
    /// </summary>
    private static SnowflakeOptionsExtension GetOrCreateExtension(IDbContextOptions options)
        => options.FindExtension<SnowflakeOptionsExtension>()
           ?? new SnowflakeOptionsExtension();
}
