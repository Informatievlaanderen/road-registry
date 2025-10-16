namespace RoadRegistry.BackOffice.Core;

using ProblemCodes;

public class ShapeFileGeometryTypeMustBePolygon : Error
{
    public ShapeFileGeometryTypeMustBePolygon()
        : base(ProblemCode.ShapeFile.GeometryTypeMustBePolygon)
    {
    }
}
