namespace RoadRegistry.ValueObjects.Problems;

using System.Globalization;
using RoadRegistry.ValueObjects.ProblemCodes;

public class RoadSegmentSplitPositionTooCloseToEndVertex : Error
{
    private static readonly ProblemCode ProblemCode = ProblemCode.RoadSegment.Split.PositionTooCloseToEndVertex;

    public RoadSegmentSplitPositionTooCloseToEndVertex(double minimumDistance)
        : base(ProblemCode.ToString(),
            new ProblemParameter("MinimumDistance", minimumDistance.ToString(CultureInfo.InvariantCulture)))
    {
    }
}
