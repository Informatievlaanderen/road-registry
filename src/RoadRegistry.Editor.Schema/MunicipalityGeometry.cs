namespace RoadRegistry.Editor.Schema;

using NetTopologySuite.Geometries;

public class MunicipalityGeometry
{
    public Geometry Geometry { get; set; }
    public string NisCode { get; set; }
}