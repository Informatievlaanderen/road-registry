namespace RoadRegistry.ValueObjects.Problems;

using System.Linq;
using RoadRegistry.Extensions;
using RoadRegistry.ValueObjects.ProblemCodes;

public class RoadSegmentGeometryTaken : Error
{
    public RoadSegmentGeometryTaken(RoadSegmentIdReference byOtherSegment)
        : base(ProblemCode.RoadSegment.Geometry.Taken.ToString(),
            byOtherSegment.ToRoadSegmentProblemParameters("ByOtherWegsegment").ToArray())
    {
    }

    public RoadSegmentGeometryTaken(RoadSegmentId byOtherSegment)
        : base(ProblemCode.RoadSegment.Geometry.Taken,
            new ProblemParameter(
                "ByOtherSegment",
                byOtherSegment.ToInt32().ToString()))
    {
    }
}
