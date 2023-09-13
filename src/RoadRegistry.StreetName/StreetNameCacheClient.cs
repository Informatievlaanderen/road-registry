namespace RoadRegistry.StreetName;

using System.Threading;
using System.Threading.Tasks;
using BackOffice.Abstractions;

public class StreetNameCacheClient : IStreetNameClient
{
    private readonly IStreetNameCache _streetNameCache;

    public StreetNameCacheClient(IStreetNameCache streetNameCache)
    {
        _streetNameCache = streetNameCache;
    }

    public async Task<StreetNameItem> GetAsync(int id, CancellationToken cancellationToken)
    {
        var streetName = await _streetNameCache.GetAsync(id, cancellationToken);
        if (streetName is null || streetName.IsRemoved)
        {
            return null;
        }

        return new StreetNameItem
        {
            Id = streetName.Id,
            Name = streetName.Name,
            Status = streetName.Status
        };
    }
}
