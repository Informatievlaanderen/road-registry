namespace RoadRegistry.BackOffice.Messages;

using Be.Vlaanderen.Basisregisters.EventHandling;

[EventName("RoadNetworkChangesArchiveUploaded")]
[EventDescription("Indicates the road network changes archive was uploaded.")]
public class RoadNetworkChangesArchiveUploaded
{
    public string ArchiveId { get; set; }
    public string When { get; set; }
    public bool IsFeatureCompare { get; set; }
}
