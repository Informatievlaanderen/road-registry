namespace RoadRegistry.BackOffice.Core;

using Be.Vlaanderen.Basisregisters.GrAr.Common;
using Messages;
using System;
using System.Collections.Generic;
using System.Linq;

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
        RoadSegmentAccessRestriction accessRestriction,
        IReadOnlyList<RoadSegmentLaneAttribute> lanes,
        IReadOnlyList<RoadSegmentSurfaceAttribute> surfaces,
        IReadOnlyList<RoadSegmentWidthAttribute> widths)
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
        Lanes = lanes;
        Surfaces = surfaces;
        Widths = widths;
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
    public IReadOnlyList<RoadSegmentLaneAttribute> Lanes { get; }
    public IReadOnlyList<RoadSegmentSurfaceAttribute> Surfaces { get; }
    public IReadOnlyList<RoadSegmentWidthAttribute> Widths { get; }

    public void TranslateTo(Messages.AcceptedChange message)
    {
        ArgumentNullException.ThrowIfNull(message);

        message.RoadSegmentAttributesModified = new RoadSegmentAttributesModified
        {
            Id = Id,
            Version = Version,
            AccessRestriction = AccessRestriction,
            Category = Category,
            MaintenanceAuthority = MaintenanceAuthorityId != null ? new MaintenanceAuthority
            {
                Code = MaintenanceAuthorityId.Value,
                Name = MaintenanceAuthorityName ?? Organization.PredefinedTranslations.Unknown.Name
            } : null,
            Morphology = Morphology,
            Status = Status,
            Lanes = Lanes?
                .Select(item => new Messages.RoadSegmentLaneAttributes
                {
                    AttributeId = item.Id,
                    AsOfGeometryVersion = GeometryVersion.Initial,
                    Count = item.Count,
                    Direction = item.Direction,
                    FromPosition = item.From,
                    ToPosition = item.To
                })
                .ToArray(),
            Widths = Widths?
                .Select(item => new Messages.RoadSegmentWidthAttributes
                {
                    AttributeId = item.Id,
                    AsOfGeometryVersion = GeometryVersion.Initial,
                    Width = item.Width,
                    FromPosition = item.From,
                    ToPosition = item.To
                })
                .ToArray(),
            Surfaces = Surfaces?
                .Select(item => new Messages.RoadSegmentSurfaceAttributes
                {
                    AttributeId = item.Id,
                    AsOfGeometryVersion = GeometryVersion.Initial,
                    Type = item.Type,
                    FromPosition = item.From,
                    ToPosition = item.To
                })
                .ToArray()
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
            Status = Status?.ToString(),
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

    public IEnumerable<string> GetHashFields() => ObjectHasher.GetHashFields(this);
    public string GetHash() => this.ToEventHash(EventName);
}
