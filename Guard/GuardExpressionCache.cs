using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace Guard
{
    internal static class GuardExpressionCache
    {
        private static ConcurrentDictionary<string, Delegate> Cache = new ConcurrentDictionary<string, Delegate>();

        public static Delegate GetOrAdd<T, TResult>(Expression<Func<T, TResult>> expression)
        {
            var key = $"{typeof(T).FullName}/{typeof(TResult).FullName}/{expression}";
            var compiled = (Func<T, TResult>)Cache.GetOrAdd(key, _ => expression.Compile());
            return compiled;
        }
    }
}
