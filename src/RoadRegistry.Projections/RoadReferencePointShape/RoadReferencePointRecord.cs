namespace RoadRegistry.Projections
{
    public class RoadReferencePointRecord
    {
        public int Id { get; set; }
        public byte[] ShapeRecordContent { get; set; }
        public int ShapeRecordContentLength { get; set; }
        public byte[] DbaseRecord { get; set; }
    }
}
