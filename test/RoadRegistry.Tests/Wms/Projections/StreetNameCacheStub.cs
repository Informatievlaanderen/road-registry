namespace RoadRegistry.Wms.Projections
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Syndication.Schema;

    public class StreetNameCacheStub : IStreetNameCache
    {
        private readonly StreetNameRecord _stubbedValue;

        public StreetNameCacheStub(StreetNameRecord stubbedValue = null)
        {
            _stubbedValue = stubbedValue;
        }

        public Task<StreetNameRecord> GetAsync(int streetNameId, CancellationToken token)
        {
            return Task.FromResult(_stubbedValue);
        }

        public Task<long> GetMaxPositionAsync(CancellationToken token)
        {
            return Task.FromResult(-1L);
        }

        public Task<Dictionary<int, string>> GetStreetNamesByIdAsync(IEnumerable<int> streetNameIds, CancellationToken token)
        {
            throw new NotImplementedException();
        }
    }
}
