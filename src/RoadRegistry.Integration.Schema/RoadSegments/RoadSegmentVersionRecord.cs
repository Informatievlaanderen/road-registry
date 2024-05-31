namespace RoadRegistry.Integration.Schema.RoadSegments;

using System;

public class RoadSegmentVersionRecord
{
    public string StreamId { get; set; }
    public int Id { get; set; }
    public int Method { get; set; }
    public int Version { get; set; }
    public int GeometryVersion { get; set; }
    public DateTime RecordingDate { get; set; }
    public bool IsRemoved { get; set; }
}
