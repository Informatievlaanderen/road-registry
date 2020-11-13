namespace RoadRegistry.Editor.Schema
{
    public class RoadNetworkInfoSegmentCache
    {
        public int RoadSegmentId { get; set; }
        public int ShapeLength { get; set; }
        public int SurfacesLength { get; set; }
        public int LanesLength { get; set; }
        public int WidthsLength { get; set; }
        public int PartOfEuropeanRoadsLength { get; set; }
        public int PartOfNationalRoadsLength { get; set; }
        public int PartOfNumberedRoadsLength { get; set; }
    }
}
