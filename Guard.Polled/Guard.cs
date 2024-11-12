using Microsoft.Extensions.ObjectPool;
using System.Linq.Expressions;

namespace Guard.Polled
{
    public class Guard<T> : IDisposable
    {
        private T _value;

        internal void SetValue(T value)
        {
            _value = value ?? throw new ArgumentNullException(nameof(value));
        }

        public TResult Get<TResult>(Expression<Func<T, TResult>> expression)
        {
            Func<T, TResult> getFunc = (Func<T, TResult>)GuardExpressionCache.GetOrAdd(expression);

            try
            {
                return getFunc(_value);
            }
            catch (NullReferenceException ex)
            {
                throw new ArgumentNullException(expression.Body.ToString());
            }
        }

        public void Dispose()
        {
            GuardPool<T>.Return(this);
        }
    }

    public static class Guard
    {
        public static Guard<T> New<T>(T value) => GuardPool<T>.Rent(value);
    }

    internal static class GuardPool<T>
    {
        private static readonly ObjectPool<Guard<T>> Pool = ObjectPool.Create(new DefaultPooledObjectPolicy<Guard<T>>());

        public static Guard<T> Rent(T value)
        {
            var guard = Pool.Get();
            guard.SetValue(value);
            return guard;
        }

        public static void Return(Guard<T> guard)
        {
            Pool.Return(guard);
        }
    }
}
