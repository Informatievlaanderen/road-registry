namespace RoadRegistry.BackOffice.Core;

using RoadRegistry.RoadNetwork.ValueObjects;

public class RoadSegmentSideAttributes
{
    public RoadSegmentSideAttributes(StreetNameLocalId? streetNameId)
    {
        StreetNameId = streetNameId;
    }

    public StreetNameLocalId? StreetNameId { get; }
}
