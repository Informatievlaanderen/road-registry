namespace RoadRegistry.Api.Infrastructure
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;

    public static class ContextExtensions
    {
        public static async Task<IReadOnlyCollection<T>> AsReadOnlyAsync<T>(this IQueryable<T> records, CancellationToken token)
        {
            return (await records.ToListAsync(token)).AsReadOnly();
        }
    }
}
