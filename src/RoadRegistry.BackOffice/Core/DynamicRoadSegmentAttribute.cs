namespace RoadRegistry.BackOffice.Core;

using System;

public abstract class DynamicRoadSegmentAttribute
{
    protected DynamicRoadSegmentAttribute(
        AttributeId id,
        AttributeId temporaryId,
        RoadSegmentPosition from,
        RoadSegmentPosition to,
        GeometryVersion asOfGeometryVersion
    )
    {
        if (from >= to)
            throw new ArgumentException(nameof(From),
                $"The from position ({from.ToDecimal()}) must be less than the to position ({to.ToDecimal()}).");

        Id = id;
        TemporaryId = temporaryId;
        From = from;
        To = to;
        AsOfGeometryVersion = asOfGeometryVersion;
    }

    public GeometryVersion AsOfGeometryVersion { get; }

    public RoadSegmentPosition From { get; }

    public AttributeId Id { get; }

    public AttributeId TemporaryId { get; }

    public RoadSegmentPosition To { get; }
}
