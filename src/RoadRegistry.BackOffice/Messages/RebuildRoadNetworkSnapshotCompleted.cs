namespace RoadRegistry.BackOffice.Messages;

using Be.Vlaanderen.Basisregisters.EventHandling;

[EventName("RebuildRoadNetworkSnapshotCompleted")]
[EventDescription("Indicates that a rebuild of the road network snapshot was completed")]
public class RebuildRoadNetworkSnapshotCompleted : IMessage
{
    public int CurrentVersion { get; set; }
    public string When { get; set; }
}
