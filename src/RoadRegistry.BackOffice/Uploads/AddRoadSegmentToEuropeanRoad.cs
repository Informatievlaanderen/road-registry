namespace RoadRegistry.BackOffice.Uploads;

using System;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Messages;
using RoadSegment.ValueObjects;

public class AddRoadSegmentToEuropeanRoad : ITranslatedChange
{
    public AddRoadSegmentToEuropeanRoad(
        RecordNumber recordNumber,
        AttributeId temporaryAttributeId,
        RoadSegmentGeometryDrawMethod segmentGeometryDrawMethod,
        RoadSegmentId segmentId,
        EuropeanRoadNumber number)
    {
        RecordNumber = recordNumber;
        TemporaryAttributeId = temporaryAttributeId;
        SegmentGeometryDrawMethod = segmentGeometryDrawMethod;
        SegmentId = segmentId;
        Number = number;
    }

    public EuropeanRoadNumber Number { get; }
    public RecordNumber RecordNumber { get; }
    public RoadSegmentId SegmentId { get; }
    public RoadSegmentGeometryDrawMethod SegmentGeometryDrawMethod { get; }
    public AttributeId TemporaryAttributeId { get; }

    public void TranslateTo(RequestedChange message)
    {
        if (message == null) throw new ArgumentNullException(nameof(message));

        message.AddRoadSegmentToEuropeanRoad = new Messages.AddRoadSegmentToEuropeanRoad
        {
            TemporaryAttributeId = TemporaryAttributeId,
            Number = Number,
            SegmentGeometryDrawMethod = SegmentGeometryDrawMethod,
            SegmentId = SegmentId
        };
    }
}
