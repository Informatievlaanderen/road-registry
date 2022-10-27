namespace RoadRegistry.BackOffice.Uploads;

using System;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Messages;

public class RemoveRoadSegmentFromEuropeanRoad : ITranslatedChange
{
    public RemoveRoadSegmentFromEuropeanRoad(
        RecordNumber recordNumber,
        AttributeId attributeId,
        RoadSegmentId segmentId,
        EuropeanRoadNumber number)
    {
        RecordNumber = recordNumber;
        AttributeId = attributeId;
        SegmentId = segmentId;
        Number = number;
    }

    public AttributeId AttributeId { get; }
    public EuropeanRoadNumber Number { get; }
    public RecordNumber RecordNumber { get; }
    public RoadSegmentId SegmentId { get; }

    public void TranslateTo(RequestedChange message)
    {
        if (message == null) throw new ArgumentNullException(nameof(message));

        message.RemoveRoadSegmentFromEuropeanRoad = new Messages.RemoveRoadSegmentFromEuropeanRoad
        {
            AttributeId = AttributeId,
            Number = Number,
            SegmentId = SegmentId
        };
    }
}