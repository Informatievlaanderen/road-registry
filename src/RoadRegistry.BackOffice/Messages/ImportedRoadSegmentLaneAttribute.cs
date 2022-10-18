﻿namespace RoadRegistry.BackOffice.Messages;

public class ImportedRoadSegmentLaneAttribute
{
    public int AsOfGeometryVersion { get; set; }
    public int AttributeId { get; set; }
    public int Count { get; set; }
    public string Direction { get; set; }
    public decimal FromPosition { get; set; }
    public ImportedOriginProperties Origin { get; set; }
    public decimal ToPosition { get; set; }
}