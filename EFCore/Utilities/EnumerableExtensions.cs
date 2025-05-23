namespace System.Collections.Generic;

using Diagnostics;
using Linq;

[DebuggerStepThrough]
internal static class EnumerableExtensions
{
    public static string Join(this IEnumerable<object> source, string separator = ", ")
        => string.Join(separator, source);

    public static IEnumerable<T> Distinct<T>(
        this IEnumerable<T> source,
        Func<T?, T?, bool> comparer)
        where T : class
        => source.Distinct(new DynamicEqualityComparer<T>(comparer));

    private sealed class DynamicEqualityComparer<T> : IEqualityComparer<T>
        where T : class
    {
        private readonly Func<T?, T?, bool> _func;

        public DynamicEqualityComparer(Func<T?, T?, bool> func)
        {
            _func = func;
        }

        public bool Equals(T? x, T? y)
            => _func(x, y);

        public int GetHashCode(T obj)
            => 0; // force Equals
    }
}
