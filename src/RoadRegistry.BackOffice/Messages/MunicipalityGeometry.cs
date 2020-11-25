namespace RoadRegistry.BackOffice.Messages
{
    public class MunicipalityGeometry
    {
        public int SpatialReferenceSystemIdentifier{ get; set; }
        public Polygon[] MultiPolygon { get; set; }
    }
}
