namespace RoadRegistry.BackOffice.Abstractions;

public interface IStreetNameCache
{
    Task<Dictionary<int, string>> GetStreetNamesById(IEnumerable<int> streetNameIds, CancellationToken cancellationToken);
    Task<Dictionary<int, string>> GetStreetNameStatusesById(IEnumerable<int> streetNameIds, CancellationToken cancellationToken);
}
