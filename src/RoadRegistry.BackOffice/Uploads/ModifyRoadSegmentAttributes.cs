namespace RoadRegistry.BackOffice.Uploads;

using Be.Vlaanderen.Basisregisters.Shaperon;
using Messages;
using System;

public class ModifyRoadSegmentAttributes : ITranslatedChange
{
    public ModifyRoadSegmentAttributes(
        RecordNumber recordNumber,
        RoadSegmentId id,
        RoadSegmentGeometryDrawMethod geometryDrawMethod,
        OrganizationId? maintenanceAuthority,
        RoadSegmentMorphology morphology,
        RoadSegmentStatus status,
        RoadSegmentCategory category,
        RoadSegmentAccessRestriction accessRestriction)
    {
        RecordNumber = recordNumber;
        Id = id;
        GeometryDrawMethod = geometryDrawMethod;
        MaintenanceAuthority = maintenanceAuthority;
        Morphology = morphology;
        Status = status;
        Category = category;
        AccessRestriction = accessRestriction;
    }
    
    public RecordNumber RecordNumber { get; }
    public RoadSegmentId Id { get; }
    public RoadSegmentGeometryDrawMethod GeometryDrawMethod { get; }

    public RoadSegmentAccessRestriction AccessRestriction { get; }
    public RoadSegmentCategory Category { get; }
    public OrganizationId? MaintenanceAuthority { get; }
    public RoadSegmentMorphology Morphology { get; }
    public RoadSegmentStatus Status { get; }

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
