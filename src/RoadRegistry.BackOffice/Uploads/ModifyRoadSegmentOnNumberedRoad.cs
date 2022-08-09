namespace RoadRegistry.BackOffice.Uploads;

using System;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Messages;

public class ModifyRoadSegmentOnNumberedRoad : ITranslatedChange
{
    public ModifyRoadSegmentOnNumberedRoad(
        RecordNumber recordNumber,
        AttributeId attributeId,
        RoadSegmentId segmentId,
        NumberedRoadNumber number,
        RoadSegmentNumberedRoadDirection direction,
        RoadSegmentNumberedRoadOrdinal ordinal)
    {
        RecordNumber = recordNumber;
        AttributeId = attributeId;
        SegmentId = segmentId;
        Number = number;
        Direction = direction;
        Ordinal = ordinal;
    }

    public RecordNumber RecordNumber { get; }
    public AttributeId AttributeId { get; }
    public RoadSegmentId SegmentId { get; }
    public NumberedRoadNumber Number { get; }
    public RoadSegmentNumberedRoadDirection Direction { get; }
    public RoadSegmentNumberedRoadOrdinal Ordinal { get; }

    public void TranslateTo(RequestedChange message)
    {
        if (message == null) throw new ArgumentNullException(nameof(message));

        message.ModifyRoadSegmentOnNumberedRoad = new Messages.ModifyRoadSegmentOnNumberedRoad
        {
            AttributeId = AttributeId,
            Number = Number,
            Direction = Direction,
            Ordinal = Ordinal,
            SegmentId = SegmentId
        };
    }
}
