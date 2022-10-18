namespace RoadRegistry.BackOffice.Uploads;

using System;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Messages;

public class RemoveRoadSegmentFromNationalRoad : ITranslatedChange
{
    public RemoveRoadSegmentFromNationalRoad(
        RecordNumber recordNumber,
        AttributeId attributeId,
        RoadSegmentId segmentId,
        NationalRoadNumber number)
    {
        RecordNumber = recordNumber;
        AttributeId = attributeId;
        SegmentId = segmentId;
        Number = number;
    }

    public AttributeId AttributeId { get; }
    public NationalRoadNumber Number { get; }

    public RecordNumber RecordNumber { get; }
    public RoadSegmentId SegmentId { get; }

    public void TranslateTo(RequestedChange message)
    {
        if (message == null) throw new ArgumentNullException(nameof(message));

        message.RemoveRoadSegmentFromNationalRoad = new Messages.RemoveRoadSegmentFromNationalRoad
        {
            AttributeId = AttributeId,
            Number = Number,
            SegmentId = SegmentId
        };
    }
}
