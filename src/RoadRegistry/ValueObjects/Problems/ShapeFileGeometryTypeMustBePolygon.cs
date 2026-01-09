namespace RoadRegistry.ValueObjects.Problems;

using ProblemCodes;

public class ShapeFileGeometryTypeMustBePolygon : Error
{
    public ShapeFileGeometryTypeMustBePolygon()
        : base(ProblemCode.ShapeFile.GeometryTypeMustBePolygon)
    {
    }
}
