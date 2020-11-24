namespace RoadRegistry.Editor.Schema
{
    using NetTopologySuite.Geometries;

    public class MunicipalityGeometryRecord
    {
        public string NisCode { get; set; }

        public Geometry Geometry { get; set; }
    }
}
