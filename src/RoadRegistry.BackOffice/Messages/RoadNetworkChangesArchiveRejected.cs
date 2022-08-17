namespace RoadRegistry.BackOffice.Messages;

using Be.Vlaanderen.Basisregisters.EventHandling;

[EventName("RoadNetworkChangesArchiveRejected")]
[EventDescription("Indicates the road network changes archive was rejected.")]
public class RoadNetworkChangesArchiveRejected
{
    public string ArchiveId { get; set; }
    public FileProblem[] Problems { get; set; }
    public string When { get; set; }
}
