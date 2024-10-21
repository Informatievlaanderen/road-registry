namespace RoadRegistry.BackOffice.FeatureCompare.Translators;

using System.Collections.Generic;
using System.Linq;

public interface IRoadSegmentFeatureCompareStreetNameContext
{
    bool IsRemoved(StreetNameLocalId id);
    bool Exists(StreetNameLocalId id);
    bool TryGetRenamedId(StreetNameLocalId id, out StreetNameLocalId renamedToId);
}

public sealed class RoadSegmentFeatureCompareStreetNameContext: IRoadSegmentFeatureCompareStreetNameContext
{
    private ICollection<StreetNameCacheItem> StreetNames { get; }
    public IDictionary<int, int> RenamedIds { get; }

    public RoadSegmentFeatureCompareStreetNameContext(
        ICollection<StreetNameCacheItem> streetNames,
        Dictionary<int, int> renamedIds)
    {
        StreetNames = streetNames;
        RenamedIds = renamedIds;
    }

    public bool IsRemoved(StreetNameLocalId id)
    {
        var streetName = StreetNames.SingleOrDefault(x => x.Id == id);
        return streetName is not null && streetName.IsRemoved;
    }

    public bool Exists(StreetNameLocalId id)
    {
        return StreetNames.Any(x => x.Id == id);
    }

    public bool TryGetRenamedId(StreetNameLocalId id, out StreetNameLocalId renamedToId)
    {
        if (RenamedIds.TryGetValue(id, out var intRenamedToId))
        {
            renamedToId = new StreetNameLocalId(intRenamedToId);
            return true;
        }

        renamedToId = default;
        return false;
    }
}
