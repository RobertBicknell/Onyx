using System.Linq.Expressions;

namespace Onyx.API.Products
{
    public static class LambdaFactory<T> {
        public static bool TryCreateFilter(string filterProperty, string filterValue, out Expression<Func<T, bool>> lambda)
        {
            try
            {
                var parameter = Expression.Parameter(typeof(T), "e");
                var propertyAccess = Expression.Property(parameter, filterProperty);
                var constantValue = Expression.Constant(filterValue);
                var compareExpression = Expression.Equal(propertyAccess, constantValue);
                lambda = Expression.Lambda<Func<T, bool>>(compareExpression, parameter);
                return true;
            }
            catch (Exception ex) {
                lambda = (T t) => true;
                return false;
            }
        }
    }
}
