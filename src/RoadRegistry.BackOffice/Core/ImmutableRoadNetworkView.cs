namespace RoadRegistry.BackOffice.Core;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Messages;
using NetTopologySuite.Geometries;

public partial class ImmutableRoadNetworkView : IRoadNetworkView
{
    public static readonly IRoadNetworkView Empty = new ImmutableRoadNetworkView(
        ImmutableDictionary<RoadNodeId, RoadNode>.Empty,
        ImmutableDictionary<RoadSegmentId, RoadSegment>.Empty,
        ImmutableDictionary<GradeSeparatedJunctionId, GradeSeparatedJunction>.Empty,
        ImmutableDictionary<RoadSegmentId, IReadOnlyList<AttributeId>>.Empty,
        ImmutableDictionary<RoadSegmentId, IReadOnlyList<AttributeId>>.Empty,
        ImmutableDictionary<RoadSegmentId, IReadOnlyList<AttributeId>>.Empty);

    private readonly ImmutableDictionary<GradeSeparatedJunctionId, GradeSeparatedJunction> _gradeSeparatedJunctions;
    private readonly ImmutableDictionary<RoadNodeId, RoadNode> _nodes;
    private readonly ImmutableDictionary<RoadSegmentId, RoadSegment> _segments;

    private ImmutableRoadNetworkView(
        ImmutableDictionary<RoadNodeId, RoadNode> nodes,
        ImmutableDictionary<RoadSegmentId, RoadSegment> segments,
        ImmutableDictionary<GradeSeparatedJunctionId, GradeSeparatedJunction> gradeSeparatedJunctions,
        ImmutableDictionary<RoadSegmentId, IReadOnlyList<AttributeId>> segmentReusableLaneAttributeIdentifiers,
        ImmutableDictionary<RoadSegmentId, IReadOnlyList<AttributeId>> segmentReusableWidthAttributeIdentifiers,
        ImmutableDictionary<RoadSegmentId, IReadOnlyList<AttributeId>> segmentReusableSurfaceAttributeIdentifiers)
    {
        _nodes = nodes;
        _segments = segments;
        _gradeSeparatedJunctions = gradeSeparatedJunctions;
        SegmentReusableLaneAttributeIdentifiers = segmentReusableLaneAttributeIdentifiers;
        SegmentReusableWidthAttributeIdentifiers = segmentReusableWidthAttributeIdentifiers;
        SegmentReusableSurfaceAttributeIdentifiers = segmentReusableSurfaceAttributeIdentifiers;
    }

    public IReadOnlyDictionary<GradeSeparatedJunctionId, GradeSeparatedJunction> GradeSeparatedJunctions => _gradeSeparatedJunctions;
    public IReadOnlyDictionary<RoadNodeId, RoadNode> Nodes => _nodes;
    public ImmutableDictionary<RoadSegmentId, IReadOnlyList<AttributeId>> SegmentReusableLaneAttributeIdentifiers { get; }
    public ImmutableDictionary<RoadSegmentId, IReadOnlyList<AttributeId>> SegmentReusableSurfaceAttributeIdentifiers { get; }
    public ImmutableDictionary<RoadSegmentId, IReadOnlyList<AttributeId>> SegmentReusableWidthAttributeIdentifiers { get; }
    public IReadOnlyDictionary<RoadSegmentId, RoadSegment> Segments => _segments;

    public IScopedRoadNetworkView CreateScopedView(Envelope envelope)
    {
        ArgumentNullException.ThrowIfNull(envelope);

        // Any segments that intersect the envelope
        var segments = Segments
            .Where(pair => envelope.Intersects(pair.Value.Geometry.EnvelopeInternal))
            .ToDictionary(pair => pair.Key, pair => pair.Value);
        var segmentNodes = segments.SelectMany(segment => segment.Value.Nodes).ToList();

        // Any nodes that the envelope contains or are linked to previously found segments
        var nodes = Nodes
            .Where(pair => envelope.Contains(pair.Value.Geometry.Coordinate) || segmentNodes.Contains(pair.Key))
            .ToDictionary(pair => pair.Key, pair => pair.Value);

        // Any junctions for which either the lower or the upper segment intersects the envelope
        var junctions = GradeSeparatedJunctions
            .Where(pair =>
                (Segments.TryGetValue(pair.Value.LowerSegment, out var lowerSegment) && envelope.Intersects(lowerSegment.Geometry.EnvelopeInternal))
                ||
                (Segments.TryGetValue(pair.Value.UpperSegment, out var upperSegment) && envelope.Intersects(upperSegment.Geometry.EnvelopeInternal)))
            .ToDictionary(pair => pair.Key, pair => pair.Value);

        return new ImmutableScopedRoadNetworkView(
            envelope,
            nodes,
            segments,
            junctions,
            this);
    }

