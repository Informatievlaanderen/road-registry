namespace RoadRegistry.ValueObjects.Problems;

using ProblemCodes;
using RoadRegistry.Extensions;

// VAL-5: a segment can only be realized where it can be knotted into the network, so at least one of its two
// endpoints has to have an existing road node within reach. A segment with a node at neither end would be an island.
public class RoadSegmentRealizeNoRoadNodeInReach : Error
{
    public static readonly ProblemCode ProblemCode = ProblemCode.RoadSegment.Realize.NoRoadNodeInReach;

    public RoadSegmentRealizeNoRoadNodeInReach(double maximumDistance)
        : base(ProblemCode.ToString(), new ProblemParameter("MaximumDistance", maximumDistance.ToInvariantString()))
    {
    }
}
