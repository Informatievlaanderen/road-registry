namespace RoadRegistry.BackOffice.Messages;

using System;
using Be.Vlaanderen.Basisregisters.EventHandling;

[EventName("EventHostSystemHealthCheckRequested")]
[EventDescription("Indicates the system health check for the event host was requested.")]
public class EventHostSystemHealthCheckRequested : IMessage, IWhen
{
    public Guid TicketId { get; set; }
    public string AssemblyVersion { get; set; }
    public string BucketFileName { get; set; }
    public string When { get; set; }
}
