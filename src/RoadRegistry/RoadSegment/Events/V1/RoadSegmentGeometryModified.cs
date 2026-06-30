namespace RoadRegistry.RoadSegment.Events.V1;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using ValueObjects;
using System.Collections.Generic;
using Be.Vlaanderen.Basisregisters.GrAr.Common;
using RoadRegistry.BackOffice;

public class RoadSegmentGeometryModified : IMartenEvent
{
    public const string EventName = "RoadSegmentGeometryModified"; // BE CAREFUL CHANGING THIS!!

    public required int RoadSegmentId { get; set; }
    public required int Version { get; set; }
    public required int GeometryVersion { get; set; }
    public required RoadSegmentGeometry Geometry { get; set; }
    public required RoadSegmentLaneAttributes[] Lanes { get; set; }
    public required RoadSegmentSurfaceAttributes[] Surfaces { get; set; }
    public required RoadSegmentWidthAttributes[] Widths { get; set; }

    public required ProvenanceData Provenance { get; set; }

    public IEnumerable<string> GetHashFields() => ObjectHasher.GetHashFields(this);
    public string GetHash() => this.ToEventHash(EventName);
}
