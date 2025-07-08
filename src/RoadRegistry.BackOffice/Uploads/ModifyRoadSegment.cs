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
        RoadNodeId? startNodeId,
        RoadNodeId? endNodeId,
        OrganizationId? maintenanceAuthority,
        RoadSegmentMorphology? morphology,
        RoadSegmentStatus? status,
        RoadSegmentCategory? category,
        RoadSegmentAccessRestriction? accessRestriction,
        StreetNameLocalId? leftSideStreetNameId,
        StreetNameLocalId? rightSideStreetNameId)
        : this(
            recordNumber,
            id,
            geometryDrawMethod,
            null,
            startNodeId,
            endNodeId,
            null,
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
        GeometryVersion? geometryVersion)
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
            GeometryVersion = GeometryVersion
        };
    }

    public ModifyRoadSegment WithGeometry(MultiLineString? geometry)
    {
        return new ModifyRoadSegment(
            RecordNumber, Id, GeometryDrawMethod, OriginalId, StartNodeId, EndNodeId, geometry,
            MaintenanceAuthority, Morphology, Status, Category, AccessRestriction,
            LeftSideStreetNameId, RightSideStreetNameId, Lanes, Widths, Surfaces, ConvertedFromOutlined, Version, GeometryVersion);
    }

    public ModifyRoadSegment WithConvertedFromOutlined(bool convertedFromOutlined)
    {
        return new ModifyRoadSegment(
            RecordNumber, Id, GeometryDrawMethod, OriginalId, StartNodeId, EndNodeId, Geometry,
            MaintenanceAuthority, Morphology, Status, Category, AccessRestriction,
            LeftSideStreetNameId, RightSideStreetNameId, Lanes, Widths, Surfaces, convertedFromOutlined, Version, GeometryVersion);
    }

    public ModifyRoadSegment WithOriginalId(RoadSegmentId originalId)
    {
        return new ModifyRoadSegment(
            RecordNumber, Id, GeometryDrawMethod, originalId, StartNodeId, EndNodeId, Geometry,
            MaintenanceAuthority, Morphology, Status, Category, AccessRestriction,
            LeftSideStreetNameId, RightSideStreetNameId, Lanes, Widths, Surfaces, ConvertedFromOutlined, Version, GeometryVersion);
    }

    public ModifyRoadSegment WithLane(RoadSegmentLaneAttribute lane)
    {
        var lanes = new List<RoadSegmentLaneAttribute>(Lanes ?? []) { lane };
        lanes.Sort((left, right) => left.From.CompareTo(right.From));
        return new ModifyRoadSegment(
            RecordNumber, Id, GeometryDrawMethod, OriginalId, StartNodeId, EndNodeId, Geometry,
            MaintenanceAuthority, Morphology, Status, Category, AccessRestriction,
            LeftSideStreetNameId, RightSideStreetNameId,
            lanes, Widths, Surfaces, ConvertedFromOutlined, Version, GeometryVersion);
    }

    public ModifyRoadSegment WithSurface(RoadSegmentSurfaceAttribute surface)
    {
        var surfaces = new List<RoadSegmentSurfaceAttribute>(Surfaces ?? []) { surface };
        surfaces.Sort((left, right) => left.From.CompareTo(right.From));
        return new ModifyRoadSegment(
            RecordNumber, Id, GeometryDrawMethod, OriginalId, StartNodeId, EndNodeId, Geometry,
            MaintenanceAuthority, Morphology, Status, Category, AccessRestriction,
            LeftSideStreetNameId, RightSideStreetNameId,
            Lanes, Widths, surfaces, ConvertedFromOutlined, Version, GeometryVersion);
    }

    public ModifyRoadSegment WithWidth(RoadSegmentWidthAttribute width)
    {
        var widths = new List<RoadSegmentWidthAttribute>(Widths ?? []) { width };
        widths.Sort((left, right) => left.From.CompareTo(right.From));
        return new ModifyRoadSegment(
            RecordNumber, Id, GeometryDrawMethod, OriginalId, StartNodeId, EndNodeId, Geometry,
            MaintenanceAuthority, Morphology, Status, Category, AccessRestriction,
            LeftSideStreetNameId, RightSideStreetNameId,
            Lanes, widths, Surfaces, ConvertedFromOutlined, Version, GeometryVersion);
    }

    public ModifyRoadSegment WithVersion(RoadSegmentVersion version)
    {
        return new ModifyRoadSegment(
            RecordNumber, Id, GeometryDrawMethod, OriginalId, StartNodeId, EndNodeId, Geometry,
            MaintenanceAuthority, Morphology, Status, Category, AccessRestriction,
            LeftSideStreetNameId, RightSideStreetNameId, Lanes, Widths, Surfaces, ConvertedFromOutlined, version, GeometryVersion);
    }

    public ModifyRoadSegment WithGeometryVersion(GeometryVersion geometryVersion)
    {
        return new ModifyRoadSegment(
            RecordNumber, Id, GeometryDrawMethod, OriginalId, StartNodeId, EndNodeId, Geometry,
            MaintenanceAuthority, Morphology, Status, Category, AccessRestriction,
            LeftSideStreetNameId, RightSideStreetNameId, Lanes, Widths, Surfaces, ConvertedFromOutlined, Version, geometryVersion);
    }
}
