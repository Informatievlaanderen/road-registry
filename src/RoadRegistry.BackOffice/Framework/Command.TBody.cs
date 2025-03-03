namespace RoadRegistry.BackOffice.Framework;

using System;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;

public class Command<TBody>: IRoadRegistryMessage
{
    public Command(Command command)
    {
        ArgumentNullException.ThrowIfNull(command);

        MessageId = command.MessageId;
        Body = (TBody)command.Body;
        ProvenanceData = command.ProvenanceData;
    }

    public Guid MessageId { get; }
    public TBody Body { get; }
    public ProvenanceData ProvenanceData { get; }
}
