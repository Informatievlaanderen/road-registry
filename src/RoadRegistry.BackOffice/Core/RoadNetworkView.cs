namespace RoadRegistry.BackOffice.Core
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;

    public class RoadNetworkView
    {
        public static readonly RoadNetworkView Empty = new RoadNetworkView(
            ImmutableDictionary<RoadNodeId, RoadNode>.Empty,
            ImmutableDictionary<RoadSegmentId, RoadSegment>.Empty,
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

        private RoadNetworkView(
            ImmutableDictionary<RoadNodeId, RoadNode> nodes,
            ImmutableDictionary<RoadSegmentId, RoadSegment> segments,
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

        public IReadOnlyDictionary<RoadNodeId, RoadNode> Nodes => _nodes;
        public IReadOnlyDictionary<RoadSegmentId, RoadSegment> Segments => _segments;
        public RoadNodeId MaximumNodeId => _maximumNodeId;
        public RoadSegmentId MaximumSegmentId => _maximumSegmentId;
        public GradeSeparatedJunctionId MaximumGradeSeparatedJunctionId => _maximumGradeSeparatedJunctionId;
        public AttributeId MaximumEuropeanRoadAttributeId => _maximumEuropeanRoadAttributeId;
        public AttributeId MaximumNationalRoadAttributeId => _maximumNationalRoadAttributeId;
        public AttributeId MaximumNumberedRoadAttributeId => _maximumNumberedRoadAttributeId;
        public AttributeId MaximumLaneAttributeId => _maximumLaneAttributeId;
        public AttributeId MaximumWidthAttributeId => _maximumWidthAttributeId;
        public AttributeId MaximumSurfaceAttributeId => _maximumSurfaceAttributeId;

        public ImmutableDictionary<RoadSegmentId, IReadOnlyList<AttributeId>> SegmentReusableLaneAttributeIdentifiers =>
            _segmentReusableLaneAttributeIdentifiers;

        public ImmutableDictionary<RoadSegmentId, IReadOnlyList<AttributeId>> SegmentReusableWidthAttributeIdentifiers =>
            _segmentReusableWidthAttributeIdentifiers;

        public ImmutableDictionary<RoadSegmentId, IReadOnlyList<AttributeId>> SegmentReusableSurfaceAttributeIdentifiers =>
            _segmentReusableSurfaceAttributeIdentifiers;

        public RoadNetworkView Given(Messages.ImportedRoadNode @event)
        {
            if (@event == null) throw new ArgumentNullException(nameof(@event));
            var id = new RoadNodeId(@event.Id);
            var node = new RoadNode(id, GeometryTranslator.Translate(@event.Geometry));
            return new RoadNetworkView(
                _nodes.Add(id, node),
                _segments,
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

        public RoadNetworkView Given(Messages.ImportedRoadSegment @event)
        {
            if (@event == null) throw new ArgumentNullException(nameof(@event));
            var id = new RoadSegmentId(@event.Id);
            var start = new RoadNodeId(@event.StartNodeId);
            var end = new RoadNodeId(@event.EndNodeId);

            var attributeHash = AttributeHash.None
                .With(RoadSegmentAccessRestriction.Parse(@event.AccessRestriction))
                .With(RoadSegmentCategory.Parse(@event.Category))
                .With(RoadSegmentMorphology.Parse(@event.Morphology))
                .With(RoadSegmentStatus.Parse(@event.Status))
                .WithLeftSide(@event.LeftSide.StreetNameId.HasValue
                    ? new CrabStreetnameId(@event.LeftSide.StreetNameId.Value)
                    : new CrabStreetnameId?())
                .WithRightSide(@event.RightSide.StreetNameId.HasValue
                    ? new CrabStreetnameId(@event.RightSide.StreetNameId.Value)
                    : new CrabStreetnameId?())
                .With(new OrganizationId(@event.MaintenanceAuthority.Code));

            var segment = new RoadSegment(
                id,
                GeometryTranslator.Translate(@event.Geometry),
                start,
                end,
                attributeHash);

            return new RoadNetworkView(
                _nodes
                    .TryReplace(start, node => node.ConnectWith(id))
                    .TryReplace(end, node => node.ConnectWith(id)),
                _segments.Add(id, segment),
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
                _segmentReusableLaneAttributeIdentifiers.AddOrMergeDistinct(id, @event.Lanes.Select(lane => new AttributeId(lane.AttributeId))),
                _segmentReusableWidthAttributeIdentifiers.AddOrMergeDistinct(id, @event.Widths.Select(width => new AttributeId(width.AttributeId))),
                _segmentReusableSurfaceAttributeIdentifiers.AddOrMergeDistinct(id, @event.Surfaces.Select(surface => new AttributeId(surface.AttributeId)))
            );
        }

        public RoadNetworkView Given(Messages.ImportedGradeSeparatedJunction @event)
        {
            if (@event == null) throw new ArgumentNullException(nameof(@event));
            var id = new GradeSeparatedJunctionId(@event.Id);
            return new RoadNetworkView(
                _nodes,
                _segments,
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

        public RoadNetworkView Given(Messages.RoadNetworkChangesAccepted @event)
        {
            if (@event == null) throw new ArgumentNullException(nameof(@event));
            var result = this;
            foreach (var change in Messages.ChangeExtensions.Flatten(@event.Changes))
            {
                switch (change)
                {
                    case Messages.RoadNodeAdded roadNodeAdded:
                        result = result.Given(roadNodeAdded);
                        break;
                    case Messages.RoadSegmentAdded roadSegmentAdded:
                        result = result.Given(roadSegmentAdded);
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
                }
            }

            return result;
        }

        public RoadNetworkView Given(Messages.RoadNetworkChangesBasedOnArchiveAccepted @event)
        {
            if (@event == null) throw new ArgumentNullException(nameof(@event));
            var result = this;
            foreach (var change in Messages.ChangeExtensions.Flatten(@event.Changes))
            {
                switch (change)
                {
                    case Messages.RoadNodeAdded roadNodeAdded:
                        result = result.Given(roadNodeAdded);
                        break;
                    case Messages.RoadSegmentAdded roadSegmentAdded:
                        result = result.Given(roadSegmentAdded);
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
                }
            }

            return result;
        }

        private RoadNetworkView Given(Messages.RoadNodeAdded @event)
        {
            var id = new RoadNodeId(@event.Id);
            var node = new RoadNode(id, GeometryTranslator.Translate(@event.Geometry));
            return new RoadNetworkView(
                _nodes.Add(id, node),
                _segments,
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

        private RoadNetworkView Given(Messages.RoadSegmentAdded @event)
        {
            var id = new RoadSegmentId(@event.Id);
            var start = new RoadNodeId(@event.StartNodeId);
            var end = new RoadNodeId(@event.EndNodeId);

            var attributeHash = AttributeHash.None
                .With(RoadSegmentAccessRestriction.Parse(@event.AccessRestriction))
                .With(RoadSegmentCategory.Parse(@event.Category))
                .With(RoadSegmentMorphology.Parse(@event.Morphology))
                .With(RoadSegmentStatus.Parse(@event.Status))
                .WithLeftSide(@event.LeftSide.StreetNameId.HasValue
                    ? new CrabStreetnameId(@event.LeftSide.StreetNameId.Value)
                    : new CrabStreetnameId?())
                .WithRightSide(@event.RightSide.StreetNameId.HasValue
                    ? new CrabStreetnameId(@event.RightSide.StreetNameId.Value)
                    : new CrabStreetnameId?())
                .With(new OrganizationId(@event.MaintenanceAuthority.Code));

            var segment = new RoadSegment(
                id,
                GeometryTranslator.Translate(@event.Geometry),
                start,
                end,
                attributeHash);

            return new RoadNetworkView(
                _nodes
                    .TryReplace(start, node => node.ConnectWith(id))
                    .TryReplace(end, node => node.ConnectWith(id)),
                _segments.Add(id, segment),
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
                _segmentReusableLaneAttributeIdentifiers.AddOrMergeDistinct(id, @event.Lanes.Select(lane => new AttributeId(lane.AttributeId))),
                _segmentReusableWidthAttributeIdentifiers.AddOrMergeDistinct(id, @event.Widths.Select(width => new AttributeId(width.AttributeId))),
                _segmentReusableSurfaceAttributeIdentifiers.AddOrMergeDistinct(id, @event.Surfaces.Select(surface => new AttributeId(surface.AttributeId)))
            );
        }

        private RoadNetworkView Given(Messages.GradeSeparatedJunctionAdded @event)
        {
            var id = new GradeSeparatedJunctionId(@event.Id);
            return new RoadNetworkView(
                _nodes,
                _segments,
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

        private RoadNetworkView Given(Messages.RoadSegmentAddedToEuropeanRoad @event)
        {
            return new RoadNetworkView(
                _nodes,
                _segments,
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

        private RoadNetworkView Given(Messages.RoadSegmentAddedToNationalRoad @event)
        {
            return new RoadNetworkView(
                _nodes,
                _segments,
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

        private RoadNetworkView Given(Messages.RoadSegmentAddedToNumberedRoad @event)
        {
            return new RoadNetworkView(
                _nodes,
                _segments,
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

        public RoadNetworkView With(IReadOnlyCollection<IRequestedChange> changes)
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
                    case AddRoadSegment addRoadSegment:
                        result = result.With(addRoadSegment);
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
                }
            }

            return result;
        }

        private RoadNetworkView With(AddRoadNode command)
        {
            var node = new RoadNode(command.Id, command.Geometry);
            return new RoadNetworkView(
                _nodes.Add(command.Id, node),
                _segments,
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

        private RoadNetworkView With(AddRoadSegment command)
        {
            var attributeHash = AttributeHash.None
                .With(command.AccessRestriction)
                .With(command.Category)
                .With(command.Morphology)
                .With(command.Status)
                .WithLeftSide(command.LeftSideStreetNameId)
                .WithRightSide(command.RightSideStreetNameId)
                .With(command.MaintenanceAuthority);

            return new RoadNetworkView(
                _nodes
                    .TryReplace(command.StartNodeId, node => node.ConnectWith(command.Id))
                    .TryReplace(command.EndNodeId, node => node.ConnectWith(command.Id)),
                _segments.Add(command.Id,
                    new RoadSegment(command.Id, command.Geometry, command.StartNodeId, command.EndNodeId,
                        attributeHash)),
                _maximumNodeId,
                RoadSegmentId.Max(command.Id, _maximumSegmentId),
                _maximumGradeSeparatedJunctionId,
                _maximumEuropeanRoadAttributeId,
                _maximumNationalRoadAttributeId,
                _maximumNumberedRoadAttributeId,
                _maximumLaneAttributeId,
                _maximumWidthAttributeId,
                _maximumSurfaceAttributeId,
                _segmentReusableLaneAttributeIdentifiers.AddOrMergeDistinct(command.Id, command.Lanes.Select(lane => new AttributeId(lane.Id))),
                _segmentReusableWidthAttributeIdentifiers.AddOrMergeDistinct(command.Id, command.Widths.Select(width => new AttributeId(width.Id))),
                _segmentReusableSurfaceAttributeIdentifiers.AddOrMergeDistinct(command.Id, command.Surfaces.Select(surface => new AttributeId(surface.Id)))
            );
        }

        private RoadNetworkView With(AddGradeSeparatedJunction command)
        {
            var id = new GradeSeparatedJunctionId(command.Id);
            return new RoadNetworkView(
                _nodes,
                _segments,
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

        private RoadNetworkView With(AddRoadSegmentToEuropeanRoad command)
        {
            return new RoadNetworkView(
                _nodes,
                _segments,
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

        private RoadNetworkView With(AddRoadSegmentToNationalRoad command)
        {
            return new RoadNetworkView(
                _nodes,
                _segments,
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

        private RoadNetworkView With(AddRoadSegmentToNumberedRoad command)
        {
            return new RoadNetworkView(
                _nodes,
                _segments,
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
                    AttributeHash = segment.Value.AttributeHash.GetHashCode()
                }).ToArray(),
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
                SegmentReusableSurfaceAttributeIdentifiers = _segmentReusableSurfaceAttributeIdentifiers.Select(segment =>
                    new Messages.RoadNetworkSnapshotSegmentReusableAttributeIdentifiers
                    {
                        SegmentId = segment.Key.ToInt32(),
                        ReusableAttributeIdentifiers = segment.Value.Select(surface => surface.ToInt32()).ToArray()
                    }).ToArray()
            };
        }

        public RoadNetworkView RestoreFromSnapshot(Messages.RoadNetworkSnapshot snapshot)
        {
            if (snapshot == null) throw new ArgumentNullException(nameof(snapshot));

            return new RoadNetworkView(
                snapshot.Nodes.ToImmutableDictionary(node => new RoadNodeId(node.Id),
                    node => new RoadNode(new RoadNodeId(node.Id), GeometryTranslator.Translate(node.Geometry))),
                snapshot.Segments.ToImmutableDictionary(segment => new RoadSegmentId(segment.Id),
                    segment => new RoadSegment(new RoadSegmentId(segment.Id),
                        GeometryTranslator.Translate(segment.Geometry), new RoadNodeId(segment.StartNodeId),
                        new RoadNodeId(segment.EndNodeId), AttributeHash.FromHashCode(segment.AttributeHash))),
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
    }
}
