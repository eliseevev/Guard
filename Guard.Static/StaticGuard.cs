using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace Guard.Static
{
    public static class StaticGuard
    {
        private static readonly ConcurrentDictionary<string, Delegate> _cache = new();

        /// <summary>
        /// ЗАкешированный экпрешн.
        /// </summary>
        public static TResult CachedExpression<T, TResult>(T obj, Expression<Func<T, TResult>> expression)
        {
            if (obj is null)
                throw new ArgumentNullException(nameof(obj));

            var key = $"{typeof(T).FullName}/{typeof(TResult).FullName}/{expression}";
            var compiled = (Func<T, TResult>)_cache.GetOrAdd(key, _ => expression.Compile());
            try
            {
                return compiled(obj);
            }
            catch (NullReferenceException ex)
            {
                throw new ArgumentNullException(expression.Body.ToString());
            }
        }

        /// <summary>
        /// Не кешированный экпрешн (как сейчас).
        /// </summary>
        public static TResult UncachedExpression<T, TResult>(T obj, Expression<Func<T, TResult>> expression)
        {
            if (obj is null)
                throw new ArgumentNullException(nameof(obj));

            var compiled = expression.Compile();
            try
            {
                return compiled(obj);
            }
            catch (NullReferenceException ex)
            {
                throw new ArgumentNullException(expression.Body.ToString());
            }
        }
    }
}
