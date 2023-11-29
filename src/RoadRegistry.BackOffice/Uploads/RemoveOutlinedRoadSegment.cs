namespace RoadRegistry.BackOffice.Uploads;

using System;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Messages;

public class RemoveOutlinedRoadSegment : ITranslatedChange
{
    public RemoveOutlinedRoadSegment(RecordNumber recordNumber, RoadSegmentId id)
    {
        RecordNumber = recordNumber;
        Id = id;
    }

    public RoadSegmentId Id { get; }
    public RecordNumber RecordNumber { get; }

    public void TranslateTo(RequestedChange message)
    {
        ArgumentNullException.ThrowIfNull(message);

        message.RemoveOutlinedRoadSegment = new Messages.RemoveOutlinedRoadSegment
        {
            Id = Id
        };
    }
}
