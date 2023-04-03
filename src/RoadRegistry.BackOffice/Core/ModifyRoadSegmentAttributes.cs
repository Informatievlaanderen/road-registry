namespace RoadRegistry.BackOffice.Core;

using System;
using Be.Vlaanderen.Basisregisters.GrAr.Common;
using Messages;

public class ModifyRoadSegmentAttributes : IRequestedChange, IHaveHash
{
    public const string EventName = "ModifyRoadSegmentAttributes";

    public ModifyRoadSegmentAttributes(
        RoadSegmentId id,
        RoadSegmentVersion version,
        RoadSegmentGeometryDrawMethod geometryDrawMethod,
        OrganizationId? maintenanceAuthorityId,
        OrganizationName? maintenanceAuthorityName,
        RoadSegmentMorphology morphology,
        RoadSegmentStatus status,
        RoadSegmentCategory category,
        RoadSegmentAccessRestriction accessRestriction)
    {
        Id = id;
        Version = version;
        GeometryDrawMethod = geometryDrawMethod;
        MaintenanceAuthorityId = maintenanceAuthorityId;
        MaintenanceAuthorityName = maintenanceAuthorityName;
        Morphology = morphology;
        Status = status;
        Category = category;
        AccessRestriction = accessRestriction;
    }

    public RoadSegmentId Id { get; }
    public RoadSegmentVersion Version { get; }
    public RoadSegmentGeometryDrawMethod GeometryDrawMethod { get; }
    public OrganizationId? MaintenanceAuthorityId { get; }
    public OrganizationName? MaintenanceAuthorityName { get; }
    public RoadSegmentMorphology Morphology { get; }
    public RoadSegmentStatus Status { get; }
    public RoadSegmentCategory Category { get; }
    public RoadSegmentAccessRestriction AccessRestriction { get; }

    public void TranslateTo(Messages.AcceptedChange message)
    {
        ArgumentNullException.ThrowIfNull(message);

        message.RoadSegmentAttributesModified = new RoadSegmentAttributesModified
        {
            Id = Id,
            Version = Version,
            MaintenanceAuthority = MaintenanceAuthorityId != null ? new MaintenanceAuthority
            {
                Code = MaintenanceAuthorityId.Value,
                Name = MaintenanceAuthorityName ?? string.Empty
            } : null,
            Morphology = Morphology,
            Status = Status,
            Category = Category,
            AccessRestriction = AccessRestriction,
        };
    }

    public void TranslateTo(Messages.RejectedChange message)
    {
        message.ModifyRoadSegmentAttributes = new Messages.ModifyRoadSegmentAttributes
        {
            Id = Id,
            AccessRestriction = AccessRestriction?.ToString(),
            Category = Category?.ToString(),
            MaintenanceAuthority = MaintenanceAuthorityId?.ToString(),
            Morphology = Morphology?.ToString(),
            Status = Status?.ToString()
        };
    }

    public Problems VerifyAfter(AfterVerificationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        return Problems.None;
    }

    public Problems VerifyBefore(BeforeVerificationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var problems = Problems.None;

        if (!context.BeforeView.Segments.ContainsKey(Id))
        {
            problems += new RoadSegmentNotFound();
        }
        
        return problems;
    }

    public System.Collections.Generic.IEnumerable<string> GetHashFields() => ObjectHasher.GetHashFields(this);
    public string GetHash() => this.ToEventHash(EventName);
}
