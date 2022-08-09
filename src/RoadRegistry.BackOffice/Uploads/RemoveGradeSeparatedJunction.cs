namespace RoadRegistry.BackOffice.Uploads;

using System;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Messages;

public class RemoveGradeSeparatedJunction : ITranslatedChange
{
    public RemoveGradeSeparatedJunction(RecordNumber recordNumber, GradeSeparatedJunctionId id)
    {
        RecordNumber = recordNumber;
        Id = id;
    }

    public RecordNumber RecordNumber { get; }
    public GradeSeparatedJunctionId Id { get; }

    public void TranslateTo(RequestedChange message)
    {
        if (message == null) throw new ArgumentNullException(nameof(message));

        message.RemoveGradeSeparatedJunction = new Messages.RemoveGradeSeparatedJunction
        {
            Id = Id
        };
    }
}
