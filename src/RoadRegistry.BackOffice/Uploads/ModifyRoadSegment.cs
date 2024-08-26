namespace RoadRegistry.BackOffice.Uploads;

using System;
using System.Collections.Generic;
using System.Linq;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Messages;
using NetTopologySuite.Geometries;

public class ModifyRoadSegment : ITranslatedChange
{
    public ModifyRoadSegment(
        RecordNumber recordNumber,
        RoadSegmentId id,
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
        : this(
            recordNumber,
            id,
            null,
            startNodeId,
            endNodeId,
            null,
            maintenanceAuthority,
            geometryDrawMethod.ThrowIfNull(),
            morphology.ThrowIfNull(),
            status.ThrowIfNull(),
            category.ThrowIfNull(),
            false,
            accessRestriction.ThrowIfNull(),
            leftSideStreetNameId,
            rightSideStreetNameId,
            Array.Empty<RoadSegmentLaneAttribute>(),
            Array.Empty<RoadSegmentWidthAttribute>(),
            Array.Empty<RoadSegmentSurfaceAttribute>(),
            null,
            null,
            null
        )
    {
    }

    private ModifyRoadSegment(
        RecordNumber recordNumber,
        RoadSegmentId id,
        RoadSegmentId? originalId,
        RoadNodeId startNodeId,
        RoadNodeId endNodeId,
        MultiLineString geometry,
        OrganizationId maintenanceAuthority,
        RoadSegmentGeometryDrawMethod geometryDrawMethod,
        RoadSegmentMorphology morphology,
        RoadSegmentStatus status,
        RoadSegmentCategory category,
        bool categoryModified,
        RoadSegmentAccessRestriction accessRestriction,
        StreetNameLocalId? leftSideStreetNameId,
        StreetNameLocalId? rightSideStreetNameId,
        IReadOnlyList<RoadSegmentLaneAttribute> lanes,
        IReadOnlyList<RoadSegmentWidthAttribute> widths,
        IReadOnlyList<RoadSegmentSurfaceAttribute> surfaces,
        RoadSegmentVersion? version,
        GeometryVersion? geometryVersion,
        RoadSegmentGeometryDrawMethod? previousGeometryDrawMethod)
    {
        RecordNumber = recordNumber;
        Id = id;
        OriginalId = originalId;
        StartNodeId = startNodeId;
        EndNodeId = endNodeId;
        Geometry = geometry;
        MaintenanceAuthority = maintenanceAuthority;
        GeometryDrawMethod = geometryDrawMethod;
        Morphology = morphology;
        Status = status;
        Category = category;
        CategoryModified = categoryModified;
        AccessRestriction = accessRestriction;
        LeftSideStreetNameId = leftSideStreetNameId;
        RightSideStreetNameId = rightSideStreetNameId;
        Lanes = lanes;
        Widths = widths;
        Surfaces = surfaces;
        Version = version;
        GeometryVersion = geometryVersion;
        PreviousGeometryDrawMethod = previousGeometryDrawMethod;
    }

    public RoadSegmentAccessRestriction AccessRestriction { get; }
    public RoadSegmentCategory Category { get; }
    public bool CategoryModified { get; }
    public RoadNodeId EndNodeId { get; }
    public MultiLineString Geometry { get; }
    public RoadSegmentGeometryDrawMethod GeometryDrawMethod { get; }
    public RoadSegmentId Id { get; }
    public RoadSegmentId? OriginalId { get; }
    public IReadOnlyList<RoadSegmentLaneAttribute> Lanes { get; }
    public StreetNameLocalId? LeftSideStreetNameId { get; }
    public OrganizationId MaintenanceAuthority { get; }
    public RoadSegmentMorphology Morphology { get; }
    public RecordNumber RecordNumber { get; }
    public StreetNameLocalId? RightSideStreetNameId { get; }
    public RoadNodeId StartNodeId { get; }
    public RoadSegmentStatus Status { get; }
    public IReadOnlyList<RoadSegmentSurfaceAttribute> Surfaces { get; }
    public IReadOnlyList<RoadSegmentWidthAttribute> Widths { get; }
    public RoadSegmentVersion? Version { get; }
    public GeometryVersion? GeometryVersion { get; }
    //public bool ConvertedFromOutlined { get; }
    public RoadSegmentGeometryDrawMethod? PreviousGeometryDrawMethod { get; }
    //public bool ConvertedToOutlined { get; }

    public void TranslateTo(RequestedChange message)
    {
        if (message == null) throw new ArgumentNullException(nameof(message));

        message.ModifyRoadSegment = new Messages.ModifyRoadSegment
        {
            Id = Id,
            OriginalId = OriginalId,
            StartNodeId = StartNodeId,
            EndNodeId = EndNodeId,
            Geometry = GeometryTranslator.Translate(Geometry),
            MaintenanceAuthority = MaintenanceAuthority,
            GeometryDrawMethod = GeometryDrawMethod,
            Morphology = Morphology,
            Status = Status,
            Category = Category,
            CategoryModified = CategoryModified,
            AccessRestriction = AccessRestriction,
            LeftSideStreetNameId = LeftSideStreetNameId.GetValueOrDefault(),
            RightSideStreetNameId = RightSideStreetNameId.GetValueOrDefault(),
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
                .ToArray(),
            Version = Version,
            GeometryVersion = GeometryVersion,
            PreviousGeometryDrawMethod = PreviousGeometryDrawMethod
        };
    }

    public ModifyRoadSegment WithGeometry(MultiLineString geometry)
    {
        if (geometry == null) throw new ArgumentNullException(nameof(geometry));
        return new ModifyRoadSegment(
            RecordNumber, Id, OriginalId, StartNodeId, EndNodeId, geometry,
            MaintenanceAuthority, GeometryDrawMethod, Morphology, Status, Category, CategoryModified, AccessRestriction,
            LeftSideStreetNameId, RightSideStreetNameId, Lanes, Widths, Surfaces, Version, GeometryVersion, PreviousGeometryDrawMethod);
    }

    public ModifyRoadSegment WithPreviousGeometryDrawMethod(RoadSegmentGeometryDrawMethod previousGeometryDrawMethod)
    {
        return new ModifyRoadSegment(
            RecordNumber, Id, OriginalId, StartNodeId, EndNodeId, Geometry,
            MaintenanceAuthority, GeometryDrawMethod, Morphology, Status, Category, CategoryModified, AccessRestriction,
            LeftSideStreetNameId, RightSideStreetNameId, Lanes, Widths, Surfaces, Version, GeometryVersion, previousGeometryDrawMethod);
    }

    public ModifyRoadSegment WithConvertedToOutlined(bool convertedToOutlined)
    {
        return new ModifyRoadSegment(
            RecordNumber, Id, OriginalId, StartNodeId, EndNodeId, Geometry,
            MaintenanceAuthority, GeometryDrawMethod, Morphology, Status, Category, CategoryModified, AccessRestriction,
            LeftSideStreetNameId, RightSideStreetNameId, Lanes, Widths, Surfaces, Version, GeometryVersion, PreviousGeometryDrawMethod);
    }

    public ModifyRoadSegment WithCategoryModified(bool categoryModified)
    {
        return new ModifyRoadSegment(
            RecordNumber, Id, OriginalId, StartNodeId, EndNodeId, Geometry,
            MaintenanceAuthority, GeometryDrawMethod, Morphology, Status, Category, categoryModified, AccessRestriction,
            LeftSideStreetNameId, RightSideStreetNameId, Lanes, Widths, Surfaces, Version, GeometryVersion, PreviousGeometryDrawMethod);
    }

    public ModifyRoadSegment WithOriginalId(RoadSegmentId originalId)
    {
        return new ModifyRoadSegment(
            RecordNumber, Id, originalId, StartNodeId, EndNodeId, Geometry,
            MaintenanceAuthority, GeometryDrawMethod, Morphology, Status, Category, CategoryModified, AccessRestriction,
            LeftSideStreetNameId, RightSideStreetNameId, Lanes, Widths, Surfaces, Version, GeometryVersion, PreviousGeometryDrawMethod);
    }

    public ModifyRoadSegment WithLane(RoadSegmentLaneAttribute lane)
    {
        var lanes = new List<RoadSegmentLaneAttribute>(Lanes) { lane };
        lanes.Sort((left, right) => left.From.CompareTo(right.From));
        return new ModifyRoadSegment(
            RecordNumber, Id, OriginalId, StartNodeId, EndNodeId, Geometry,
            MaintenanceAuthority, GeometryDrawMethod, Morphology, Status, Category, CategoryModified, AccessRestriction,
            LeftSideStreetNameId, RightSideStreetNameId,
            lanes, Widths, Surfaces, Version, GeometryVersion, PreviousGeometryDrawMethod);
    }

    public ModifyRoadSegment WithSurface(RoadSegmentSurfaceAttribute surface)
    {
        var surfaces = new List<RoadSegmentSurfaceAttribute>(Surfaces) { surface };
        surfaces.Sort((left, right) => left.From.CompareTo(right.From));
        return new ModifyRoadSegment(
            RecordNumber, Id, OriginalId, StartNodeId, EndNodeId, Geometry,
            MaintenanceAuthority, GeometryDrawMethod, Morphology, Status, Category, CategoryModified, AccessRestriction,
            LeftSideStreetNameId, RightSideStreetNameId,
            Lanes, Widths, surfaces, Version, GeometryVersion, PreviousGeometryDrawMethod);
    }

    public ModifyRoadSegment WithWidth(RoadSegmentWidthAttribute width)
    {
        var widths = new List<RoadSegmentWidthAttribute>(Widths) { width };
        widths.Sort((left, right) => left.From.CompareTo(right.From));
        return new ModifyRoadSegment(
            RecordNumber, Id, OriginalId, StartNodeId, EndNodeId, Geometry,
            MaintenanceAuthority, GeometryDrawMethod, Morphology, Status, Category, CategoryModified, AccessRestriction,
            LeftSideStreetNameId, RightSideStreetNameId,
            Lanes, widths, Surfaces, Version, GeometryVersion, PreviousGeometryDrawMethod);
    }

    public ModifyRoadSegment WithVersion(RoadSegmentVersion version)
    {
        return new ModifyRoadSegment(
            RecordNumber, Id, OriginalId, StartNodeId, EndNodeId, Geometry,
            MaintenanceAuthority, GeometryDrawMethod, Morphology, Status, Category, CategoryModified, AccessRestriction,
            LeftSideStreetNameId, RightSideStreetNameId, Lanes, Widths, Surfaces, version, GeometryVersion, PreviousGeometryDrawMethod);
    }

    public ModifyRoadSegment WithGeometryVersion(GeometryVersion geometryVersion)
    {
        return new ModifyRoadSegment(
            RecordNumber, Id, OriginalId, StartNodeId, EndNodeId, Geometry,
            MaintenanceAuthority, GeometryDrawMethod, Morphology, Status, Category, CategoryModified, AccessRestriction,
            LeftSideStreetNameId, RightSideStreetNameId, Lanes, Widths, Surfaces, Version, geometryVersion, PreviousGeometryDrawMethod);
    }
}
