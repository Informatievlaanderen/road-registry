namespace RoadRegistry.RoadSegment.Events.V1;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using ValueObjects;

public class RoadSegmentGeometryModified : IMartenEvent
{
    public required int RoadSegmentId { get; set; }
    public required int Version { get; set; }
    public required int GeometryVersion { get; set; }
    public required RoadSegmentGeometry Geometry { get; set; }
    public required RoadSegmentLaneAttributes[] Lanes { get; set; }
    public required RoadSegmentSurfaceAttributes[] Surfaces { get; set; }
    public required RoadSegmentWidthAttributes[] Widths { get; set; }

    public required ProvenanceData Provenance { get; set; }
}
