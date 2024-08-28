using System.Linq.Expressions;

namespace Onyx.API.Products
{
    internal static class LambdaFactory<T> {
        public static Expression<Func<T, bool>> CreateFilter(string filterProperty, string filterValue)
        {
            //TODO check filterProperty exists in T, and is of type string
            //TODO check for possible injection / errors on filterValue
            var parameter = Expression.Parameter(typeof(T), "e");
            var propertyAccess = Expression.Property(parameter, filterProperty);
            var constantValue = Expression.Constant(filterValue);
            var compareExpression = Expression.Equal(propertyAccess, constantValue);
            var lambda = Expression.Lambda<Func<T, bool>>(compareExpression, parameter);
            return lambda;
        }
    }
}
