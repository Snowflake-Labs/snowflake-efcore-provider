using System;
using System.Threading.Tasks;

#nullable enable

namespace Snowflake.EntityFrameworkCore.Utilities
{
    internal static class DisposableExtensions
    {
        public static ValueTask DisposeAsyncIfAvailable(this IDisposable? disposable)
        {
            if (disposable != null)
            {
                if (disposable is IAsyncDisposable asyncDisposable)
                {
                    return asyncDisposable.DisposeAsync();
                }

                disposable.Dispose();
            }

            return default;
        }
    }
}
