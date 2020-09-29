namespace RoadRegistry.Wms.Projections
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;
    using Syndication.Schema;

    public interface IMunicipalityCache
    {
        Task<MunicipalityRecord> Get(string nisCode);
    }

    public class MunicipalityCache : IMunicipalityCache
    {
        private readonly Func<SyndicationContext> _contextFactory;

        public MunicipalityCache(Func<SyndicationContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<MunicipalityRecord> Get(string nisCode)
        {
            using (var context = _contextFactory())
            {
                return await context.Municipalities
                    .SingleOrDefaultAsync(record => record.NisCode == nisCode);
            }
        }
    }
}
