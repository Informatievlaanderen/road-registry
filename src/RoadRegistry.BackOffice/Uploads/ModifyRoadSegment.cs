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
            accessRestriction.ThrowIfNull(),
            leftSideStreetNameId,
            rightSideStreetNameId,
            Array.Empty<RoadSegmentLaneAttribute>(),
            Array.Empty<RoadSegmentWidthAttribute>(),
            Array.Empty<RoadSegmentSurfaceAttribute>(),
            false,
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
        RoadSegmentAccessRestriction accessRestriction,
        StreetNameLocalId? leftSideStreetNameId,
        StreetNameLocalId? rightSideStreetNameId,
        IReadOnlyList<RoadSegmentLaneAttribute> lanes,
        IReadOnlyList<RoadSegmentWidthAttribute> widths,
        IReadOnlyList<RoadSegmentSurfaceAttribute> surfaces,
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

    public RoadSegmentAccessRestriction AccessRestriction { get; }
    public RoadSegmentCategory Category { get; }
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
    public bool ConvertedFromOutlined { get; }
    public RoadSegmentVersion? Version { get; }
    public GeometryVersion? GeometryVersion { get; }

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
            ConvertedFromOutlined = ConvertedFromOutlined,
            Version = Version,
            GeometryVersion = GeometryVersion
        };
    }

    public ModifyRoadSegment WithGeometry(MultiLineString geometry)
    {
        if (geometry == null) throw new ArgumentNullException(nameof(geometry));
        return new ModifyRoadSegment(
            RecordNumber, Id, OriginalId, StartNodeId, EndNodeId, geometry,
            MaintenanceAuthority, GeometryDrawMethod, Morphology, Status, Category, AccessRestriction,
            LeftSideStreetNameId, RightSideStreetNameId, Lanes, Widths, Surfaces, ConvertedFromOutlined, Version, GeometryVersion);
    }

    public ModifyRoadSegment WithConvertedFromOutlined(bool convertedFromOutlined)
    {
        return new ModifyRoadSegment(
            RecordNumber, Id, OriginalId, StartNodeId, EndNodeId, Geometry,
            MaintenanceAuthority, GeometryDrawMethod, Morphology, Status, Category, AccessRestriction,
            LeftSideStreetNameId, RightSideStreetNameId, Lanes, Widths, Surfaces, convertedFromOutlined, Version, GeometryVersion);
    }

    public ModifyRoadSegment WithOriginalId(RoadSegmentId originalId)
    {
        return new ModifyRoadSegment(
            RecordNumber, Id, originalId, StartNodeId, EndNodeId, Geometry,
            MaintenanceAuthority, GeometryDrawMethod, Morphology, Status, Category, AccessRestriction,
            LeftSideStreetNameId, RightSideStreetNameId, Lanes, Widths, Surfaces, ConvertedFromOutlined, Version, GeometryVersion);
    }
    
    public ModifyRoadSegment WithLane(RoadSegmentLaneAttribute lane)
    {
        var lanes = new List<RoadSegmentLaneAttribute>(Lanes) { lane };
        lanes.Sort((left, right) => left.From.CompareTo(right.From));
        return new ModifyRoadSegment(
            RecordNumber, Id, OriginalId, StartNodeId, EndNodeId, Geometry,
            MaintenanceAuthority, GeometryDrawMethod, Morphology, Status, Category, AccessRestriction,
            LeftSideStreetNameId, RightSideStreetNameId,
            lanes, Widths, Surfaces, ConvertedFromOutlined, Version, GeometryVersion);
    }

    public ModifyRoadSegment WithSurface(RoadSegmentSurfaceAttribute surface)
    {
        var surfaces = new List<RoadSegmentSurfaceAttribute>(Surfaces) { surface };
        surfaces.Sort((left, right) => left.From.CompareTo(right.From));
        return new ModifyRoadSegment(
            RecordNumber, Id, OriginalId, StartNodeId, EndNodeId, Geometry,
            MaintenanceAuthority, GeometryDrawMethod, Morphology, Status, Category, AccessRestriction,
            LeftSideStreetNameId, RightSideStreetNameId,
            Lanes, Widths, surfaces, ConvertedFromOutlined, Version, GeometryVersion);
    }

    public ModifyRoadSegment WithWidth(RoadSegmentWidthAttribute width)
    {
        var widths = new List<RoadSegmentWidthAttribute>(Widths) { width };
        widths.Sort((left, right) => left.From.CompareTo(right.From));
        return new ModifyRoadSegment(
            RecordNumber, Id, OriginalId, StartNodeId, EndNodeId, Geometry,
            MaintenanceAuthority, GeometryDrawMethod, Morphology, Status, Category, AccessRestriction,
            LeftSideStreetNameId, RightSideStreetNameId,
            Lanes, widths, Surfaces, ConvertedFromOutlined, Version, GeometryVersion);
    }

    public ModifyRoadSegment WithVersion(RoadSegmentVersion version)
    {
        return new ModifyRoadSegment(
            RecordNumber, Id, OriginalId, StartNodeId, EndNodeId, Geometry,
            MaintenanceAuthority, GeometryDrawMethod, Morphology, Status, Category, AccessRestriction,
            LeftSideStreetNameId, RightSideStreetNameId, Lanes, Widths, Surfaces, ConvertedFromOutlined, version, GeometryVersion);
    }

    public ModifyRoadSegment WithGeometryVersion(GeometryVersion geometryVersion)
    {
        return new ModifyRoadSegment(
            RecordNumber, Id, OriginalId, StartNodeId, EndNodeId, Geometry,
            MaintenanceAuthority, GeometryDrawMethod, Morphology, Status, Category, AccessRestriction,
            LeftSideStreetNameId, RightSideStreetNameId, Lanes, Widths, Surfaces, ConvertedFromOutlined, Version, geometryVersion);
    }
}
