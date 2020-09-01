namespace RoadRegistry.Wms.Projections
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;
    using Syndication.Schema;

    public interface IStreetNameCache
    {
        Task<StreetNameRecord> Get(int streetNameId);
        Task<long> GetHighWaterMark();
        Task<IEnumerable<StreetNameRecord>> GetBetween(long previous, long until);
    }

    public class StreetNameCache : IStreetNameCache
    {
        private readonly Func<SyndicationContext> _contextFactory;

        public StreetNameCache(Func<SyndicationContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<StreetNameRecord> Get(int streetNameId)
        {
            using (var context = _contextFactory())
            {
                return await context.StreetNames
                    .SingleOrDefaultAsync(record => record.PersistentLocalId == streetNameId);
            }
        }

        public async Task<long> GetHighWaterMark()
        {
            using (var context = _contextFactory())
            {
                return await context.StreetNames
                    .MaxAsync(record => record.Position);
            }
        }

        public async Task<IEnumerable<StreetNameRecord>> GetBetween(long previous, long until)
        {
            using (var context = _contextFactory())
            {
                return await context.StreetNames
                    .Where(record => record.Position > previous && record.Position <= until).ToListAsync();
            }
        }
    }
}
