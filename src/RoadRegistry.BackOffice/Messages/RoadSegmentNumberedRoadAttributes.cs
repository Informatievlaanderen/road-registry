﻿namespace RoadRegistry.BackOffice.Messages;

public class RoadSegmentNumberedRoadAttributes
{
    public int AttributeId { get; set; }
    public string Direction { get; set; }
    public string Number { get; set; }
    public int Ordinal { get; set; }
}