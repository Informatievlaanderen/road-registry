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
        RoadSegmentGeometryDrawMethod geometryDrawMethod,
        RoadNodeId? startNodeId = null,
        RoadNodeId? endNodeId = null,
        OrganizationId? maintenanceAuthority = null,
        RoadSegmentMorphology? morphology = null,
        RoadSegmentStatus? status = null,
        RoadSegmentCategory? category = null,
        RoadSegmentAccessRestriction? accessRestriction = null,
        StreetNameLocalId? leftSideStreetNameId = null,
        StreetNameLocalId? rightSideStreetNameId = null,
        MultiLineString? geometry = null,
        RoadSegmentId? originalId = null)
        : this(
            recordNumber,
            id,
            geometryDrawMethod,
            originalId,
            startNodeId,
            endNodeId,
            geometry,
            maintenanceAuthority,
            morphology,
            status,
            category,
            accessRestriction,
            leftSideStreetNameId,
            rightSideStreetNameId,
            null,
            null,
            null,
            false,
            null,
            null,
            null
        )
    {
    }

    private ModifyRoadSegment(
        RecordNumber recordNumber,
        RoadSegmentId id,
        RoadSegmentGeometryDrawMethod geometryDrawMethod,
        RoadSegmentId? originalId,
        RoadNodeId? startNodeId,
        RoadNodeId? endNodeId,
        MultiLineString? geometry,
        OrganizationId? maintenanceAuthority,
        RoadSegmentMorphology? morphology,
        RoadSegmentStatus? status,
        RoadSegmentCategory? category,
        RoadSegmentAccessRestriction? accessRestriction,
        StreetNameLocalId? leftSideStreetNameId,
        StreetNameLocalId? rightSideStreetNameId,
        IReadOnlyList<RoadSegmentLaneAttribute>? lanes,
        IReadOnlyList<RoadSegmentWidthAttribute>? widths,
        IReadOnlyList<RoadSegmentSurfaceAttribute>? surfaces,
        bool convertedFromOutlined,
        RoadSegmentVersion? version,
        GeometryVersion? geometryVersion,
        bool? categoryModified)
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
        AccessRestriction = accessRestriction;
        LeftSideStreetNameId = leftSideStreetNameId;
        RightSideStreetNameId = rightSideStreetNameId;
        Lanes = lanes;
        Widths = widths;
        Surfaces = surfaces;
        ConvertedFromOutlined = convertedFromOutlined;
        Version = version;
        GeometryVersion = geometryVersion;
        CategoryModified = categoryModified;
    }

    public RecordNumber RecordNumber { get; }
    public RoadSegmentId Id { get; }
    public RoadSegmentGeometryDrawMethod GeometryDrawMethod { get; }
    public RoadSegmentId? OriginalId { get; }
    public MultiLineString? Geometry { get; }
    public RoadNodeId? StartNodeId { get; }
    public RoadNodeId? EndNodeId { get; }
    public RoadSegmentAccessRestriction? AccessRestriction { get; }
    public RoadSegmentCategory? Category { get; }
    public OrganizationId? MaintenanceAuthority { get; }
    public RoadSegmentMorphology? Morphology { get; }
    public RoadSegmentStatus? Status { get; }
    public StreetNameLocalId? LeftSideStreetNameId { get; }
    public StreetNameLocalId? RightSideStreetNameId { get; }
    public IReadOnlyList<RoadSegmentLaneAttribute>? Lanes { get; }
    public IReadOnlyList<RoadSegmentSurfaceAttribute>? Surfaces { get; }
    public IReadOnlyList<RoadSegmentWidthAttribute>? Widths { get; }
    public bool ConvertedFromOutlined { get; }
    public RoadSegmentVersion? Version { get; }
    public GeometryVersion? GeometryVersion { get; }
    [Obsolete]
    public bool? CategoryModified { get; }

    public void TranslateTo(RequestedChange message)
    {
        ArgumentNullException.ThrowIfNull(message);

        message.ModifyRoadSegment = new Messages.ModifyRoadSegment
        {
            Id = Id,
            GeometryDrawMethod = GeometryDrawMethod,
            OriginalId = OriginalId,
            StartNodeId = StartNodeId,
            EndNodeId = EndNodeId,
            Geometry = Geometry is not null ? GeometryTranslator.Translate(Geometry) : null,
            MaintenanceAuthority = MaintenanceAuthority,
            Morphology = Morphology,
            Status = Status,
            Category = Category,
            AccessRestriction = AccessRestriction,
            LeftSideStreetNameId = LeftSideStreetNameId,
            RightSideStreetNameId = RightSideStreetNameId,
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
                .ToArray(),
            ConvertedFromOutlined = ConvertedFromOutlined,
            Version = Version,
            GeometryVersion = GeometryVersion,
            CategoryModified = CategoryModified
        };
    }

    public ModifyRoadSegment WithGeometry(MultiLineString? geometry)
    {
        return new ModifyRoadSegment(
            RecordNumber, Id, GeometryDrawMethod, OriginalId, StartNodeId, EndNodeId, geometry,
            MaintenanceAuthority, Morphology, Status, Category, AccessRestriction,
            LeftSideStreetNameId, RightSideStreetNameId, Lanes, Widths, Surfaces, ConvertedFromOutlined, Version, GeometryVersion, CategoryModified);
    }

    public ModifyRoadSegment WithConvertedFromOutlined(bool convertedFromOutlined)
    {
        return new ModifyRoadSegment(
            RecordNumber, Id, GeometryDrawMethod, OriginalId, StartNodeId, EndNodeId, Geometry,
            MaintenanceAuthority, Morphology, Status, Category, AccessRestriction,
            LeftSideStreetNameId, RightSideStreetNameId, Lanes, Widths, Surfaces, convertedFromOutlined, Version, GeometryVersion, CategoryModified);
    }

    [Obsolete]
    public ModifyRoadSegment WithCategoryModified(bool categoryModified)
    {
        return new ModifyRoadSegment(
            RecordNumber, Id, GeometryDrawMethod, OriginalId, StartNodeId, EndNodeId, Geometry,
            MaintenanceAuthority, Morphology, Status, Category, AccessRestriction,
            LeftSideStreetNameId, RightSideStreetNameId, Lanes, Widths, Surfaces, ConvertedFromOutlined, Version, GeometryVersion, categoryModified: categoryModified);
    }

    public ModifyRoadSegment WithOriginalId(RoadSegmentId originalId)
    {
        return new ModifyRoadSegment(
            RecordNumber, Id, GeometryDrawMethod, originalId, StartNodeId, EndNodeId, Geometry,
            MaintenanceAuthority, Morphology, Status, Category, AccessRestriction,
            LeftSideStreetNameId, RightSideStreetNameId, Lanes, Widths, Surfaces, ConvertedFromOutlined, Version, GeometryVersion, CategoryModified);
    }

    public ModifyRoadSegment WithLanes(IEnumerable<RoadSegmentLaneAttribute>? lanes)
    {
        if (lanes is null)
        {
            return this;
        }

        var segment = this;
        foreach (var lane in lanes)
        {
            segment = segment.WithLane(lane);
        }

        return segment;
    }
    public ModifyRoadSegment WithLane(RoadSegmentLaneAttribute lane)
    {
        var lanes = new List<RoadSegmentLaneAttribute>(Lanes ?? []) { lane };
        lanes.Sort((left, right) => left.From.CompareTo(right.From));
        return new ModifyRoadSegment(
            RecordNumber, Id, GeometryDrawMethod, OriginalId, StartNodeId, EndNodeId, Geometry,
            MaintenanceAuthority, Morphology, Status, Category, AccessRestriction,
            LeftSideStreetNameId, RightSideStreetNameId,
            lanes, Widths, Surfaces, ConvertedFromOutlined, Version, GeometryVersion, CategoryModified);
    }

    public ModifyRoadSegment WithSurfaces(IEnumerable<RoadSegmentSurfaceAttribute>? surfaces)
    {
        if (surfaces is null)
        {
            return this;
        }

        var segment = this;
        foreach (var surface in surfaces)
        {
            segment = segment.WithSurface(surface);
        }

        return segment;
    }
    public ModifyRoadSegment WithSurface(RoadSegmentSurfaceAttribute surface)
    {
        var surfaces = new List<RoadSegmentSurfaceAttribute>(Surfaces ?? []) { surface };
        surfaces.Sort((left, right) => left.From.CompareTo(right.From));
        return new ModifyRoadSegment(
            RecordNumber, Id, GeometryDrawMethod, OriginalId, StartNodeId, EndNodeId, Geometry,
            MaintenanceAuthority, Morphology, Status, Category, AccessRestriction,
            LeftSideStreetNameId, RightSideStreetNameId,
            Lanes, Widths, surfaces, ConvertedFromOutlined, Version, GeometryVersion, CategoryModified);
    }

    public ModifyRoadSegment WithWidths(IEnumerable<RoadSegmentWidthAttribute>? widths)
    {
        if (widths is null)
        {
            return this;
        }

        var segment = this;
        foreach (var width in widths)
        {
            segment = segment.WithWidth(width);
        }

        return segment;
    }
    public ModifyRoadSegment WithWidth(RoadSegmentWidthAttribute width)
    {
        var widths = new List<RoadSegmentWidthAttribute>(Widths ?? []) { width };
        widths.Sort((left, right) => left.From.CompareTo(right.From));
        return new ModifyRoadSegment(
            RecordNumber, Id, GeometryDrawMethod, OriginalId, StartNodeId, EndNodeId, Geometry,
            MaintenanceAuthority, Morphology, Status, Category, AccessRestriction,
            LeftSideStreetNameId, RightSideStreetNameId,
            Lanes, widths, Surfaces, ConvertedFromOutlined, Version, GeometryVersion, CategoryModified);
    }

    public ModifyRoadSegment WithVersion(RoadSegmentVersion version)
    {
        return new ModifyRoadSegment(
            RecordNumber, Id, GeometryDrawMethod, OriginalId, StartNodeId, EndNodeId, Geometry,
            MaintenanceAuthority, Morphology, Status, Category, AccessRestriction,
            LeftSideStreetNameId, RightSideStreetNameId, Lanes, Widths, Surfaces, ConvertedFromOutlined, version, GeometryVersion, CategoryModified);
    }

    public ModifyRoadSegment WithGeometryVersion(GeometryVersion geometryVersion)
    {
        return new ModifyRoadSegment(
            RecordNumber, Id, GeometryDrawMethod, OriginalId, StartNodeId, EndNodeId, Geometry,
            MaintenanceAuthority, Morphology, Status, Category, AccessRestriction,
            LeftSideStreetNameId, RightSideStreetNameId, Lanes, Widths, Surfaces, ConvertedFromOutlined, Version, GeometryVersion, CategoryModified);
    }
}
