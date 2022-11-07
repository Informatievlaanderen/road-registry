namespace RoadRegistry.BackOffice.Core;

public class ShapeFileHasNoValidPolygons : Error
{
    public ShapeFileHasNoValidPolygons() : base(nameof(ShapeFileHasNoValidPolygons))
    {
    }
}
