namespace RoadRegistry.BackOffice.Core;

using ProblemCodes;

public class ShapeFileInvalidPolygonShellOrientation : Error
{
    public ShapeFileInvalidPolygonShellOrientation()
        : base(ProblemCode.ShapeFile.InvalidPolygonShellOrientation)
    {
    }
}
