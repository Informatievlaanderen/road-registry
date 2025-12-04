namespace RoadRegistry.ValueObjects.Problems;

using ProblemCodes;

public class ShapeFileGeometrySridMustBeEqual : Error
{
    public ShapeFileGeometrySridMustBeEqual()
        : base(ProblemCode.ShapeFile.GeometrySridMustBeEqual)
    {
    }
}
