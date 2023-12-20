namespace RoadRegistry.BackOffice.Framework;

using System;
using System.Security.Claims;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;

public class Event<TBody> : IRoadRegistryMessage
{
    public Event(Event @event)
    {
        ArgumentNullException.ThrowIfNull(@event);

        MessageId = @event.MessageId;
        Body = (TBody)@event.Body;
        Principal = @event.Principal;
        ProvenanceData = @event.ProvenanceData;
        StreamId = @event.StreamId;
        StreamVersion = @event.StreamVersion;
    }

    public Guid MessageId { get; }
    public TBody Body { get; }
    public ClaimsPrincipal Principal { get; }
    public string StreamId { get; }
    public int StreamVersion { get; }
    public ProvenanceData ProvenanceData { get; }
}
