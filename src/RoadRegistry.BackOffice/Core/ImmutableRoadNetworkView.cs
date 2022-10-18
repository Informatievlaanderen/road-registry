namespace RoadRegistry.BackOffice.Core;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Messages;
using NetTopologySuite.Geometries;

public class ImmutableRoadNetworkView : IRoadNetworkView
{
    public static readonly ImmutableRoadNetworkView Empty = new(
        ImmutableDictionary<RoadNodeId, RoadNode>.Empty,
        ImmutableDictionary<RoadSegmentId, RoadSegment>.Empty,
        ImmutableDictionary<GradeSeparatedJunctionId, GradeSeparatedJunction>.Empty,
        new TransactionId(0),
        new RoadNodeId(0),
        new RoadSegmentId(0),
        new GradeSeparatedJunctionId(0),
        new AttributeId(0),
        new AttributeId(0),
        new AttributeId(0),
        new AttributeId(0),
        new AttributeId(0),
        new AttributeId(0),
        ImmutableDictionary<RoadSegmentId, IReadOnlyList<AttributeId>>.Empty,
        ImmutableDictionary<RoadSegmentId, IReadOnlyList<AttributeId>>.Empty,
        ImmutableDictionary<RoadSegmentId, IReadOnlyList<AttributeId>>.Empty);

    private readonly ImmutableDictionary<GradeSeparatedJunctionId, GradeSeparatedJunction> _gradeSeparatedJunctions;
    private readonly AttributeId _maximumEuropeanRoadAttributeId;
    private readonly GradeSeparatedJunctionId _maximumGradeSeparatedJunctionId;
    private readonly AttributeId _maximumLaneAttributeId;
    private readonly AttributeId _maximumNationalRoadAttributeId;
    private readonly RoadNodeId _maximumNodeId;
    private readonly AttributeId _maximumNumberedRoadAttributeId;
    private readonly RoadSegmentId _maximumSegmentId;
    private readonly AttributeId _maximumSurfaceAttributeId;
    private readonly TransactionId _maximumTransactionId;
    private readonly AttributeId _maximumWidthAttributeId;

    private readonly ImmutableDictionary<RoadNodeId, RoadNode> _nodes;

    private readonly ImmutableDictionary<RoadSegmentId, RoadSegment> _segments;

    private ImmutableRoadNetworkView(
        ImmutableDictionary<RoadNodeId, RoadNode> nodes,
        ImmutableDictionary<RoadSegmentId, RoadSegment> segments,
        ImmutableDictionary<GradeSeparatedJunctionId, GradeSeparatedJunction> gradeSeparatedJunctions,
        TransactionId maximumTransactionId,
        RoadNodeId maximumNodeId,
        RoadSegmentId maximumSegmentId,
        GradeSeparatedJunctionId maximumGradeSeparatedJunctionId,
        AttributeId maximumEuropeanRoadAttributeId,
        AttributeId maximumNationalRoadAttributeId,
        AttributeId maximumNumberedRoadAttributeId,
        AttributeId maximumLaneAttributeId,
        AttributeId maximumWidthAttributeId,
        AttributeId maximumSurfaceAttributeId,
        ImmutableDictionary<RoadSegmentId, IReadOnlyList<AttributeId>> segmentReusableLaneAttributeIdentifiers,
        ImmutableDictionary<RoadSegmentId, IReadOnlyList<AttributeId>> segmentReusableWidthAttributeIdentifiers,
        ImmutableDictionary<RoadSegmentId, IReadOnlyList<AttributeId>> segmentReusableSurfaceAttributeIdentifiers)
    {
        _nodes = nodes;
        _segments = segments;
        _gradeSeparatedJunctions = gradeSeparatedJunctions;
        _maximumTransactionId = maximumTransactionId;
        _maximumNodeId = maximumNodeId;
        _maximumSegmentId = maximumSegmentId;
        _maximumGradeSeparatedJunctionId = maximumGradeSeparatedJunctionId;
        _maximumEuropeanRoadAttributeId = maximumEuropeanRoadAttributeId;
        _maximumNationalRoadAttributeId = maximumNationalRoadAttributeId;
        _maximumNumberedRoadAttributeId = maximumNumberedRoadAttributeId;
        _maximumLaneAttributeId = maximumLaneAttributeId;
        _maximumWidthAttributeId = maximumWidthAttributeId;
        _maximumSurfaceAttributeId = maximumSurfaceAttributeId;
        SegmentReusableLaneAttributeIdentifiers = segmentReusableLaneAttributeIdentifiers;
        SegmentReusableWidthAttributeIdentifiers = segmentReusableWidthAttributeIdentifiers;
        SegmentReusableSurfaceAttributeIdentifiers = segmentReusableSurfaceAttributeIdentifiers;
    }

    private sealed class Builder : IRoadNetworkView
    {
        private readonly ImmutableDictionary<GradeSeparatedJunctionId, GradeSeparatedJunction>.Builder _gradeSeparatedJunctions;
        private readonly ImmutableDictionary<RoadNodeId, RoadNode>.Builder _nodes;

        private readonly ImmutableDictionary<RoadSegmentId, IReadOnlyList<AttributeId>>.Builder
            _segmentReusableLaneAttributeIdentifiers;

        private readonly ImmutableDictionary<RoadSegmentId, IReadOnlyList<AttributeId>>.Builder
            _segmentReusableSurfaceAttributeIdentifiers;

        private readonly ImmutableDictionary<RoadSegmentId, IReadOnlyList<AttributeId>>.Builder
            _segmentReusableWidthAttributeIdentifiers;

        private readonly ImmutableDictionary<RoadSegmentId, RoadSegment>.Builder _segments;
        private AttributeId _maximumEuropeanRoadAttributeId;
        private GradeSeparatedJunctionId _maximumGradeSeparatedJunctionId;
        private AttributeId _maximumLaneAttributeId;
        private AttributeId _maximumNationalRoadAttributeId;
        private RoadNodeId _maximumNodeId;
        private AttributeId _maximumNumberedRoadAttributeId;
        private RoadSegmentId _maximumSegmentId;
        private AttributeId _maximumSurfaceAttributeId;
        private TransactionId _maximumTransactionId;
        private AttributeId _maximumWidthAttributeId;

        public Builder(
            ImmutableDictionary<RoadNodeId, RoadNode>.Builder nodes,
            ImmutableDictionary<RoadSegmentId, RoadSegment>.Builder segments,
            ImmutableDictionary<GradeSeparatedJunctionId, GradeSeparatedJunction>.Builder gradeSeparatedJunctions,
            TransactionId maximumTransactionId,
            RoadNodeId maximumNodeId,
            RoadSegmentId maximumSegmentId,
            GradeSeparatedJunctionId maximumGradeSeparatedJunctionId,
            AttributeId maximumEuropeanRoadAttributeId,
            AttributeId maximumNationalRoadAttributeId,
            AttributeId maximumNumberedRoadAttributeId,
            AttributeId maximumLaneAttributeId,
            AttributeId maximumWidthAttributeId,
            AttributeId maximumSurfaceAttributeId,
            ImmutableDictionary<RoadSegmentId, IReadOnlyList<AttributeId>>.Builder
                segmentReusableLaneAttributeIdentifiers,
            ImmutableDictionary<RoadSegmentId, IReadOnlyList<AttributeId>>.Builder
                segmentReusableWidthAttributeIdentifiers,
            ImmutableDictionary<RoadSegmentId, IReadOnlyList<AttributeId>>.Builder
                segmentReusableSurfaceAttributeIdentifiers)
        {
            _nodes = nodes;
            _segments = segments;
            _gradeSeparatedJunctions = gradeSeparatedJunctions;
            _maximumTransactionId = maximumTransactionId;
            _maximumNodeId = maximumNodeId;
            _maximumSegmentId = maximumSegmentId;
            _maximumGradeSeparatedJunctionId = maximumGradeSeparatedJunctionId;
            _maximumEuropeanRoadAttributeId = maximumEuropeanRoadAttributeId;
            _maximumNationalRoadAttributeId = maximumNationalRoadAttributeId;
            _maximumNumberedRoadAttributeId = maximumNumberedRoadAttributeId;
            _maximumLaneAttributeId = maximumLaneAttributeId;
            _maximumWidthAttributeId = maximumWidthAttributeId;
            _maximumSurfaceAttributeId = maximumSurfaceAttributeId;
            _segmentReusableLaneAttributeIdentifiers = segmentReusableLaneAttributeIdentifiers;
            _segmentReusableWidthAttributeIdentifiers = segmentReusableWidthAttributeIdentifiers;
            _segmentReusableSurfaceAttributeIdentifiers = segmentReusableSurfaceAttributeIdentifiers;
        }

        public IScopedRoadNetworkView CreateScopedView(Envelope envelope)
        {
            if (envelope == null) throw new ArgumentNullException(nameof(envelope));

            // Any nodes that the envelope contains
            var nodes = Nodes
                .Where(pair => envelope.Contains(pair.Value.Geometry.Coordinate))
                .ToDictionary(pair => pair.Key, pair => pair.Value);
            // Any segments that intersect the envelope
            var segments = Segments
                .Where(pair => envelope.Intersects(pair.Value.Geometry.EnvelopeInternal))
                .ToDictionary(pair => pair.Key, pair => pair.Value);
            // Any junctions for which either the lower or the upper segment intersects the envelope
            var junctions = GradeSeparatedJunctions
                .Where(pair => envelope.Intersects(Segments[pair.Value.LowerSegment].Geometry.EnvelopeInternal) || envelope.Intersects(Segments[pair.Value.UpperSegment].Geometry.EnvelopeInternal))
                .ToDictionary(pair => pair.Key, pair => pair.Value);

            return new ImmutableScopedRoadNetworkView(
                envelope,
                nodes,
                segments,
                junctions,
                this);
        }

        private void Given(ImportedRoadNode @event)
        {
            if (@event == null) throw new ArgumentNullException(nameof(@event));

            var id = new RoadNodeId(@event.Id);
            var type = RoadNodeType.Parse(@event.Type);
            var node = new RoadNode(id, type, GeometryTranslator.Translate(@event.Geometry));
            _nodes.Add(id, node);
            _maximumTransactionId =
                TransactionId.Max(new TransactionId(@event.Origin.TransactionId), _maximumTransactionId);
            _maximumNodeId = RoadNodeId.Max(id, _maximumNodeId);
        }

        private void Given(ImportedRoadSegment @event)
        {
            if (@event == null) throw new ArgumentNullException(nameof(@event));

            var id = new RoadSegmentId(@event.Id);
            var start = new RoadNodeId(@event.StartNodeId);
            var end = new RoadNodeId(@event.EndNodeId);

            var attributeHash = new AttributeHash(
                RoadSegmentAccessRestriction.Parse(@event.AccessRestriction),
                RoadSegmentCategory.Parse(@event.Category),
                RoadSegmentMorphology.Parse(@event.Morphology),
                RoadSegmentStatus.Parse(@event.Status),
                @event.LeftSide.StreetNameId.HasValue
                    ? new CrabStreetnameId(@event.LeftSide.StreetNameId.Value)
                    : new CrabStreetnameId?(),
                @event.RightSide.StreetNameId.HasValue
                    ? new CrabStreetnameId(@event.RightSide.StreetNameId.Value)
                    : new CrabStreetnameId?(),
                new OrganizationId(@event.MaintenanceAuthority.Code));

            var segment = new RoadSegment(
                id,
                GeometryTranslator.Translate(@event.Geometry),
                start,
                end,
                attributeHash);
            segment = @event.PartOfEuropeanRoads.Aggregate(segment, (current, europeanRoad) => current.PartOfEuropeanRoad(EuropeanRoadNumber.Parse(europeanRoad.Number)));
            segment = @event.PartOfNationalRoads.Aggregate(segment, (current, nationalRoad) => current.PartOfNationalRoad(NationalRoadNumber.Parse(nationalRoad.Number)));
            segment = @event.PartOfNumberedRoads.Aggregate(segment, (current, numberedRoad) => current.PartOfNumberedRoad(NumberedRoadNumber.Parse(numberedRoad.Number)));

            _nodes
                .TryReplace(start, node => node.ConnectWith(id))
                .TryReplace(end, node => node.ConnectWith(id));
            _segments.Add(id, segment);
            _maximumTransactionId =
                TransactionId.Max(new TransactionId(@event.Origin.TransactionId), _maximumTransactionId);
            _maximumSegmentId = RoadSegmentId.Max(id, _maximumSegmentId);
            _maximumEuropeanRoadAttributeId = @event.PartOfEuropeanRoads.Length != 0
                ? AttributeId.Max(
                    new AttributeId(@event.PartOfEuropeanRoads.Max(_ => _.AttributeId)),
                    _maximumEuropeanRoadAttributeId)
                : _maximumEuropeanRoadAttributeId;
            _maximumNationalRoadAttributeId = @event.PartOfNationalRoads.Length != 0
                ? AttributeId.Max(
                    new AttributeId(@event.PartOfNationalRoads.Max(_ => _.AttributeId)),
                    _maximumNationalRoadAttributeId)
                : _maximumNationalRoadAttributeId;
            _maximumNumberedRoadAttributeId = @event.PartOfNumberedRoads.Length != 0
                ? AttributeId.Max(
                    new AttributeId(@event.PartOfNumberedRoads.Max(_ => _.AttributeId)),
                    _maximumNumberedRoadAttributeId)
                : _maximumNumberedRoadAttributeId;
            _maximumLaneAttributeId = @event.Lanes.Length != 0
                ? AttributeId.Max(
                    new AttributeId(@event.Lanes.Max(_ => _.AttributeId)),
                    _maximumLaneAttributeId)
                : _maximumLaneAttributeId;
            _maximumWidthAttributeId = @event.Widths.Length != 0
                ? AttributeId.Max(
                    new AttributeId(@event.Widths.Max(_ => _.AttributeId)),
                    _maximumWidthAttributeId)
                : _maximumWidthAttributeId;
            _maximumSurfaceAttributeId = @event.Surfaces.Length != 0
                ? AttributeId.Max(
                    new AttributeId(@event.Surfaces.Max(_ => _.AttributeId)),
                    _maximumSurfaceAttributeId)
                : _maximumSurfaceAttributeId;
            _segmentReusableLaneAttributeIdentifiers.Merge(id,
                @event.Lanes.Select(lane => new AttributeId(lane.AttributeId)));
            _segmentReusableWidthAttributeIdentifiers.Merge(id,
                @event.Widths.Select(width => new AttributeId(width.AttributeId)));
            _segmentReusableSurfaceAttributeIdentifiers.Merge(id,
                @event.Surfaces.Select(surface => new AttributeId(surface.AttributeId)));
        }

