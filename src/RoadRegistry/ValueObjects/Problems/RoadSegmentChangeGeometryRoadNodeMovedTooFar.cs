namespace RoadRegistry.ValueObjects.Problems;

using ProblemCodes;
using RoadRegistry.Extensions;
using RoadRegistry.RoadSegment.ValueObjects;

// VAL-22: how far a road node may be dragged depends on what it is. An 'eindknoop' terminates a road and may travel
// up to 100m; a 'validatieknoop' or 'echte knoop' has other segments hanging off it and may only travel 20m. Which
// road segment dragged it comes from the problem context; the node itself is its own parameter.
public class RoadSegmentChangeGeometryRoadNodeMovedTooFar : Error
{
    public static readonly ProblemCode ProblemCode = ProblemCode.RoadSegment.ChangeGeometry.RoadNodeMovedTooFar;

    public RoadSegmentChangeGeometryRoadNodeMovedTooFar(RoadNodeId roadNodeId, double maximumDistance)
        : base(ProblemCode.ToString(),
            new ProblemParameter("RoadNodeId", roadNodeId.ToString()),
            new ProblemParameter("MaximumDistance", maximumDistance.ToInvariantString()))
    {
    }
}
