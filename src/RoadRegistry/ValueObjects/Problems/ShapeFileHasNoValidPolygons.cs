namespace RoadRegistry.ValueObjects.Problems;

using ProblemCodes;

public class ShapeFileHasNoValidPolygons : Error
{
    public ShapeFileHasNoValidPolygons()
        : base(ProblemCode.ShapeFile.HasNoValidPolygons)
    {
    }
}
