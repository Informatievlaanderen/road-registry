namespace RoadRegistry.BackOffice.Uploads;

using System;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Messages;

public class RemoveRoadNode : ITranslatedChange
{
    public RemoveRoadNode(RecordNumber recordNumber, RoadNodeId id)
    {
        RecordNumber = recordNumber;
        Id = id;
    }

    public RoadNodeId Id { get; }
    public RecordNumber RecordNumber { get; }

    public void TranslateTo(RequestedChange message)
    {
        if (message == null) throw new ArgumentNullException(nameof(message));

        message.RemoveRoadNode = new Messages.RemoveRoadNode
        {
            Id = Id
        };
    }
}
