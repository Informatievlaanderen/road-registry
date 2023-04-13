namespace RoadRegistry.BackOffice.Uploads;

using Be.Vlaanderen.Basisregisters.Shaperon;
using Messages;
using System;

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
            AccessRestriction = AccessRestriction
        };
    }
}