        private void Given(ImportedGradeSeparatedJunction @event)
        {
            if (@event == null) throw new ArgumentNullException(nameof(@event));

            var id = new GradeSeparatedJunctionId(@event.Id);
            _maximumTransactionId =
                TransactionId.Max(new TransactionId(@event.Origin.TransactionId), _maximumTransactionId);
            _maximumGradeSeparatedJunctionId = GradeSeparatedJunctionId.Max(id, _maximumGradeSeparatedJunctionId);
            _gradeSeparatedJunctions.Add(
                id,
                new GradeSeparatedJunction(
                    id,
                    GradeSeparatedJunctionType.Parse(@event.Type),
                    new RoadSegmentId(@event.UpperRoadSegmentId),
                    new RoadSegmentId(@event.LowerRoadSegmentId)
                )
            );
        }

        private void Given(RoadNetworkChangesAccepted @event)
        {
            if (@event == null) throw new ArgumentNullException(nameof(@event));

            _maximumTransactionId =
                TransactionId.Max(new TransactionId(@event.TransactionId), _maximumTransactionId);

            foreach (var change in @event.Changes.Flatten())
                switch (change)
                {
                    case RoadNodeAdded roadNodeAdded:
                        Given(roadNodeAdded);
                        break;
                    case RoadNodeModified roadNodeModified:
                        Given(roadNodeModified);
                        break;
                    case RoadNodeRemoved roadNodeRemoved:
                        Given(roadNodeRemoved);
                        break;
                    case RoadSegmentAdded roadSegmentAdded:
                        Given(roadSegmentAdded);
                        break;
                    case RoadSegmentModified roadSegmentModified:
                        Given(roadSegmentModified);
                        break;
                    case RoadSegmentRemoved roadSegmentRemoved:
                        Given(roadSegmentRemoved);
                        break;
                    case RoadSegmentAddedToEuropeanRoad roadSegmentAddedToEuropeanRoad:
                        Given(roadSegmentAddedToEuropeanRoad);
                        break;
                    case RoadSegmentRemovedFromEuropeanRoad roadSegmentRemovedFromEuropeanRoad:
                        Given(roadSegmentRemovedFromEuropeanRoad);
                        break;
                    case RoadSegmentAddedToNationalRoad roadSegmentAddedToNationalRoad:
                        Given(roadSegmentAddedToNationalRoad);
                        break;
                    case RoadSegmentRemovedFromNationalRoad roadSegmentRemovedFromNationalRoad:
                        Given(roadSegmentRemovedFromNationalRoad);
                        break;
                    case RoadSegmentAddedToNumberedRoad roadSegmentAddedToNumberedRoad:
                        Given(roadSegmentAddedToNumberedRoad);
                        break;
                    case RoadSegmentOnNumberedRoadModified roadSegmentOnNumberedRoadModified:
                        Given(roadSegmentOnNumberedRoadModified);
                        break;
                    case RoadSegmentRemovedFromNumberedRoad roadSegmentRemovedFromNumberedRoad:
                        Given(roadSegmentRemovedFromNumberedRoad);
                        break;
                    case GradeSeparatedJunctionAdded gradeSeparatedJunctionAdded:
                        Given(gradeSeparatedJunctionAdded);
                        break;
                    case GradeSeparatedJunctionModified gradeSeparatedJunctionModified:
                        Given(gradeSeparatedJunctionModified);
                        break;
                    case GradeSeparatedJunctionRemoved gradeSeparatedJunctionRemoved:
                        Given(gradeSeparatedJunctionRemoved);
                        break;
                }
        }

        private void Given(RoadNodeAdded @event)
        {
            var id = new RoadNodeId(@event.Id);
            var type = RoadNodeType.Parse(@event.Type);
            var node = new RoadNode(id, type, GeometryTranslator.Translate(@event.Geometry));
            _nodes.Add(id, node);
            _maximumNodeId = RoadNodeId.Max(id, _maximumNodeId);
        }

        private void Given(RoadNodeModified @event)
        {
            var id = new RoadNodeId(@event.Id);
            _nodes.TryReplace(id, node => node
                .WithType(RoadNodeType.Parse(@event.Type))
                .WithGeometry(GeometryTranslator.Translate(@event.Geometry)));
        }

        private void Given(RoadNodeRemoved @event)
        {
            var id = new RoadNodeId(@event.Id);
            _nodes.Remove(id);
        }

        private void Given(RoadSegmentAdded @event)
        {
            var id = new RoadSegmentId(@event.Id);
            var start = new RoadNodeId(@event.StartNodeId);
            var end = new RoadNodeId(@event.EndNodeId);

            var attributeHash = new AttributeHash(
                RoadSegmentAccessRestriction.Parse(@event.AccessRestriction),
                RoadSegmentCategory.Parse(@event.Category),
                RoadSegmentMorphology.Parse(@event.Morphology),
                RoadSegmentStatus.Parse(@event.Status),
                @event.LeftSide.StreetNameId.HasValue
                    ? new CrabStreetnameId(@event.LeftSide.StreetNameId.Value)
                    : new CrabStreetnameId?(),
                @event.RightSide.StreetNameId.HasValue
                    ? new CrabStreetnameId(@event.RightSide.StreetNameId.Value)
                    : new CrabStreetnameId?(),
                new OrganizationId(@event.MaintenanceAuthority.Code));

            var segment = new RoadSegment(
                id,
                GeometryTranslator.Translate(@event.Geometry),
                start,
                end,
                attributeHash);

            _nodes
                .TryReplace(start, node => node.ConnectWith(id))
                .TryReplace(end, node => node.ConnectWith(id));
            _segments.Add(id, segment);
            _maximumSegmentId = RoadSegmentId.Max(id, _maximumSegmentId);
            _maximumLaneAttributeId =
                @event.Lanes.Length != 0
                    ? AttributeId.Max(
                        new AttributeId(@event.Lanes.Max(_ => _.AttributeId)),
                        _maximumLaneAttributeId)
                    : _maximumLaneAttributeId;
            _maximumWidthAttributeId = @event.Widths.Length != 0
                ? AttributeId.Max(
                    new AttributeId(@event.Widths.Max(_ => _.AttributeId)),
                    _maximumWidthAttributeId)
                : _maximumWidthAttributeId;
            _maximumSurfaceAttributeId = @event.Surfaces.Length != 0
                ? AttributeId.Max(
                    new AttributeId(@event.Surfaces.Max(_ => _.AttributeId)),
                    _maximumSurfaceAttributeId)
                : _maximumSurfaceAttributeId;
            _segmentReusableLaneAttributeIdentifiers.Merge(id,
                @event.Lanes.Select(lane => new AttributeId(lane.AttributeId)));
            _segmentReusableWidthAttributeIdentifiers.Merge(id,
                @event.Widths.Select(width => new AttributeId(width.AttributeId)));
            _segmentReusableSurfaceAttributeIdentifiers.Merge(id,
                @event.Surfaces.Select(surface => new AttributeId(surface.AttributeId)));
        }

        private void Given(RoadSegmentModified @event)
        {
            var id = new RoadSegmentId(@event.Id);
            var start = new RoadNodeId(@event.StartNodeId);
            var end = new RoadNodeId(@event.EndNodeId);

            var attributeHash = new AttributeHash(
                RoadSegmentAccessRestriction.Parse(@event.AccessRestriction),
                RoadSegmentCategory.Parse(@event.Category),
                RoadSegmentMorphology.Parse(@event.Morphology),
                RoadSegmentStatus.Parse(@event.Status),
                @event.LeftSide.StreetNameId.HasValue
                    ? new CrabStreetnameId(@event.LeftSide.StreetNameId.Value)
                    : new CrabStreetnameId?(),
                @event.RightSide.StreetNameId.HasValue
                    ? new CrabStreetnameId(@event.RightSide.StreetNameId.Value)
                    : new CrabStreetnameId?(),
                new OrganizationId(@event.MaintenanceAuthority.Code));

            _nodes
                .TryReplace(start, node => node.ConnectWith(id))
                .TryReplace(end, node => node.ConnectWith(id));
            _segments.TryReplace(id, segment =>
                segment
                    .WithGeometry(GeometryTranslator.Translate(@event.Geometry))
                    .WithStart(start)
                    .WithEnd(end)
                    .WithAttributeHash(attributeHash));
            _maximumLaneAttributeId =
                @event.Lanes.Length != 0
                    ? AttributeId.Max(
                        new AttributeId(@event.Lanes.Max(_ => _.AttributeId)),
                        _maximumLaneAttributeId)
                    : _maximumLaneAttributeId;
            _maximumWidthAttributeId = @event.Widths.Length != 0
                ? AttributeId.Max(
                    new AttributeId(@event.Widths.Max(_ => _.AttributeId)),
                    _maximumWidthAttributeId)
                : _maximumWidthAttributeId;
            _maximumSurfaceAttributeId = @event.Surfaces.Length != 0
                ? AttributeId.Max(
                    new AttributeId(@event.Surfaces.Max(_ => _.AttributeId)),
                    _maximumSurfaceAttributeId)
                : _maximumSurfaceAttributeId;
            _segmentReusableLaneAttributeIdentifiers.Merge(id,
                @event.Lanes.Select(lane => new AttributeId(lane.AttributeId)));
            _segmentReusableWidthAttributeIdentifiers.Merge(id,
                @event.Widths.Select(width => new AttributeId(width.AttributeId)));
            _segmentReusableSurfaceAttributeIdentifiers.Merge(id,
                @event.Surfaces.Select(surface => new AttributeId(surface.AttributeId)));
        }

        private void Given(RoadSegmentRemoved @event)
        {
            var id = new RoadSegmentId(@event.Id);
            if (_segments.TryGetValue(id, out var segment))
            {
                _nodes.TryReplace(segment.Start, node => node.DisconnectFrom(id));
                _nodes.TryReplace(segment.End, node => node.DisconnectFrom(id));
            }

            _segments.Remove(id);
        }

        private void Given(GradeSeparatedJunctionAdded @event)
        {
            var id = new GradeSeparatedJunctionId(@event.Id);
            _maximumGradeSeparatedJunctionId = GradeSeparatedJunctionId.Max(id, _maximumGradeSeparatedJunctionId);
            _gradeSeparatedJunctions.Add(
                id,
                new GradeSeparatedJunction(
                    id,
                    GradeSeparatedJunctionType.Parse(@event.Type),
                    new RoadSegmentId(@event.UpperRoadSegmentId),
                    new RoadSegmentId(@event.LowerRoadSegmentId)
                )
            );
        }

        private void Given(GradeSeparatedJunctionModified @event)
        {
            var id = new GradeSeparatedJunctionId(@event.Id);
            _gradeSeparatedJunctions.TryReplace(
                id,
                gradeSeparatedJunction =>
                    gradeSeparatedJunction
                        .WithType(GradeSeparatedJunctionType.Parse(@event.Type))
                        .WithUpperSegment(new RoadSegmentId(@event.UpperRoadSegmentId))
                        .WithLowerSegment(new RoadSegmentId(@event.LowerRoadSegmentId))
            );
        }

        private void Given(GradeSeparatedJunctionRemoved @event)
        {
            var id = new GradeSeparatedJunctionId(@event.Id);
            _gradeSeparatedJunctions.Remove(id);
        }

        private void Given(RoadSegmentAddedToEuropeanRoad @event)
        {
            var attributeId = new AttributeId(@event.AttributeId);
            var roadSegmentId = new RoadSegmentId(@event.SegmentId);
            var europeanRoadNumber = EuropeanRoadNumber.Parse(@event.Number);

            _maximumEuropeanRoadAttributeId = AttributeId.Max(attributeId, _maximumEuropeanRoadAttributeId);
            _segments.TryReplace(roadSegmentId, segment => segment.PartOfEuropeanRoad(europeanRoadNumber));
        }

        private void Given(RoadSegmentRemovedFromEuropeanRoad @event)
        {
            var roadSegmentId = new RoadSegmentId(@event.SegmentId);
            var europeanRoadNumber = EuropeanRoadNumber.Parse(@event.Number);

            _segments.TryReplace(roadSegmentId, segment => segment.NotPartOfEuropeanRoad(europeanRoadNumber));
        }

        private void Given(RoadSegmentAddedToNationalRoad @event)
        {
            var attributeId = new AttributeId(@event.AttributeId);
            var roadSegmentId = new RoadSegmentId(@event.SegmentId);
            var nationalRoadNumber = NationalRoadNumber.Parse(@event.Number);

            _maximumNationalRoadAttributeId = AttributeId.Max(attributeId, _maximumNationalRoadAttributeId);
            _segments.TryReplace(roadSegmentId, segment => segment.PartOfNationalRoad(nationalRoadNumber));
        }

