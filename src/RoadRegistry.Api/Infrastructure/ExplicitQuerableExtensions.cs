namespace RoadRegistry.Api.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    public static class ExplicitQuerableExtensions
    {
        public static IOrderedQueryable<T> OrderQueryBy<T, TKey>(this IQueryable<T> query, Expression<Func<T, TKey>> orderKey)
        {
            return query.OrderBy(orderKey);
        }

        public static IOrderedQueryable<T> OrderQueryBy<T, TKey>(this IQueryable<T> query, Expression<Func<T, TKey>> orderKey, IComparer<TKey> comparer)
        {
            return query.OrderBy(orderKey, comparer);
        }

        public static IOrderedQueryable<T> OrderQueryByDescending<T, TKey>(this IQueryable<T> query, Expression<Func<T, TKey>> orderKey)
        {
            return query.OrderByDescending(orderKey);
        }

        public static IOrderedQueryable<T> OrderQueryByDescending<T, TKey>(this IQueryable<T> query, Expression<Func<T, TKey>> orderKey, IComparer<TKey> comparer)
        {
            return query.OrderByDescending(orderKey, comparer);
        }
    }
}