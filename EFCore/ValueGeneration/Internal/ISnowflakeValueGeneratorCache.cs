namespace Snowflake.EntityFrameworkCore.ValueGeneration.Internal;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Microsoft.Extensions.DependencyInjection;

/// <remarks>
///     The service lifetime is <see cref="ServiceLifetime.Singleton" />. This means a single instance
///     is used by many <see cref="DbContext" /> instances. The implementation must be thread-safe.
///     This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped" />.
/// </remarks>
public interface ISnowflakeValueGeneratorCache : IValueGeneratorCache
{
    SnowflakeSequenceValueGeneratorState GetOrAddSequenceState(
        IProperty property,
        IRelationalConnection connection);
}
