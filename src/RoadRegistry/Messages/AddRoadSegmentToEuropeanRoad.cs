﻿namespace RoadRegistry.Messages
{
    public class AddRoadSegmentToEuropeanRoad
    {
        public int TemporaryAttributeId { get; set; }
        public int SegmentId { get; set; }
        public string RoadNumber { get; set; }
    }
}
