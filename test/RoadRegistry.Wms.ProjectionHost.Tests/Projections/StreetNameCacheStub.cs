namespace RoadRegistry.Wms.ProjectionHost.Tests.Projections;

using BackOffice.Abstractions;
using Sync.StreetNameRegistry;

public class StreetNameCacheStub : IStreetNameCache
{
    private readonly long _position;
    private readonly StreetNameRecord _stubbedValue;

    public StreetNameCacheStub(StreetNameRecord stubbedValue = null, long position = -1L)
    {
        _stubbedValue = stubbedValue;
        _position = position;
    }

    public Task<Dictionary<int, string>> GetStreetNamesById(IEnumerable<int> streetNameIds, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<StreetNameCacheItem> GetAsync(int streetNameId, CancellationToken cancellationToken)
    {
        return Task.FromResult(new StreetNameCacheItem
        {
            NisCode = _stubbedValue?.NisCode,
            Name = _stubbedValue?.DutchName
        });
    }
    
    public Task<long> GetMaxPositionAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult(_position);
    }
}
