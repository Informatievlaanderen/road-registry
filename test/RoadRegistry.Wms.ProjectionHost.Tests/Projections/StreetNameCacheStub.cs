namespace RoadRegistry.Wms.ProjectionHost.Tests.Projections;

using Syndication.Schema;
using Wms.Projections;

public class StreetNameCacheStub : IStreetNameCache
{
    public StreetNameCacheStub(StreetNameRecord stubbedValue = null, long position = -1L)
    {
        _stubbedValue = stubbedValue;
        _position = position;
    }

    private readonly long _position;
    private readonly StreetNameRecord _stubbedValue;

    public Task<StreetNameRecord> GetAsync(int streetNameId, CancellationToken token)
    {
        return Task.FromResult(_stubbedValue);
    }

    public Task<long> GetMaxPositionAsync(CancellationToken token)
    {
        return Task.FromResult(_position);
    }

    public Task<Dictionary<int, string>> GetStreetNamesByIdAsync(IEnumerable<int> streetNameIds, CancellationToken token)
    {
        throw new NotImplementedException();
    }
}
