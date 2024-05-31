namespace RoadRegistry.Integration.Schema;

using NetTopologySuite.Geometries;

public class MunicipalityGeometry
{
    public Geometry Geometry { get; set; }
    public string NisCode { get; set; }
}