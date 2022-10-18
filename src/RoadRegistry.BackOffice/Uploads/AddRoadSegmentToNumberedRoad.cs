namespace RoadRegistry.BackOffice.Uploads;

using System;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Messages;

public class AddRoadSegmentToNumberedRoad : ITranslatedChange
{
    public AddRoadSegmentToNumberedRoad(
        RecordNumber recordNumber,
        AttributeId temporaryAttributeId,
        RoadSegmentId segmentId,
        NumberedRoadNumber number,
        RoadSegmentNumberedRoadDirection direction,
        RoadSegmentNumberedRoadOrdinal ordinal)
    {
        RecordNumber = recordNumber;
        TemporaryAttributeId = temporaryAttributeId;
        SegmentId = segmentId;
        Number = number;
        Direction = direction;
        Ordinal = ordinal;
    }

    public RoadSegmentNumberedRoadDirection Direction { get; }
    public NumberedRoadNumber Number { get; }
    public RoadSegmentNumberedRoadOrdinal Ordinal { get; }

    public RecordNumber RecordNumber { get; }
    public RoadSegmentId SegmentId { get; }
    public AttributeId TemporaryAttributeId { get; }

    public void TranslateTo(RequestedChange message)
    {
        if (message == null) throw new ArgumentNullException(nameof(message));

        message.AddRoadSegmentToNumberedRoad = new Messages.AddRoadSegmentToNumberedRoad
        {
            TemporaryAttributeId = TemporaryAttributeId,
            Number = Number,
            Direction = Direction,
            Ordinal = Ordinal,
            SegmentId = SegmentId
        };
    }
}