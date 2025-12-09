namespace RoadRegistry.RoadSegment.Events.V1;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using RoadRegistry.ValueObjects;

public class RoadSegmentRemoved : IMartenEvent
{
    public required int Id { get; set; }
    public required string GeometryDrawMethod { get; set; }

    public required ProvenanceData Provenance { get; set; }
}
