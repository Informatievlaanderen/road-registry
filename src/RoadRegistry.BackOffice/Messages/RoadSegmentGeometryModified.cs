namespace RoadRegistry.BackOffice.Messages;

using Be.Vlaanderen.Basisregisters.GrAr.Common;

public class RoadSegmentGeometryModified : IHaveHash
{
    public const string EventName = "RoadSegmentGeometryModified";

    public int Id { get; set; }
    public int Version { get; set; }
    public int GeometryVersion { get; set; }
    
    public RoadSegmentGeometry Geometry { get; set; }
    public RoadSegmentLaneAttributes[] Lanes { get; set; }
    public RoadSegmentSurfaceAttributes[] Surfaces { get; set; }
    public RoadSegmentWidthAttributes[] Widths { get; set; }

    public System.Collections.Generic.IEnumerable<string> GetHashFields() => ObjectHasher.GetHashFields(this);
    public string GetHash() => this.ToEventHash(EventName);
}
