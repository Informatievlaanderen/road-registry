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
    public static async Task<T> IncludeLocalSingleOrDefaultAsync<T>(this DbSet<T> dbSet, Expression<Func<T, bool>> predicate, CancellationToken cancellationToken)
        where T : class
    {
        return dbSet.Local.SingleOrDefault(predicate.Compile())
               ?? await dbSet.SingleOrDefaultAsync(predicate, cancellationToken);
    }

    public static async Task<T> IncludeLocalWithoutQueryFiltersSingleOrDefaultAsync<T>(this DbSet<T> dbSet, Expression<Func<T, bool>> predicate, CancellationToken cancellationToken)
        where T : class
    {
        return dbSet.Local.SingleOrDefault(predicate.Compile())
               ?? await dbSet.IgnoreQueryFilters().SingleOrDefaultAsync(predicate, cancellationToken);
    }

    public static async Task<T> IncludeLocalSingleAsync<T>(this DbSet<T> dbSet, Expression<Func<T, bool>> predicate, CancellationToken cancellationToken)
        where T : class
    {
        return dbSet.Local.SingleOrDefault(predicate.Compile())
               ?? await dbSet.SingleAsync(predicate, cancellationToken);
    }

    public static async Task<List<T>> IncludeLocalToListAsync<T>(this DbSet<T> dbSet, Func<IQueryable<T>, IQueryable<T>> query, CancellationToken cancellationToken)
        where T : class
    {
        var items = await query(dbSet).ToListAsync(cancellationToken);
        var localItems = query(dbSet.Local.AsQueryable()).ToList();

        return items.Union(localItems).ToList();
    }

    public static async Task IncludeLocalForEachBatchAsync<T>(this DbSet<T> dbSet, Func<IQueryable<T>, IQueryable<T>> queryBuilder, int batchSize, Func<ICollection<T>, Task> action, CancellationToken cancellationToken)
        where T : class
    {
        if (dbSet.Local.Any())
        {
            await queryBuilder(dbSet.Local.AsQueryable()).ForEachBatchAsync(batchSize, action, cancellationToken);
        }

        await queryBuilder(dbSet).ForEachBatchAsync(batchSize, action, cancellationToken);
    }

    public static void Synchronize<T>(this DbSet<T> source,
        Dictionary<int, T> currentSet,
        Dictionary<int, T> nextSet,
        Action<T, T> modifier) where T : class
    {
        var allKeys = new HashSet<int>(currentSet.Keys.Concat(nextSet.Keys));
        foreach (var key in allKeys)
        {
            var gotCurrent = currentSet.TryGetValue(key, out var current);
            var gotNext = nextSet.TryGetValue(key, out var next);
            if (gotCurrent && gotNext)
            {
                modifier(current, next);
            }
            else if (gotCurrent)
            {
                source.Remove(current);
                source.Local.Remove(current);
            }
            else if (gotNext)
            {
                source.Add(next);
            }
        }
    }

    public static void Synchronize<T>(this DbSet<T> source,
        Dictionary<int, T> currentSet,
        Dictionary<int, T> nextSet,
        Action<T, T> modifier,
        Action<T> remove) where T : class
    {
        var allKeys = new HashSet<int>(currentSet.Keys.Concat(nextSet.Keys));
        foreach (var key in allKeys)
        {
            var gotCurrent = currentSet.TryGetValue(key, out var current);
            var gotNext = nextSet.TryGetValue(key, out var next);
            if (gotCurrent && gotNext)
            {
                modifier(current, next);
            }
            else if (gotCurrent)
            {
                remove(current);
            }
            else if (gotNext)
            {
                source.Add(next);
            }
        }
    }

    public static async Task<ICollection<T>> Synchronize<T>(this DbSet<T> source,
        Dictionary<int, T> currentSet,
        Dictionary<int, T> nextSet,
        Action<T, T> modifier,
        Action<T> remove,
        Func<T, Task> add) where T : class
    {
        var allItems = new List<T>();

        var allKeys = new HashSet<int>(currentSet.Keys.Concat(nextSet.Keys));
        foreach (var key in allKeys)
        {
            var gotCurrent = currentSet.TryGetValue(key, out var current);
            var gotNext = nextSet.TryGetValue(key, out var next);
            if (gotCurrent && gotNext)
            {
                modifier(current, next);
                allItems.Add(next);
            }
            else if (gotCurrent)
            {
                remove(current);
                allItems.Add(current);
            }
            else if (gotNext)
            {
                await add(next);
                source.Add(next);
                allItems.Add(next);
            }
        }

        return allItems;
    }
}
