namespace RoadRegistry.ScopedRoadNetwork;

using System.Collections.Generic;
using System.Linq;
using RoadRegistry.RoadSegment.ValueObjects;
using RoadRegistry.ValueObjects.Problems;

public partial class ScopedRoadNetwork
{
    // A road segment whose inwinning is not finished is not a V2 segment yet: it carries no attributes, so there is
    // nothing to record about it and nothing to derive from it. Every editing action refuses it, and the actions on
    // the objects that are *about* road segments - the crossings, and the road nodes they meet at - refuse it too.
    //
    // Only the segments the road network actually knows are judged: an identifier it does not have is the calling
    // action's to report as it sees fit, and 'has not migrated' would be the wrong thing to say about it.
    private Problems ValidateRoadSegmentsHaveCompletedInwinning(IEnumerable<RoadSegmentId> roadSegmentIds)
    {
        var problems = Problems.None;

        foreach (var roadSegmentId in roadSegmentIds.Distinct())
        {
            if (_roadSegments.TryGetValue(roadSegmentId, out var roadSegment) && !roadSegment.HasMigrated())
            {
                problems += new RoadSegmentNotCompletedInwinning(roadSegmentId);
            }
        }

        return problems;
    }
}
