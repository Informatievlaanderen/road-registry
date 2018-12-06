namespace RoadRegistry.Model
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using Messages;

    public class RoadNetworkView
    {
        public static readonly RoadNetworkView Empty = new RoadNetworkView(
            ImmutableDictionary<RoadNodeId, RoadNode>.Empty,
            ImmutableDictionary<RoadSegmentId, RoadSegment>.Empty,
            new RoadNodeId(0),
            new RoadSegmentId(0),
            new AttributeId(0),
            new AttributeId(0),
            new AttributeId(0),
            new AttributeId(0),
            new AttributeId(0),
            new AttributeId(0));

        private readonly ImmutableDictionary<RoadNodeId, RoadNode> _nodes;
        private readonly ImmutableDictionary<RoadSegmentId, RoadSegment> _segments;
        private readonly RoadNodeId _maximumNodeId;
        private readonly RoadSegmentId _maximumSegmentId;
        private readonly AttributeId _maximumEuropeanRoadAttributeId;
        private readonly AttributeId _maximumNationalRoadAttributeId;
        private readonly AttributeId _maximumNumberedRoadAttributeId;
        private readonly AttributeId _maximumLaneAttributeId;
        private readonly AttributeId _maximumWidthAttributeId;
        private readonly AttributeId _maximumSurfaceAttributeId;

        private RoadNetworkView(
            ImmutableDictionary<RoadNodeId, RoadNode> nodes,
            ImmutableDictionary<RoadSegmentId, RoadSegment> segments,
            RoadNodeId maximumNodeId,
            RoadSegmentId maximumSegmentId,
            AttributeId maximumEuropeanRoadAttributeId,
            AttributeId maximumNationalRoadAttributeId,
            AttributeId maximumNumberedRoadAttributeId,
            AttributeId maximumLaneAttributeId,
            AttributeId maximumWidthAttributeId,
            AttributeId maximumSurfaceAttributeId)
        {
            _nodes = nodes;
            _segments = segments;
            _maximumNodeId = maximumNodeId;
            _maximumSegmentId = maximumSegmentId;
            _maximumEuropeanRoadAttributeId = maximumEuropeanRoadAttributeId;
            _maximumNationalRoadAttributeId = maximumNationalRoadAttributeId;
            _maximumNumberedRoadAttributeId = maximumNumberedRoadAttributeId;
            _maximumLaneAttributeId = maximumLaneAttributeId;
            _maximumWidthAttributeId = maximumWidthAttributeId;
            _maximumSurfaceAttributeId = maximumSurfaceAttributeId;
        }

        public IReadOnlyDictionary<RoadNodeId, RoadNode> Nodes => _nodes;
        public IReadOnlyDictionary<RoadSegmentId, RoadSegment> Segments => _segments;
        public RoadNodeId MaximumNodeId => _maximumNodeId;
        public RoadSegmentId MaximumSegmentId => _maximumSegmentId;
        public AttributeId MaximumEuropeanRoadAttributeId => _maximumEuropeanRoadAttributeId;
        public AttributeId MaximumNationalRoadAttributeId => _maximumNationalRoadAttributeId;
        public AttributeId MaximumNumberedRoadAttributeId => _maximumNumberedRoadAttributeId;
        public AttributeId MaximumLaneAttributeId => _maximumLaneAttributeId;
        public AttributeId MaximumWidthAttributeId => _maximumWidthAttributeId;
        public AttributeId MaximumSurfaceAttributeId => _maximumSurfaceAttributeId;

        public RoadNetworkView Given(ImportedRoadNode @event)
        {
            if (@event == null) throw new ArgumentNullException(nameof(@event));
            var id = new RoadNodeId(@event.Id);
            var node = new RoadNode(id, GeometryTranslator.Translate(@event.Geometry));
            return new RoadNetworkView(
                _nodes.Add(id, node),
                _segments,
                RoadNodeId.Max(id, _maximumNodeId),
                _maximumSegmentId,
                _maximumEuropeanRoadAttributeId,
                _maximumNationalRoadAttributeId,
                _maximumNumberedRoadAttributeId,
                _maximumLaneAttributeId,
                _maximumWidthAttributeId,
                _maximumSurfaceAttributeId
            );
        }

        public RoadNetworkView Given(ImportedRoadSegment @event)
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
                RoadSegmentId.Max(id, _maximumSegmentId),
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
                    : _maximumSurfaceAttributeId
            );
        }

        public RoadNetworkView Given(RoadNetworkChangesAccepted @event)
        {
            if (@event == null) throw new ArgumentNullException(nameof(@event));
            var result = this;
            foreach (var change in @event.Changes.Flatten())
            {
                switch (change)
                {
                    case RoadNodeAdded roadNodeAdded:
                        result = result.Given(roadNodeAdded);
                        break;
                    case RoadSegmentAdded roadSegmentAdded:
                        result = result.Given(roadSegmentAdded);
                        break;
                }
            }

            return result;
        }

        private RoadNetworkView Given(RoadNodeAdded @event)
        {
            var id = new RoadNodeId(@event.Id);
            var node = new RoadNode(id, GeometryTranslator.Translate(@event.Geometry));
            return new RoadNetworkView(
                _nodes.Add(id, node),
                _segments,
                RoadNodeId.Max(id, _maximumNodeId),
                _maximumSegmentId,
                _maximumEuropeanRoadAttributeId,
                _maximumNationalRoadAttributeId,
                _maximumNumberedRoadAttributeId,
                _maximumLaneAttributeId,
                _maximumWidthAttributeId,
                _maximumSurfaceAttributeId
            );
        }

        private RoadNetworkView Given(RoadSegmentAdded @event)
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
                    : _maximumSurfaceAttributeId
            );
        }

        public RoadNetworkView When(IReadOnlyCollection<IRequestedChange> changes)
        {
            if (changes == null)
                throw new ArgumentNullException(nameof(changes));

            var result = this;
            foreach (var change in changes)
            {
                switch (change)
                {
                    case AddRoadNode addRoadNode:
                        result = result.When(addRoadNode);
                        break;
                    case AddRoadSegment addRoadSegment:
                        result = result.When(addRoadSegment);
                        break;
                }
            }

            return result;
        }

        private RoadNetworkView When(AddRoadNode command)
        {
            var node = new RoadNode(command.Id, command.Geometry);
            return new RoadNetworkView(
                _nodes.Add(command.Id, node),
                _segments,
                RoadNodeId.Max(command.Id, _maximumNodeId),
                _maximumSegmentId,
                _maximumEuropeanRoadAttributeId,
                _maximumNationalRoadAttributeId,
                _maximumNumberedRoadAttributeId,
                _maximumLaneAttributeId,
                _maximumWidthAttributeId,
                _maximumSurfaceAttributeId
            );
        }

        private RoadNetworkView When(AddRoadSegment command)
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
                    new RoadSegment(command.Id, command.Geometry, command.StartNodeId, command.EndNodeId, attributeHash)),
                _maximumNodeId,
                RoadSegmentId.Max(command.Id, _maximumSegmentId),
                _maximumEuropeanRoadAttributeId,
                _maximumNationalRoadAttributeId,
                _maximumNumberedRoadAttributeId,
                _maximumLaneAttributeId,
                _maximumWidthAttributeId,
                _maximumSurfaceAttributeId
            );
        }
    }
}
