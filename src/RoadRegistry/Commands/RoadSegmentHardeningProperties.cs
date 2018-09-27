using RoadRegistry.Shared;

namespace RoadRegistry.Commands
{
    public class RoadSegmentHardeningProperties
    {
        public HardeningType Type { get; set; }
        public double FromPosition { get; set; }
        public double ToPosition { get; set; }
    }
}
