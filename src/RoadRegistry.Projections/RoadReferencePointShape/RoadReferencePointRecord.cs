namespace RoadRegistry.Projections
{
    public class RoadReferencePointRecord
    {
        public int Id { get; set; }
        public byte[] ShapeRecordContent { get; set; }
        public int ShapeRecordContentLegth { get; set; }
        public byte[] DbaseRecord { get; set; }
    }
}
