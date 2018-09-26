using RoadRegistry.Shared;

namespace RoadRegistry.Commands
{
    public class RoadSegmentNumberedRoadProperties
    {
        public string Ident8 { get; set; }
        public NumberedRoadSegmentDirection Direction { get; set; }        
        public int Ordinal { get; set; }
    }
}
