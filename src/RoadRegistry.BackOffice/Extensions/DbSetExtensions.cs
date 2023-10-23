namespace RoadRegistry.BackOffice.Extensions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

public static class DbSetExtensions
{
    public static async Task ForEachBatchAsync<T>(this DbSet<T> dbSet, Func<IQueryable<T>, IQueryable<T>> queryBuilder, int batchSize, Func<ICollection<T>, Task> action, CancellationToken cancellationToken)
        where T : class
    {
        await queryBuilder(dbSet.Local.AsQueryable()).ForEachBatchAsync(batchSize, action, cancellationToken);
        await queryBuilder(dbSet).ForEachBatchAsync(batchSize, action, cancellationToken);
    }
}
