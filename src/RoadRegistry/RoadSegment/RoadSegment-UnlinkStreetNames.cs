namespace RoadRegistry.RoadSegment;

using System.Collections.Generic;
using System.Linq;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Events.V2;
using RoadRegistry.ValueObjects;
using ValueObjects;

public partial class RoadSegment
{
    // A street name is linked to a road segment only while both are active, the same as every other link between
    // two objects in the base registries that does not identify either of them (see "Beheer koppeling
    // straatnaam-wegsegment"). The link goes when either of them is removed or turns inactive, and it does not come
    // back on its own when that is undone: a segment that is corrected back to an active status gets its street name
    // from whoever corrects it.
    //
    // Each street name that is let go is recorded as a RoadSegmentStreetNameWasUnlinked, so a segment linked to two
    // street names, one on either side, records one event for each.
    //
    // This also accepts a removed road segment: the run-once correction of the links that were left behind before
    // this rule existed has to reach those too. The road segment's own actions never get here once it is removed.
    //
    // Without streetNameIds every street name is let go; with them only those, which is how the run-once correction
    // lets an active road segment keep the street name on the one side that is still active.
    public void UnlinkStreetNames(Provenance provenance, IReadOnlyCollection<StreetNameLocalId>? streetNameIds = null)
    {
        if (Attributes is null)
        {
            return;
        }

        var linkedStreetNameIds = Attributes.StreetNameId.Values
            .Select(value => value.Value)
            .Where(streetNameId => !StreetNameLocalId.IsEmpty(streetNameId))
            .Where(streetNameId => streetNameIds is null || streetNameIds.Contains(streetNameId))
            .Distinct()
            .ToList();

        foreach (var streetNameId in linkedStreetNameIds)
        {
            UnlinkStreetName(streetNameId, provenance);
        }
    }

    // Lets go of one street name: wherever the road segment carries it, it now carries 'niet van toepassing'. A street
    // name the road segment does not carry records nothing.
    public void UnlinkStreetName(StreetNameLocalId streetNameId, Provenance provenance)
    {
        if (Attributes is null
            || StreetNameLocalId.IsEmpty(streetNameId)
            || Attributes.StreetNameId.Values.All(value => value.Value != streetNameId))
        {
            return;
        }

        Apply(new RoadSegmentStreetNameWasUnlinked
        {
            RoadSegmentId = RoadSegmentId,
            StreetNameId = streetNameId,
            StreetNameIdAttributeValues = ReplaceStreetNameId(streetNameId, StreetNameLocalId.NotApplicable),
            Provenance = new ProvenanceData(provenance)
        });
    }

    public void Apply(RoadSegmentStreetNameWasUnlinked @event)
    {
        UncommittedEvents.Add(@event);

        Attributes = Attributes! with
        {
            StreetNameId = @event.StreetNameIdAttributeValues
        };
    }

    // Called after anything that can leave the segment in a status that is not active: 'gehistoreerd' or 'niet
    // gerealiseerd'.
    private void UnlinkStreetNamesUnlessActive(Provenance provenance)
    {
        if (Status.IsActive)
        {
            return;
        }

        UnlinkStreetNames(provenance);
    }
}
