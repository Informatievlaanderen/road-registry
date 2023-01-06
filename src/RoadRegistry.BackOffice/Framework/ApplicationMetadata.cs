namespace RoadRegistry.BackOffice.Framework;

public class ApplicationMetadata
{
    public ApplicationMetadata(RoadRegistryApplication messageProcessor)
    {
        MessageProcessor = messageProcessor;
    }

    public RoadRegistryApplication MessageProcessor { get; }
}
