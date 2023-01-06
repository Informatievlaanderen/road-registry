namespace RoadRegistry.BackOffice.Framework;

public sealed class MessageMetadata
{
    public Claim[] Principal { get; init; }
    public RoadRegistryApplication Processor { get; set; } = RoadRegistryApplication.BackOffice;
}
