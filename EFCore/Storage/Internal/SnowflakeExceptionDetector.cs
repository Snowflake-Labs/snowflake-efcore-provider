using System;
using System.Threading;
using Microsoft.EntityFrameworkCore.Storage;

namespace Snowflake.EntityFrameworkCore.Storage.Internal;

/// <summary>
/// Represents a Snowflake-specific exception detector.
/// </summary>
public class SnowflakeExceptionDetector : IExceptionDetector
{
    /// <inheritdoc />
    public virtual bool IsCancellation(Exception exception, CancellationToken cancellationToken = default)
        => exception is OperationCanceledException || cancellationToken.IsCancellationRequested;
}