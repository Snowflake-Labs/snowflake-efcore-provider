using System;
using Snowflake.Data.Client;

namespace Snowflake.EntityFrameworkCore.Storage.Internal;

/// <summary>
///     Detects the exceptions caused by Snowflake transient failures.
/// </summary>
public static class SnowflakeTransientExceptionDetector
{
    /// <summary>
    /// Should retry on the given exception.
    /// </summary>
    /// <param name="ex"></param>
    /// <returns></returns>
    public static bool ShouldRetryOn(Exception? ex)
    {
        if (ex is SnowflakeDbException)
        {
            return false;
        }

        return ex is TimeoutException;
    }
}