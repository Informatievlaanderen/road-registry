namespace RoadRegistry.BackOffice.Schema.RoadSegments
{
    using RoadNodes;

    public class RoadSegmentRecord
    {
        public int Id { get; set; }
        public byte[] ShapeRecordContent { get; set; }
        public int ShapeRecordContentLength { get; set; }
        public byte[] DbaseRecord { get; set; }
        public RoadSegmentBoundingBox BoundingBox { get; set; }
    }
}
