using Snowflake.EntityFrameworkCore.Infrastructure.Internal;

namespace Snowflake.EntityFrameworkCore.Update.Internal;

using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Update;

public class SnowflakeModificationCommandBatchFactory : IModificationCommandBatchFactory
{
    private const int DefaultMaxBatchSize = 42;
    private const int MaxMaxBatchSize = 1000;
    private readonly int _maxBatchSize;

    public  SnowflakeModificationCommandBatchFactory(
        ModificationCommandBatchFactoryDependencies dependencies,
        IDbContextOptions options)
    {
        Dependencies = dependencies;

        _maxBatchSize = Math.Min(
            options.Extensions.OfType<SnowflakeOptionsExtension>().FirstOrDefault()?.MaxBatchSize ?? DefaultMaxBatchSize,
            MaxMaxBatchSize);

        if (_maxBatchSize <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(RelationalOptionsExtension.MaxBatchSize), RelationalStrings.InvalidMaxBatchSize(_maxBatchSize));
        }
    }

    /// <summary>
    ///     Relational provider-specific dependencies for this service.
    /// </summary>
    protected virtual ModificationCommandBatchFactoryDependencies Dependencies { get; }

    public  virtual ModificationCommandBatch Create()
        => new SnowflakeModificationCommandBatch(Dependencies, _maxBatchSize);
}
