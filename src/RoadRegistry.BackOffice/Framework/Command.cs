namespace RoadRegistry.BackOffice.Framework;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using System;
using System.Collections.Generic;
using System.Security.Claims;

public class Command: IRoadRegistryMessage
{
    public Command(object body)
        : this(Guid.NewGuid(),
            new ClaimsPrincipal(new ClaimsIdentity(Array.Empty<System.Security.Claims.Claim>())),
            default,
            body ?? throw new ArgumentNullException(nameof(body))
        )
    {
    }

    private Command(Guid messageId, ClaimsPrincipal principal, ProvenanceData provenanceData, object body)
    {
        MessageId = messageId;
        Principal = principal;
        ProvenanceData = provenanceData;
        Body = body;
    }

    public object Body { get; }
    public Guid MessageId { get; }
    public ClaimsPrincipal Principal { get; }
    public ProvenanceData ProvenanceData { get; }

    public Command WithMessageId(Guid value)
    {
        return new Command(value, Principal, ProvenanceData, Body);
    }
    
    public Command WithPrincipal(ClaimsPrincipal value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return new Command(MessageId, value, ProvenanceData, Body);
    }

    public Command WithProvenanceData(ProvenanceData value)
    {
        return value is not null
            ? new Command(MessageId, Principal, value, Body)
            : this;
    }
}
