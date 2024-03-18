namespace RoadRegistry.BackOffice.Messages;

using System;
using Be.Vlaanderen.Basisregisters.EventHandling;

[EventName("RoadNetworkChangesArchiveAccepted")]
[EventDescription("Indicates the road network changes archive was accepted.")]
public class RoadNetworkChangesArchiveAccepted : IMessage
{
    public string ArchiveId { get; set; }
    public string Description { get; set; }
    public FileProblem[] Problems { get; set; }
    public Guid? TicketId { get; set; }
    public string When { get; set; }
}
