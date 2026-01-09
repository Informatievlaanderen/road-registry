namespace RoadRegistry.BackOffice.FeatureCompare.V2.Translators;

using System.Collections.Generic;
using System.Linq;
using RoadRegistry.Infrastructure;

public interface IRoadSegmentFeatureCompareStreetNameContext
{
    bool IsValid(StreetNameLocalId id);
    bool IsRemoved(StreetNameLocalId id);
    bool TryGetRenamedId(StreetNameLocalId id, out StreetNameLocalId renamedToId);
}

public sealed class RoadSegmentFeatureCompareStreetNameContext: IRoadSegmentFeatureCompareStreetNameContext
{
    private readonly ICollection<StreetNameCacheItem> _streetNames;
    private readonly IDictionary<int, int> _renamedIds;

    public RoadSegmentFeatureCompareStreetNameContext(
        ICollection<StreetNameCacheItem> streetNames,
        Dictionary<int, int> renamedIds)
    {
        _streetNames = streetNames;
        _renamedIds = renamedIds;
    }

    public bool IsValid(StreetNameLocalId id)
    {
        return _streetNames.Any(x => x.Id == id);
    }

    public bool IsRemoved(StreetNameLocalId id)
    {
        var streetName = _streetNames.SingleOrDefault(x => x.Id == id);
        return streetName is not null && streetName.IsRemoved;
    }

    public bool TryGetRenamedId(StreetNameLocalId id, out StreetNameLocalId renamedToId)
    {
        if (_renamedIds.TryGetValue(id, out var intRenamedToId))
        {
            renamedToId = new StreetNameLocalId(intRenamedToId);
            return true;
        }

        renamedToId = default;
        return false;
    }
}
