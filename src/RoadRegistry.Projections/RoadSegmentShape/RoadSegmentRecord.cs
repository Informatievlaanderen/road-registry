namespace RoadRegistry.Projections
{
    public class RoadSegmentRecord
    {
        public int Id { get; set; }
        public byte[] ShapeRecordContent { get; set; }
        public int ShapeRecordContentLength { get; set; }
        public byte[] DbaseRecord { get; set; }
        public BoundingBox2D Envelope { get; set; }
    }
}
