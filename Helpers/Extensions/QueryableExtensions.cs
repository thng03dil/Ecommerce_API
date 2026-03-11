using System.Linq.Expressions;
using System.Reflection;

namespace Ecommerce_API.Helpers.Extensions
{
    public static class QueryableExtensions
    {  
        public static IQueryable<T> ApplySorting<T>(
            this IQueryable<T> query,
            string sortBy,
            string sortOrder)
        {
            var property = typeof(T).GetProperty(
    sortBy,
    BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

            if (property == null)
                return query;

            var parameter = Expression.Parameter(typeof(T), "x");

            var propertyAccess = Expression.MakeMemberAccess(parameter, property);

            var orderByExp = Expression.Lambda(propertyAccess, parameter);

            string method = sortOrder.ToLower() == "desc"
                ? "OrderByDescending"
                : "OrderBy";

            var resultExp = Expression.Call(
                typeof(Queryable),
                method,
                new Type[] { typeof(T), property.PropertyType },
                query.Expression,
                Expression.Quote(orderByExp));

            return query.Provider.CreateQuery<T>(resultExp);
        }
    }
}
