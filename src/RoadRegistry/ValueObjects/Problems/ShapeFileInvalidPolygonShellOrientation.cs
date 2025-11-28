namespace RoadRegistry.ValueObjects.Problems;

using ProblemCodes;

public class ShapeFileInvalidPolygonShellOrientation : Error
{
    public ShapeFileInvalidPolygonShellOrientation()
        : base(ProblemCode.ShapeFile.InvalidPolygonShellOrientation)
    {
    }
}
