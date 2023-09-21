namespace RoadRegistry.BackOffice.Framework;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;

public sealed class MessageMetadata
{
    public Claim[] Principal { get; init; }
    public ProvenanceData ProvenanceData { get; init; }
    public RoadRegistryApplication Processor { get; set; } = RoadRegistryApplication.BackOffice;
}
