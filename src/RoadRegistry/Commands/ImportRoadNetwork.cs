namespace RoadRegistry.Commands
{
    public class ImportRoadNetwork
    {
        public RoadNode[] Nodes { get; set; }
        public RoadSegment[] Segments { get; set; }
        public EuropeanRoad[] EuropeanRoads { get; set; }
        public NationalRoad[] NationalRoads { get; set; }
        public NumberedRoad[] NumberedRoads { get; set; }
    }
}