namespace RoadRegistry.RoadSegment;

using System;
using System.Linq;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Events.V2;
using RoadRegistry.ValueObjects;
using ValueObjects;

public partial class RoadSegment
{
    // The road segment follows a street name to another one, a renamed street name for instance. Letting go of a street
    // name is UnlinkStreetName, not a relink to 'niet van toepassing'.
    public void RelinkStreetName(StreetNameLocalId oldStreetNameId, StreetNameLocalId newStreetNameId, Provenance provenance)
    {
        // Callers pick the road segments to relink and leave the removed ones out; one that does not is a defect
        // rather than something the caller did, so it is not reported as a problem on a ticket.
        if (IsRemoved)
        {
            throw new InvalidOperationException($"Road segment {RoadSegmentId} is removed and cannot have its street name changed. This should not happen.");
        }

        if (newStreetNameId == StreetNameLocalId.NotApplicable)
        {
            throw new InvalidOperationException($"Road segment {RoadSegmentId} cannot be relinked to 'niet van toepassing'; its street name {oldStreetNameId} is unlinked instead. This should not happen.");
        }

        Apply(new RoadSegmentStreetNameWasRelinked
        {
            RoadSegmentId = RoadSegmentId,
            OldStreetNameId = oldStreetNameId,
            NewStreetNameId = newStreetNameId,
            StreetNameIdAttributeValues = ReplaceStreetNameId(oldStreetNameId, newStreetNameId),
            Provenance = new ProvenanceData(provenance)
        });
    }

    public void Apply(RoadSegmentStreetNameWasRelinked @event)
    {
        UncommittedEvents.Add(@event);

        Attributes = Attributes! with
        {
            StreetNameId = @event.StreetNameIdAttributeValues
        };
    }

    private RoadSegmentDynamicAttributeValues<StreetNameLocalId> ReplaceStreetNameId(StreetNameLocalId oldStreetNameId, StreetNameLocalId newStreetNameId)
    {
        return new RoadSegmentDynamicAttributeValues<StreetNameLocalId>(
            Attributes!.StreetNameId.Values
                .Select(value => (value.Coverage, value.Side, value.Value == oldStreetNameId ? newStreetNameId : value.Value)));
    }
}