    public RoadNetworkSnapshot TakeSnapshot()
    {
        return new RoadNetworkSnapshot
        {
            Nodes = _nodes.Select(node => new RoadNetworkSnapshotNode
            {
                Id = node.Value.Id.ToInt32(),
                Segments = node.Value.Segments.Select(segment => segment.ToInt32()).ToArray(),
                Type = node.Value.Type,
                Geometry = GeometryTranslator.Translate(node.Value.Geometry),
                Version = node.Value.Version
            }).ToArray(),
            Segments = _segments.Select(segment => new RoadNetworkSnapshotSegment
            {
                Id = segment.Value.Id.ToInt32(),
                Version = segment.Value.Version.ToInt32(),
                StartNodeId = segment.Value.Start.ToInt32(),
                EndNodeId = segment.Value.End.ToInt32(),
                Geometry = GeometryTranslator.Translate(segment.Value.Geometry),
                GeometryVersion = segment.Value.GeometryVersion.ToInt32(),
                AttributeHash = new RoadNetworkSnapshotSegmentAttributeHash
                {
                    AccessRestriction = segment.Value.AttributeHash.AccessRestriction,
                    Category = segment.Value.AttributeHash.Category,
                    Morphology = segment.Value.AttributeHash.Morphology,
                    Status = segment.Value.AttributeHash.Status,
                    LeftSideStreetNameId = segment.Value.AttributeHash.LeftStreetNameId?.ToInt32(),
                    RightSideStreetNameId = segment.Value.AttributeHash.RightStreetNameId?.ToInt32(),
                    OrganizationId = segment.Value.AttributeHash.OrganizationId,
                    GeometryDrawMethod = segment.Value.AttributeHash.GeometryDrawMethod.ToString()
                },
                EuropeanRoadAttributes = segment.Value.EuropeanRoadAttributes
                    .Select(attribute => new RoadNetworkSnapshotSegmentEuropeanRoadAttribute
                    {
                        AttributeId = attribute.Value.AttributeId,
                        Number = attribute.Value.Number
                    }).ToArray(),
                NationalRoadAttributes = segment.Value.NationalRoadAttributes
                    .Select(attribute => new RoadNetworkSnapshotSegmentNationalRoadAttribute
                    {
                        AttributeId = attribute.Value.AttributeId,
                        Number = attribute.Value.Number
                    }).ToArray(),
                NumberedRoadAttributes = segment.Value.NumberedRoadAttributes
                    .Select(attribute => new RoadNetworkSnapshotSegmentNumberedRoadAttribute
                    {
                        AttributeId = attribute.Value.AttributeId,
                        Direction = attribute.Value.Direction,
                        Number = attribute.Value.Number,
                        Ordinal = attribute.Value.Ordinal
                    }).ToArray(),
                Lanes = segment.Value.Lanes.Select(lane => new RoadNetworkSnapshotSegmentLaneAttribute
                {
                    FromPosition = lane.From.ToDecimal(),
                    ToPosition = lane.To.ToDecimal(),
                    Count = lane.Count.ToInt32(),
                    Direction = lane.Direction.Translation.Identifier,
                    AsOfGeometryVersion = lane.AsOfGeometryVersion
                }).ToArray(),
                Surfaces = segment.Value.Surfaces.Select(surface => new RoadNetworkSnapshotSegmentSurfaceAttribute
                {
                    FromPosition = surface.From.ToDecimal(),
                    ToPosition = surface.To.ToDecimal(),
                    Type = surface.Type.Translation.Identifier,
                    AsOfGeometryVersion = surface.AsOfGeometryVersion
                }).ToArray(),
                Widths = segment.Value.Widths.Select(width => new RoadNetworkSnapshotSegmentWidthAttribute
                {
                    FromPosition = width.From.ToDecimal(),
                    ToPosition = width.To.ToDecimal(),
                    Width = width.Width.ToInt32(),
                    AsOfGeometryVersion = width.AsOfGeometryVersion
                }).ToArray()
            }).ToArray(),
            GradeSeparatedJunctions = _gradeSeparatedJunctions.Select(gradeSeparatedJunction => new RoadNetworkSnapshotGradeSeparatedJunction
            {
                Id = gradeSeparatedJunction.Value.Id,
                Type = gradeSeparatedJunction.Value.Type,
                UpperSegmentId = gradeSeparatedJunction.Value.UpperSegment,
                LowerSegmentId = gradeSeparatedJunction.Value.LowerSegment
            }).ToArray(),
            SegmentReusableLaneAttributeIdentifiers = SegmentReusableLaneAttributeIdentifiers.Select(segment =>
                new RoadNetworkSnapshotSegmentReusableAttributeIdentifiers
                {
                    SegmentId = segment.Key.ToInt32(),
                    ReusableAttributeIdentifiers = segment.Value.Select(lane => lane.ToInt32()).ToArray()
                }).ToArray(),
            SegmentReusableWidthAttributeIdentifiers = SegmentReusableWidthAttributeIdentifiers.Select(segment =>
                new RoadNetworkSnapshotSegmentReusableAttributeIdentifiers
                {
                    SegmentId = segment.Key.ToInt32(),
                    ReusableAttributeIdentifiers = segment.Value.Select(width => width.ToInt32()).ToArray()
                }).ToArray(),
            SegmentReusableSurfaceAttributeIdentifiers = SegmentReusableSurfaceAttributeIdentifiers.Select(
                segment =>
                    new RoadNetworkSnapshotSegmentReusableAttributeIdentifiers
                    {
                        SegmentId = segment.Key.ToInt32(),
                        ReusableAttributeIdentifiers = segment.Value.Select(surface => surface.ToInt32()).ToArray()
                    }).ToArray()
        };
    }

