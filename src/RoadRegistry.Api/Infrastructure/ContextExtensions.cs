namespace RoadRegistry.Api.Infrastructure
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;

    public static class ContextExtensions
    {
        public static Task<IReadOnlyCollection<T>> AsUntrackedCollectionAsync<T>(this IQueryable<T> records)
            where T : class
        {
            return records
                .AsNoTracking()
                .ToListAsync()
                .AsReadOnlyAsync();
        }

        public static async Task<IReadOnlyCollection<T>> AsReadOnlyAsync<T>(this Task<List<T>> list)
        {
            return (await list).AsReadOnly();
        }
    }
}