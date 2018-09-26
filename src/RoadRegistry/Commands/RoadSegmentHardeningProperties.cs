using RoadRegistry.Shared;

namespace RoadRegistry.Commands
{
    public class RoadSegmentHardeningProperties
    {
        public HardeningType Type { get; set; }
        public decimal FromPosition { get; set; }
        public decimal ToPosition { get; set; }
    }
}
