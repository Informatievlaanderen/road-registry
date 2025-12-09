namespace RoadRegistry.GradeSeparatedJunction.Events.V1;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;

public class GradeSeparatedJunctionRemoved : IMartenEvent
{
    public required int Id { get; set; }

    public required ProvenanceData Provenance { get; init; }
}
