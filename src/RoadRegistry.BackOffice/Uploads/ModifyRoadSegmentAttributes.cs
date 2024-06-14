namespace RoadRegistry.BackOffice.Uploads;

using Be.Vlaanderen.Basisregisters.Shaperon;
using Messages;
using System;
using System.Linq;

public class ModifyRoadSegmentAttributes : ITranslatedChange
{
    public ModifyRoadSegmentAttributes(
        RecordNumber recordNumber,
        RoadSegmentId id,
        RoadSegmentGeometryDrawMethod geometryDrawMethod)
    {
        RecordNumber = recordNumber;
        Id = id;
        GeometryDrawMethod = geometryDrawMethod;
    }
    
    public RecordNumber RecordNumber { get; }
    public RoadSegmentId Id { get; }
    public RoadSegmentGeometryDrawMethod GeometryDrawMethod { get; }

    public RoadSegmentAccessRestriction AccessRestriction { get; init; }
    public RoadSegmentCategory Category { get; init; }
    public OrganizationId? MaintenanceAuthority { get; init; }
    public RoadSegmentMorphology Morphology { get; init; }
    public RoadSegmentStatus Status { get; init; }
    public RoadSegmentLaneAttribute[] Lanes { get; init; }
    public RoadSegmentSurfaceAttribute[] Surfaces { get; init; }
    public RoadSegmentWidthAttribute[] Widths { get; init; }

    public void TranslateTo(RequestedChange message)
    {
        ArgumentNullException.ThrowIfNull(message);

        message.ModifyRoadSegmentAttributes = new Messages.ModifyRoadSegmentAttributes
        {
            Id = Id,
            GeometryDrawMethod = GeometryDrawMethod,
            MaintenanceAuthority = MaintenanceAuthority,
            Morphology = Morphology,
            Status = Status,
            Category = Category,
            AccessRestriction = AccessRestriction,
            Lanes = Lanes?
                .Select(item => new RequestedRoadSegmentLaneAttribute
                {
                    AttributeId = item.TemporaryId,
                    Count = item.Count,
                    Direction = item.Direction,
                    FromPosition = item.From,
                    ToPosition = item.To
                })
                .ToArray(),
            Widths = Widths?
                .Select(item => new RequestedRoadSegmentWidthAttribute
                {
                    AttributeId = item.TemporaryId,
                    Width = item.Width,
                    FromPosition = item.From,
                    ToPosition = item.To
                })
                .ToArray(),
            Surfaces = Surfaces?
                .Select(item => new RequestedRoadSegmentSurfaceAttribute
                {
                    AttributeId = item.TemporaryId,
                    Type = item.Type,
                    FromPosition = item.From,
                    ToPosition = item.To
                })
                .ToArray()
        };
    }
}
