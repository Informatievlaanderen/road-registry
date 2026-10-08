namespace RoadRegistry.BackOffice.Extensions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

public static class QueryableExtensions
{
    // The ordering is a parameter rather than something the caller may leave out: each page is a separate query, and
    // without an ordering the database is free to hand back the rows of one page in an order unrelated to the next,
    // so a row can end up in two pages or in none at all. Every caller here walks a result set while changing the
    // records in it, where a row that falls between two pages silently keeps its old value.
    public static async Task ForEachBatchAsync<T, TKey>(this IQueryable<T> query, Expression<Func<T, TKey>> orderBy, int batchSize, Func<ICollection<T>, Task> action, CancellationToken cancellationToken)
    {
        var orderedQuery = query.OrderBy(orderBy);
        var pageIndex = 0;

        while (true)
        {
            var batchItems = new List<T>();
            var page = orderedQuery
                .Skip(pageIndex * batchSize)
                .Take(batchSize);

            if (page is IAsyncEnumerable<T> asyncPage)
            {
                await foreach (var element in asyncPage.WithCancellation(cancellationToken))
                {
                    batchItems.Add(element);
                }
            }
            else
            {
                batchItems.AddRange(page);
            }

            if (batchItems.Count == 0)
            {
                break;
            }

            pageIndex++;

            await action(batchItems);
        }
    }
}
