namespace RoadRegistry.ScopedRoadNetwork;

using System.Collections.Generic;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using RoadRegistry.ValueObjects;

public partial class ScopedRoadNetwork
{
    public void ChangeStreetNameId(
        IReadOnlyCollection<RoadSegmentId> roadSegmentIds,
        StreetNameLocalId oldStreetNameId,
        StreetNameLocalId newStreetNameId,
        Provenance provenance)
    {
        foreach (var roadSegmentId in roadSegmentIds)
        {
            if (_roadSegments.TryGetValue(roadSegmentId, out var segment) && !segment.IsRemoved)
            {
                segment.ChangeStreetNameId(oldStreetNameId, newStreetNameId, provenance);
            }
        }
    }
}
