namespace RoadRegistry.BackOffice.Extensions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public static class QueryableExtensions
{
    public static async Task ForEachBatchAsync<T>(this IQueryable<T> query, int batchSize, Func<ICollection<T>, Task> action, CancellationToken cancellationToken)
    {
        var pageIndex = 0;

        while (true)
        {
            var batchItems = new List<T>();
            if (query is IAsyncEnumerable<T>)
            {
                await foreach (var element in ((IAsyncEnumerable<T>)query
                                   .Skip(pageIndex * batchSize)
                                   .Take(batchSize))
                               .WithCancellation(cancellationToken))
                {
                    batchItems.Add(element);
                }
            }
            else
            {
                batchItems.AddRange(query
                    .Skip(pageIndex * batchSize)
                    .Take(batchSize)
                );
            }

            if (!batchItems.Any())
            {
                break;
            }

            pageIndex++;

            await action(batchItems);
        }
    }
}
