namespace RoadRegistry.ScopedRoadNetwork;

using System.Collections.Generic;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using RoadRegistry.ValueObjects;

public partial class ScopedRoadNetwork
{
    // Relinks the given road segments from one street name to another; removed road segments are left out.
    public void RelinkStreetName(
        IReadOnlyCollection<RoadSegmentId> roadSegmentIds,
        StreetNameLocalId oldStreetNameId,
        StreetNameLocalId newStreetNameId,
        Provenance provenance)
    {
        // Every event of one save needs an ordinal of its own; without a provider of this change they would all get
        // the same one, and saving more than one road segment would fail on it.
        AttachOrdinalProvider(new EventOrdinalProvider());

        foreach (var roadSegmentId in roadSegmentIds)
        {
            if (_roadSegments.TryGetValue(roadSegmentId, out var segment) && !segment.IsRemoved)
            {
                segment.RelinkStreetName(oldStreetNameId, newStreetNameId, provenance);
            }
        }
    }

    // Lets the given road segments go of a street name; removed road segments are left out.
    public void UnlinkStreetName(
        IReadOnlyCollection<RoadSegmentId> roadSegmentIds,
        StreetNameLocalId streetNameId,
        Provenance provenance)
    {
        AttachOrdinalProvider(new EventOrdinalProvider());

        foreach (var roadSegmentId in roadSegmentIds)
        {
            if (_roadSegments.TryGetValue(roadSegmentId, out var segment) && !segment.IsRemoved)
            {
                segment.UnlinkStreetName(streetNameId, provenance);
            }
        }
    }

    // Lets go of the given street names of each road segment, removed road segments included. Only meant for
    // correcting the links that were left behind before a road segment let go of its street names on its own
    // (GAWR-7784): see RoadSegment.UnlinkStreetNames.
    public void UnlinkStreetNames(IReadOnlyDictionary<RoadSegmentId, IReadOnlyCollection<StreetNameLocalId>> streetNameIdsByRoadSegment, Provenance provenance)
    {
        AttachOrdinalProvider(new EventOrdinalProvider());

        foreach (var (roadSegmentId, streetNameIds) in streetNameIdsByRoadSegment)
        {
            if (_roadSegments.TryGetValue(roadSegmentId, out var segment))
            {
                segment.UnlinkStreetNames(provenance, streetNameIds);
            }
        }
    }
}
