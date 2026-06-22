namespace RoadRegistry.RoadSegment;

using System.Linq;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Events.V2;
using RoadRegistry.ValueObjects;
using ValueObjects;

public partial class RoadSegment
{
    public void ChangeStreetNameId(StreetNameLocalId oldStreetNameId, StreetNameLocalId newStreetNameId, Provenance provenance)
    {
        var updatedStreetNameId = new RoadSegmentDynamicAttributeValues<StreetNameLocalId>(
            Attributes!.StreetNameId.Values
                .Select(value => (value.Coverage, value.Side, value.Value == oldStreetNameId ? newStreetNameId : value.Value)));

        Apply(new RoadSegmentStreetNameIdWasChanged
        {
            RoadSegmentId = RoadSegmentId,
            OldStreetNameId = oldStreetNameId,
            NewStreetNameId = newStreetNameId,
            StreetNameId = updatedStreetNameId,
            Provenance = new ProvenanceData(provenance)
        });
    }

    public void Apply(RoadSegmentStreetNameIdWasChanged @event)
    {
        UncommittedEvents.Add(@event);

        Attributes = Attributes! with
        {
            StreetNameId = @event.StreetNameId
        };
    }
}
