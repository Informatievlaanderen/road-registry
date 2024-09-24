namespace RoadRegistry.BackOffice.Uploads;

using System;
using System.Collections.Generic;
using System.Linq;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Messages;
using NetTopologySuite.Geometries;

public class AddRoadSegment : ITranslatedChange
{
    public AddRoadSegment(
        RecordNumber recordNumber,
        RoadSegmentId temporaryId,
        RoadSegmentId? originalId,
        RoadNodeId startNodeId,
        RoadNodeId endNodeId,
        OrganizationId maintenanceAuthority,
        RoadSegmentGeometryDrawMethod geometryDrawMethod,
        RoadSegmentMorphology morphology,
        RoadSegmentStatus status,
        RoadSegmentCategory category,
        RoadSegmentAccessRestriction accessRestriction,
        StreetNameLocalId? leftSideStreetNameId,
        StreetNameLocalId? rightSideStreetNameId)
        : this(recordNumber,
            temporaryId,
            originalId,
            null,
            startNodeId,
            endNodeId,
            null,
            maintenanceAuthority,
            geometryDrawMethod.ThrowIfNull(),
            morphology.ThrowIfNull(),
            status.ThrowIfNull(),
            category.ThrowIfNull(),
            accessRestriction.ThrowIfNull(),
            leftSideStreetNameId,
            rightSideStreetNameId,
            [], [], [])
    {
    }

    private AddRoadSegment(
        RecordNumber recordNumber,
        RoadSegmentId temporaryId,
        RoadSegmentId? originalId,
        RoadSegmentId? permanentId,
        RoadNodeId startNodeId,
        RoadNodeId endNodeId,
        MultiLineString geometry,
        OrganizationId maintenanceAuthority,
        RoadSegmentGeometryDrawMethod geometryDrawMethod,
        RoadSegmentMorphology morphology,
        RoadSegmentStatus status,
        RoadSegmentCategory category,
        RoadSegmentAccessRestriction accessRestriction,
        StreetNameLocalId? leftSideStreetNameId,
        StreetNameLocalId? rightSideStreetNameId,
        IReadOnlyList<RoadSegmentLaneAttribute> lanes,
        IReadOnlyList<RoadSegmentWidthAttribute> widths,
        IReadOnlyList<RoadSegmentSurfaceAttribute> surfaces)
    {
        RecordNumber = recordNumber;
        TemporaryId = temporaryId;
        OriginalId = originalId;
        PermanentId = permanentId;
        StartNodeId = startNodeId;
        EndNodeId = endNodeId;
        Geometry = geometry;
        MaintenanceAuthority = maintenanceAuthority;
        GeometryDrawMethod = geometryDrawMethod;
        Morphology = morphology;
        Status = status;
        Category = category;
        AccessRestriction = accessRestriction;
        LeftSideStreetNameId = leftSideStreetNameId ?? StreetNameLocalId.NotApplicable;
        RightSideStreetNameId = rightSideStreetNameId ?? StreetNameLocalId.NotApplicable;
        Lanes = lanes;
        Widths = widths;
        Surfaces = surfaces;
    }

    public RoadSegmentAccessRestriction AccessRestriction { get; }
    public RoadSegmentCategory Category { get; }
    public RoadNodeId EndNodeId { get; }
    public MultiLineString Geometry { get; }
    public RoadSegmentGeometryDrawMethod GeometryDrawMethod { get; }
    public IReadOnlyList<RoadSegmentLaneAttribute> Lanes { get; }
    public StreetNameLocalId LeftSideStreetNameId { get; }
    public OrganizationId MaintenanceAuthority { get; }
    public RoadSegmentMorphology Morphology { get; }
    public RecordNumber RecordNumber { get; }
    public StreetNameLocalId RightSideStreetNameId { get; }
    public RoadNodeId StartNodeId { get; }
    public RoadSegmentStatus Status { get; }
    public IReadOnlyList<RoadSegmentSurfaceAttribute> Surfaces { get; }
    public RoadSegmentId TemporaryId { get; }
    public RoadSegmentId? OriginalId { get; }
    public RoadSegmentId? PermanentId { get; init; }
    public IReadOnlyList<RoadSegmentWidthAttribute> Widths { get; }

    public void TranslateTo(RequestedChange message)
    {
        if (message == null) throw new ArgumentNullException(nameof(message));

        message.AddRoadSegment = new Messages.AddRoadSegment
        {
            TemporaryId = TemporaryId,
            OriginalId = OriginalId,
            PermanentId = PermanentId,
            StartNodeId = StartNodeId,
            EndNodeId = EndNodeId,
            Geometry = GeometryTranslator.Translate(Geometry),
            MaintenanceAuthority = MaintenanceAuthority,
            GeometryDrawMethod = GeometryDrawMethod,
            Morphology = Morphology,
            Status = Status,
            Category = Category,
            AccessRestriction = AccessRestriction,
            LeftSideStreetNameId = LeftSideStreetNameId,
            RightSideStreetNameId = RightSideStreetNameId,
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
            Widths = Widths
                .Select(item => new RequestedRoadSegmentWidthAttribute
                {
                    AttributeId = item.TemporaryId,
                    Width = item.Width,
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
                .ToArray()
        };
    }

    public AddRoadSegment WithGeometry(MultiLineString geometry)
    {
        if (geometry == null) throw new ArgumentNullException(nameof(geometry));
        return new AddRoadSegment(
            RecordNumber, TemporaryId, OriginalId, PermanentId, StartNodeId, EndNodeId, geometry,
            MaintenanceAuthority, GeometryDrawMethod, Morphology, Status, Category, AccessRestriction,
            LeftSideStreetNameId, RightSideStreetNameId, Lanes, Widths, Surfaces);
    }

    public AddRoadSegment WithLane(RoadSegmentLaneAttribute lane)
    {
        var lanes = new List<RoadSegmentLaneAttribute>(Lanes) { lane };
        lanes.Sort((left, right) => left.From.CompareTo(right.From));
        return new AddRoadSegment(
            RecordNumber, TemporaryId, OriginalId, PermanentId, StartNodeId, EndNodeId, Geometry,
            MaintenanceAuthority, GeometryDrawMethod, Morphology, Status, Category, AccessRestriction,
            LeftSideStreetNameId, RightSideStreetNameId,
            lanes, Widths, Surfaces);
    }

    public AddRoadSegment WithSurface(RoadSegmentSurfaceAttribute surface)
    {
        var surfaces = new List<RoadSegmentSurfaceAttribute>(Surfaces) { surface };
        surfaces.Sort((left, right) => left.From.CompareTo(right.From));
        return new AddRoadSegment(
            RecordNumber, TemporaryId, OriginalId, PermanentId, StartNodeId, EndNodeId, Geometry,
            MaintenanceAuthority, GeometryDrawMethod, Morphology, Status, Category, AccessRestriction,
            LeftSideStreetNameId, RightSideStreetNameId,
            Lanes, Widths, surfaces);
    }

    public AddRoadSegment WithWidth(RoadSegmentWidthAttribute width)
    {
        var widths = new List<RoadSegmentWidthAttribute>(Widths) { width };
        widths.Sort((left, right) => left.From.CompareTo(right.From));
        return new AddRoadSegment(
            RecordNumber, TemporaryId, OriginalId, PermanentId, StartNodeId, EndNodeId, Geometry,
            MaintenanceAuthority, GeometryDrawMethod, Morphology, Status, Category, AccessRestriction,
            LeftSideStreetNameId, RightSideStreetNameId,
            Lanes, widths, Surfaces);
    }
}
