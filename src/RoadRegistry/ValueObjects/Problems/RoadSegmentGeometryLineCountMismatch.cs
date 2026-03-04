namespace RoadRegistry.ValueObjects.Problems;

using RoadRegistry.ValueObjects.ProblemCodes;

public class RoadSegmentGeometryLineCountMismatch : Error
{
    public RoadSegmentGeometryLineCountMismatch(RoadSegmentId identifier, int expectedLineCount, int actualLineCount)
        : base(ProblemCode.RoadSegment.Geometry.LineCountMismatch.ToString(),
            new ProblemParameter("Identifier", identifier.ToString()),
            new ProblemParameter("ExpectedLineCount", expectedLineCount.ToString()),
            new ProblemParameter("ActualLineCount", actualLineCount.ToString()))
    {
    }
}
