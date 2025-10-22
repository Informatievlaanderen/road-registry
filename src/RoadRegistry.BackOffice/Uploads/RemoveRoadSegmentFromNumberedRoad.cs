namespace RoadRegistry.BackOffice.Uploads;

using System;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Messages;
using RoadSegment.ValueObjects;

public class RemoveRoadSegmentFromNumberedRoad : ITranslatedChange
{
    public RemoveRoadSegmentFromNumberedRoad(
        RecordNumber recordNumber,
        AttributeId attributeId,
        RoadSegmentGeometryDrawMethod segmentGeometryDrawMethod,
        RoadSegmentId segmentId,
        NumberedRoadNumber number)
    {
        RecordNumber = recordNumber;
        AttributeId = attributeId;
        SegmentGeometryDrawMethod = segmentGeometryDrawMethod;
        SegmentId = segmentId;
        Number = number;
    }

    public RecordNumber RecordNumber { get; }
    public AttributeId AttributeId { get; }
    public RoadSegmentGeometryDrawMethod SegmentGeometryDrawMethod { get; }
    public RoadSegmentId SegmentId { get; }
    public NumberedRoadNumber Number { get; }

    public void TranslateTo(RequestedChange message)
    {
        if (message == null) throw new ArgumentNullException(nameof(message));

        message.RemoveRoadSegmentFromNumberedRoad = new Messages.RemoveRoadSegmentFromNumberedRoad
        {
            AttributeId = AttributeId,
            Number = Number,
            SegmentGeometryDrawMethod = SegmentGeometryDrawMethod,
            SegmentId = SegmentId
        };
    }
}
