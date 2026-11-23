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
    // Each street name that is let go is recorded as a RoadSegmentStreetNameIdWasChanged to 'niet van toepassing', the
    // event a street name relink already records, so every projection that follows a segment's street names follows
    // this as well. A segment linked to two street names, one on either side, records one event for each.
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
            var updatedStreetNameId = new RoadSegmentDynamicAttributeValues<StreetNameLocalId>(
                Attributes.StreetNameId.Values
                    .Select(value => (value.Coverage, value.Side, value.Value == streetNameId ? StreetNameLocalId.NotApplicable : value.Value)));

            Apply(new RoadSegmentStreetNameIdWasChanged
            {
                RoadSegmentId = RoadSegmentId,
                OldStreetNameId = streetNameId,
                NewStreetNameId = StreetNameLocalId.NotApplicable,
                StreetNameId = updatedStreetNameId,
                Provenance = new ProvenanceData(provenance)
            });
        }
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
