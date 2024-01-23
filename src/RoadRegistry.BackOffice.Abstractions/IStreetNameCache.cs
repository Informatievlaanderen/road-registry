namespace RoadRegistry.BackOffice.Abstractions;

public interface IStreetNameCache
{
    Task<Dictionary<int, string>> GetStreetNamesById(IEnumerable<int> streetNameIds, CancellationToken cancellationToken);
    Task<StreetNameCacheItem?> GetAsync(int streetNameId, CancellationToken cancellationToken);
}

public record StreetNameCacheItem
{
    public int Id { get; init; }
    public string NisCode { get; init; }
    public string Name { get; init; }
    public string Status { get; init; }
    public bool IsRemoved { get; init; }
}
