namespace RoadRegistry.RoadSegment.Events.V1;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;

public class RoadSegmentRemoved : IMartenEvent
{
    public required int RoadSegmentId { get; set; }
    public required string GeometryDrawMethod { get; set; }

    public required ProvenanceData Provenance { get; set; }
}
