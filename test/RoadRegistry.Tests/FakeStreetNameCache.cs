namespace RoadRegistry;

using BackOffice.Abstractions;

public class FakeStreetNameCache : IStreetNameCache
{
    public Task<Dictionary<int, string>> GetStreetNamesById(IEnumerable<int> streetNameIds, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
