﻿namespace RoadRegistry.Messages
{
    public class RoadSegmentLaneAttributes
    {
        public int AttributeId { get; set; }
        public int Count { get; set; }
        public LaneDirection Direction { get; set; }
        public decimal FromPosition { get; set; }
        public decimal ToPosition { get; set; }
        public int AsOfGeometryVersion { get; set; }
    }
}
