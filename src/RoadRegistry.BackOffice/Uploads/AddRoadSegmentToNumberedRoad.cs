namespace RoadRegistry.BackOffice.Uploads;

using System;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Messages;

public class AddRoadSegmentToNumberedRoad : ITranslatedChange
{
    public AddRoadSegmentToNumberedRoad(
        RecordNumber recordNumber,
        AttributeId temporaryAttributeId,
        RoadSegmentGeometryDrawMethod segmentGeometryDrawMethod,
        RoadSegmentId segmentId,
        NumberedRoadNumber number,
        RoadSegmentNumberedRoadDirection direction,
        RoadSegmentNumberedRoadOrdinal ordinal)
    {
        RecordNumber = recordNumber;
        TemporaryAttributeId = temporaryAttributeId;
        SegmentGeometryDrawMethod = segmentGeometryDrawMethod;
        SegmentId = segmentId;
        Number = number;
        Direction = direction;
        Ordinal = ordinal;
    }

    public RecordNumber RecordNumber { get; }
    public AttributeId TemporaryAttributeId { get; }
    public RoadSegmentGeometryDrawMethod SegmentGeometryDrawMethod { get; }
    public RoadSegmentId SegmentId { get; }
    public NumberedRoadNumber Number { get; }
    public RoadSegmentNumberedRoadDirection Direction { get; }
    public RoadSegmentNumberedRoadOrdinal Ordinal { get; }

    public void TranslateTo(RequestedChange message)
    {
        if (message == null) throw new ArgumentNullException(nameof(message));

        message.AddRoadSegmentToNumberedRoad = new Messages.AddRoadSegmentToNumberedRoad
        {
            TemporaryAttributeId = TemporaryAttributeId,
            Number = Number,
            Direction = Direction,
            Ordinal = Ordinal,
            SegmentGeometryDrawMethod = SegmentGeometryDrawMethod,
            SegmentId = SegmentId
        };
    }
}
