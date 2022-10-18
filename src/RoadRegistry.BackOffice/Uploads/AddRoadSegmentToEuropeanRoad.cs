namespace RoadRegistry.BackOffice.Uploads;

using System;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Messages;

public class AddRoadSegmentToEuropeanRoad : ITranslatedChange
{
    public AddRoadSegmentToEuropeanRoad(
        RecordNumber recordNumber,
        AttributeId temporaryAttributeId,
        RoadSegmentId segmentId,
        EuropeanRoadNumber number)
    {
        RecordNumber = recordNumber;
        TemporaryAttributeId = temporaryAttributeId;
        SegmentId = segmentId;
        Number = number;
    }

    public EuropeanRoadNumber Number { get; }

    public RecordNumber RecordNumber { get; }
    public RoadSegmentId SegmentId { get; }
    public AttributeId TemporaryAttributeId { get; }


    public void TranslateTo(RequestedChange message)
    {
        if (message == null) throw new ArgumentNullException(nameof(message));

        message.AddRoadSegmentToEuropeanRoad = new Messages.AddRoadSegmentToEuropeanRoad
        {
            TemporaryAttributeId = TemporaryAttributeId,
            Number = Number,
            SegmentId = SegmentId
        };
    }
}