        private void Given(RoadSegmentRemovedFromNationalRoad @event)
        {
            var roadSegmentId = new RoadSegmentId(@event.SegmentId);
            var nationalRoadNumber = NationalRoadNumber.Parse(@event.Number);

            _segments.TryReplace(roadSegmentId, segment => segment.NotPartOfNationalRoad(nationalRoadNumber));
        }

        private void Given(RoadSegmentAddedToNumberedRoad @event)
        {
            var attributeId = new AttributeId(@event.AttributeId);
            var roadSegmentId = new RoadSegmentId(@event.SegmentId);
            var numberedRoadNumber = NumberedRoadNumber.Parse(@event.Number);

            _maximumNumberedRoadAttributeId = AttributeId.Max(attributeId, _maximumNumberedRoadAttributeId);
            _segments.TryReplace(roadSegmentId, segment => segment.PartOfNumberedRoad(numberedRoadNumber));
        }

        private void Given(RoadSegmentOnNumberedRoadModified @event)
        {
            // no action required
        }

        private void Given(RoadSegmentRemovedFromNumberedRoad @event)
        {
            var roadSegmentId = new RoadSegmentId(@event.SegmentId);
            var numberedRoadNumber = NumberedRoadNumber.Parse(@event.Number);

            _segments.TryReplace(roadSegmentId, segment => segment.NotPartOfNumberedRoad(numberedRoadNumber));
        }

        public IReadOnlyDictionary<GradeSeparatedJunctionId, GradeSeparatedJunction> GradeSeparatedJunctions => _gradeSeparatedJunctions.ToImmutable();
        public AttributeId MaximumEuropeanRoadAttributeId => _maximumEuropeanRoadAttributeId;
        public GradeSeparatedJunctionId MaximumGradeSeparatedJunctionId => _maximumGradeSeparatedJunctionId;
        public AttributeId MaximumLaneAttributeId => _maximumLaneAttributeId;
        public AttributeId MaximumNationalRoadAttributeId => _maximumNationalRoadAttributeId;
        public RoadNodeId MaximumNodeId => _maximumNodeId;
        public AttributeId MaximumNumberedRoadAttributeId => _maximumNumberedRoadAttributeId;
        public RoadSegmentId MaximumSegmentId => _maximumSegmentId;
        public AttributeId MaximumSurfaceAttributeId => _maximumSurfaceAttributeId;
        public TransactionId MaximumTransactionId => _maximumTransactionId;
        public AttributeId MaximumWidthAttributeId => _maximumWidthAttributeId;

        public IReadOnlyDictionary<RoadNodeId, RoadNode> Nodes => _nodes.ToImmutable();

        public IRoadNetworkView RestoreFromEvent(object @event)
        {
            if (@event == null) throw new ArgumentNullException(nameof(@event));

            switch (@event)
            {
                case ImportedRoadNode importedRoadNode:
                    Given(importedRoadNode);
                    break;
                case ImportedRoadSegment importedRoadSegment:
                    Given(importedRoadSegment);
                    break;
                case ImportedGradeSeparatedJunction importedGradeSeparatedJunction:
                    Given(importedGradeSeparatedJunction);
                    break;
                case RoadNetworkChangesAccepted roadNetworkChangesAccepted:
                    Given(roadNetworkChangesAccepted);
                    break;
            }

            return this;
        }

        public IRoadNetworkView RestoreFromEvents(IReadOnlyCollection<object> events)
        {
            if (events == null) throw new ArgumentNullException(nameof(events));

            foreach (var @event in events)
                switch (@event)
                {
                    case ImportedRoadNode importedRoadNode:
                        Given(importedRoadNode);
                        break;
                    case ImportedRoadSegment importedRoadSegment:
                        Given(importedRoadSegment);
                        break;
                    case ImportedGradeSeparatedJunction importedGradeSeparatedJunction:
                        Given(importedGradeSeparatedJunction);
                        break;
                    case RoadNetworkChangesAccepted roadNetworkChangesAccepted:
                        Given(roadNetworkChangesAccepted);
                        break;
                }

            return this;
        }

        public IRoadNetworkView RestoreFromSnapshot(RoadNetworkSnapshot snapshot)
        {
            if (snapshot == null) throw new ArgumentNullException(nameof(snapshot));

            return new Builder(
                snapshot.Nodes.ToImmutableDictionary(
                    node => new RoadNodeId(node.Id),
                    node =>
                    {
                        var roadNode = new RoadNode(new RoadNodeId(node.Id), RoadNodeType.Parse(node.Type),
                            GeometryTranslator.Translate(node.Geometry));

                        return node.Segments.Aggregate(roadNode, (current, segment) => current.ConnectWith(new RoadSegmentId(segment)));
                    }).ToBuilder(),
                snapshot.Segments.ToImmutableDictionary(
                    segment => new RoadSegmentId(segment.Id),
                    segment =>
                    {
                        var roadSegment = new RoadSegment(new RoadSegmentId(segment.Id),
                            GeometryTranslator.Translate(segment.Geometry), new RoadNodeId(segment.StartNodeId),
                            new RoadNodeId(segment.EndNodeId),
                            new AttributeHash(
                                RoadSegmentAccessRestriction.Parse(segment.AttributeHash.AccessRestriction),
                                RoadSegmentCategory.Parse(segment.AttributeHash.Category),
                                RoadSegmentMorphology.Parse(segment.AttributeHash.Morphology),
                                RoadSegmentStatus.Parse(segment.AttributeHash.Status),
                                segment.AttributeHash.LeftSideStreetNameId.HasValue
                                    ? new CrabStreetnameId(segment.AttributeHash.LeftSideStreetNameId.Value)
                                    : new CrabStreetnameId?(),
                                segment.AttributeHash.RightSideStreetNameId.HasValue
                                    ? new CrabStreetnameId(segment.AttributeHash.RightSideStreetNameId.Value)
                                    : new CrabStreetnameId?(),
                                new OrganizationId(segment.AttributeHash.OrganizationId)));
                        roadSegment = segment.PartOfEuropeanRoads.Aggregate(roadSegment, (current, number) => current.PartOfEuropeanRoad(EuropeanRoadNumber.Parse(number)));
                        roadSegment = segment.PartOfNationalRoads.Aggregate(roadSegment, (current, number) => current.PartOfNationalRoad(NationalRoadNumber.Parse(number)));
                        roadSegment = segment.PartOfNumberedRoads.Aggregate(roadSegment, (current, number) => current.PartOfNumberedRoad(NumberedRoadNumber.Parse(number)));
                        return roadSegment;
                    }).ToBuilder(),
                snapshot.GradeSeparatedJunctions.ToImmutableDictionary(gradeSeparatedJunction => new GradeSeparatedJunctionId(gradeSeparatedJunction.Id),
                    gradeSeparatedJunction => new GradeSeparatedJunction(
                        new GradeSeparatedJunctionId(gradeSeparatedJunction.Id),
                        GradeSeparatedJunctionType.Parse(gradeSeparatedJunction.Type),
                        new RoadSegmentId(gradeSeparatedJunction.UpperSegmentId),
                        new RoadSegmentId(gradeSeparatedJunction.LowerSegmentId))).ToBuilder(),
                new TransactionId(snapshot.MaximumTransactionId),
                new RoadNodeId(snapshot.MaximumNodeId),
                new RoadSegmentId(snapshot.MaximumSegmentId),
                new GradeSeparatedJunctionId(snapshot.MaximumGradeSeparatedJunctionId),
                new AttributeId(snapshot.MaximumEuropeanRoadAttributeId),
                new AttributeId(snapshot.MaximumNationalRoadAttributeId),
                new AttributeId(snapshot.MaximumNumberedRoadAttributeId),
                new AttributeId(snapshot.MaximumLaneAttributeId),
                new AttributeId(snapshot.MaximumWidthAttributeId),
                new AttributeId(snapshot.MaximumSurfaceAttributeId),
                snapshot.SegmentReusableLaneAttributeIdentifiers.ToImmutableDictionary(
                    segment => new RoadSegmentId(segment.SegmentId),
                    segment => (IReadOnlyList<AttributeId>)segment.ReusableAttributeIdentifiers
                        .Select(identifier => new AttributeId(identifier)).ToArray()).ToBuilder(),
                snapshot.SegmentReusableWidthAttributeIdentifiers.ToImmutableDictionary(
                    segment => new RoadSegmentId(segment.SegmentId),
                    segment => (IReadOnlyList<AttributeId>)segment.ReusableAttributeIdentifiers
                        .Select(identifier => new AttributeId(identifier)).ToArray()).ToBuilder(),
                snapshot.SegmentReusableSurfaceAttributeIdentifiers.ToImmutableDictionary(
                    segment => new RoadSegmentId(segment.SegmentId),
                    segment => (IReadOnlyList<AttributeId>)segment.ReusableAttributeIdentifiers
                        .Select(identifier => new AttributeId(identifier)).ToArray()).ToBuilder()
            );
        }

        public ImmutableDictionary<RoadSegmentId, IReadOnlyList<AttributeId>>
            SegmentReusableLaneAttributeIdentifiers =>
            _segmentReusableLaneAttributeIdentifiers.ToImmutable();

        public ImmutableDictionary<RoadSegmentId, IReadOnlyList<AttributeId>>
            SegmentReusableSurfaceAttributeIdentifiers =>
            _segmentReusableSurfaceAttributeIdentifiers.ToImmutable();

        public ImmutableDictionary<RoadSegmentId, IReadOnlyList<AttributeId>>
            SegmentReusableWidthAttributeIdentifiers =>
            _segmentReusableWidthAttributeIdentifiers.ToImmutable();

        public IReadOnlyDictionary<RoadSegmentId, RoadSegment> Segments => _segments.ToImmutable();

        public RoadNetworkSnapshot TakeSnapshot()
        {
            return new RoadNetworkSnapshot
            {
                Nodes = _nodes.Select(node => new RoadNetworkSnapshotNode
                {
                    Id = node.Value.Id.ToInt32(),
                    Segments = node.Value.Segments.Select(segment => segment.ToInt32()).ToArray(),
                    Type = node.Value.Type,
                    Geometry = GeometryTranslator.Translate(node.Value.Geometry)
                }).ToArray(),
                Segments = _segments.Select(segment => new RoadNetworkSnapshotSegment
                {
                    Id = segment.Value.Id.ToInt32(),
                    StartNodeId = segment.Value.Start.ToInt32(),
                    EndNodeId = segment.Value.End.ToInt32(),
                    Geometry = GeometryTranslator.Translate(segment.Value.Geometry),
                    AttributeHash = new RoadNetworkSnapshotSegmentAttributeHash
                    {
                        AccessRestriction = segment.Value.AttributeHash.AccessRestriction,
                        Category = segment.Value.AttributeHash.Category,
                        Morphology = segment.Value.AttributeHash.Morphology,
                        Status = segment.Value.AttributeHash.Status,
                        LeftSideStreetNameId = segment.Value.AttributeHash.LeftStreetNameId?.ToInt32(),
                        RightSideStreetNameId = segment.Value.AttributeHash.RightStreetNameId?.ToInt32(),
                        OrganizationId = segment.Value.AttributeHash.OrganizationId
                    },
                    PartOfEuropeanRoads = segment.Value.PartOfEuropeanRoads.Select(number => number.ToString()).ToArray(),
                    PartOfNationalRoads = segment.Value.PartOfNationalRoads.Select(number => number.ToString()).ToArray(),
                    PartOfNumberedRoads = segment.Value.PartOfNumberedRoads.Select(number => number.ToString()).ToArray()
                }).ToArray(),
                GradeSeparatedJunctions = _gradeSeparatedJunctions.Select(gradeSeparatedJunction => new RoadNetworkSnapshotGradeSeparatedJunction
                {
                    Id = gradeSeparatedJunction.Value.Id,
                    Type = gradeSeparatedJunction.Value.Type,
                    UpperSegmentId = gradeSeparatedJunction.Value.UpperSegment,
                    LowerSegmentId = gradeSeparatedJunction.Value.LowerSegment
                }).ToArray(),
                MaximumTransactionId = _maximumTransactionId.ToInt32(),
                MaximumNodeId = _maximumNodeId.ToInt32(),
                MaximumSegmentId = _maximumSegmentId.ToInt32(),
                MaximumGradeSeparatedJunctionId = _maximumGradeSeparatedJunctionId.ToInt32(),
                MaximumEuropeanRoadAttributeId = _maximumEuropeanRoadAttributeId.ToInt32(),
                MaximumNationalRoadAttributeId = _maximumNationalRoadAttributeId.ToInt32(),
                MaximumNumberedRoadAttributeId = _maximumNumberedRoadAttributeId.ToInt32(),
                MaximumLaneAttributeId = _maximumLaneAttributeId.ToInt32(),
                MaximumWidthAttributeId = _maximumWidthAttributeId.ToInt32(),
                MaximumSurfaceAttributeId = _maximumSurfaceAttributeId.ToInt32(),
                SegmentReusableLaneAttributeIdentifiers = _segmentReusableLaneAttributeIdentifiers.Select(segment =>
                    new RoadNetworkSnapshotSegmentReusableAttributeIdentifiers
                    {
                        SegmentId = segment.Key.ToInt32(),
                        ReusableAttributeIdentifiers = segment.Value.Select(lane => lane.ToInt32()).ToArray()
                    }).ToArray(),
                SegmentReusableWidthAttributeIdentifiers = _segmentReusableWidthAttributeIdentifiers.Select(
                    segment =>
                        new RoadNetworkSnapshotSegmentReusableAttributeIdentifiers
                        {
                            SegmentId = segment.Key.ToInt32(),
                            ReusableAttributeIdentifiers = segment.Value.Select(width => width.ToInt32()).ToArray()
                        }).ToArray(),
                SegmentReusableSurfaceAttributeIdentifiers = _segmentReusableSurfaceAttributeIdentifiers.Select(
                    segment =>
                        new RoadNetworkSnapshotSegmentReusableAttributeIdentifiers
                        {
                            SegmentId = segment.Key.ToInt32(),
                            ReusableAttributeIdentifiers =
                                segment.Value.Select(surface => surface.ToInt32()).ToArray()
                        }).ToArray()
            };
        }

