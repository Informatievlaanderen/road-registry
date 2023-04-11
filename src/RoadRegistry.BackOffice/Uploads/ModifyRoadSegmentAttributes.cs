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
        OrganizationId? maintenanceAuthority = null,
        RoadSegmentMorphology morphology = null,
        RoadSegmentStatus status = null,
        RoadSegmentCategory category = null,
        RoadSegmentAccessRestriction accessRestriction = null,
        RoadSegmentGeometry geometry = null)
    {
        RecordNumber = recordNumber;
        Id = id;
        GeometryDrawMethod = geometryDrawMethod;
        MaintenanceAuthority = maintenanceAuthority;
        Morphology = morphology;
        Status = status;
        Category = category;
        AccessRestriction = accessRestriction;
        Geometry = geometry;
    }
    
    public RecordNumber RecordNumber { get; }
    public RoadSegmentId Id { get; }
    public RoadSegmentGeometryDrawMethod GeometryDrawMethod { get; }

    public RoadSegmentAccessRestriction AccessRestriction { get; }
    public RoadSegmentGeometry Geometry { get; }
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
            AccessRestriction = AccessRestriction,
            Geometry = Geometry
        };
    }
}
