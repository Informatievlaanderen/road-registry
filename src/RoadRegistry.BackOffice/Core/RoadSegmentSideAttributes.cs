namespace RoadRegistry.BackOffice.Core;

public class RoadSegmentSideAttributes
{
    public RoadSegmentSideAttributes(StreetNameLocalId? streetNameId)
    {
        StreetNameId = streetNameId;
    }

    public StreetNameLocalId? StreetNameId { get; }
}
