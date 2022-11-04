namespace RoadRegistry.BackOffice.Core;

public class ShapeFileGeometryTypeMustBePolygon : Error
{
    public ShapeFileGeometryTypeMustBePolygon()
        : base(nameof(ShapeFileGeometryTypeMustBePolygon))
    {
    }
}
