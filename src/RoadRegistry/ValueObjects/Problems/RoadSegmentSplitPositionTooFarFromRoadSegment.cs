namespace RoadRegistry.ValueObjects.Problems;

using ProblemCodes;
using RoadRegistry.Extensions;

public class RoadSegmentSplitPositionTooFarFromRoadSegment : Error
{
    public static readonly ProblemCode ProblemCode = ProblemCode.RoadSegment.Split.PositionTooFarFromRoadSegment;

    public RoadSegmentSplitPositionTooFarFromRoadSegment(double maximumDistance)
        : base(ProblemCode,
            new ProblemParameter("MaximumDistance", maximumDistance.ToInvariantString()))
    {
    }
}
