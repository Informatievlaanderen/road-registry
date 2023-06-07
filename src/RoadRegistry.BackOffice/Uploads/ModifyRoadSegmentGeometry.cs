namespace RoadRegistry.BackOffice.Uploads;

using Be.Vlaanderen.Basisregisters.Shaperon;
using Messages;
using System;
using System.Collections.Generic;
using System.Linq;

public class ModifyRoadSegmentGeometry : ITranslatedChange
{
    public ModifyRoadSegmentGeometry(
        RecordNumber recordNumber,
        RoadSegmentId id,
        RoadSegmentGeometryDrawMethod geometryDrawMethod,
        RoadSegmentGeometry geometry,
        IReadOnlyCollection<RoadSegmentLaneAttribute> lanes,
        IReadOnlyCollection<RoadSegmentSurfaceAttribute> surfaces,
        IReadOnlyCollection<RoadSegmentWidthAttribute> widths)
    {
        RecordNumber = recordNumber;
        Id = id;
        GeometryDrawMethod = geometryDrawMethod;
        Geometry = geometry.ThrowIfNull();
        Lanes = lanes.ThrowIfNull();
        Surfaces = surfaces.ThrowIfNull();
        Widths = widths.ThrowIfNull();
    }
    
    public RecordNumber RecordNumber { get; }
    public RoadSegmentId Id { get; }
    public RoadSegmentGeometryDrawMethod GeometryDrawMethod { get; }
    
    public RoadSegmentGeometry Geometry { get; }
    public IReadOnlyCollection<RoadSegmentLaneAttribute> Lanes { get; }
    public IReadOnlyCollection<RoadSegmentSurfaceAttribute> Surfaces { get; }
    public IReadOnlyCollection<RoadSegmentWidthAttribute> Widths { get; }

    public void TranslateTo(RequestedChange message)
    {
        ArgumentNullException.ThrowIfNull(message);

        message.ModifyRoadSegmentGeometry = new Messages.ModifyRoadSegmentGeometry
        {
            Id = Id,
            GeometryDrawMethod = GeometryDrawMethod,
            Geometry = Geometry,
            Lanes = Lanes
                .Select(item => new RequestedRoadSegmentLaneAttribute
                {
                    AttributeId = item.TemporaryId,
                    Count = item.Count,
                    Direction = item.Direction,
                    FromPosition = item.From,
                    ToPosition = item.To
                })
                .ToArray(),
            Surfaces = Surfaces
                .Select(item => new RequestedRoadSegmentSurfaceAttribute
                {
                    AttributeId = item.TemporaryId,
                    Type = item.Type,
                    FromPosition = item.From,
                    ToPosition = item.To
                })
                .ToArray(),
            Widths = Widths
                .Select(item => new RequestedRoadSegmentWidthAttribute
                {
                    AttributeId = item.TemporaryId,
                    Width = item.Width,
                    FromPosition = item.From,
                    ToPosition = item.To
                })
                .ToArray()
        };
    }
}
