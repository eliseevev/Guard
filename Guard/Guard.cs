using System.Linq.Expressions;

namespace Guard
{
    public class Guard<T> : IDisposable
    {
        private T _value;

        public Guard(T value)
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
            // на всякий случай, на будущее.
        }
    }

    public static class Guard
    {
        public static Guard<T> New<T>(T value) => new Guard<T>(value);
    }
}
