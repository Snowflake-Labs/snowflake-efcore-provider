using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Snowflake.EntityFrameworkCore.Infrastructure.Internal;

// ReSharper disable once CheckNamespace
namespace Snowflake.EntityFrameworkCore;

/// <summary>
///     Snowflake specific extension methods for <see cref="DbContext.Database" />.
/// </summary>
public static class SnowflakeDatabaseFacadeExtensions
{
    /// <summary>
    ///     Returns <see langword="true" /> if the database provider currently in use is the Snowflake provider.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         This method can only be used after the <see cref="DbContext" /> has been configured because
    ///         it is only then that the provider is known. This means that this method cannot be used
    ///         in <see cref="DbContext.OnConfiguring" /> because this is where application code sets the
    ///         provider to use as part of configuring the context.
    ///     </para>
    /// </remarks>
    /// <param name="database">The facade from <see cref="DbContext.Database" />.</param>
    /// <returns><see langword="true" /> if Snowflake is being used; <see langword="false" /> otherwise.</returns>
    public static bool IsSnowflake(this DatabaseFacade database)
        => database.ProviderName == typeof(SnowflakeOptionsExtension).Assembly.GetName().Name;
}
