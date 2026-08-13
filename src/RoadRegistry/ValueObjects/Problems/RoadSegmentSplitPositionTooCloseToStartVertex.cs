namespace RoadRegistry.ValueObjects.Problems;

using System.Globalization;
using RoadRegistry.ValueObjects.ProblemCodes;

public class RoadSegmentSplitPositionTooCloseToStartVertex : Error
{
    private static readonly ProblemCode ProblemCode = ProblemCode.RoadSegment.Split.PositionTooCloseToStartVertex;

    public RoadSegmentSplitPositionTooCloseToStartVertex(double minimumDistance)
        : base(ProblemCode.ToString(),
            new ProblemParameter("MinimumDistance", minimumDistance.ToString(CultureInfo.InvariantCulture)))
    {
    }
}
