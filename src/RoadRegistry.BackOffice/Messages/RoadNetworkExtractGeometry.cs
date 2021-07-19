namespace RoadRegistry.BackOffice.Messages
{
    public class RoadNetworkExtractGeometry
    {
        public int SpatialReferenceSystemIdentifier{ get; set; }
        public Polygon[] MultiPolygon { get; set; }
        public Polygon Polygon { get; set; }
    }
}
