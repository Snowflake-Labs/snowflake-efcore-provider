using System.Collections.Concurrent;

namespace Snowflake.EntityFrameworkCore.ValueGeneration.Internal;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Utilities;

public class SnowflakeValueGeneratorCache : ValueGeneratorCache, ISnowflakeValueGeneratorCache
{
    private readonly ConcurrentDictionary<string, SnowflakeSequenceValueGeneratorState> _sequenceGeneratorCache = new();

    /// <summary>
    ///     Initializes a new instance of the <see cref="ValueGeneratorCache" /> class.
    /// </summary>
    /// <param name="dependencies">Parameter object containing dependencies for this service.</param>
    public SnowflakeValueGeneratorCache(ValueGeneratorCacheDependencies dependencies)
        : base(dependencies)
    {
    }

    public  virtual SnowflakeSequenceValueGeneratorState GetOrAddSequenceState(
        IProperty property,
        IRelationalConnection connection)
    {
        var tableIdentifier = StoreObjectIdentifier.Create(property.DeclaringType, StoreObjectType.Table);
        var sequence = tableIdentifier != null
            ? property.FindHiLoSequence(tableIdentifier.Value)
            : property.FindHiLoSequence();

        Check.DebugAssert(sequence != null, "sequence is null");

        return _sequenceGeneratorCache.GetOrAdd(
            GetSequenceName(sequence, connection),
            _ => new SnowflakeSequenceValueGeneratorState(sequence));
    }

    private static string GetSequenceName(ISequence sequence, IRelationalConnection connection)
    {
        var dbConnection = connection.DbConnection;

        return dbConnection.Database.ToUpperInvariant()
            + "::"
            + dbConnection.DataSource.ToUpperInvariant()
            + "::"
            + (sequence.Schema == null ? "" : sequence.Schema + ".")
            + sequence.Name;
    }
}
