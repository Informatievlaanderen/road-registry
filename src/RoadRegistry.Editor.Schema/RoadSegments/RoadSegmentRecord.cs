namespace RoadRegistry.Editor.Schema.RoadSegments
{
    using NetTopologySuite.Geometries;

    public class RoadSegmentRecord
    {
        public int Id { get; set; }
        public byte[] ShapeRecordContent { get; set; }
        public int ShapeRecordContentLength { get; set; }
        public byte[] DbaseRecord { get; set; }
        //public Geometry Geometry { get; set; }
        public RoadSegmentBoundingBox BoundingBox { get; set; }
    }
}
