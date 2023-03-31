namespace RoadRegistry.BackOffice.Core.ProblemCodes;

public sealed partial record ProblemCode
{
    public static class ShapeFile
    {
        public static ProblemCode HasNoValidPolygons = new("ShapeFileHasNoValidPolygons");
        public static ProblemCode GeometrySridMustBeEqual = new("ShapeFileGeometrySridMustBeEqual");
        public static ProblemCode GeometryTypeMustBePolygon = new("ShapeFileGeometryTypeMustBePolygon");
        public static ProblemCode InvalidHeader = new("ShapeFileInvalidHeader");
        public static ProblemCode InvalidPolygonShellOrientation = new("ShapeFileInvalidPolygonShellOrientation");
    }
}
