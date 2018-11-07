namespace RoadRegistry.Commands
{
    using System.Collections.Generic;
    using System.Linq;
    using RoadRegistry.Shared;

    public class RequestedChange
    {
        public AddRoadNode AddRoadNode { get; set; }
        public AddRoadSegment AddRoadSegment { get; set; }
    }
}
