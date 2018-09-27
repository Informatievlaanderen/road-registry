using RoadRegistry.Shared;

namespace RoadRegistry.Commands
{
    public class RoadSegmentLaneProperties
    {
        public int Count { get; set; }
        public LaneDirection Direction { get; set; }
        public double FromPosition { get; set; }
        public double ToPosition { get; set; }
    }
}
