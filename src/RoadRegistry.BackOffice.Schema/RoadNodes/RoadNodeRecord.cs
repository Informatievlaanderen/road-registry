namespace RoadRegistry.BackOffice.Schema.RoadNodes
{
    public class RoadNodeRecord
    {
        public int Id { get; set; }
        public byte[] ShapeRecordContent { get; set; }
        public int ShapeRecordContentLength { get; set; }
        public byte[] DbaseRecord { get; set; }
        public RoadNodeBoundingBox BoundingBox { get; set; }
    }
}
