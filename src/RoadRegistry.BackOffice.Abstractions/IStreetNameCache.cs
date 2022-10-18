namespace RoadRegistry.BackOffice.Abstractions;

public interface IStreetNameCache
{
    Task<Dictionary<int, string>> GetStreetNamesById(IEnumerable<int> streetNameIds, CancellationToken cancellationToken);
}