namespace RoadRegistry.Model
{
    using System;
    using System.Collections.Immutable;
    using System.Linq;
    using Framework;
    using Messages;

    public partial class RoadNetwork : EventSourcedEntity
    {
        public static readonly Func<RoadNetwork> Factory = () => new RoadNetwork();
        public static readonly double TooCloseDistance = 2.0;

        private ImmutableDictionary<RoadNodeId, RoadNode> _acceptedNodes;
        private ImmutableDictionary<RoadSegmentId, RoadSegment> _acceptedSegments;
        private RoadNodeId _maximumNodeId = new RoadNodeId(0);
        private RoadSegmentId _maximumSegmentId = new RoadSegmentId(0);
        private AttributeId _maximumEuropeanRoadAttributeId = new AttributeId(0);
        private AttributeId _maximumNationalRoadAttributeId = new AttributeId(0);
        private AttributeId _maximumNumberedRoadAttributeId = new AttributeId(0);
        private AttributeId _maximumLaneAttributeId = new AttributeId(0);
        private AttributeId _maximumWidthAttributeId = new AttributeId(0);
        private AttributeId _maximumSurfaceAttributeId = new AttributeId(0);

        private RoadNetwork()
        {
            _acceptedNodes = ImmutableDictionary<RoadNodeId, RoadNode>.Empty;
            _acceptedSegments = ImmutableDictionary<RoadSegmentId, RoadSegment>.Empty;

            On<ImportedRoadNode>(e =>
            {
                var id = new RoadNodeId(e.Id);
                var node = new RoadNode(id, GeometryTranslator.Translate(e.Geometry));
                _acceptedNodes = _acceptedNodes.Add(id, node);
                _maximumNodeId = RoadNodeId.Max(id, _maximumNodeId);
            });

            On<ImportedRoadSegment>(e =>
            {
                var id = new RoadSegmentId(e.Id);
                var start = new RoadNodeId(e.StartNodeId);
                var end = new RoadNodeId(e.EndNodeId);
                _acceptedNodes = _acceptedNodes
                    .TryReplaceValue(start, node => node.ConnectWith(id))
                    .TryReplaceValue(end, node => node.ConnectWith(id));
                var segment = new RoadSegment(
                    id,
                    GeometryTranslator.Translate(e.Geometry),
                    start,
                    end,
                    AttributeHash.None);
                _acceptedSegments = _acceptedSegments.Add(id, segment);
                _maximumSegmentId = RoadSegmentId.Max(id, _maximumSegmentId);
                if (e.PartOfEuropeanRoads.Length > 0)
                {
                    _maximumEuropeanRoadAttributeId = AttributeId.Max(
                        new AttributeId(e.PartOfEuropeanRoads.Max(_ => _.AttributeId)),
                        _maximumEuropeanRoadAttributeId);
                }

                if (e.PartOfNationalRoads.Length > 0)
                {
                    _maximumNationalRoadAttributeId = AttributeId.Max(
                        new AttributeId(e.PartOfNationalRoads.Max(_ => _.AttributeId)),
                        _maximumNationalRoadAttributeId);
                }

                if (e.PartOfNumberedRoads.Length > 0)
                {
                    _maximumNumberedRoadAttributeId = AttributeId.Max(
                        new AttributeId(e.PartOfNumberedRoads.Max(_ => _.AttributeId)),
                        _maximumNumberedRoadAttributeId);
                }
                if (e.Lanes.Length > 0)
                {
                    _maximumLaneAttributeId = AttributeId.Max(
                        new AttributeId(e.Lanes.Max(_ => _.AttributeId)),
                        _maximumLaneAttributeId);
                }

                if (e.Widths.Length > 0)
                {
                    _maximumWidthAttributeId = AttributeId.Max(
                        new AttributeId(e.Widths.Max(_ => _.AttributeId)),
                        _maximumWidthAttributeId);
                }

                if (e.Surfaces.Length > 0)
                {
                    _maximumSurfaceAttributeId = AttributeId.Max(
                        new AttributeId(e.Surfaces.Max(_ => _.AttributeId)),
                        _maximumSurfaceAttributeId);
                }
            });

            On<RoadNetworkChangesAccepted>(e =>
            {
                foreach (var change in e.Changes.Flatten())
                {
                    switch (change)
                    {
                        case RoadNodeAdded roadNodeAdded:
                            {
                                var id = new RoadNodeId(roadNodeAdded.Id);
                                var node = new RoadNode(id, GeometryTranslator.Translate(roadNodeAdded.Geometry));
                                _acceptedNodes = _acceptedNodes.Add(id, node);
                                _maximumNodeId = RoadNodeId.Max(id, _maximumNodeId);
                            }
                            break;
                        case RoadSegmentAdded roadSegmentAdded:
                            {
                                var id = new RoadSegmentId(roadSegmentAdded.Id);
                                var start = new RoadNodeId(roadSegmentAdded.StartNodeId);
                                var end = new RoadNodeId(roadSegmentAdded.EndNodeId);
                                _acceptedNodes = _acceptedNodes
                                    .TryReplaceValue(start, node => node.ConnectWith(id))
                                    .TryReplaceValue(end, node => node.ConnectWith(id));
                                var segment = new RoadSegment(
                                    id,
                                    GeometryTranslator.Translate(roadSegmentAdded.Geometry),
                                    start,
                                    end,
                                    AttributeHash.None);
                                _acceptedSegments = _acceptedSegments.Add(id, segment);
                                _maximumSegmentId = RoadSegmentId.Max(id, _maximumSegmentId);
                                if (roadSegmentAdded.PartOfEuropeanRoads.Length > 0)
                                {
                                    _maximumEuropeanRoadAttributeId = AttributeId.Max(
                                        new AttributeId(roadSegmentAdded.PartOfEuropeanRoads.Max(_ => _.AttributeId)),
                                        _maximumEuropeanRoadAttributeId);
                                }

                                if (roadSegmentAdded.PartOfNationalRoads.Length > 0)
                                {
                                    _maximumNationalRoadAttributeId = AttributeId.Max(
                                        new AttributeId(roadSegmentAdded.PartOfNationalRoads.Max(_ => _.AttributeId)),
                                        _maximumNationalRoadAttributeId);
                                }

                                if (roadSegmentAdded.PartOfNumberedRoads.Length > 0)
                                {
                                    _maximumNumberedRoadAttributeId = AttributeId.Max(
                                        new AttributeId(roadSegmentAdded.PartOfNumberedRoads.Max(_ => _.AttributeId)),
                                        _maximumNumberedRoadAttributeId);
                                }
                                if (roadSegmentAdded.Lanes.Length > 0)
                                {
                                    _maximumLaneAttributeId = AttributeId.Max(
                                        new AttributeId(roadSegmentAdded.Lanes.Max(_ => _.AttributeId)),
                                        _maximumLaneAttributeId);
                                }

                                if (roadSegmentAdded.Widths.Length > 0)
                                {
                                    _maximumWidthAttributeId = AttributeId.Max(
                                        new AttributeId(roadSegmentAdded.Widths.Max(_ => _.AttributeId)),
                                        _maximumWidthAttributeId);
                                }

                                if (roadSegmentAdded.Surfaces.Length > 0)
                                {
                                    _maximumSurfaceAttributeId = AttributeId.Max(
                                        new AttributeId(roadSegmentAdded.Surfaces.Max(_ => _.AttributeId)),
                                        _maximumSurfaceAttributeId);
                                }
                            }
                            break;
                    }
                }
            });
        }


    }
}
