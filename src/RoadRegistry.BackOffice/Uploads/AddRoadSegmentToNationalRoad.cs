namespace RoadRegistry.BackOffice.Uploads;

using System;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Messages;

public class AddRoadSegmentToNationalRoad : ITranslatedChange
{
    public AddRoadSegmentToNationalRoad(
        RecordNumber recordNumber,
        AttributeId temporaryAttributeId,
        RoadSegmentId segmentId,
        NationalRoadNumber number)
    {
        RecordNumber = recordNumber;
        TemporaryAttributeId = temporaryAttributeId;
        SegmentId = segmentId;
        Number = number;
    }

    public RecordNumber RecordNumber { get; }
    public AttributeId TemporaryAttributeId { get; }
    public RoadSegmentId SegmentId { get; }
    public NationalRoadNumber Number { get; }

    public void TranslateTo(RequestedChange message)
    {
        if (message == null) throw new ArgumentNullException(nameof(message));

        message.AddRoadSegmentToNationalRoad = new Messages.AddRoadSegmentToNationalRoad
        {
            TemporaryAttributeId = TemporaryAttributeId,
            Number = Number,
            SegmentId = SegmentId
        };
    }
}
