namespace RoadRegistry.Tests;

using RoadRegistry.BackOffice.Abstractions;

public class FakeStreetNameCache : IStreetNameCache
{
    private readonly Dictionary<int, StreetNameItem> _cache = new();

    public Task<Dictionary<int, string>> GetStreetNamesById(IEnumerable<int> streetNameIds, CancellationToken cancellationToken)
    {
        return Task.FromResult(streetNameIds
            .Distinct()
            .Where(streetNameId => _cache.ContainsKey(streetNameId))
            .ToDictionary(streetNameId => streetNameId, streetNameId => _cache[streetNameId].Name)
        );
    }
    
    public Task<StreetNameCacheItem> GetAsync(int streetNameId, CancellationToken cancellationToken)
    {
        StreetNameCacheItem result = null;

        if (_cache.TryGetValue(streetNameId, out var streetName))
        {
            result = new StreetNameCacheItem
            {
                Id = streetNameId,
                Name = streetName.Name,
                Status = streetName.Status
            };
        }

        return Task.FromResult(result);
    }

    public FakeStreetNameCache AddStreetName(int id, string name, string status)
    {
        _cache.Add(id, new StreetNameItem(name, status));
        return this;
    }

    private sealed record StreetNameItem(string Name, string Status);
}
