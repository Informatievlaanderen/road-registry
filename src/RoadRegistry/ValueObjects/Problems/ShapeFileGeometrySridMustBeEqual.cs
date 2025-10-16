namespace RoadRegistry.BackOffice.Core;

using ProblemCodes;

public class ShapeFileGeometrySridMustBeEqual : Error
{
    public ShapeFileGeometrySridMustBeEqual()
        : base(ProblemCode.ShapeFile.GeometrySridMustBeEqual)
    {
    }
}
