namespace RoadRegistry.BackOffice.Extensions;

using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

public static class DbSetExtensions
{
    public static async Task ForEachBatchAsync<T>(this DbSet<T> dbSet, Func<IQueryable<T>, IQueryable<T>> queryBuilder, int batchSize, Func<ICollection<T>, Task> action, CancellationToken cancellationToken)
        where T : class
    {
        await queryBuilder(dbSet.Local.AsQueryable()).ForEachBatchAsync(batchSize, action, cancellationToken);
        await queryBuilder(dbSet).ForEachBatchAsync(batchSize, action, cancellationToken);
    }

    public static async Task<T> FindAsync<T>(this DbSet<T> dbSet, Expression<Func<T, bool>> predicate, CancellationToken cancellationToken)
        where T : class
    {
        return dbSet.Local.SingleOrDefault(predicate.Compile())
               ?? await dbSet.SingleOrDefaultAsync(predicate, cancellationToken);
    }

    public static async Task<T> FindWithoutQueryFiltersAsync<T>(this DbSet<T> dbSet, Expression<Func<T, bool>> predicate, CancellationToken cancellationToken)
        where T : class
    {
        return dbSet.Local.SingleOrDefault(predicate.Compile())
               ?? await dbSet.IgnoreQueryFilters().SingleOrDefaultAsync(predicate, cancellationToken);
    }

    public static async Task<T> SingleIncludingLocalAsync<T>(this DbSet<T> dbSet, Expression<Func<T, bool>> predicate, CancellationToken cancellationToken)
        where T : class
    {
        return dbSet.Local.SingleOrDefault(predicate.Compile())
               ?? await dbSet.SingleAsync(predicate, cancellationToken);
    }

    public static async Task<List<T>> ToListIncludingLocalAsync<T>(this DbSet<T> dbSet, Expression<Func<T, bool>> predicate, CancellationToken cancellationToken)
        where T : class
    {
        return dbSet.Local.AsQueryable()
            .Where(predicate)
            .Concat(await dbSet
                .Where(predicate)
                .ToListAsync(cancellationToken))
        .ToList();
    }

    public static async Task<List<T>> ToListIncludingLocalAsync<T>(this DbSet<T> dbSet, Func<IQueryable<T>, IQueryable<T>> query, CancellationToken cancellationToken)
        where T : class
    {
        return query(dbSet.Local.AsQueryable())
            .Concat(await query(dbSet)
                .ToListAsync(cancellationToken))
            .ToList();
    }
}
