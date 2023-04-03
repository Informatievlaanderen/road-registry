namespace RoadRegistry.BackOffice.Core.ProblemCodes;

public sealed partial record ProblemCode
{
    public static class ShapeFile
    {
        public static readonly ProblemCode HasNoValidPolygons = new("ShapeFileHasNoValidPolygons");
        public static readonly ProblemCode GeometrySridMustBeEqual = new("ShapeFileGeometrySridMustBeEqual");
        public static readonly ProblemCode GeometryTypeMustBePolygon = new("ShapeFileGeometryTypeMustBePolygon");
        public static readonly ProblemCode InvalidHeader = new("ShapeFileInvalidHeader");
        public static readonly ProblemCode InvalidPolygonShellOrientation = new("ShapeFileInvalidPolygonShellOrientation");
    }
}
