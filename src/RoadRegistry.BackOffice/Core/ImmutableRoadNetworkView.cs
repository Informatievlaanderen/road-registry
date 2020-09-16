namespace RoadRegistry.BackOffice.Core
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;

    public class ImmutableRoadNetworkView : IRoadNetworkView
    {
        public static readonly ImmutableRoadNetworkView Empty = new ImmutableRoadNetworkView(
            ImmutableDictionary<RoadNodeId, RoadNode>.Empty,
            ImmutableDictionary<RoadSegmentId, RoadSegment>.Empty,
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

        private readonly ImmutableDictionary<RoadNodeId, RoadNode> _nodes;
        private readonly ImmutableDictionary<RoadSegmentId, RoadSegment> _segments;
        private readonly TransactionId _maximumTransactionId;
        private readonly RoadNodeId _maximumNodeId;
        private readonly RoadSegmentId _maximumSegmentId;
        private readonly GradeSeparatedJunctionId _maximumGradeSeparatedJunctionId;
        private readonly AttributeId _maximumEuropeanRoadAttributeId;
        private readonly AttributeId _maximumNationalRoadAttributeId;
        private readonly AttributeId _maximumNumberedRoadAttributeId;
        private readonly AttributeId _maximumLaneAttributeId;
        private readonly AttributeId _maximumWidthAttributeId;
        private readonly AttributeId _maximumSurfaceAttributeId;

        private readonly ImmutableDictionary<RoadSegmentId, IReadOnlyList<AttributeId>>
            _segmentReusableLaneAttributeIdentifiers;

        private readonly ImmutableDictionary<RoadSegmentId, IReadOnlyList<AttributeId>>
            _segmentReusableWidthAttributeIdentifiers;

        private readonly ImmutableDictionary<RoadSegmentId, IReadOnlyList<AttributeId>>
            _segmentReusableSurfaceAttributeIdentifiers;

        private ImmutableRoadNetworkView(
            ImmutableDictionary<RoadNodeId, RoadNode> nodes,
            ImmutableDictionary<RoadSegmentId, RoadSegment> segments,
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

        public IRoadNetworkView ToBuilder() => new Builder(
            _nodes.ToBuilder(),
            _segments.ToBuilder(),
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
            _segmentReusableLaneAttributeIdentifiers.ToBuilder(),
            _segmentReusableWidthAttributeIdentifiers.ToBuilder(),
            _segmentReusableSurfaceAttributeIdentifiers.ToBuilder());

        public IRoadNetworkView ToImmutable() => this;

        public IReadOnlyDictionary<RoadNodeId, RoadNode> Nodes => _nodes;
        public IReadOnlyDictionary<RoadSegmentId, RoadSegment> Segments => _segments;
        public TransactionId MaximumTransactionId => _maximumTransactionId;
        public RoadNodeId MaximumNodeId => _maximumNodeId;
        public RoadSegmentId MaximumSegmentId => _maximumSegmentId;
        public GradeSeparatedJunctionId MaximumGradeSeparatedJunctionId => _maximumGradeSeparatedJunctionId;
        public AttributeId MaximumEuropeanRoadAttributeId => _maximumEuropeanRoadAttributeId;
        public AttributeId MaximumNationalRoadAttributeId => _maximumNationalRoadAttributeId;
        public AttributeId MaximumNumberedRoadAttributeId => _maximumNumberedRoadAttributeId;
        public AttributeId MaximumLaneAttributeId => _maximumLaneAttributeId;
        public AttributeId MaximumWidthAttributeId => _maximumWidthAttributeId;
        public AttributeId MaximumSurfaceAttributeId => _maximumSurfaceAttributeId;

        public ImmutableDictionary<RoadSegmentId, IReadOnlyList<AttributeId>> SegmentReusableLaneAttributeIdentifiers => _segmentReusableLaneAttributeIdentifiers;

        public ImmutableDictionary<RoadSegmentId, IReadOnlyList<AttributeId>> SegmentReusableWidthAttributeIdentifiers => _segmentReusableWidthAttributeIdentifiers;

        public ImmutableDictionary<RoadSegmentId, IReadOnlyList<AttributeId>> SegmentReusableSurfaceAttributeIdentifiers => _segmentReusableSurfaceAttributeIdentifiers;

        public IRoadNetworkView RestoreFromEvents(IReadOnlyCollection<object> events)
        {
            if (events == null) throw new ArgumentNullException(nameof(events));
            var result = this;
            foreach (var @event in events)
            {
                switch (@event)
                {
                    case Messages.ImportedRoadNode importedRoadNode:
                        result = result.Given(importedRoadNode);
                        break;
                    case Messages.ImportedRoadSegment importedRoadSegment:
                        result = result.Given(importedRoadSegment);
                        break;
                    case Messages.ImportedGradeSeparatedJunction importedGradeSeparatedJunction:
                        result = result.Given(importedGradeSeparatedJunction);
                        break;
                    case Messages.RoadNetworkChangesAccepted roadNetworkChangesAccepted:
                        result = result.Given(roadNetworkChangesAccepted);
                        break;
                }
            }

            return result;
        }

        public IRoadNetworkView RestoreFromEvent(object @event)
        {
            if (@event == null) throw new ArgumentNullException(nameof(@event));

            var result = this;
            switch (@event)
            {
                case Messages.ImportedRoadNode importedRoadNode:
                    result = result.Given(importedRoadNode);
                    break;
                case Messages.ImportedRoadSegment importedRoadSegment:
                    result = result.Given(importedRoadSegment);
                    break;
                case Messages.ImportedGradeSeparatedJunction importedGradeSeparatedJunction:
                    result = result.Given(importedGradeSeparatedJunction);
                    break;
                case Messages.RoadNetworkChangesAccepted roadNetworkChangesAccepted:
                    result = result.Given(roadNetworkChangesAccepted);
                    break;
            }

            return result;
        }

        private ImmutableRoadNetworkView Given(Messages.ImportedRoadNode @event)
        {
            if (@event == null) throw new ArgumentNullException(nameof(@event));
            var id = new RoadNodeId(@event.Id);
            var node = new RoadNode(id, GeometryTranslator.Translate(@event.Geometry));
            return new ImmutableRoadNetworkView(
                _nodes.Add(id, node),
                _segments,
                TransactionId.Max(new TransactionId(@event.Origin.TransactionId), _maximumTransactionId),
                RoadNodeId.Max(id, _maximumNodeId),
                _maximumSegmentId, _maximumGradeSeparatedJunctionId,
                _maximumEuropeanRoadAttributeId,
                _maximumNationalRoadAttributeId,
                _maximumNumberedRoadAttributeId,
                _maximumLaneAttributeId,
                _maximumWidthAttributeId,
                _maximumSurfaceAttributeId,
                _segmentReusableLaneAttributeIdentifiers,
                _segmentReusableWidthAttributeIdentifiers,
                _segmentReusableSurfaceAttributeIdentifiers);
        }

        private ImmutableRoadNetworkView Given(Messages.ImportedRoadSegment @event)
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

            return new ImmutableRoadNetworkView(
                _nodes
                    .TryReplace(start, node => node.ConnectWith(id))
                    .TryReplace(end, node => node.ConnectWith(id)),
                _segments.Add(id, segment),
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
                _segmentReusableLaneAttributeIdentifiers.Merge(id,
                    @event.Lanes.Select(lane => new AttributeId(lane.AttributeId))),
                _segmentReusableWidthAttributeIdentifiers.Merge(id,
                    @event.Widths.Select(width => new AttributeId(width.AttributeId))),
                _segmentReusableSurfaceAttributeIdentifiers.Merge(id,
                    @event.Surfaces.Select(surface => new AttributeId(surface.AttributeId)))
            );
        }

        private ImmutableRoadNetworkView Given(Messages.ImportedGradeSeparatedJunction @event)
        {
            if (@event == null) throw new ArgumentNullException(nameof(@event));
            var id = new GradeSeparatedJunctionId(@event.Id);
            return new ImmutableRoadNetworkView(
                _nodes,
                _segments,
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
                _segmentReusableLaneAttributeIdentifiers,
                _segmentReusableWidthAttributeIdentifiers,
                _segmentReusableSurfaceAttributeIdentifiers);
        }

        private ImmutableRoadNetworkView Given(Messages.RoadNetworkChangesAccepted @event)
        {
            if (@event == null) throw new ArgumentNullException(nameof(@event));
            var result = new ImmutableRoadNetworkView(
                _nodes,
                _segments,
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
                _segmentReusableLaneAttributeIdentifiers,
                _segmentReusableWidthAttributeIdentifiers,
                _segmentReusableSurfaceAttributeIdentifiers);
            foreach (var change in Messages.ChangeExtensions.Flatten(@event.Changes))
            {
                switch (change)
                {
                    case Messages.RoadNodeAdded roadNodeAdded:
                        result = result.Given(roadNodeAdded);
                        break;
                    case Messages.RoadNodeModified roadNodeModified:
                        result = result.Given(roadNodeModified);
                        break;
                    case Messages.RoadSegmentAdded roadSegmentAdded:
                        result = result.Given(roadSegmentAdded);
                        break;
                    case Messages.RoadSegmentModified roadSegmentModified:
                        result = result.Given(roadSegmentModified);
                        break;
                    case Messages.RoadSegmentAddedToEuropeanRoad roadSegmentAddedToEuropeanRoad:
                        result = result.Given(roadSegmentAddedToEuropeanRoad);
                        break;
                    case Messages.RoadSegmentAddedToNationalRoad roadSegmentAddedToNationalRoad:
                        result = result.Given(roadSegmentAddedToNationalRoad);
                        break;
                    case Messages.RoadSegmentAddedToNumberedRoad roadSegmentAddedToNumberedRoad:
                        result = result.Given(roadSegmentAddedToNumberedRoad);
                        break;
                    case Messages.GradeSeparatedJunctionAdded gradeSeparatedJunctionAdded:
                        result = result.Given(gradeSeparatedJunctionAdded);
                        break;
                    case Messages.GradeSeparatedJunctionModified gradeSeparatedJunctionModified:
                        result = result.Given(gradeSeparatedJunctionModified);
                        break;
                }
            }

            return result;
        }

        private ImmutableRoadNetworkView Given(Messages.RoadNodeAdded @event)
        {
            var id = new RoadNodeId(@event.Id);
            var node = new RoadNode(id, GeometryTranslator.Translate(@event.Geometry));
            return new ImmutableRoadNetworkView(
                _nodes.Add(id, node),
                _segments,
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
                _segmentReusableLaneAttributeIdentifiers,
                _segmentReusableWidthAttributeIdentifiers,
                _segmentReusableSurfaceAttributeIdentifiers);
        }

        private ImmutableRoadNetworkView Given(Messages.RoadNodeModified @event)
        {
            var id = new RoadNodeId(@event.Id);
            return new ImmutableRoadNetworkView(
                _nodes.TryReplace(id, node => node.WithGeometry(GeometryTranslator.Translate(@event.Geometry))),
                _segments,
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
                _segmentReusableLaneAttributeIdentifiers,
                _segmentReusableWidthAttributeIdentifiers,
                _segmentReusableSurfaceAttributeIdentifiers);
        }

        private ImmutableRoadNetworkView Given(Messages.RoadSegmentAdded @event)
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
                _segmentReusableLaneAttributeIdentifiers.Merge(id,
                    @event.Lanes.Select(lane => new AttributeId(lane.AttributeId))),
                _segmentReusableWidthAttributeIdentifiers.Merge(id,
                    @event.Widths.Select(width => new AttributeId(width.AttributeId))),
                _segmentReusableSurfaceAttributeIdentifiers.Merge(id,
                    @event.Surfaces.Select(surface => new AttributeId(surface.AttributeId)))
            );
        }

        private ImmutableRoadNetworkView Given(Messages.RoadSegmentModified @event)
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
                _segmentReusableLaneAttributeIdentifiers.Merge(id,
                    @event.Lanes.Select(lane => new AttributeId(lane.AttributeId))),
                _segmentReusableWidthAttributeIdentifiers.Merge(id,
                    @event.Widths.Select(width => new AttributeId(width.AttributeId))),
                _segmentReusableSurfaceAttributeIdentifiers.Merge(id,
                    @event.Surfaces.Select(surface => new AttributeId(surface.AttributeId)))
            );
        }

        private ImmutableRoadNetworkView Given(Messages.GradeSeparatedJunctionAdded @event)
        {
            var id = new GradeSeparatedJunctionId(@event.Id);
            return new ImmutableRoadNetworkView(
                _nodes,
                _segments,
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
                _segmentReusableLaneAttributeIdentifiers,
                _segmentReusableWidthAttributeIdentifiers,
                _segmentReusableSurfaceAttributeIdentifiers);
        }

        private ImmutableRoadNetworkView Given(Messages.GradeSeparatedJunctionModified @event)
        {
            var id = new GradeSeparatedJunctionId(@event.Id);
            return new ImmutableRoadNetworkView(
                _nodes,
                _segments,
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
                _segmentReusableLaneAttributeIdentifiers,
                _segmentReusableWidthAttributeIdentifiers,
                _segmentReusableSurfaceAttributeIdentifiers);
        }

        private ImmutableRoadNetworkView Given(Messages.RoadSegmentAddedToEuropeanRoad @event)
        {
            return new ImmutableRoadNetworkView(
                _nodes,
                _segments,
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
                _segmentReusableLaneAttributeIdentifiers,
                _segmentReusableWidthAttributeIdentifiers,
                _segmentReusableSurfaceAttributeIdentifiers);
        }

        private ImmutableRoadNetworkView Given(Messages.RoadSegmentAddedToNationalRoad @event)
        {
            return new ImmutableRoadNetworkView(
                _nodes,
                _segments,
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
                _segmentReusableLaneAttributeIdentifiers,
                _segmentReusableWidthAttributeIdentifiers,
                _segmentReusableSurfaceAttributeIdentifiers);
        }

        private ImmutableRoadNetworkView Given(Messages.RoadSegmentAddedToNumberedRoad @event)
        {
            return new ImmutableRoadNetworkView(
                _nodes,
                _segments,
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
                _segmentReusableLaneAttributeIdentifiers,
                _segmentReusableWidthAttributeIdentifiers,
                _segmentReusableSurfaceAttributeIdentifiers);
        }

        public IRoadNetworkView With(IReadOnlyCollection<IRequestedChange> changes)
        {
            if (changes == null)
                throw new ArgumentNullException(nameof(changes));

            var result = this;
            foreach (var change in changes)
            {
                switch (change)
                {
                    case AddRoadNode addRoadNode:
                        result = result.With(addRoadNode);
                        break;
                    case ModifyRoadNode modifyRoadNode:
                        result = result.With(modifyRoadNode);
                        break;
                    case AddRoadSegment addRoadSegment:
                        result = result.With(addRoadSegment);
                        break;
                    case ModifyRoadSegment modifyRoadSegment:
                        result = result.With(modifyRoadSegment);
                        break;
                    case AddRoadSegmentToEuropeanRoad addRoadSegmentToEuropeanRoad:
                        result = result.With(addRoadSegmentToEuropeanRoad);
                        break;
                    case AddRoadSegmentToNationalRoad addRoadSegmentToNationalRoad:
                        result = result.With(addRoadSegmentToNationalRoad);
                        break;
                    case AddRoadSegmentToNumberedRoad addRoadSegmentToNumberedRoad:
                        result = result.With(addRoadSegmentToNumberedRoad);
                        break;
                    case AddGradeSeparatedJunction addGradeSeparatedJunction:
                        result = result.With(addGradeSeparatedJunction);
                        break;
                    case ModifyGradeSeparatedJunction modifyGradeSeparatedJunction:
                        result = result.With(modifyGradeSeparatedJunction);
                        break;
                }
            }

            return result;
        }

        private ImmutableRoadNetworkView With(AddRoadNode command)
        {
            var node = new RoadNode(command.Id, command.Geometry);
            return new ImmutableRoadNetworkView(
                _nodes.Add(command.Id, node),
                _segments,
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
                _segmentReusableLaneAttributeIdentifiers,
                _segmentReusableWidthAttributeIdentifiers,
                _segmentReusableSurfaceAttributeIdentifiers);
        }

        private ImmutableRoadNetworkView With(ModifyRoadNode command)
        {
            return new ImmutableRoadNetworkView(
                _nodes.TryReplace(command.Id, node => node.WithGeometry(command.Geometry)),
                _segments,
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
                _segmentReusableLaneAttributeIdentifiers,
                _segmentReusableWidthAttributeIdentifiers,
                _segmentReusableSurfaceAttributeIdentifiers);
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
                command.MaintenanceAuthority);

            return new ImmutableRoadNetworkView(
                _nodes
                    .TryReplace(command.StartNodeId, node => node.ConnectWith(command.Id))
                    .TryReplace(command.EndNodeId, node => node.ConnectWith(command.Id)),
                _segments.Add(command.Id,
                    new RoadSegment(command.Id, command.Geometry, command.StartNodeId, command.EndNodeId,
                        attributeHash)),
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
                _segmentReusableLaneAttributeIdentifiers.Merge(command.Id,
                    command.Lanes.Select(lane => new AttributeId(lane.Id))),
                _segmentReusableWidthAttributeIdentifiers.Merge(command.Id,
                    command.Widths.Select(width => new AttributeId(width.Id))),
                _segmentReusableSurfaceAttributeIdentifiers.Merge(command.Id,
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
                command.MaintenanceAuthority);

            return new ImmutableRoadNetworkView(
                //TODO: What if you used different start and end nodes
                //We will want to disconnect those first, no?
                _nodes
                    .TryReplace(command.StartNodeId, node => node.ConnectWith(command.Id))
                    .TryReplace(command.EndNodeId, node => node.ConnectWith(command.Id)),
                _segments.
                    TryReplace(command.Id, segment => segment
                        .WithGeometry(command.Geometry)
                        .WithStart(command.StartNodeId)
                        .WithEnd(command.EndNodeId)
                        .WithAttributeHash(attributeHash)),
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
                _segmentReusableLaneAttributeIdentifiers.Merge(command.Id,
                    command.Lanes.Select(lane => new AttributeId(lane.Id))),
                _segmentReusableWidthAttributeIdentifiers.Merge(command.Id,
                    command.Widths.Select(width => new AttributeId(width.Id))),
                _segmentReusableSurfaceAttributeIdentifiers.Merge(command.Id,
                    command.Surfaces.Select(surface => new AttributeId(surface.Id)))
            );
        }

        private ImmutableRoadNetworkView With(AddGradeSeparatedJunction command)
        {
            return new ImmutableRoadNetworkView(
                _nodes,
                _segments,
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
                _segmentReusableLaneAttributeIdentifiers,
                _segmentReusableWidthAttributeIdentifiers,
                _segmentReusableSurfaceAttributeIdentifiers);
        }

        private ImmutableRoadNetworkView With(ModifyGradeSeparatedJunction command)
        {
            return new ImmutableRoadNetworkView(
                _nodes,
                _segments,
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
                _segmentReusableLaneAttributeIdentifiers,
                _segmentReusableWidthAttributeIdentifiers,
                _segmentReusableSurfaceAttributeIdentifiers);
        }

        private ImmutableRoadNetworkView With(AddRoadSegmentToEuropeanRoad command)
        {
            return new ImmutableRoadNetworkView(
                _nodes,
                _segments,
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
                _segmentReusableLaneAttributeIdentifiers,
                _segmentReusableWidthAttributeIdentifiers,
                _segmentReusableSurfaceAttributeIdentifiers);
        }

        private ImmutableRoadNetworkView With(AddRoadSegmentToNationalRoad command)
        {
            return new ImmutableRoadNetworkView(
                _nodes,
                _segments,
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
                _segmentReusableLaneAttributeIdentifiers,
                _segmentReusableWidthAttributeIdentifiers,
                _segmentReusableSurfaceAttributeIdentifiers);
        }

        private ImmutableRoadNetworkView With(AddRoadSegmentToNumberedRoad command)
        {
            return new ImmutableRoadNetworkView(
                _nodes,
                _segments,
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
                _segmentReusableLaneAttributeIdentifiers,
                _segmentReusableWidthAttributeIdentifiers,
                _segmentReusableSurfaceAttributeIdentifiers);
        }

        public Messages.RoadNetworkSnapshot TakeSnapshot()
        {
            return new Messages.RoadNetworkSnapshot
            {
                Nodes = _nodes.Select(node => new Messages.RoadNetworkSnapshotNode
                {
                    Id = node.Value.Id.ToInt32(),
                    Segments = node.Value.Segments.Select(segment => segment.ToInt32()).ToArray(),
                    Geometry = GeometryTranslator.Translate(node.Value.Geometry)
                }).ToArray(),
                Segments = _segments.Select(segment => new Messages.RoadNetworkSnapshotSegment
                {
                    Id = segment.Value.Id.ToInt32(),
                    StartNodeId = segment.Value.Start.ToInt32(),
                    EndNodeId = segment.Value.End.ToInt32(),
                    Geometry = GeometryTranslator.Translate(segment.Value.Geometry),
                    AttributeHash = new Messages.RoadNetworkSnapshotSegmentAttributeHash
                    {
                        AccessRestriction = segment.Value.AttributeHash.AccessRestriction,
                        Category = segment.Value.AttributeHash.Category,
                        Morphology = segment.Value.AttributeHash.Morphology,
                        Status = segment.Value.AttributeHash.Status,
                        LeftStreetNameId = segment.Value.AttributeHash.LeftStreetNameId?.ToInt32(),
                        RightStreetNameId = segment.Value.AttributeHash.RightStreetNameId?.ToInt32(),
                        OrganizationId = segment.Value.AttributeHash.OrganizationId
                    }
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
                    new Messages.RoadNetworkSnapshotSegmentReusableAttributeIdentifiers
                    {
                        SegmentId = segment.Key.ToInt32(),
                        ReusableAttributeIdentifiers = segment.Value.Select(lane => lane.ToInt32()).ToArray()
                    }).ToArray(),
                SegmentReusableWidthAttributeIdentifiers = _segmentReusableWidthAttributeIdentifiers.Select(segment =>
                    new Messages.RoadNetworkSnapshotSegmentReusableAttributeIdentifiers
                    {
                        SegmentId = segment.Key.ToInt32(),
                        ReusableAttributeIdentifiers = segment.Value.Select(width => width.ToInt32()).ToArray()
                    }).ToArray(),
                SegmentReusableSurfaceAttributeIdentifiers = _segmentReusableSurfaceAttributeIdentifiers.Select(
                    segment =>
                        new Messages.RoadNetworkSnapshotSegmentReusableAttributeIdentifiers
                        {
                            SegmentId = segment.Key.ToInt32(),
                            ReusableAttributeIdentifiers = segment.Value.Select(surface => surface.ToInt32()).ToArray()
                        }).ToArray()
            };
        }

        public IRoadNetworkView RestoreFromSnapshot(Messages.RoadNetworkSnapshot snapshot)
        {
            if (snapshot == null) throw new ArgumentNullException(nameof(snapshot));

            return new ImmutableRoadNetworkView(
                snapshot.Nodes.ToImmutableDictionary(node => new RoadNodeId(node.Id),
                    node => new RoadNode(new RoadNodeId(node.Id), GeometryTranslator.Translate(node.Geometry))),
                snapshot.Segments.ToImmutableDictionary(segment => new RoadSegmentId(segment.Id),
                    segment => new RoadSegment(new RoadSegmentId(segment.Id),
                        GeometryTranslator.Translate(segment.Geometry), new RoadNodeId(segment.StartNodeId),
                        new RoadNodeId(segment.EndNodeId), new AttributeHash(
                            RoadSegmentAccessRestriction.Parse(segment.AttributeHash.AccessRestriction),
                            RoadSegmentCategory.Parse(segment.AttributeHash.Category),
                            RoadSegmentMorphology.Parse(segment.AttributeHash.Morphology),
                            RoadSegmentStatus.Parse(segment.AttributeHash.Status),
                            segment.AttributeHash.LeftStreetNameId.HasValue ? new CrabStreetnameId(segment.AttributeHash.LeftStreetNameId.Value) : new CrabStreetnameId?(),
                            segment.AttributeHash.RightStreetNameId.HasValue ? new CrabStreetnameId(segment.AttributeHash.RightStreetNameId.Value) : new CrabStreetnameId?(),
                            new OrganizationId(segment.AttributeHash.OrganizationId)))),
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
                    segment => (IReadOnlyList<AttributeId>) segment.ReusableAttributeIdentifiers
                        .Select(identifier => new AttributeId(identifier)).ToArray()),
                snapshot.SegmentReusableWidthAttributeIdentifiers.ToImmutableDictionary(
                    segment => new RoadSegmentId(segment.SegmentId),
                    segment => (IReadOnlyList<AttributeId>) segment.ReusableAttributeIdentifiers
                        .Select(identifier => new AttributeId(identifier)).ToArray()),
                snapshot.SegmentReusableSurfaceAttributeIdentifiers.ToImmutableDictionary(
                    segment => new RoadSegmentId(segment.SegmentId),
                    segment => (IReadOnlyList<AttributeId>) segment.ReusableAttributeIdentifiers
                        .Select(identifier => new AttributeId(identifier)).ToArray())
            );
        }

        private class Builder : IRoadNetworkView
        {
            private readonly ImmutableDictionary<RoadNodeId, RoadNode>.Builder _nodes;
            private readonly ImmutableDictionary<RoadSegmentId, RoadSegment>.Builder _segments;
            private TransactionId _maximumTransactionId;
            private RoadNodeId _maximumNodeId;
            private RoadSegmentId _maximumSegmentId;
            private GradeSeparatedJunctionId _maximumGradeSeparatedJunctionId;
            private AttributeId _maximumEuropeanRoadAttributeId;
            private AttributeId _maximumNationalRoadAttributeId;
            private AttributeId _maximumNumberedRoadAttributeId;
            private AttributeId _maximumLaneAttributeId;
            private AttributeId _maximumWidthAttributeId;
            private AttributeId _maximumSurfaceAttributeId;

            private readonly ImmutableDictionary<RoadSegmentId, IReadOnlyList<AttributeId>>.Builder
                _segmentReusableLaneAttributeIdentifiers;

            private readonly ImmutableDictionary<RoadSegmentId, IReadOnlyList<AttributeId>>.Builder
                _segmentReusableWidthAttributeIdentifiers;

            private readonly ImmutableDictionary<RoadSegmentId, IReadOnlyList<AttributeId>>.Builder
                _segmentReusableSurfaceAttributeIdentifiers;

            public Builder(
                ImmutableDictionary<RoadNodeId, RoadNode>.Builder nodes,
                ImmutableDictionary<RoadSegmentId, RoadSegment>.Builder segments,
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

            public IRoadNetworkView ToBuilder() => this;

            public IRoadNetworkView ToImmutable() => new ImmutableRoadNetworkView(
                _nodes.ToImmutable(),
                _segments.ToImmutable(),
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
                _segmentReusableWidthAttributeIdentifiers.ToImmutable());

            public IReadOnlyDictionary<RoadNodeId, RoadNode> Nodes => _nodes.ToImmutable();
            public IReadOnlyDictionary<RoadSegmentId, RoadSegment> Segments => _segments.ToImmutable();
            public TransactionId MaximumTransactionId => _maximumTransactionId;
            public RoadNodeId MaximumNodeId => _maximumNodeId;
            public RoadSegmentId MaximumSegmentId => _maximumSegmentId;
            public GradeSeparatedJunctionId MaximumGradeSeparatedJunctionId => _maximumGradeSeparatedJunctionId;
            public AttributeId MaximumEuropeanRoadAttributeId => _maximumEuropeanRoadAttributeId;
            public AttributeId MaximumNationalRoadAttributeId => _maximumNationalRoadAttributeId;
            public AttributeId MaximumNumberedRoadAttributeId => _maximumNumberedRoadAttributeId;
            public AttributeId MaximumLaneAttributeId => _maximumLaneAttributeId;
            public AttributeId MaximumWidthAttributeId => _maximumWidthAttributeId;
            public AttributeId MaximumSurfaceAttributeId => _maximumSurfaceAttributeId;

            public ImmutableDictionary<RoadSegmentId, IReadOnlyList<AttributeId>>
                SegmentReusableLaneAttributeIdentifiers =>
                _segmentReusableLaneAttributeIdentifiers.ToImmutable();

            public ImmutableDictionary<RoadSegmentId, IReadOnlyList<AttributeId>>
                SegmentReusableWidthAttributeIdentifiers =>
                _segmentReusableWidthAttributeIdentifiers.ToImmutable();

            public ImmutableDictionary<RoadSegmentId, IReadOnlyList<AttributeId>>
                SegmentReusableSurfaceAttributeIdentifiers =>
                _segmentReusableSurfaceAttributeIdentifiers.ToImmutable();

            public IRoadNetworkView RestoreFromEvents(IReadOnlyCollection<object> events)
            {
                if (events == null) throw new ArgumentNullException(nameof(events));
                foreach (var @event in events)
                {
                    switch (@event)
                    {
                        case Messages.ImportedRoadNode importedRoadNode:
                            Given(importedRoadNode);
                            break;
                        case Messages.ImportedRoadSegment importedRoadSegment:
                            Given(importedRoadSegment);
                            break;
                        case Messages.ImportedGradeSeparatedJunction importedGradeSeparatedJunction:
                            Given(importedGradeSeparatedJunction);
                            break;
                        case Messages.RoadNetworkChangesAccepted roadNetworkChangesAccepted:
                            Given(roadNetworkChangesAccepted);
                            break;
                    }
                }

                return this;
            }

            public IRoadNetworkView RestoreFromEvent(object @event)
            {
                if (@event == null) throw new ArgumentNullException(nameof(@event));

                switch (@event)
                {
                    case Messages.ImportedRoadNode importedRoadNode:
                        Given(importedRoadNode);
                        break;
                    case Messages.ImportedRoadSegment importedRoadSegment:
                        Given(importedRoadSegment);
                        break;
                    case Messages.ImportedGradeSeparatedJunction importedGradeSeparatedJunction:
                        Given(importedGradeSeparatedJunction);
                        break;
                    case Messages.RoadNetworkChangesAccepted roadNetworkChangesAccepted:
                        Given(roadNetworkChangesAccepted);
                        break;
                }

                return this;
            }

            private void Given(Messages.ImportedRoadNode @event)
            {
                if (@event == null) throw new ArgumentNullException(nameof(@event));
                var id = new RoadNodeId(@event.Id);
                var node = new RoadNode(id, GeometryTranslator.Translate(@event.Geometry));
                _nodes.Add(id, node);
                _maximumTransactionId =
                    TransactionId.Max(new TransactionId(@event.Origin.TransactionId), _maximumTransactionId);
                _maximumNodeId = RoadNodeId.Max(id, _maximumNodeId);
            }

            private void Given(Messages.ImportedRoadSegment @event)
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

            private void Given(Messages.ImportedGradeSeparatedJunction @event)
            {
                if (@event == null) throw new ArgumentNullException(nameof(@event));
                var id = new GradeSeparatedJunctionId(@event.Id);
                _maximumTransactionId =
                    TransactionId.Max(new TransactionId(@event.Origin.TransactionId), _maximumTransactionId);
                _maximumGradeSeparatedJunctionId = GradeSeparatedJunctionId.Max(id, _maximumGradeSeparatedJunctionId);
            }

            private void Given(Messages.RoadNetworkChangesAccepted @event)
            {
                if (@event == null) throw new ArgumentNullException(nameof(@event));
                _maximumTransactionId =
                    TransactionId.Max(new TransactionId(@event.TransactionId), _maximumTransactionId);

                foreach (var change in Messages.ChangeExtensions.Flatten(@event.Changes))
                {
                    switch (change)
                    {
                        case Messages.RoadNodeAdded roadNodeAdded:
                            Given(roadNodeAdded);
                            break;
                        case Messages.RoadNodeModified roadNodeModified:
                            Given(roadNodeModified);
                            break;
                        case Messages.RoadSegmentAdded roadSegmentAdded:
                            Given(roadSegmentAdded);
                            break;
                        case Messages.RoadSegmentModified roadSegmentModified:
                            Given(roadSegmentModified);
                            break;
                        case Messages.RoadSegmentAddedToEuropeanRoad roadSegmentAddedToEuropeanRoad:
                            Given(roadSegmentAddedToEuropeanRoad);
                            break;
                        case Messages.RoadSegmentAddedToNationalRoad roadSegmentAddedToNationalRoad:
                            Given(roadSegmentAddedToNationalRoad);
                            break;
                        case Messages.RoadSegmentAddedToNumberedRoad roadSegmentAddedToNumberedRoad:
                            Given(roadSegmentAddedToNumberedRoad);
                            break;
                        case Messages.GradeSeparatedJunctionAdded gradeSeparatedJunctionAdded:
                            Given(gradeSeparatedJunctionAdded);
                            break;
                        case Messages.GradeSeparatedJunctionModified gradeSeparatedJunctionModified:
                            Given(gradeSeparatedJunctionModified);
                            break;
                    }
                }
            }

            private void Given(Messages.RoadNodeAdded @event)
            {
                var id = new RoadNodeId(@event.Id);
                var node = new RoadNode(id, GeometryTranslator.Translate(@event.Geometry));
                _nodes.Add(id, node);
                _maximumNodeId = RoadNodeId.Max(id, _maximumNodeId);
            }

            private void Given(Messages.RoadNodeModified @event)
            {
                // Place holder
            }

            private void Given(Messages.RoadSegmentAdded @event)
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

            private void Given(Messages.RoadSegmentModified @event)
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

            private void Given(Messages.GradeSeparatedJunctionAdded @event)
            {
                var id = new GradeSeparatedJunctionId(@event.Id);
                _maximumGradeSeparatedJunctionId = GradeSeparatedJunctionId.Max(id, _maximumGradeSeparatedJunctionId);
            }

            private void Given(Messages.GradeSeparatedJunctionModified @event)
            {
                // Place holder
            }

            private void Given(Messages.RoadSegmentAddedToEuropeanRoad @event)
            {
                _maximumEuropeanRoadAttributeId = AttributeId.Max(new AttributeId(@event.AttributeId),
                    _maximumEuropeanRoadAttributeId);
            }

            private void Given(Messages.RoadSegmentAddedToNationalRoad @event)
            {
                _maximumNationalRoadAttributeId = AttributeId.Max(new AttributeId(@event.AttributeId),
                    _maximumNationalRoadAttributeId);
            }

            private void Given(Messages.RoadSegmentAddedToNumberedRoad @event)
            {
                _maximumNumberedRoadAttributeId = AttributeId.Max(new AttributeId(@event.AttributeId),
                    _maximumNumberedRoadAttributeId);
            }

            public IRoadNetworkView With(IReadOnlyCollection<IRequestedChange> changes)
            {
                if (changes == null)
                    throw new ArgumentNullException(nameof(changes));

                foreach (var change in changes)
                {
                    switch (change)
                    {
                        case AddRoadNode addRoadNode:
                            With(addRoadNode);
                            break;
                        case ModifyRoadNode modifyRoadNode:
                            With(modifyRoadNode);
                            break;
                        case AddRoadSegment addRoadSegment:
                            With(addRoadSegment);
                            break;
                        case ModifyRoadSegment modifyRoadSegment:
                            With(modifyRoadSegment);
                            break;
                        case AddRoadSegmentToEuropeanRoad addRoadSegmentToEuropeanRoad:
                            With(addRoadSegmentToEuropeanRoad);
                            break;
                        case AddRoadSegmentToNationalRoad addRoadSegmentToNationalRoad:
                            With(addRoadSegmentToNationalRoad);
                            break;
                        case AddRoadSegmentToNumberedRoad addRoadSegmentToNumberedRoad:
                            With(addRoadSegmentToNumberedRoad);
                            break;
                        case AddGradeSeparatedJunction addGradeSeparatedJunction:
                            With(addGradeSeparatedJunction);
                            break;
                        case ModifyGradeSeparatedJunction modifyGradeSeparatedJunction:
                            With(modifyGradeSeparatedJunction);
                            break;
                    }
                }

                return this;
            }

            private void With(AddRoadNode command)
            {
                var node = new RoadNode(command.Id, command.Geometry);
                _nodes.Add(command.Id, node);
                _maximumNodeId = RoadNodeId.Max(command.Id, _maximumNodeId);
            }

            private void With(ModifyRoadNode command)
            {
                //Place holder
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
                    command.MaintenanceAuthority);

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
                    command.MaintenanceAuthority);

                //TODO: Disconnect previous nodes first
                _nodes
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

            private void With(AddGradeSeparatedJunction command)
            {
                var id = new GradeSeparatedJunctionId(command.Id);
                _maximumGradeSeparatedJunctionId = GradeSeparatedJunctionId.Max(id, _maximumGradeSeparatedJunctionId);
            }

            private void With(ModifyGradeSeparatedJunction command)
            {
                //Place holder
            }

            private void With(AddRoadSegmentToEuropeanRoad command)
            {
                _maximumEuropeanRoadAttributeId = AttributeId.Max(command.AttributeId, _maximumEuropeanRoadAttributeId);
            }

            private void With(AddRoadSegmentToNationalRoad command)
            {
                _maximumNationalRoadAttributeId = AttributeId.Max(command.AttributeId, _maximumNationalRoadAttributeId);
            }

            private void With(AddRoadSegmentToNumberedRoad command)
            {
                _maximumNumberedRoadAttributeId = AttributeId.Max(command.AttributeId, _maximumNumberedRoadAttributeId);
            }

            public Messages.RoadNetworkSnapshot TakeSnapshot()
            {
                return new Messages.RoadNetworkSnapshot
                {
                    Nodes = _nodes.Select(node => new Messages.RoadNetworkSnapshotNode
                    {
                        Id = node.Value.Id.ToInt32(),
                        Segments = node.Value.Segments.Select(segment => segment.ToInt32()).ToArray(),
                        Geometry = GeometryTranslator.Translate(node.Value.Geometry)
                    }).ToArray(),
                    Segments = _segments.Select(segment => new Messages.RoadNetworkSnapshotSegment
                    {
                        Id = segment.Value.Id.ToInt32(),
                        StartNodeId = segment.Value.Start.ToInt32(),
                        EndNodeId = segment.Value.End.ToInt32(),
                        Geometry = GeometryTranslator.Translate(segment.Value.Geometry),
                        AttributeHash = new Messages.RoadNetworkSnapshotSegmentAttributeHash
                        {
                            AccessRestriction = segment.Value.AttributeHash.AccessRestriction,
                            Category = segment.Value.AttributeHash.Category,
                            Morphology = segment.Value.AttributeHash.Morphology,
                            Status = segment.Value.AttributeHash.Status,
                            LeftStreetNameId = segment.Value.AttributeHash.LeftStreetNameId?.ToInt32(),
                            RightStreetNameId = segment.Value.AttributeHash.RightStreetNameId?.ToInt32(),
                            OrganizationId = segment.Value.AttributeHash.OrganizationId
                        }
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
                        new Messages.RoadNetworkSnapshotSegmentReusableAttributeIdentifiers
                        {
                            SegmentId = segment.Key.ToInt32(),
                            ReusableAttributeIdentifiers = segment.Value.Select(lane => lane.ToInt32()).ToArray()
                        }).ToArray(),
                    SegmentReusableWidthAttributeIdentifiers = _segmentReusableWidthAttributeIdentifiers.Select(
                        segment =>
                            new Messages.RoadNetworkSnapshotSegmentReusableAttributeIdentifiers
                            {
                                SegmentId = segment.Key.ToInt32(),
                                ReusableAttributeIdentifiers = segment.Value.Select(width => width.ToInt32()).ToArray()
                            }).ToArray(),
                    SegmentReusableSurfaceAttributeIdentifiers = _segmentReusableSurfaceAttributeIdentifiers.Select(
                        segment =>
                            new Messages.RoadNetworkSnapshotSegmentReusableAttributeIdentifiers
                            {
                                SegmentId = segment.Key.ToInt32(),
                                ReusableAttributeIdentifiers =
                                    segment.Value.Select(surface => surface.ToInt32()).ToArray()
                            }).ToArray()
                };
            }

            public IRoadNetworkView RestoreFromSnapshot(Messages.RoadNetworkSnapshot snapshot)
            {
                if (snapshot == null) throw new ArgumentNullException(nameof(snapshot));

                return new Builder(
                    snapshot.Nodes.ToImmutableDictionary(node => new RoadNodeId(node.Id),
                        node => new RoadNode(new RoadNodeId(node.Id), GeometryTranslator.Translate(node.Geometry))).ToBuilder(),
                    snapshot.Segments.ToImmutableDictionary(segment => new RoadSegmentId(segment.Id),
                        segment => new RoadSegment(new RoadSegmentId(segment.Id),
                            GeometryTranslator.Translate(segment.Geometry), new RoadNodeId(segment.StartNodeId),
                            new RoadNodeId(segment.EndNodeId),
                            new AttributeHash(
                                RoadSegmentAccessRestriction.Parse(segment.AttributeHash.AccessRestriction),
                                RoadSegmentCategory.Parse(segment.AttributeHash.Category),
                                RoadSegmentMorphology.Parse(segment.AttributeHash.Morphology),
                                RoadSegmentStatus.Parse(segment.AttributeHash.Status),
                                segment.AttributeHash.LeftStreetNameId.HasValue ? new CrabStreetnameId(segment.AttributeHash.LeftStreetNameId.Value) : new CrabStreetnameId?(),
                                segment.AttributeHash.RightStreetNameId.HasValue ? new CrabStreetnameId(segment.AttributeHash.RightStreetNameId.Value) : new CrabStreetnameId?(),
                                new OrganizationId(segment.AttributeHash.OrganizationId)))).ToBuilder(),
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
                        segment => (IReadOnlyList<AttributeId>) segment.ReusableAttributeIdentifiers
                            .Select(identifier => new AttributeId(identifier)).ToArray()).ToBuilder(),
                    snapshot.SegmentReusableWidthAttributeIdentifiers.ToImmutableDictionary(
                        segment => new RoadSegmentId(segment.SegmentId),
                        segment => (IReadOnlyList<AttributeId>) segment.ReusableAttributeIdentifiers
                            .Select(identifier => new AttributeId(identifier)).ToArray()).ToBuilder(),
                    snapshot.SegmentReusableSurfaceAttributeIdentifiers.ToImmutableDictionary(
                        segment => new RoadSegmentId(segment.SegmentId),
                        segment => (IReadOnlyList<AttributeId>) segment.ReusableAttributeIdentifiers
                            .Select(identifier => new AttributeId(identifier)).ToArray()).ToBuilder()
                );
            }
        }
    }
}
