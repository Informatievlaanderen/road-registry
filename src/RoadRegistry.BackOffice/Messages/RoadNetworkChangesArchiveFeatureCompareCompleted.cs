namespace RoadRegistry.BackOffice.Messages;

using Be.Vlaanderen.Basisregisters.EventHandling;

[EventName("RoadNetworkChangesArchiveFeatureCompareCompleted")]
[EventDescription("Indicates the road network changes archive has gone through feature compare.")]
public class RoadNetworkChangesArchiveFeatureCompareCompleted : IMessage
{
    public string ArchiveId { get; set; }
    public string Description { get; set; }
    public string When { get; set; }
}
