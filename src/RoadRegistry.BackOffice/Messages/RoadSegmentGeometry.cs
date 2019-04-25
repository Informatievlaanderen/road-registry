namespace RoadRegistry.BackOffice.Messages
{
    public class RoadSegmentGeometry
    {
        public int SpatialReferenceSystemIdentifier { get; set; }
        public LineString[] MultiLineString { get; set; }
    }
}
