namespace RoadRegistry.ValueObjects.Problems;

using ProblemCodes;
using RoadRegistry.Extensions;
using RoadRegistry.RoadSegment.ValueObjects;

// VAL-21: a moved start or end vertex must stay clear of the other road nodes. Landing on one would connect the
// segment to a different node, which is a topology change and out of scope for this action. Which road segment moved
// comes from the problem context; the road node it came too close to is its own parameter.
public class RoadSegmentChangeGeometryPointTooCloseToRoadNode : Error
{
    public static readonly ProblemCode ProblemCode = ProblemCode.RoadSegment.ChangeGeometry.PointTooCloseToRoadNode;

    public RoadSegmentChangeGeometryPointTooCloseToRoadNode(RoadNodeId roadNodeId, double minimumDistance)
        : base(ProblemCode.ToString(),
            new ProblemParameter("RoadNodeId", roadNodeId.ToString()),
            new ProblemParameter("MinimumDistance", minimumDistance.ToInvariantString()))
    {
    }
}