    public IRoadNetworkView With(IReadOnlyCollection<IRequestedChange> changes)
    {
        ArgumentNullException.ThrowIfNull(changes);

        var result = this;
        foreach (var change in changes)
            switch (change)
            {
                case AddRoadNode addRoadNode:
                    result = result.With(addRoadNode);
                    break;
                case ModifyRoadNode modifyRoadNode:
                    result = result.With(modifyRoadNode);
                    break;
                case RemoveRoadNode removeRoadNode:
                    result = result.With(removeRoadNode);
                    break;
                case AddRoadSegment addRoadSegment:
                    result = result.With(addRoadSegment);
                    break;
                case ModifyRoadSegment modifyRoadSegment:
                    result = result.With(modifyRoadSegment);
                    break;
                case ModifyRoadSegmentAttributes modifyRoadSegmentAttributes:
                    result = result.With(modifyRoadSegmentAttributes);
                    break;
                case ModifyRoadSegmentGeometry modifyRoadSegmentGeometry:
                    result = result.With(modifyRoadSegmentGeometry);
                    break;
                case RemoveRoadSegment removeRoadSegment:
                    result = result.With(removeRoadSegment);
                    break;
                case RemoveRoadSegments removeRoadSegments:
                    result = result.With(removeRoadSegments);
                    break;
                case AddRoadSegmentToEuropeanRoad addRoadSegmentToEuropeanRoad:
                    result = result.With(addRoadSegmentToEuropeanRoad);
                    break;
                case RemoveRoadSegmentFromEuropeanRoad removeRoadSegmentFromEuropeanRoad:
                    result = result.With(removeRoadSegmentFromEuropeanRoad);
                    break;
                case AddRoadSegmentToNationalRoad addRoadSegmentToNationalRoad:
                    result = result.With(addRoadSegmentToNationalRoad);
                    break;
                case RemoveRoadSegmentFromNationalRoad removeRoadSegmentFromNationalRoad:
                    result = result.With(removeRoadSegmentFromNationalRoad);
                    break;
                case AddRoadSegmentToNumberedRoad addRoadSegmentToNumberedRoad:
                    result = result.With(addRoadSegmentToNumberedRoad);
                    break;
                case RemoveRoadSegmentFromNumberedRoad removeRoadSegmentFromNumberedRoad:
                    result = result.With(removeRoadSegmentFromNumberedRoad);
                    break;
                case AddGradeSeparatedJunction addGradeSeparatedJunction:
                    result = result.With(addGradeSeparatedJunction);
                    break;
                case ModifyGradeSeparatedJunction modifyGradeSeparatedJunction:
                    result = result.With(modifyGradeSeparatedJunction);
                    break;
                case RemoveGradeSeparatedJunction removeGradeSeparatedJunction:
                    result = result.With(removeGradeSeparatedJunction);
                    break;
            }

        return result;
    }

    private ImmutableRoadNetworkView WithRemovedRoadSegment(RoadSegmentId id)
    {
        return new ImmutableRoadNetworkView(
            _segments.TryGetValue(id, out var segment)
                ? _nodes
                    .TryReplace(segment.Start, node => node.DisconnectFrom(id))
                    .TryReplace(segment.End, node => node.DisconnectFrom(id))
                : _nodes,
            _segments.Remove(id),
            _gradeSeparatedJunctions,
            SegmentReusableLaneAttributeIdentifiers,
            SegmentReusableWidthAttributeIdentifiers,
            SegmentReusableSurfaceAttributeIdentifiers
        );
    }

    private ImmutableRoadNetworkView WithRemovedGradeSeparatedJunction(GradeSeparatedJunctionId id)
    {
        return new ImmutableRoadNetworkView(
            _nodes,
            _segments,
            _gradeSeparatedJunctions.Remove(id),
            SegmentReusableLaneAttributeIdentifiers,
            SegmentReusableWidthAttributeIdentifiers,
            SegmentReusableSurfaceAttributeIdentifiers
        );
    }

    private ImmutableRoadNetworkView WithRemovedRoadNode(RoadNodeId id)
    {
        return new ImmutableRoadNetworkView(
            _nodes.Remove(id),
            _segments,
            _gradeSeparatedJunctions,
            SegmentReusableLaneAttributeIdentifiers,
            SegmentReusableWidthAttributeIdentifiers,
            SegmentReusableSurfaceAttributeIdentifiers
        );
    }

    private ImmutableRoadNetworkView WithChangedRoadNodeType(RoadNodeId id, RoadNodeType type)
    {
        return new ImmutableRoadNetworkView(
            _nodes.TryReplace(id, x => x.WithType(type)),
            _segments,
            _gradeSeparatedJunctions,
            SegmentReusableLaneAttributeIdentifiers,
            SegmentReusableWidthAttributeIdentifiers,
            SegmentReusableSurfaceAttributeIdentifiers
        );
    }
}
