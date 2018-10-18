namespace RoadRegistry.Api.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    internal static class ExplicitQueryableExtensions
    {
        public static IOrderedQueryable<T> OrderQueryBy<T, TKey>(this IQueryable<T> query, Expression<Func<T, TKey>> orderKey)
        {
            return query.OrderBy(orderKey);
        }
    }
}
