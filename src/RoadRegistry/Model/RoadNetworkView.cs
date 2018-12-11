namespace RoadRegistry.Model
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
            _segmentLaneAttributeIdentifiers;
        private readonly ImmutableDictionary<RoadSegmentId, IReadOnlyList<AttributeId>>
            _segmentWidthAttributeIdentifiers;
        private readonly ImmutableDictionary<RoadSegmentId, IReadOnlyList<AttributeId>>
            _segmentSurfaceAttributeIdentifiers;

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
            ImmutableDictionary<RoadSegmentId, IReadOnlyList<AttributeId>> segmentLaneAttributeIdentifiers,
            ImmutableDictionary<RoadSegmentId, IReadOnlyList<AttributeId>> segmentWidthAttributeIdentifiers,
            ImmutableDictionary<RoadSegmentId, IReadOnlyList<AttributeId>> segmentSurfaceAttributeIdentifiers)
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
            _segmentLaneAttributeIdentifiers = segmentLaneAttributeIdentifiers;
            _segmentWidthAttributeIdentifiers = segmentWidthAttributeIdentifiers;
            _segmentSurfaceAttributeIdentifiers = segmentSurfaceAttributeIdentifiers;
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

        public ImmutableDictionary<RoadSegmentId, IReadOnlyList<AttributeId>> SegmentLaneAttributeIdentifiers =>
            _segmentLaneAttributeIdentifiers;

        public ImmutableDictionary<RoadSegmentId, IReadOnlyList<AttributeId>> SegmentWidthAttributeIdentifiers =>
            _segmentWidthAttributeIdentifiers;

        public ImmutableDictionary<RoadSegmentId, IReadOnlyList<AttributeId>> SegmentSurfaceAttributeIdentifiers =>
            _segmentSurfaceAttributeIdentifiers;

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
                _segmentLaneAttributeIdentifiers,
                _segmentWidthAttributeIdentifiers,
                _segmentSurfaceAttributeIdentifiers);
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
                .With(new MaintenanceAuthorityId(@event.MaintenanceAuthority.Code));

            var segment = new RoadSegment(
                id,
                GeometryTranslator.Translate(@event.Geometry),
                start,
                end,
                attributeHash);

            return new RoadNetworkView(
                _nodes
                    .TryReplaceValue(start, node => node.ConnectWith(id))
                    .TryReplaceValue(end, node => node.ConnectWith(id)),
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
                _segmentLaneAttributeIdentifiers.AddOrMergeDistinct(id, @event.Lanes.Select(lane => new AttributeId(lane.AttributeId))),
                _segmentWidthAttributeIdentifiers.AddOrMergeDistinct(id, @event.Widths.Select(width => new AttributeId(width.AttributeId))),
                _segmentSurfaceAttributeIdentifiers.AddOrMergeDistinct(id, @event.Surfaces.Select(surface => new AttributeId(surface.AttributeId)))
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
                _segmentLaneAttributeIdentifiers,
                _segmentWidthAttributeIdentifiers,
                _segmentSurfaceAttributeIdentifiers);
        }

        public RoadNetworkView Given(Messages.RoadNetworkChangesAccepted @event)
        {
            if (@event == null) throw new ArgumentNullException(nameof(@event));
            var result = this;
            foreach (var change in @event.Changes.Flatten())
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
                _segmentLaneAttributeIdentifiers,
                _segmentWidthAttributeIdentifiers,
                _segmentSurfaceAttributeIdentifiers);
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
                .With(new MaintenanceAuthorityId(@event.MaintenanceAuthority.Code));

            var segment = new RoadSegment(
                id,
                GeometryTranslator.Translate(@event.Geometry),
                start,
                end,
                attributeHash);

            return new RoadNetworkView(
                _nodes
                    .TryReplaceValue(start, node => node.ConnectWith(id))
                    .TryReplaceValue(end, node => node.ConnectWith(id)),
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
                _segmentLaneAttributeIdentifiers.AddOrMergeDistinct(id, @event.Lanes.Select(lane => new AttributeId(lane.AttributeId))),
                _segmentWidthAttributeIdentifiers.AddOrMergeDistinct(id, @event.Widths.Select(width => new AttributeId(width.AttributeId))),
                _segmentSurfaceAttributeIdentifiers.AddOrMergeDistinct(id, @event.Surfaces.Select(surface => new AttributeId(surface.AttributeId)))
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
                _segmentLaneAttributeIdentifiers,
                _segmentWidthAttributeIdentifiers,
                _segmentSurfaceAttributeIdentifiers);
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
                _segmentLaneAttributeIdentifiers,
                _segmentWidthAttributeIdentifiers,
                _segmentSurfaceAttributeIdentifiers);
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
                _segmentLaneAttributeIdentifiers,
                _segmentWidthAttributeIdentifiers,
                _segmentSurfaceAttributeIdentifiers);
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
                _segmentLaneAttributeIdentifiers,
                _segmentWidthAttributeIdentifiers,
                _segmentSurfaceAttributeIdentifiers);
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
                _segmentLaneAttributeIdentifiers,
                _segmentWidthAttributeIdentifiers,
                _segmentSurfaceAttributeIdentifiers);
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
                    .TryReplaceValue(command.StartNodeId, node => node.ConnectWith(command.Id))
                    .TryReplaceValue(command.EndNodeId, node => node.ConnectWith(command.Id)),
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
                _segmentLaneAttributeIdentifiers.AddOrMergeDistinct(command.Id, command.Lanes.Select(lane => new AttributeId(lane.Id))),
                _segmentWidthAttributeIdentifiers.AddOrMergeDistinct(command.Id, command.Widths.Select(width => new AttributeId(width.Id))),
                _segmentSurfaceAttributeIdentifiers.AddOrMergeDistinct(command.Id, command.Surfaces.Select(surface => new AttributeId(surface.Id)))
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
                _segmentLaneAttributeIdentifiers,
                _segmentWidthAttributeIdentifiers,
                _segmentSurfaceAttributeIdentifiers);
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
                _segmentLaneAttributeIdentifiers,
                _segmentWidthAttributeIdentifiers,
                _segmentSurfaceAttributeIdentifiers);
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
                _segmentLaneAttributeIdentifiers,
                _segmentWidthAttributeIdentifiers,
                _segmentSurfaceAttributeIdentifiers);
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
                _segmentLaneAttributeIdentifiers,
                _segmentWidthAttributeIdentifiers,
                _segmentSurfaceAttributeIdentifiers);
        }
    }
}
