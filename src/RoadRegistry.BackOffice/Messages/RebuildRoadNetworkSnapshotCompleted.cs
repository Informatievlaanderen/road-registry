namespace RoadRegistry.BackOffice.Messages;

using Be.Vlaanderen.Basisregisters.EventHandling;

//[EventName("RebuildRoadNetworkSnapshotCompleted")]
//[EventDescription("Indicates that a rebuild of the road network snapshot was completed")]
public class RebuildRoadNetworkSnapshotCompleted : IMessage
{
    public int StartFromVersion { get; set; }
    public int CurrentVersion { get; set; }
}
