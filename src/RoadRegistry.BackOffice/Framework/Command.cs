namespace RoadRegistry.BackOffice.Framework;

using System;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;

public class Command: IRoadRegistryMessage
{
    public Command(object body)
        : this(Guid.NewGuid(),
            default,
            body ?? throw new ArgumentNullException(nameof(body))
        )
    {
    }

    private Command(Guid messageId, ProvenanceData provenanceData, object body)
    {
        MessageId = messageId;
        ProvenanceData = provenanceData;
        Body = body;
    }

    public object Body { get; }
    public Guid MessageId { get; }
    public ProvenanceData ProvenanceData { get; }

    public Command WithMessageId(Guid value)
    {
        return new Command(value, ProvenanceData, Body);
    }

    public Command WithProvenanceData(ProvenanceData? value)
    {
        return value is not null
            ? new Command(MessageId, value, Body)
            : this;
    }
}
