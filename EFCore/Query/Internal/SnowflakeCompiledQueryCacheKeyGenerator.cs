using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using Snowflake.EntityFrameworkCore.Storage.Internal;

namespace Snowflake.EntityFrameworkCore.Query.Internal;

/// <inheritdoc />
public class SnowflakeCompiledQueryCacheKeyGenerator : RelationalCompiledQueryCacheKeyGenerator
{
    private readonly ISnowflakeConnection _SnowflakeConnection;

    /// <summary>
    /// Constructor for <see cref="SnowflakeCompiledQueryCacheKeyGenerator"/>
    /// </summary>
    /// <param name="dependencies"></param>
    /// <param name="relationalDependencies"></param>
    /// <param name="SnowflakeConnection"></param>
    public SnowflakeCompiledQueryCacheKeyGenerator(
        CompiledQueryCacheKeyGeneratorDependencies dependencies,
        RelationalCompiledQueryCacheKeyGeneratorDependencies relationalDependencies,
        ISnowflakeConnection SnowflakeConnection)
        : base(dependencies, relationalDependencies)
    {
        _SnowflakeConnection = SnowflakeConnection;
    }

    /// <inheritdoc />
    public override object GenerateCacheKey(Expression query, bool async)
        => new SnowflakeCompiledQueryCacheKey(
            GenerateCacheKeyCore(query, async),
            _SnowflakeConnection.IsMultipleActiveResultSetsEnabled);

    private readonly struct SnowflakeCompiledQueryCacheKey : IEquatable<SnowflakeCompiledQueryCacheKey>
    {
        private readonly RelationalCompiledQueryCacheKey _relationalCompiledQueryCacheKey;
        private readonly bool _multipleActiveResultSetsEnabled;

        public SnowflakeCompiledQueryCacheKey(
            RelationalCompiledQueryCacheKey relationalCompiledQueryCacheKey,
            bool multipleActiveResultSetsEnabled)
        {
            _relationalCompiledQueryCacheKey = relationalCompiledQueryCacheKey;
            _multipleActiveResultSetsEnabled = multipleActiveResultSetsEnabled;
        }

        public override bool Equals(object? obj)
            => obj is SnowflakeCompiledQueryCacheKey SnowflakeCompiledQueryCacheKey
               && Equals(SnowflakeCompiledQueryCacheKey);

        public bool Equals(SnowflakeCompiledQueryCacheKey other)
            => _relationalCompiledQueryCacheKey.Equals(other._relationalCompiledQueryCacheKey)
               && _multipleActiveResultSetsEnabled == other._multipleActiveResultSetsEnabled;

        public override int GetHashCode()
            => HashCode.Combine(_relationalCompiledQueryCacheKey, _multipleActiveResultSetsEnabled);
    }
}