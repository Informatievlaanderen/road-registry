namespace RoadRegistry.RoadSegment.Events.V1;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;

public class OutlinedRoadSegmentRemoved : IMartenEvent
{
    public required int Id { get; set; }

    public required ProvenanceData Provenance { get; set; }
}
