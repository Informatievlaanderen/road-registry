namespace RoadRegistry.BackOffice.Uploads;

using System;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Messages;
using RoadSegment.ValueObjects;

public class RemoveRoadSegment : ITranslatedChange
{
    public RemoveRoadSegment(RecordNumber recordNumber, RoadSegmentId id, RoadSegmentGeometryDrawMethod geometryDrawMethod)
    {
        RecordNumber = recordNumber;
        Id = id;
        GeometryDrawMethod = geometryDrawMethod;
    }

    public RoadSegmentId Id { get; }
    public RoadSegmentGeometryDrawMethod GeometryDrawMethod { get; }
    public RecordNumber RecordNumber { get; }

    public void TranslateTo(RequestedChange message)
    {
        if (message == null) throw new ArgumentNullException(nameof(message));

        message.RemoveRoadSegment = new Messages.RemoveRoadSegment
        {
            Id = Id,
            GeometryDrawMethod = GeometryDrawMethod
        };
    }
}