        public IRoadNetworkView ToBuilder()
        {
            return this;
        }

        public IRoadNetworkView ToImmutable()
        {
            return new ImmutableRoadNetworkView(
                _nodes.ToImmutable(),
                _segments.ToImmutable(),
                _gradeSeparatedJunctions.ToImmutable(),
                _maximumTransactionId,
                _maximumNodeId,
                _maximumSegmentId,
                _maximumGradeSeparatedJunctionId,
                _maximumEuropeanRoadAttributeId,
                _maximumNationalRoadAttributeId,
                _maximumNumberedRoadAttributeId,
                _maximumLaneAttributeId,
                _maximumWidthAttributeId,
                _maximumSurfaceAttributeId,
                _segmentReusableLaneAttributeIdentifiers.ToImmutable(),
                _segmentReusableWidthAttributeIdentifiers.ToImmutable(),
                _segmentReusableSurfaceAttributeIdentifiers.ToImmutable());
        }

        public IRoadNetworkView With(IReadOnlyCollection<IRequestedChange> changes)
        {
            if (changes == null) throw new ArgumentNullException(nameof(changes));

            foreach (var change in changes)
                switch (change)
                {
                    case AddRoadNode addRoadNode:
                        With(addRoadNode);
                        break;
                    case ModifyRoadNode modifyRoadNode:
                        With(modifyRoadNode);
                        break;
                    case RemoveRoadNode removeRoadNode:
                        With(removeRoadNode);
                        break;
                    case AddRoadSegment addRoadSegment:
                        With(addRoadSegment);
                        break;
                    case ModifyRoadSegment modifyRoadSegment:
                        With(modifyRoadSegment);
                        break;
                    case RemoveRoadSegment removeRoadSegment:
                        With(removeRoadSegment);
                        break;
                    case AddRoadSegmentToEuropeanRoad addRoadSegmentToEuropeanRoad:
                        With(addRoadSegmentToEuropeanRoad);
                        break;
                    case RemoveRoadSegmentFromEuropeanRoad removeRoadSegmentFromEuropeanRoad:
                        With(removeRoadSegmentFromEuropeanRoad);
                        break;
                    case AddRoadSegmentToNationalRoad addRoadSegmentToNationalRoad:
                        With(addRoadSegmentToNationalRoad);
                        break;
                    case RemoveRoadSegmentFromNationalRoad removeRoadSegmentFromNationalRoad:
                        With(removeRoadSegmentFromNationalRoad);
                        break;
                    case AddRoadSegmentToNumberedRoad addRoadSegmentToNumberedRoad:
                        With(addRoadSegmentToNumberedRoad);
                        break;
                    case ModifyRoadSegmentOnNumberedRoad modifyRoadSegmentOnNumberedRoad:
                        With(modifyRoadSegmentOnNumberedRoad);
                        break;
                    case RemoveRoadSegmentFromNumberedRoad removeRoadSegmentFromNumberedRoad:
                        With(removeRoadSegmentFromNumberedRoad);
                        break;
                    case AddGradeSeparatedJunction addGradeSeparatedJunction:
                        With(addGradeSeparatedJunction);
                        break;
                    case ModifyGradeSeparatedJunction modifyGradeSeparatedJunction:
                        With(modifyGradeSeparatedJunction);
                        break;
                    case RemoveGradeSeparatedJunction removeGradeSeparatedJunction:
                        With(removeGradeSeparatedJunction);
                        break;
                }

            return this;
        }

        private void With(AddRoadNode command)
        {
            var node = new RoadNode(command.Id, command.Type, command.Geometry);
            _nodes.Add(command.Id, node);
            _maximumNodeId = RoadNodeId.Max(command.Id, _maximumNodeId);
        }

        private void With(ModifyRoadNode command)
        {
            _nodes[command.Id] = _nodes[command.Id].WithGeometry(command.Geometry).WithType(command.Type);
        }

        private void With(RemoveRoadNode command)
        {
            _nodes.Remove(command.Id);
        }

        private void With(AddRoadSegment command)
        {
            var attributeHash = new AttributeHash(
                command.AccessRestriction,
                command.Category,
                command.Morphology,
                command.Status,
                command.LeftSideStreetNameId,
                command.RightSideStreetNameId,
                command.MaintenanceAuthorityId);

            _nodes
                .TryReplace(command.StartNodeId, node => node.ConnectWith(command.Id))
                .TryReplace(command.EndNodeId, node => node.ConnectWith(command.Id));
            _segments.Add(command.Id,
                new RoadSegment(command.Id, command.Geometry, command.StartNodeId, command.EndNodeId,
                    attributeHash));
            _maximumSegmentId = RoadSegmentId.Max(command.Id, _maximumSegmentId);
            _segmentReusableLaneAttributeIdentifiers.Merge(command.Id,
                command.Lanes.Select(lane => new AttributeId(lane.Id)));
            _segmentReusableWidthAttributeIdentifiers.Merge(command.Id,
                command.Widths.Select(width => new AttributeId(width.Id)));
            _segmentReusableSurfaceAttributeIdentifiers.Merge(command.Id,
                command.Surfaces.Select(surface => new AttributeId(surface.Id)));
        }

        private void With(ModifyRoadSegment command)
        {
            var attributeHash = new AttributeHash(
                command.AccessRestriction,
                command.Category,
                command.Morphology,
                command.Status,
                command.LeftSideStreetNameId,
                command.RightSideStreetNameId,
                command.MaintenanceAuthorityId);

            var segmentBefore = _segments[command.Id];

            _nodes
                .TryReplaceIf(segmentBefore.Start, node => node.Id != command.StartNodeId, node => node.DisconnectFrom(command.Id))
                .TryReplaceIf(segmentBefore.End, node => node.Id != command.EndNodeId, node => node.DisconnectFrom(command.Id))
                .TryReplace(command.StartNodeId, node => node.ConnectWith(command.Id))
                .TryReplace(command.EndNodeId, node => node.ConnectWith(command.Id));
            _segments.TryReplace(command.Id, segment =>
                segment
                    .WithGeometry(command.Geometry)
                    .WithStart(command.StartNodeId)
                    .WithEnd(command.EndNodeId)
                    .WithAttributeHash(attributeHash));
            _maximumSegmentId = RoadSegmentId.Max(command.Id, _maximumSegmentId);
            _segmentReusableLaneAttributeIdentifiers.Merge(command.Id,
                command.Lanes.Select(lane => new AttributeId(lane.Id)));
            _segmentReusableWidthAttributeIdentifiers.Merge(command.Id,
                command.Widths.Select(width => new AttributeId(width.Id)));
            _segmentReusableSurfaceAttributeIdentifiers.Merge(command.Id,
                command.Surfaces.Select(surface => new AttributeId(surface.Id)));
        }

        private void With(RemoveRoadSegment command)
        {
            if (_segments.TryGetValue(command.Id, out var segment))
            {
                _nodes.TryReplace(segment.Start, node => node.DisconnectFrom(command.Id));
                _nodes.TryReplace(segment.End, node => node.DisconnectFrom(command.Id));
            }

            _segments.Remove(command.Id);
        }

        private void With(AddGradeSeparatedJunction command)
        {
            _maximumGradeSeparatedJunctionId = GradeSeparatedJunctionId.Max(command.Id, _maximumGradeSeparatedJunctionId);
            _gradeSeparatedJunctions.Add(
                command.Id,
                new GradeSeparatedJunction(command.Id, command.Type, command.UpperSegmentId, command.LowerSegmentId));
        }

        private void With(ModifyGradeSeparatedJunction command)
        {
            _gradeSeparatedJunctions.TryReplace(
                command.Id,
                gradeSeparatedJunction =>
                    gradeSeparatedJunction
                        .WithType(command.Type)
                        .WithUpperSegment(command.UpperSegmentId)
                        .WithLowerSegment(command.LowerSegmentId));
        }

        private void With(RemoveGradeSeparatedJunction command)
        {
            _gradeSeparatedJunctions.Remove(command.Id);
        }

        private void With(AddRoadSegmentToEuropeanRoad command)
        {
            _maximumEuropeanRoadAttributeId = AttributeId.Max(command.AttributeId, _maximumEuropeanRoadAttributeId);
        }

        private void With(RemoveRoadSegmentFromEuropeanRoad command)
        {
            // not supported/no action required
        }

        private void With(AddRoadSegmentToNationalRoad command)
        {
            _maximumNationalRoadAttributeId = AttributeId.Max(command.AttributeId, _maximumNationalRoadAttributeId);
        }

        private void With(RemoveRoadSegmentFromNationalRoad command)
        {
            // not supported/no action required
        }

        private void With(AddRoadSegmentToNumberedRoad command)
        {
            _maximumNumberedRoadAttributeId = AttributeId.Max(command.AttributeId, _maximumNumberedRoadAttributeId);
        }

        private void With(ModifyRoadSegmentOnNumberedRoad command)
        {
            // not supported/no action required
        }

        private void With(RemoveRoadSegmentFromNumberedRoad command)
        {
            // not supported/no action required
        }
    }

