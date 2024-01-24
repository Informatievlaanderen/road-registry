namespace RoadRegistry.BackOffice.Uploads;

using System;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Messages;

public class RemoveOutlinedRoadSegmentFromRoadNetwork : ITranslatedChange
{
    public RemoveOutlinedRoadSegmentFromRoadNetwork(RecordNumber recordNumber, RoadSegmentId id)
    {
        RecordNumber = recordNumber;
        Id = id;
    }

    public RoadSegmentId Id { get; }
    public RecordNumber RecordNumber { get; }

    public void TranslateTo(RequestedChange message)
    {
        if (message == null) throw new ArgumentNullException(nameof(message));

        message.RemoveOutlinedRoadSegmentFromRoadNetwork = new Messages.RemoveOutlinedRoadSegmentFromRoadNetwork
        {
            Id = Id
        };
    }
}
