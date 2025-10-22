namespace RoadRegistry.BackOffice.Uploads;

using System;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Messages;
using RoadSegment.ValueObjects;

public class AddRoadSegmentToNationalRoad : ITranslatedChange
{
    public AddRoadSegmentToNationalRoad(
        RecordNumber recordNumber,
        AttributeId temporaryAttributeId,
        RoadSegmentGeometryDrawMethod segmentGeometryDrawMethod,
        RoadSegmentId segmentId,
        NationalRoadNumber number)
    {
        RecordNumber = recordNumber;
        TemporaryAttributeId = temporaryAttributeId;
        SegmentGeometryDrawMethod = segmentGeometryDrawMethod;
        SegmentId = segmentId;
        Number = number;
    }

    public RecordNumber RecordNumber { get; }
    public AttributeId TemporaryAttributeId { get; }
    public RoadSegmentGeometryDrawMethod SegmentGeometryDrawMethod { get; }
    public RoadSegmentId SegmentId { get; }
    public NationalRoadNumber Number { get; }

    public void TranslateTo(RequestedChange message)
    {
        if (message == null) throw new ArgumentNullException(nameof(message));

        message.AddRoadSegmentToNationalRoad = new Messages.AddRoadSegmentToNationalRoad
        {
            TemporaryAttributeId = TemporaryAttributeId,
            Number = Number,
            SegmentGeometryDrawMethod = SegmentGeometryDrawMethod,
            SegmentId = SegmentId
        };
    }
}
