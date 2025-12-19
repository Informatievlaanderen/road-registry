namespace RoadRegistry.BackOffice.Messages;

using System;
using Be.Vlaanderen.Basisregisters.EventHandling;
using RoadRegistry.Extracts.Messages;

[EventName("RoadNetworkChangesArchiveRejected")]
[EventDescription("Indicates the road network changes archive was rejected.")]
public class RoadNetworkChangesArchiveRejected : IMessage, IWhen
{
    public string ArchiveId { get; set; }
    public FileProblem[] Problems { get; set; }
    public string Description { get; set; }
    public Guid? TicketId { get; set; }
    public string When { get; set; }
}
