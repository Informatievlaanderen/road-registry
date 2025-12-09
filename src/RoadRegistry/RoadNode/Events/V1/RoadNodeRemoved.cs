namespace RoadRegistry.RoadNode.Events.V1;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;

public class RoadNodeRemoved : IMartenEvent
{
    public required int Id { get; set; }

    public required ProvenanceData Provenance { get; set; }
}
