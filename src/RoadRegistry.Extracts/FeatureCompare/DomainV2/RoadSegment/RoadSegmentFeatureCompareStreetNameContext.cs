namespace RoadRegistry.Extracts.FeatureCompare.DomainV2.RoadSegment;

using System;
using System.Collections.Generic;
using System.Linq;
using RoadRegistry.Infrastructure;

public interface IRoadSegmentFeatureCompareStreetNameContext
{
    bool IsValid(StreetNameLocalId id);
    bool IsRemoved(StreetNameLocalId id);
    bool IsRetiredOrRejected(StreetNameLocalId id);
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

    // The two statuses of the street name registry in which a street name is no longer active. A status this does not
    // know (or none at all) is left to the other checks rather than refused here.
    public bool IsRetiredOrRejected(StreetNameLocalId id)
    {
        var streetName = _streetNames.SingleOrDefault(x => x.Id == id);
        return streetName is not null
            && (string.Equals(streetName.Status, RetiredStatus, StringComparison.InvariantCultureIgnoreCase)
                || string.Equals(streetName.Status, RejectedStatus, StringComparison.InvariantCultureIgnoreCase));
    }

    // The values of StreetNameStatus.Retired and StreetNameStatus.Denied (RoadRegistry.StreetName), which this project
    // does not reference.
    private const string RetiredStatus = "gehistoreerd";
    private const string RejectedStatus = "afgekeurd";

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
