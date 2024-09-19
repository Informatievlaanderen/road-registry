namespace RoadRegistry.BackOffice.Messages;

using System;
using Be.Vlaanderen.Basisregisters.EventHandling;

[EventName("ExtractHostSystemHealthCheckRequested")]
[EventDescription("Indicates the system health check for the extract host was requested.")]
public class ExtractHostSystemHealthCheckRequested : IMessage
{
    public Guid TicketId { get; set; }
    public string BucketFileName { get; set; }
    public string When { get; set; }
}
