namespace RoadRegistry.ValueObjects.Problems;

using ProblemCodes;
using RoadRegistry.RoadSegment.ValueObjects;

public class RoadSegmentSplitPositionTooCloseToRoadNode : Error
{
    public static readonly ProblemCode ProblemCode = ProblemCode.RoadSegment.Split.PositionTooCloseToRoadNode;

    public RoadSegmentSplitPositionTooCloseToRoadNode(RoadNodeId roadNodeId, double minimumDistance)
        : base(ProblemCode,
            new ProblemParameter("RoadNodeId", roadNodeId.ToString()),
            new ProblemParameter("MinimumDistance", minimumDistance.ToString(System.Globalization.CultureInfo.InvariantCulture)))
    {
    }
}
