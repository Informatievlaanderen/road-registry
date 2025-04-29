namespace RoadRegistry.Tests;

using RoadRegistry.BackOffice;

public class FakeStreetNameCache : IStreetNameCache
{
    private readonly Dictionary<int, StreetNameItem> _cache = new();
    private readonly Dictionary<int, int> _renamedIds = new();

    public async Task<Dictionary<int, string>> GetStreetNamesById(IEnumerable<int> streetNameIds, CancellationToken cancellationToken)
    {
        var result = await GetAsync(streetNameIds, cancellationToken);
        return result.ToDictionary(x => x.Id, x => x.Name);
    }

    public async Task<StreetNameCacheItem> GetAsync(int streetNameId, CancellationToken cancellationToken)
    {
        var result = await GetAsync([streetNameId], cancellationToken);
        return result.SingleOrDefault();
    }

    public Task<ICollection<StreetNameCacheItem>> GetAsync(IEnumerable<int> streetNameIds, CancellationToken cancellationToken)
    {
        return Task.FromResult<ICollection<StreetNameCacheItem>>(
            streetNameIds
                .Distinct()
                .Where(_cache.ContainsKey)
                .Select(streetNameId =>
                {
                    var streetName = _cache[streetNameId];

                    return new StreetNameCacheItem
                    {
                        Id = streetNameId,
                        Name = streetName.Name,
                        Status = streetName.Status,
                        IsRemoved = streetName.IsRemoved
                    };
                })
                .ToList());
    }

    public Task<Dictionary<int, int>> GetRenamedIdsAsync(IEnumerable<int> streetNameIds, CancellationToken cancellationToken)
    {
        return Task.FromResult(_renamedIds.Where(x => streetNameIds.Contains(x.Key)).ToDictionary(x => x.Key, x => x.Value));
    }

    public FakeStreetNameCache AddStreetName(int id, string name, string status, bool isRemoved = false)
    {
        _cache.Add(id, new StreetNameItem(name, status, isRemoved));
        return this;
    }

    public FakeStreetNameCache AddRenamedStreetName(int id, int destinationId)
    {
        _renamedIds.Add(id, destinationId);
        return this;
    }

    private sealed record StreetNameItem(string Name, string Status, bool IsRemoved);
}
