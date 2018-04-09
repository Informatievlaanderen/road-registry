namespace RoadRegistry.Commands
{
    public class ImportRoadNetwork
    {
        public RoadNode[] Nodes { get; set; }
        public RoadSegment[] Segments { get; set; }
        public GradeSeparatedJunction[] GradeSeparatedJunctions { get; set; }
    }
}