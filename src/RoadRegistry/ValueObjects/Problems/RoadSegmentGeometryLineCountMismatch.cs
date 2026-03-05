namespace RoadRegistry.ValueObjects.Problems;

using RoadRegistry.ValueObjects.ProblemCodes;

public class RoadSegmentGeometryLineCountMismatch : Error
{
    public RoadSegmentGeometryLineCountMismatch(int expectedLineCount, int actualLineCount)
        : base(ProblemCode.RoadSegment.Geometry.LineCountMismatch.ToString(),
            new ProblemParameter("ExpectedLineCount", expectedLineCount.ToString()),
            new ProblemParameter("ActualLineCount", actualLineCount.ToString()))
    {
    }
}
