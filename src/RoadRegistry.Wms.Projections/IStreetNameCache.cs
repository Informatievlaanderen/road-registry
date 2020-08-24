namespace RoadRegistry.Wms.Projections
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;
    using Syndication.Schema;

    public interface IStreetNameCache
    {
        Task<StreetNameRecord> Get(int streetNameId);
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
    }
}
