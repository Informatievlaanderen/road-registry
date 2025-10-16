namespace RoadRegistry.BackOffice.Core;

using ProblemCodes;

public class ShapeFileHasNoValidPolygons : Error
{
    public ShapeFileHasNoValidPolygons()
        : base(ProblemCode.ShapeFile.HasNoValidPolygons)
    {
    }
}
