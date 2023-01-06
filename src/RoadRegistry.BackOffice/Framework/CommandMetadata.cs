namespace RoadRegistry.BackOffice.Framework;

public class CommandMetadata
{
    public CommandMetadata(RoadRegistryApplication processor)
    {
        Processor = processor;
    }

    public RoadRegistryApplication Processor { get; }
}