    public IScopedRoadNetworkView CreateScopedView(Envelope envelope)
    {
        if (envelope == null) throw new ArgumentNullException(nameof(envelope));

        // Any nodes that the envelope contains
        var nodes = Nodes
            .Where(pair => envelope.Contains(pair.Value.Geometry.Coordinate))
            .ToDictionary(pair => pair.Key, pair => pair.Value);
        // Any segments that intersect the envelope
        var segments = Segments
            .Where(pair => envelope.Intersects(pair.Value.Geometry.EnvelopeInternal))
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

    private ImmutableRoadNetworkView Given(ImportedRoadNode @event)
    {
        if (@event == null) throw new ArgumentNullException(nameof(@event));

        var id = new RoadNodeId(@event.Id);
        var type = RoadNodeType.Parse(@event.Type);
        var node = new RoadNode(id, type, GeometryTranslator.Translate(@event.Geometry));
        return new ImmutableRoadNetworkView(
            _nodes.Add(id, node),
            _segments,
            _gradeSeparatedJunctions,
            TransactionId.Max(new TransactionId(@event.Origin.TransactionId), _maximumTransactionId),
            RoadNodeId.Max(id, _maximumNodeId),
            _maximumSegmentId, _maximumGradeSeparatedJunctionId,
            _maximumEuropeanRoadAttributeId,
            _maximumNationalRoadAttributeId,
            _maximumNumberedRoadAttributeId,
            _maximumLaneAttributeId,
            _maximumWidthAttributeId,
            _maximumSurfaceAttributeId,
            SegmentReusableLaneAttributeIdentifiers,
            SegmentReusableWidthAttributeIdentifiers,
            SegmentReusableSurfaceAttributeIdentifiers);
    }

    private ImmutableRoadNetworkView Given(ImportedRoadSegment @event)
    {
        if (@event == null) throw new ArgumentNullException(nameof(@event));

        var id = new RoadSegmentId(@event.Id);
        var start = new RoadNodeId(@event.StartNodeId);
        var end = new RoadNodeId(@event.EndNodeId);

        var attributeHash = new AttributeHash(
            RoadSegmentAccessRestriction.Parse(@event.AccessRestriction),
            RoadSegmentCategory.Parse(@event.Category),
            RoadSegmentMorphology.Parse(@event.Morphology),
            RoadSegmentStatus.Parse(@event.Status),
            @event.LeftSide.StreetNameId.HasValue
                ? new CrabStreetnameId(@event.LeftSide.StreetNameId.Value)
                : new CrabStreetnameId?(),
            @event.RightSide.StreetNameId.HasValue
                ? new CrabStreetnameId(@event.RightSide.StreetNameId.Value)
                : new CrabStreetnameId?(),
            new OrganizationId(@event.MaintenanceAuthority.Code));

        var segment = new RoadSegment(
            id,
            GeometryTranslator.Translate(@event.Geometry),
            start,
            end,
            attributeHash);
        segment = @event.PartOfEuropeanRoads.Aggregate(segment, (current, europeanRoad) => current.PartOfEuropeanRoad(EuropeanRoadNumber.Parse(europeanRoad.Number)));
        segment = @event.PartOfNationalRoads.Aggregate(segment, (current, nationalRoad) => current.PartOfNationalRoad(NationalRoadNumber.Parse(nationalRoad.Number)));
        segment = @event.PartOfNumberedRoads.Aggregate(segment, (current, numberedRoad) => current.PartOfNumberedRoad(NumberedRoadNumber.Parse(numberedRoad.Number)));

        return new ImmutableRoadNetworkView(
            _nodes
                .TryReplace(start, node => node.ConnectWith(id))
                .TryReplace(end, node => node.ConnectWith(id)),
            _segments.Add(id, segment),
            _gradeSeparatedJunctions,
            TransactionId.Max(new TransactionId(@event.Origin.TransactionId), _maximumTransactionId),
            _maximumNodeId,
            RoadSegmentId.Max(id, _maximumSegmentId), _maximumGradeSeparatedJunctionId,
            @event.PartOfEuropeanRoads.Length != 0
                ? AttributeId.Max(
                    new AttributeId(@event.PartOfEuropeanRoads.Max(_ => _.AttributeId)),
                    _maximumEuropeanRoadAttributeId)
                : _maximumEuropeanRoadAttributeId,
            @event.PartOfNationalRoads.Length != 0
                ? AttributeId.Max(
                    new AttributeId(@event.PartOfNationalRoads.Max(_ => _.AttributeId)),
                    _maximumNationalRoadAttributeId)
                : _maximumNationalRoadAttributeId,
            @event.PartOfNumberedRoads.Length != 0
                ? AttributeId.Max(
                    new AttributeId(@event.PartOfNumberedRoads.Max(_ => _.AttributeId)),
                    _maximumNumberedRoadAttributeId)
                : _maximumNumberedRoadAttributeId,
            @event.Lanes.Length != 0
                ? AttributeId.Max(
                    new AttributeId(@event.Lanes.Max(_ => _.AttributeId)),
                    _maximumLaneAttributeId)
                : _maximumLaneAttributeId,
            @event.Widths.Length != 0
                ? AttributeId.Max(
                    new AttributeId(@event.Widths.Max(_ => _.AttributeId)),
                    _maximumWidthAttributeId)
                : _maximumWidthAttributeId,
            @event.Surfaces.Length != 0
                ? AttributeId.Max(
                    new AttributeId(@event.Surfaces.Max(_ => _.AttributeId)),
                    _maximumSurfaceAttributeId)
                : _maximumSurfaceAttributeId,
            SegmentReusableLaneAttributeIdentifiers.Merge(id,
                @event.Lanes.Select(lane => new AttributeId(lane.AttributeId))),
            SegmentReusableWidthAttributeIdentifiers.Merge(id,
                @event.Widths.Select(width => new AttributeId(width.AttributeId))),
            SegmentReusableSurfaceAttributeIdentifiers.Merge(id,
                @event.Surfaces.Select(surface => new AttributeId(surface.AttributeId)))
        );
    }

    private ImmutableRoadNetworkView Given(ImportedGradeSeparatedJunction @event)
    {
        if (@event == null) throw new ArgumentNullException(nameof(@event));

        var id = new GradeSeparatedJunctionId(@event.Id);
        return new ImmutableRoadNetworkView(
            _nodes,
            _segments,
            _gradeSeparatedJunctions.Add(
                id,
                new GradeSeparatedJunction(
                    id,
                    GradeSeparatedJunctionType.Parse(@event.Type),
                    new RoadSegmentId(@event.UpperRoadSegmentId),
                    new RoadSegmentId(@event.LowerRoadSegmentId)
                )
            ),
            TransactionId.Max(new TransactionId(@event.Origin.TransactionId), _maximumTransactionId),
            _maximumNodeId,
            _maximumSegmentId,
            GradeSeparatedJunctionId.Max(id, _maximumGradeSeparatedJunctionId),
            _maximumEuropeanRoadAttributeId,
            _maximumNationalRoadAttributeId,
            _maximumNumberedRoadAttributeId,
            _maximumLaneAttributeId,
            _maximumWidthAttributeId,
            _maximumSurfaceAttributeId,
            SegmentReusableLaneAttributeIdentifiers,
            SegmentReusableWidthAttributeIdentifiers,
            SegmentReusableSurfaceAttributeIdentifiers);
    }

    private ImmutableRoadNetworkView Given(RoadNetworkChangesAccepted @event)
    {
        if (@event == null) throw new ArgumentNullException(nameof(@event));

        var result = new ImmutableRoadNetworkView(
            _nodes,
            _segments,
            _gradeSeparatedJunctions,
            TransactionId.Max(new TransactionId(@event.TransactionId), _maximumTransactionId),
            _maximumNodeId,
            _maximumSegmentId,
            _maximumGradeSeparatedJunctionId,
            _maximumEuropeanRoadAttributeId,
            _maximumNationalRoadAttributeId,
            _maximumNumberedRoadAttributeId,
            _maximumLaneAttributeId,
            _maximumWidthAttributeId,
            _maximumSurfaceAttributeId,
            SegmentReusableLaneAttributeIdentifiers,
            SegmentReusableWidthAttributeIdentifiers,
            SegmentReusableSurfaceAttributeIdentifiers);
        foreach (var change in @event.Changes.Flatten())
            switch (change)
            {
                case RoadNodeAdded roadNodeAdded:
                    result = result.Given(roadNodeAdded);
                    break;
                case RoadNodeModified roadNodeModified:
                    result = result.Given(roadNodeModified);
                    break;
                case RoadNodeRemoved roadNodeRemoved:
                    result = result.Given(roadNodeRemoved);
                    break;
                case RoadSegmentAdded roadSegmentAdded:
                    result = result.Given(roadSegmentAdded);
                    break;
                case RoadSegmentModified roadSegmentModified:
                    result = result.Given(roadSegmentModified);
                    break;
                case RoadSegmentRemoved roadSegmentRemoved:
                    result = result.Given(roadSegmentRemoved);
                    break;
                case RoadSegmentAddedToEuropeanRoad roadSegmentAddedToEuropeanRoad:
                    result = result.Given(roadSegmentAddedToEuropeanRoad);
                    break;
                case RoadSegmentRemovedFromEuropeanRoad roadSegmentRemovedFromEuropeanRoad:
                    result = result.Given(roadSegmentRemovedFromEuropeanRoad);
                    break;
                case RoadSegmentAddedToNationalRoad roadSegmentAddedToNationalRoad:
                    result = result.Given(roadSegmentAddedToNationalRoad);
                    break;
                case RoadSegmentRemovedFromNationalRoad roadSegmentRemovedFromNationalRoad:
                    result = result.Given(roadSegmentRemovedFromNationalRoad);
                    break;
                case RoadSegmentAddedToNumberedRoad roadSegmentAddedToNumberedRoad:
                    result = result.Given(roadSegmentAddedToNumberedRoad);
                    break;
                case RoadSegmentOnNumberedRoadModified roadSegmentOnNumberedRoadModified:
                    result = result.Given(roadSegmentOnNumberedRoadModified);
                    break;
                case RoadSegmentRemovedFromNumberedRoad roadSegmentRemovedFromNumberedRoad:
                    result = result.Given(roadSegmentRemovedFromNumberedRoad);
                    break;
                case GradeSeparatedJunctionAdded gradeSeparatedJunctionAdded:
                    result = result.Given(gradeSeparatedJunctionAdded);
                    break;
                case GradeSeparatedJunctionModified gradeSeparatedJunctionModified:
                    result = result.Given(gradeSeparatedJunctionModified);
                    break;
                case GradeSeparatedJunctionRemoved gradeSeparatedJunctionRemoved:
                    result = result.Given(gradeSeparatedJunctionRemoved);
                    break;
            }

        return result;
    }

    private ImmutableRoadNetworkView Given(RoadNodeAdded @event)
    {
        var id = new RoadNodeId(@event.Id);
        var type = RoadNodeType.Parse(@event.Type);
        var node = new RoadNode(id, type, GeometryTranslator.Translate(@event.Geometry));
        return new ImmutableRoadNetworkView(
            _nodes.Add(id, node),
            _segments,
            _gradeSeparatedJunctions,
            _maximumTransactionId,
            RoadNodeId.Max(id, _maximumNodeId),
            _maximumSegmentId,
            _maximumGradeSeparatedJunctionId,
            _maximumEuropeanRoadAttributeId,
            _maximumNationalRoadAttributeId,
            _maximumNumberedRoadAttributeId,
            _maximumLaneAttributeId,
            _maximumWidthAttributeId,
            _maximumSurfaceAttributeId,
            SegmentReusableLaneAttributeIdentifiers,
            SegmentReusableWidthAttributeIdentifiers,
            SegmentReusableSurfaceAttributeIdentifiers);
    }

    private ImmutableRoadNetworkView Given(RoadNodeModified @event)
    {
        var id = new RoadNodeId(@event.Id);
        return new ImmutableRoadNetworkView(
            _nodes.TryReplace(id, node => node.WithGeometry(GeometryTranslator.Translate(@event.Geometry)).WithType(RoadNodeType.Parse(@event.Type))),
            _segments,
            _gradeSeparatedJunctions,
            _maximumTransactionId,
            _maximumNodeId,
            _maximumSegmentId,
            _maximumGradeSeparatedJunctionId,
            _maximumEuropeanRoadAttributeId,
            _maximumNationalRoadAttributeId,
            _maximumNumberedRoadAttributeId,
            _maximumLaneAttributeId,
            _maximumWidthAttributeId,
            _maximumSurfaceAttributeId,
            SegmentReusableLaneAttributeIdentifiers,
            SegmentReusableWidthAttributeIdentifiers,
            SegmentReusableSurfaceAttributeIdentifiers);
    }

    private ImmutableRoadNetworkView Given(RoadNodeRemoved @event)
    {
        var id = new RoadNodeId(@event.Id);
        return new ImmutableRoadNetworkView(
            _nodes.Remove(id),
            _segments,
            _gradeSeparatedJunctions,
            _maximumTransactionId,
            _maximumNodeId,
            _maximumSegmentId,
            _maximumGradeSeparatedJunctionId,
            _maximumEuropeanRoadAttributeId,
            _maximumNationalRoadAttributeId,
            _maximumNumberedRoadAttributeId,
            _maximumLaneAttributeId,
            _maximumWidthAttributeId,
            _maximumSurfaceAttributeId,
            SegmentReusableLaneAttributeIdentifiers,
            SegmentReusableWidthAttributeIdentifiers,
            SegmentReusableSurfaceAttributeIdentifiers);
    }

    private ImmutableRoadNetworkView Given(RoadSegmentAdded @event)
    {
        var id = new RoadSegmentId(@event.Id);
        var start = new RoadNodeId(@event.StartNodeId);
        var end = new RoadNodeId(@event.EndNodeId);

        var attributeHash = new AttributeHash(
            RoadSegmentAccessRestriction.Parse(@event.AccessRestriction),
            RoadSegmentCategory.Parse(@event.Category),
            RoadSegmentMorphology.Parse(@event.Morphology),
            RoadSegmentStatus.Parse(@event.Status),
            @event.LeftSide.StreetNameId.HasValue
                ? new CrabStreetnameId(@event.LeftSide.StreetNameId.Value)
                : new CrabStreetnameId?(),
            @event.RightSide.StreetNameId.HasValue
                ? new CrabStreetnameId(@event.RightSide.StreetNameId.Value)
                : new CrabStreetnameId?(),
            new OrganizationId(@event.MaintenanceAuthority.Code));

        var segment = new RoadSegment(
            id,
            GeometryTranslator.Translate(@event.Geometry),
            start,
            end,
            attributeHash);

        return new ImmutableRoadNetworkView(
            _nodes
                .TryReplace(start, node => node.ConnectWith(id))
                .TryReplace(end, node => node.ConnectWith(id)),
            _segments.Add(id, segment),
            _gradeSeparatedJunctions,
            _maximumTransactionId,
            _maximumNodeId,
            RoadSegmentId.Max(id, _maximumSegmentId),
            _maximumGradeSeparatedJunctionId,
            _maximumEuropeanRoadAttributeId,
            _maximumNationalRoadAttributeId,
            _maximumNumberedRoadAttributeId,
            @event.Lanes.Length != 0
                ? AttributeId.Max(
                    new AttributeId(@event.Lanes.Max(_ => _.AttributeId)),
                    _maximumLaneAttributeId)
                : _maximumLaneAttributeId,
            @event.Widths.Length != 0
                ? AttributeId.Max(
                    new AttributeId(@event.Widths.Max(_ => _.AttributeId)),
                    _maximumWidthAttributeId)
                : _maximumWidthAttributeId,
            @event.Surfaces.Length != 0
                ? AttributeId.Max(
                    new AttributeId(@event.Surfaces.Max(_ => _.AttributeId)),
                    _maximumSurfaceAttributeId)
                : _maximumSurfaceAttributeId,
            SegmentReusableLaneAttributeIdentifiers.Merge(id,
                @event.Lanes.Select(lane => new AttributeId(lane.AttributeId))),
            SegmentReusableWidthAttributeIdentifiers.Merge(id,
                @event.Widths.Select(width => new AttributeId(width.AttributeId))),
            SegmentReusableSurfaceAttributeIdentifiers.Merge(id,
                @event.Surfaces.Select(surface => new AttributeId(surface.AttributeId)))
        );
    }

    private ImmutableRoadNetworkView Given(RoadSegmentModified @event)
    {
        var id = new RoadSegmentId(@event.Id);
        var start = new RoadNodeId(@event.StartNodeId);
        var end = new RoadNodeId(@event.EndNodeId);

        var attributeHash = new AttributeHash(
            RoadSegmentAccessRestriction.Parse(@event.AccessRestriction),
            RoadSegmentCategory.Parse(@event.Category),
            RoadSegmentMorphology.Parse(@event.Morphology),
            RoadSegmentStatus.Parse(@event.Status),
            @event.LeftSide.StreetNameId.HasValue
                ? new CrabStreetnameId(@event.LeftSide.StreetNameId.Value)
                : new CrabStreetnameId?(),
            @event.RightSide.StreetNameId.HasValue
                ? new CrabStreetnameId(@event.RightSide.StreetNameId.Value)
                : new CrabStreetnameId?(),
            new OrganizationId(@event.MaintenanceAuthority.Code));

        return new ImmutableRoadNetworkView(
            _nodes
                .TryReplace(start, node => node.ConnectWith(id))
                .TryReplace(end, node => node.ConnectWith(id)),
            _segments
                .TryReplace(id, segment => segment
                    .WithGeometry(GeometryTranslator.Translate(@event.Geometry))
                    .WithStart(start)
                    .WithEnd(end)
                    .WithAttributeHash(attributeHash)
                ),
            _gradeSeparatedJunctions,
            _maximumTransactionId,
            _maximumNodeId,
            RoadSegmentId.Max(id, _maximumSegmentId),
            _maximumGradeSeparatedJunctionId,
            _maximumEuropeanRoadAttributeId,
            _maximumNationalRoadAttributeId,
            _maximumNumberedRoadAttributeId,
            @event.Lanes.Length != 0
                ? AttributeId.Max(
                    new AttributeId(@event.Lanes.Max(_ => _.AttributeId)),
                    _maximumLaneAttributeId)
                : _maximumLaneAttributeId,
            @event.Widths.Length != 0
                ? AttributeId.Max(
                    new AttributeId(@event.Widths.Max(_ => _.AttributeId)),
                    _maximumWidthAttributeId)
                : _maximumWidthAttributeId,
            @event.Surfaces.Length != 0
                ? AttributeId.Max(
                    new AttributeId(@event.Surfaces.Max(_ => _.AttributeId)),
                    _maximumSurfaceAttributeId)
                : _maximumSurfaceAttributeId,
            SegmentReusableLaneAttributeIdentifiers.Merge(id,
                @event.Lanes.Select(lane => new AttributeId(lane.AttributeId))),
            SegmentReusableWidthAttributeIdentifiers.Merge(id,
                @event.Widths.Select(width => new AttributeId(width.AttributeId))),
            SegmentReusableSurfaceAttributeIdentifiers.Merge(id,
                @event.Surfaces.Select(surface => new AttributeId(surface.AttributeId)))
        );
    }

    private ImmutableRoadNetworkView Given(RoadSegmentRemoved @event)
    {
        var id = new RoadSegmentId(@event.Id);
        return new ImmutableRoadNetworkView(
            _segments.TryGetValue(id, out var segment)
                ? _nodes
                    .TryReplace(segment.Start, node => node.DisconnectFrom(id))
                    .TryReplace(segment.End, node => node.DisconnectFrom(id))
                : _nodes,
            _segments.Remove(id),
            _gradeSeparatedJunctions,
            _maximumTransactionId,
            _maximumNodeId,
            _maximumSegmentId,
            _maximumGradeSeparatedJunctionId,
            _maximumEuropeanRoadAttributeId,
            _maximumNationalRoadAttributeId,
            _maximumNumberedRoadAttributeId,
            _maximumLaneAttributeId,
            _maximumWidthAttributeId,
            _maximumSurfaceAttributeId,
            SegmentReusableLaneAttributeIdentifiers,
            SegmentReusableWidthAttributeIdentifiers,
            SegmentReusableSurfaceAttributeIdentifiers
        );
    }

    private ImmutableRoadNetworkView Given(GradeSeparatedJunctionAdded @event)
    {
        var id = new GradeSeparatedJunctionId(@event.Id);
        return new ImmutableRoadNetworkView(
            _nodes,
            _segments,
            _gradeSeparatedJunctions.Add(
                id,
                new GradeSeparatedJunction(
                    id,
                    GradeSeparatedJunctionType.Parse(@event.Type),
                    new RoadSegmentId(@event.UpperRoadSegmentId),
                    new RoadSegmentId(@event.LowerRoadSegmentId)
                )
            ),
            _maximumTransactionId,
            _maximumNodeId,
            _maximumSegmentId,
            GradeSeparatedJunctionId.Max(id, _maximumGradeSeparatedJunctionId),
            _maximumEuropeanRoadAttributeId,
            _maximumNationalRoadAttributeId,
            _maximumNumberedRoadAttributeId,
            _maximumLaneAttributeId,
            _maximumWidthAttributeId,
            _maximumSurfaceAttributeId,
            SegmentReusableLaneAttributeIdentifiers,
            SegmentReusableWidthAttributeIdentifiers,
            SegmentReusableSurfaceAttributeIdentifiers);
    }

    private ImmutableRoadNetworkView Given(GradeSeparatedJunctionModified @event)
    {
        var id = new GradeSeparatedJunctionId(@event.Id);
        return new ImmutableRoadNetworkView(
            _nodes,
            _segments,
            _gradeSeparatedJunctions.TryReplace(
                id,
                gradeSeparatedJunction =>
                    gradeSeparatedJunction
                        .WithType(GradeSeparatedJunctionType.Parse(@event.Type))
                        .WithUpperSegment(new RoadSegmentId(@event.UpperRoadSegmentId))
                        .WithLowerSegment(new RoadSegmentId(@event.LowerRoadSegmentId))
            ),
            _maximumTransactionId,
            _maximumNodeId,
            _maximumSegmentId,
            GradeSeparatedJunctionId.Max(id, _maximumGradeSeparatedJunctionId),
            _maximumEuropeanRoadAttributeId,
            _maximumNationalRoadAttributeId,
            _maximumNumberedRoadAttributeId,
            _maximumLaneAttributeId,
            _maximumWidthAttributeId,
            _maximumSurfaceAttributeId,
            SegmentReusableLaneAttributeIdentifiers,
            SegmentReusableWidthAttributeIdentifiers,
            SegmentReusableSurfaceAttributeIdentifiers);
    }

    private ImmutableRoadNetworkView Given(GradeSeparatedJunctionRemoved @event)
    {
        var id = new GradeSeparatedJunctionId(@event.Id);
        return new ImmutableRoadNetworkView(
            _nodes,
            _segments,
            _gradeSeparatedJunctions.Remove(id),
            _maximumTransactionId,
            _maximumNodeId,
            _maximumSegmentId,
            _maximumGradeSeparatedJunctionId,
            _maximumEuropeanRoadAttributeId,
            _maximumNationalRoadAttributeId,
            _maximumNumberedRoadAttributeId,
            _maximumLaneAttributeId,
            _maximumWidthAttributeId,
            _maximumSurfaceAttributeId,
            SegmentReusableLaneAttributeIdentifiers,
            SegmentReusableWidthAttributeIdentifiers,
            SegmentReusableSurfaceAttributeIdentifiers);
    }

    private ImmutableRoadNetworkView Given(RoadSegmentAddedToEuropeanRoad @event)
    {
        return new ImmutableRoadNetworkView(
            _nodes,
            _segments.TryReplace(new RoadSegmentId(@event.SegmentId), segment => segment.PartOfEuropeanRoad(EuropeanRoadNumber.Parse(@event.Number))),
            _gradeSeparatedJunctions,
            _maximumTransactionId,
            _maximumNodeId,
            _maximumSegmentId,
            _maximumGradeSeparatedJunctionId,
            AttributeId.Max(new AttributeId(@event.AttributeId), _maximumEuropeanRoadAttributeId),
            _maximumNationalRoadAttributeId,
            _maximumNumberedRoadAttributeId,
            _maximumLaneAttributeId,
            _maximumWidthAttributeId,
            _maximumSurfaceAttributeId,
            SegmentReusableLaneAttributeIdentifiers,
            SegmentReusableWidthAttributeIdentifiers,
            SegmentReusableSurfaceAttributeIdentifiers);
    }

    private ImmutableRoadNetworkView Given(RoadSegmentRemovedFromEuropeanRoad @event)
    {
        return new ImmutableRoadNetworkView(
            _nodes,
            _segments.TryReplace(new RoadSegmentId(@event.SegmentId), segment => segment.NotPartOfEuropeanRoad(EuropeanRoadNumber.Parse(@event.Number))),
            _gradeSeparatedJunctions,
            _maximumTransactionId,
            _maximumNodeId,
            _maximumSegmentId,
            _maximumGradeSeparatedJunctionId,
            _maximumEuropeanRoadAttributeId,
            _maximumNationalRoadAttributeId,
            _maximumNumberedRoadAttributeId,
            _maximumLaneAttributeId,
            _maximumWidthAttributeId,
            _maximumSurfaceAttributeId,
            SegmentReusableLaneAttributeIdentifiers,
            SegmentReusableWidthAttributeIdentifiers,
            SegmentReusableSurfaceAttributeIdentifiers);
    }

    private ImmutableRoadNetworkView Given(RoadSegmentAddedToNationalRoad @event)
    {
        return new ImmutableRoadNetworkView(
            _nodes,
            _segments.TryReplace(new RoadSegmentId(@event.SegmentId), segment => segment.PartOfNationalRoad(NationalRoadNumber.Parse(@event.Number))),
            _gradeSeparatedJunctions,
            _maximumTransactionId,
            _maximumNodeId,
            _maximumSegmentId,
            _maximumGradeSeparatedJunctionId,
            _maximumEuropeanRoadAttributeId,
            AttributeId.Max(new AttributeId(@event.AttributeId), _maximumNationalRoadAttributeId),
            _maximumNumberedRoadAttributeId,
            _maximumLaneAttributeId,
            _maximumWidthAttributeId,
            _maximumSurfaceAttributeId,
            SegmentReusableLaneAttributeIdentifiers,
            SegmentReusableWidthAttributeIdentifiers,
            SegmentReusableSurfaceAttributeIdentifiers);
    }

    private ImmutableRoadNetworkView Given(RoadSegmentRemovedFromNationalRoad @event)
    {
        return new ImmutableRoadNetworkView(
            _nodes,
            _segments.TryReplace(new RoadSegmentId(@event.SegmentId), segment => segment.NotPartOfNationalRoad(NationalRoadNumber.Parse(@event.Number))),
            _gradeSeparatedJunctions,
            _maximumTransactionId,
            _maximumNodeId,
            _maximumSegmentId,
            _maximumGradeSeparatedJunctionId,
            _maximumEuropeanRoadAttributeId,
            _maximumNationalRoadAttributeId,
            _maximumNumberedRoadAttributeId,
            _maximumLaneAttributeId,
            _maximumWidthAttributeId,
            _maximumSurfaceAttributeId,
            SegmentReusableLaneAttributeIdentifiers,
            SegmentReusableWidthAttributeIdentifiers,
            SegmentReusableSurfaceAttributeIdentifiers);
    }

    private ImmutableRoadNetworkView Given(RoadSegmentAddedToNumberedRoad @event)
    {
        return new ImmutableRoadNetworkView(
            _nodes,
            _segments.TryReplace(new RoadSegmentId(@event.SegmentId), segment => segment.PartOfNumberedRoad(NumberedRoadNumber.Parse(@event.Number))),
            _gradeSeparatedJunctions,
            _maximumTransactionId,
            _maximumNodeId,
            _maximumSegmentId,
            _maximumGradeSeparatedJunctionId,
            _maximumEuropeanRoadAttributeId,
            _maximumNationalRoadAttributeId,
            AttributeId.Max(new AttributeId(@event.AttributeId), _maximumNumberedRoadAttributeId),
            _maximumLaneAttributeId,
            _maximumWidthAttributeId,
            _maximumSurfaceAttributeId,
            SegmentReusableLaneAttributeIdentifiers,
            SegmentReusableWidthAttributeIdentifiers,
            SegmentReusableSurfaceAttributeIdentifiers);
    }

    private ImmutableRoadNetworkView Given(RoadSegmentOnNumberedRoadModified @event)
    {
        return new ImmutableRoadNetworkView(
            _nodes,
            _segments,
            _gradeSeparatedJunctions,
            _maximumTransactionId,
            _maximumNodeId,
            _maximumSegmentId,
            _maximumGradeSeparatedJunctionId,
            _maximumEuropeanRoadAttributeId,
            _maximumNationalRoadAttributeId,
            _maximumNumberedRoadAttributeId,
            _maximumLaneAttributeId,
            _maximumWidthAttributeId,
            _maximumSurfaceAttributeId,
            SegmentReusableLaneAttributeIdentifiers,
            SegmentReusableWidthAttributeIdentifiers,
            SegmentReusableSurfaceAttributeIdentifiers);
    }

    private ImmutableRoadNetworkView Given(RoadSegmentRemovedFromNumberedRoad @event)
    {
        return new ImmutableRoadNetworkView(
            _nodes,
            _segments.TryReplace(new RoadSegmentId(@event.SegmentId), segment => segment.NotPartOfNumberedRoad(NumberedRoadNumber.Parse(@event.Number))),
            _gradeSeparatedJunctions,
            _maximumTransactionId,
            _maximumNodeId,
            _maximumSegmentId,
            _maximumGradeSeparatedJunctionId,
            _maximumEuropeanRoadAttributeId,
            _maximumNationalRoadAttributeId,
            _maximumNumberedRoadAttributeId,
            _maximumLaneAttributeId,
            _maximumWidthAttributeId,
            _maximumSurfaceAttributeId,
            SegmentReusableLaneAttributeIdentifiers,
            SegmentReusableWidthAttributeIdentifiers,
            SegmentReusableSurfaceAttributeIdentifiers);
    }

    public IReadOnlyDictionary<GradeSeparatedJunctionId, GradeSeparatedJunction> GradeSeparatedJunctions => _gradeSeparatedJunctions;
    public AttributeId MaximumEuropeanRoadAttributeId => _maximumEuropeanRoadAttributeId;
    public GradeSeparatedJunctionId MaximumGradeSeparatedJunctionId => _maximumGradeSeparatedJunctionId;
    public AttributeId MaximumLaneAttributeId => _maximumLaneAttributeId;
    public AttributeId MaximumNationalRoadAttributeId => _maximumNationalRoadAttributeId;
    public RoadNodeId MaximumNodeId => _maximumNodeId;
    public AttributeId MaximumNumberedRoadAttributeId => _maximumNumberedRoadAttributeId;
    public RoadSegmentId MaximumSegmentId => _maximumSegmentId;
    public AttributeId MaximumSurfaceAttributeId => _maximumSurfaceAttributeId;
    public TransactionId MaximumTransactionId => _maximumTransactionId;
    public AttributeId MaximumWidthAttributeId => _maximumWidthAttributeId;

    public IReadOnlyDictionary<RoadNodeId, RoadNode> Nodes => _nodes;

    public IRoadNetworkView RestoreFromEvent(object @event)
    {
        if (@event == null) throw new ArgumentNullException(nameof(@event));

        var result = this;
        switch (@event)
        {
            case ImportedRoadNode importedRoadNode:
                result = result.Given(importedRoadNode);
                break;
            case ImportedRoadSegment importedRoadSegment:
                result = result.Given(importedRoadSegment);
                break;
            case ImportedGradeSeparatedJunction importedGradeSeparatedJunction:
                result = result.Given(importedGradeSeparatedJunction);
                break;
            case RoadNetworkChangesAccepted roadNetworkChangesAccepted:
                result = result.Given(roadNetworkChangesAccepted);
                break;
        }

        return result;
    }

    public IRoadNetworkView RestoreFromEvents(IReadOnlyCollection<object> events)
    {
        if (events == null) throw new ArgumentNullException(nameof(events));

        var result = this;
        foreach (var @event in events)
            switch (@event)
            {
                case ImportedRoadNode importedRoadNode:
                    result = result.Given(importedRoadNode);
                    break;
                case ImportedRoadSegment importedRoadSegment:
                    result = result.Given(importedRoadSegment);
                    break;
                case ImportedGradeSeparatedJunction importedGradeSeparatedJunction:
                    result = result.Given(importedGradeSeparatedJunction);
                    break;
                case RoadNetworkChangesAccepted roadNetworkChangesAccepted:
                    result = result.Given(roadNetworkChangesAccepted);
                    break;
            }

        return result;
    }

    public IRoadNetworkView RestoreFromSnapshot(RoadNetworkSnapshot snapshot)
    {
        if (snapshot == null) throw new ArgumentNullException(nameof(snapshot));

        return new ImmutableRoadNetworkView(
            snapshot.Nodes.ToImmutableDictionary(node => new RoadNodeId(node.Id),
                node =>
                {
                    var roadNode = new RoadNode(new RoadNodeId(node.Id), RoadNodeType.Parse(node.Type),
                        GeometryTranslator.Translate(node.Geometry));

                    return node.Segments.Aggregate(roadNode, (current, segment) => current.ConnectWith(new RoadSegmentId(segment)));
                }),
            snapshot.Segments.ToImmutableDictionary(segment => new RoadSegmentId(segment.Id),
                segment =>
                {
                    var roadSegment = new RoadSegment(new RoadSegmentId(segment.Id),
                        GeometryTranslator.Translate(segment.Geometry), new RoadNodeId(segment.StartNodeId),
                        new RoadNodeId(segment.EndNodeId), new AttributeHash(
                            RoadSegmentAccessRestriction.Parse(segment.AttributeHash.AccessRestriction),
                            RoadSegmentCategory.Parse(segment.AttributeHash.Category),
                            RoadSegmentMorphology.Parse(segment.AttributeHash.Morphology),
                            RoadSegmentStatus.Parse(segment.AttributeHash.Status),
                            segment.AttributeHash.LeftSideStreetNameId.HasValue
                                ? new CrabStreetnameId(segment.AttributeHash.LeftSideStreetNameId.Value)
                                : new CrabStreetnameId?(),
                            segment.AttributeHash.RightSideStreetNameId.HasValue
                                ? new CrabStreetnameId(segment.AttributeHash.RightSideStreetNameId.Value)
                                : new CrabStreetnameId?(),
                            new OrganizationId(segment.AttributeHash.OrganizationId)));
                    roadSegment = segment.PartOfEuropeanRoads.Aggregate(roadSegment, (current, number) => current.PartOfEuropeanRoad(EuropeanRoadNumber.Parse(number)));
                    roadSegment = segment.PartOfNationalRoads.Aggregate(roadSegment, (current, number) => current.PartOfNationalRoad(NationalRoadNumber.Parse(number)));
                    roadSegment = segment.PartOfNumberedRoads.Aggregate(roadSegment, (current, number) => current.PartOfNumberedRoad(NumberedRoadNumber.Parse(number)));
                    return roadSegment;
                }),
            snapshot.GradeSeparatedJunctions.ToImmutableDictionary(gradeSeparatedJunction => new GradeSeparatedJunctionId(gradeSeparatedJunction.Id),
                gradeSeparatedJunction => new GradeSeparatedJunction(
                    new GradeSeparatedJunctionId(gradeSeparatedJunction.Id),
                    GradeSeparatedJunctionType.Parse(gradeSeparatedJunction.Type),
                    new RoadSegmentId(gradeSeparatedJunction.UpperSegmentId),
                    new RoadSegmentId(gradeSeparatedJunction.LowerSegmentId))),
            new TransactionId(snapshot.MaximumTransactionId),
            new RoadNodeId(snapshot.MaximumNodeId),
            new RoadSegmentId(snapshot.MaximumSegmentId),
            new GradeSeparatedJunctionId(snapshot.MaximumGradeSeparatedJunctionId),
            new AttributeId(snapshot.MaximumEuropeanRoadAttributeId),
            new AttributeId(snapshot.MaximumNationalRoadAttributeId),
            new AttributeId(snapshot.MaximumNumberedRoadAttributeId),
            new AttributeId(snapshot.MaximumLaneAttributeId),
            new AttributeId(snapshot.MaximumWidthAttributeId),
            new AttributeId(snapshot.MaximumSurfaceAttributeId),
            snapshot.SegmentReusableLaneAttributeIdentifiers.ToImmutableDictionary(
                segment => new RoadSegmentId(segment.SegmentId),
                segment => (IReadOnlyList<AttributeId>)segment.ReusableAttributeIdentifiers
                    .Select(identifier => new AttributeId(identifier)).ToArray()),
            snapshot.SegmentReusableWidthAttributeIdentifiers.ToImmutableDictionary(
                segment => new RoadSegmentId(segment.SegmentId),
                segment => (IReadOnlyList<AttributeId>)segment.ReusableAttributeIdentifiers
                    .Select(identifier => new AttributeId(identifier)).ToArray()),
            snapshot.SegmentReusableSurfaceAttributeIdentifiers.ToImmutableDictionary(
                segment => new RoadSegmentId(segment.SegmentId),
                segment => (IReadOnlyList<AttributeId>)segment.ReusableAttributeIdentifiers
                    .Select(identifier => new AttributeId(identifier)).ToArray())
        );
    }

    public ImmutableDictionary<RoadSegmentId, IReadOnlyList<AttributeId>> SegmentReusableLaneAttributeIdentifiers { get; }

    public ImmutableDictionary<RoadSegmentId, IReadOnlyList<AttributeId>> SegmentReusableSurfaceAttributeIdentifiers { get; }

    public ImmutableDictionary<RoadSegmentId, IReadOnlyList<AttributeId>> SegmentReusableWidthAttributeIdentifiers { get; }
    public IReadOnlyDictionary<RoadSegmentId, RoadSegment> Segments => _segments;

    public RoadNetworkSnapshot TakeSnapshot()
    {
        return new RoadNetworkSnapshot
        {
            Nodes = _nodes.Select(node => new RoadNetworkSnapshotNode
            {
                Id = node.Value.Id.ToInt32(),
                Segments = node.Value.Segments.Select(segment => segment.ToInt32()).ToArray(),
                Type = node.Value.Type,
                Geometry = GeometryTranslator.Translate(node.Value.Geometry)
            }).ToArray(),
            Segments = _segments.Select(segment => new RoadNetworkSnapshotSegment
            {
                Id = segment.Value.Id.ToInt32(),
                StartNodeId = segment.Value.Start.ToInt32(),
                EndNodeId = segment.Value.End.ToInt32(),
                Geometry = GeometryTranslator.Translate(segment.Value.Geometry),
                AttributeHash = new RoadNetworkSnapshotSegmentAttributeHash
                {
                    AccessRestriction = segment.Value.AttributeHash.AccessRestriction,
                    Category = segment.Value.AttributeHash.Category,
                    Morphology = segment.Value.AttributeHash.Morphology,
                    Status = segment.Value.AttributeHash.Status,
                    LeftSideStreetNameId = segment.Value.AttributeHash.LeftStreetNameId?.ToInt32(),
                    RightSideStreetNameId = segment.Value.AttributeHash.RightStreetNameId?.ToInt32(),
                    OrganizationId = segment.Value.AttributeHash.OrganizationId
                },
                PartOfEuropeanRoads = segment.Value.PartOfEuropeanRoads.Select(number => number.ToString()).ToArray(),
                PartOfNationalRoads = segment.Value.PartOfNationalRoads.Select(number => number.ToString()).ToArray(),
                PartOfNumberedRoads = segment.Value.PartOfNumberedRoads.Select(number => number.ToString()).ToArray()
            }).ToArray(),
            GradeSeparatedJunctions = _gradeSeparatedJunctions.Select(gradeSeparatedJunction => new RoadNetworkSnapshotGradeSeparatedJunction
            {
                Id = gradeSeparatedJunction.Value.Id,
                Type = gradeSeparatedJunction.Value.Type,
                UpperSegmentId = gradeSeparatedJunction.Value.UpperSegment,
                LowerSegmentId = gradeSeparatedJunction.Value.LowerSegment
            }).ToArray(),
            MaximumTransactionId = _maximumTransactionId.ToInt32(),
            MaximumNodeId = _maximumNodeId.ToInt32(),
            MaximumSegmentId = _maximumSegmentId.ToInt32(),
            MaximumGradeSeparatedJunctionId = _maximumGradeSeparatedJunctionId.ToInt32(),
            MaximumEuropeanRoadAttributeId = _maximumEuropeanRoadAttributeId.ToInt32(),
            MaximumNationalRoadAttributeId = _maximumNationalRoadAttributeId.ToInt32(),
            MaximumNumberedRoadAttributeId = _maximumNumberedRoadAttributeId.ToInt32(),
            MaximumLaneAttributeId = _maximumLaneAttributeId.ToInt32(),
            MaximumWidthAttributeId = _maximumWidthAttributeId.ToInt32(),
            MaximumSurfaceAttributeId = _maximumSurfaceAttributeId.ToInt32(),
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

    public IRoadNetworkView ToBuilder()
    {
        return new Builder(
            _nodes.ToBuilder(),
            _segments.ToBuilder(),
            _gradeSeparatedJunctions.ToBuilder(),
            _maximumTransactionId,
            _maximumNodeId,
            _maximumSegmentId,
            _maximumGradeSeparatedJunctionId,
            _maximumEuropeanRoadAttributeId,
            _maximumNationalRoadAttributeId,
            _maximumNumberedRoadAttributeId,
            _maximumLaneAttributeId,
            _maximumWidthAttributeId,
            _maximumSurfaceAttributeId,
            SegmentReusableLaneAttributeIdentifiers.ToBuilder(),
            SegmentReusableWidthAttributeIdentifiers.ToBuilder(),
            SegmentReusableSurfaceAttributeIdentifiers.ToBuilder());
    }

    public IRoadNetworkView ToImmutable()
    {
        return this;
    }

    public IRoadNetworkView With(IReadOnlyCollection<IRequestedChange> changes)
    {
        if (changes == null) throw new ArgumentNullException(nameof(changes));

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
                case RemoveRoadSegment removeRoadSegment:
                    result = result.With(removeRoadSegment);
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
                case ModifyRoadSegmentOnNumberedRoad modifyRoadSegmentOnNumberedRoad:
                    result = result.With(modifyRoadSegmentOnNumberedRoad);
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

    private ImmutableRoadNetworkView With(AddRoadNode command)
    {
        var node = new RoadNode(command.Id, command.Type, command.Geometry);
        return new ImmutableRoadNetworkView(
            _nodes.Add(command.Id, node),
            _segments,
            _gradeSeparatedJunctions,
            _maximumTransactionId,
            RoadNodeId.Max(command.Id, _maximumNodeId),
            _maximumSegmentId,
            _maximumGradeSeparatedJunctionId,
            _maximumEuropeanRoadAttributeId,
            _maximumNationalRoadAttributeId,
            _maximumNumberedRoadAttributeId,
            _maximumLaneAttributeId,
            _maximumWidthAttributeId,
            _maximumSurfaceAttributeId,
            SegmentReusableLaneAttributeIdentifiers,
            SegmentReusableWidthAttributeIdentifiers,
            SegmentReusableSurfaceAttributeIdentifiers);
    }

    private ImmutableRoadNetworkView With(ModifyRoadNode command)
    {
        return new ImmutableRoadNetworkView(
            _nodes.TryReplace(command.Id, node => node.WithGeometry(command.Geometry).WithType(command.Type)),
            _segments,
            _gradeSeparatedJunctions,
            _maximumTransactionId,
            _maximumNodeId,
            _maximumSegmentId,
            _maximumGradeSeparatedJunctionId,
            _maximumEuropeanRoadAttributeId,
            _maximumNationalRoadAttributeId,
            _maximumNumberedRoadAttributeId,
            _maximumLaneAttributeId,
            _maximumWidthAttributeId,
            _maximumSurfaceAttributeId,
            SegmentReusableLaneAttributeIdentifiers,
            SegmentReusableWidthAttributeIdentifiers,
            SegmentReusableSurfaceAttributeIdentifiers);
    }

    private ImmutableRoadNetworkView With(RemoveRoadNode command)
    {
        return new ImmutableRoadNetworkView(
            _nodes.Remove(command.Id),
            _segments,
            _gradeSeparatedJunctions,
            _maximumTransactionId,
            _maximumNodeId,
            _maximumSegmentId,
            _maximumGradeSeparatedJunctionId,
            _maximumEuropeanRoadAttributeId,
            _maximumNationalRoadAttributeId,
            _maximumNumberedRoadAttributeId,
            _maximumLaneAttributeId,
            _maximumWidthAttributeId,
            _maximumSurfaceAttributeId,
            SegmentReusableLaneAttributeIdentifiers,
            SegmentReusableWidthAttributeIdentifiers,
            SegmentReusableSurfaceAttributeIdentifiers);
    }

    private ImmutableRoadNetworkView With(AddRoadSegment command)
    {
        var attributeHash = new AttributeHash(
            command.AccessRestriction,
            command.Category,
            command.Morphology,
            command.Status,
            command.LeftSideStreetNameId,
            command.RightSideStreetNameId,
            command.MaintenanceAuthorityId);

        return new ImmutableRoadNetworkView(
            _nodes
                .TryReplace(command.StartNodeId, node => node.ConnectWith(command.Id))
                .TryReplace(command.EndNodeId, node => node.ConnectWith(command.Id)),
            _segments.Add(command.Id,
                new RoadSegment(command.Id, command.Geometry, command.StartNodeId, command.EndNodeId,
                    attributeHash)),
            _gradeSeparatedJunctions,
            _maximumTransactionId,
            _maximumNodeId,
            RoadSegmentId.Max(command.Id, _maximumSegmentId),
            _maximumGradeSeparatedJunctionId,
            _maximumEuropeanRoadAttributeId,
            _maximumNationalRoadAttributeId,
            _maximumNumberedRoadAttributeId,
            _maximumLaneAttributeId,
            _maximumWidthAttributeId,
            _maximumSurfaceAttributeId,
            SegmentReusableLaneAttributeIdentifiers.Merge(command.Id,
                command.Lanes.Select(lane => new AttributeId(lane.Id))),
            SegmentReusableWidthAttributeIdentifiers.Merge(command.Id,
                command.Widths.Select(width => new AttributeId(width.Id))),
            SegmentReusableSurfaceAttributeIdentifiers.Merge(command.Id,
                command.Surfaces.Select(surface => new AttributeId(surface.Id)))
        );
    }

    private ImmutableRoadNetworkView With(ModifyRoadSegment command)
    {
        var attributeHash = new AttributeHash(
            command.AccessRestriction,
            command.Category,
            command.Morphology,
            command.Status,
            command.LeftSideStreetNameId,
            command.RightSideStreetNameId,
            command.MaintenanceAuthorityId);

        var segmentBefore = _segments[command.Id];

        return new ImmutableRoadNetworkView(
            _nodes
                .TryReplaceIf(segmentBefore.Start, node => node.Id != command.StartNodeId, node => node.DisconnectFrom(command.Id))
                .TryReplaceIf(segmentBefore.End, node => node.Id != command.EndNodeId, node => node.DisconnectFrom(command.Id))
                .TryReplace(command.StartNodeId, node => node.ConnectWith(command.Id))
                .TryReplace(command.EndNodeId, node => node.ConnectWith(command.Id)),
            _segments.TryReplace(command.Id, segment => segment
                .WithGeometry(command.Geometry)
                .WithStart(command.StartNodeId)
                .WithEnd(command.EndNodeId)
                .WithAttributeHash(attributeHash)),
            _gradeSeparatedJunctions,
            _maximumTransactionId,
            _maximumNodeId,
            _maximumSegmentId,
            _maximumGradeSeparatedJunctionId,
            _maximumEuropeanRoadAttributeId,
            _maximumNationalRoadAttributeId,
            _maximumNumberedRoadAttributeId,
            _maximumLaneAttributeId,
            _maximumWidthAttributeId,
            _maximumSurfaceAttributeId,
            SegmentReusableLaneAttributeIdentifiers.Merge(command.Id,
                command.Lanes.Select(lane => new AttributeId(lane.Id))),
            SegmentReusableWidthAttributeIdentifiers.Merge(command.Id,
                command.Widths.Select(width => new AttributeId(width.Id))),
            SegmentReusableSurfaceAttributeIdentifiers.Merge(command.Id,
                command.Surfaces.Select(surface => new AttributeId(surface.Id)))
        );
    }

    private ImmutableRoadNetworkView With(RemoveRoadSegment command)
    {
        return new ImmutableRoadNetworkView(
            _segments.TryGetValue(command.Id, out var segment)
                ? _nodes
                    .TryReplace(segment.Start, node => node.DisconnectFrom(command.Id))
                    .TryReplace(segment.End, node => node.DisconnectFrom(command.Id))
                : _nodes,
            _segments.Remove(command.Id),
            _gradeSeparatedJunctions,
            _maximumTransactionId,
            _maximumNodeId,
            _maximumSegmentId,
            _maximumGradeSeparatedJunctionId,
            _maximumEuropeanRoadAttributeId,
            _maximumNationalRoadAttributeId,
            _maximumNumberedRoadAttributeId,
            _maximumLaneAttributeId,
            _maximumWidthAttributeId,
            _maximumSurfaceAttributeId,
            SegmentReusableLaneAttributeIdentifiers,
            SegmentReusableWidthAttributeIdentifiers,
            SegmentReusableSurfaceAttributeIdentifiers
        );
    }

    private ImmutableRoadNetworkView With(AddGradeSeparatedJunction command)
    {
        return new ImmutableRoadNetworkView(
            _nodes,
            _segments,
            _gradeSeparatedJunctions.Add(command.Id, new GradeSeparatedJunction(command.Id, command.Type, command.UpperSegmentId, command.LowerSegmentId)),
            _maximumTransactionId,
            _maximumNodeId,
            _maximumSegmentId,
            GradeSeparatedJunctionId.Max(command.Id, _maximumGradeSeparatedJunctionId),
            _maximumEuropeanRoadAttributeId,
            _maximumNationalRoadAttributeId,
            _maximumNumberedRoadAttributeId,
            _maximumLaneAttributeId,
            _maximumWidthAttributeId,
            _maximumSurfaceAttributeId,
            SegmentReusableLaneAttributeIdentifiers,
            SegmentReusableWidthAttributeIdentifiers,
            SegmentReusableSurfaceAttributeIdentifiers);
    }

    private ImmutableRoadNetworkView With(ModifyGradeSeparatedJunction command)
    {
        return new ImmutableRoadNetworkView(
            _nodes,
            _segments,
            _gradeSeparatedJunctions.TryReplace(
                command.Id,
                gradeSeparatedJunction =>
                    gradeSeparatedJunction
                        .WithType(command.Type)
                        .WithUpperSegment(command.UpperSegmentId)
                        .WithLowerSegment(command.LowerSegmentId)),
            _maximumTransactionId,
            _maximumNodeId,
            _maximumSegmentId,
            _maximumGradeSeparatedJunctionId,
            _maximumEuropeanRoadAttributeId,
            _maximumNationalRoadAttributeId,
            _maximumNumberedRoadAttributeId,
            _maximumLaneAttributeId,
            _maximumWidthAttributeId,
            _maximumSurfaceAttributeId,
            SegmentReusableLaneAttributeIdentifiers,
            SegmentReusableWidthAttributeIdentifiers,
            SegmentReusableSurfaceAttributeIdentifiers);
    }

    private ImmutableRoadNetworkView With(RemoveGradeSeparatedJunction command)
    {
        return new ImmutableRoadNetworkView(
            _nodes,
            _segments,
            _gradeSeparatedJunctions.Remove(command.Id),
            _maximumTransactionId,
            _maximumNodeId,
            _maximumSegmentId,
            _maximumGradeSeparatedJunctionId,
            _maximumEuropeanRoadAttributeId,
            _maximumNationalRoadAttributeId,
            _maximumNumberedRoadAttributeId,
            _maximumLaneAttributeId,
            _maximumWidthAttributeId,
            _maximumSurfaceAttributeId,
            SegmentReusableLaneAttributeIdentifiers,
            SegmentReusableWidthAttributeIdentifiers,
            SegmentReusableSurfaceAttributeIdentifiers);
    }

    private ImmutableRoadNetworkView With(AddRoadSegmentToEuropeanRoad command)
    {
        return new ImmutableRoadNetworkView(
            _nodes,
            _segments.TryReplace(command.SegmentId,
                segment => segment.PartOfEuropeanRoad(command.Number)),
            _gradeSeparatedJunctions,
            _maximumTransactionId,
            _maximumNodeId,
            _maximumSegmentId,
            _maximumGradeSeparatedJunctionId,
            AttributeId.Max(command.AttributeId, _maximumEuropeanRoadAttributeId),
            _maximumNationalRoadAttributeId,
            _maximumNumberedRoadAttributeId,
            _maximumLaneAttributeId,
            _maximumWidthAttributeId,
            _maximumSurfaceAttributeId,
            SegmentReusableLaneAttributeIdentifiers,
            SegmentReusableWidthAttributeIdentifiers,
            SegmentReusableSurfaceAttributeIdentifiers);
    }

    private ImmutableRoadNetworkView With(RemoveRoadSegmentFromEuropeanRoad command)
    {
        return new ImmutableRoadNetworkView(
            _nodes,
            _segments.TryReplace(command.SegmentId,
                segment => segment.NotPartOfEuropeanRoad(command.Number)),
            _gradeSeparatedJunctions,
            _maximumTransactionId,
            _maximumNodeId,
            _maximumSegmentId,
            _maximumGradeSeparatedJunctionId,
            _maximumEuropeanRoadAttributeId,
            _maximumNationalRoadAttributeId,
            _maximumNumberedRoadAttributeId,
            _maximumLaneAttributeId,
            _maximumWidthAttributeId,
            _maximumSurfaceAttributeId,
            SegmentReusableLaneAttributeIdentifiers,
            SegmentReusableWidthAttributeIdentifiers,
            SegmentReusableSurfaceAttributeIdentifiers);
    }

    private ImmutableRoadNetworkView With(AddRoadSegmentToNationalRoad command)
    {
        return new ImmutableRoadNetworkView(
            _nodes,
            _segments.TryReplace(command.SegmentId,
                segment => segment.PartOfNationalRoad(command.Number)),
            _gradeSeparatedJunctions,
            _maximumTransactionId,
            _maximumNodeId,
            _maximumSegmentId,
            _maximumGradeSeparatedJunctionId,
            _maximumEuropeanRoadAttributeId,
            AttributeId.Max(command.AttributeId, _maximumNationalRoadAttributeId),
            _maximumNumberedRoadAttributeId,
            _maximumLaneAttributeId,
            _maximumWidthAttributeId,
            _maximumSurfaceAttributeId,
            SegmentReusableLaneAttributeIdentifiers,
            SegmentReusableWidthAttributeIdentifiers,
            SegmentReusableSurfaceAttributeIdentifiers);
    }

    private ImmutableRoadNetworkView With(RemoveRoadSegmentFromNationalRoad command)
    {
        return new ImmutableRoadNetworkView(
            _nodes,
            _segments.TryReplace(command.SegmentId,
                segment => segment.NotPartOfNationalRoad(command.Number)),
            _gradeSeparatedJunctions,
            _maximumTransactionId,
            _maximumNodeId,
            _maximumSegmentId,
            _maximumGradeSeparatedJunctionId,
            _maximumEuropeanRoadAttributeId,
            _maximumNationalRoadAttributeId,
            _maximumNumberedRoadAttributeId,
            _maximumLaneAttributeId,
            _maximumWidthAttributeId,
            _maximumSurfaceAttributeId,
            SegmentReusableLaneAttributeIdentifiers,
            SegmentReusableWidthAttributeIdentifiers,
            SegmentReusableSurfaceAttributeIdentifiers);
    }

    private ImmutableRoadNetworkView With(AddRoadSegmentToNumberedRoad command)
    {
        return new ImmutableRoadNetworkView(
            _nodes,
            _segments.TryReplace(command.SegmentId,
                segment => segment.PartOfNumberedRoad(command.Number)),
            _gradeSeparatedJunctions,
            _maximumTransactionId,
            _maximumNodeId,
            _maximumSegmentId,
            _maximumGradeSeparatedJunctionId,
            _maximumEuropeanRoadAttributeId,
            _maximumNationalRoadAttributeId,
            AttributeId.Max(command.AttributeId, _maximumNumberedRoadAttributeId),
            _maximumLaneAttributeId,
            _maximumWidthAttributeId,
            _maximumSurfaceAttributeId,
            SegmentReusableLaneAttributeIdentifiers,
            SegmentReusableWidthAttributeIdentifiers,
            SegmentReusableSurfaceAttributeIdentifiers);
    }

    private ImmutableRoadNetworkView With(ModifyRoadSegmentOnNumberedRoad command)
    {
        return new ImmutableRoadNetworkView(
            _nodes,
            _segments,
            _gradeSeparatedJunctions,
            _maximumTransactionId,
            _maximumNodeId,
            _maximumSegmentId,
            _maximumGradeSeparatedJunctionId,
            _maximumEuropeanRoadAttributeId,
            _maximumNationalRoadAttributeId,
            _maximumNumberedRoadAttributeId,
            _maximumLaneAttributeId,
            _maximumWidthAttributeId,
            _maximumSurfaceAttributeId,
            SegmentReusableLaneAttributeIdentifiers,
            SegmentReusableWidthAttributeIdentifiers,
            SegmentReusableSurfaceAttributeIdentifiers);
    }

    private ImmutableRoadNetworkView With(RemoveRoadSegmentFromNumberedRoad command)
    {
        return new ImmutableRoadNetworkView(
            _nodes,
            _segments.TryReplace(command.SegmentId,
                segment => segment.NotPartOfNumberedRoad(command.Number)),
            _gradeSeparatedJunctions,
            _maximumTransactionId,
            _maximumNodeId,
            _maximumSegmentId,
            _maximumGradeSeparatedJunctionId,
            _maximumEuropeanRoadAttributeId,
            _maximumNationalRoadAttributeId,
            _maximumNumberedRoadAttributeId,
            _maximumLaneAttributeId,
            _maximumWidthAttributeId,
            _maximumSurfaceAttributeId,
            SegmentReusableLaneAttributeIdentifiers,
            SegmentReusableWidthAttributeIdentifiers,
            SegmentReusableSurfaceAttributeIdentifiers);
    }
}