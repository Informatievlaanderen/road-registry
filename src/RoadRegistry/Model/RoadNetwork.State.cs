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

                var attributeHash = AttributeHash.None
                    .With(RoadSegmentAccessRestriction.Parse(e.AccessRestriction))
                    .With(RoadSegmentCategory.Parse(e.Category))
                    .With(RoadSegmentMorphology.Parse(e.Morphology))
                    .With(RoadSegmentStatus.Parse(e.Status))
                    .WithLeftSide(e.LeftSide.StreetNameId.HasValue
                        ? new CrabStreetnameId(e.LeftSide.StreetNameId.Value)
                        : new CrabStreetnameId?())
                    .WithRightSide(e.RightSide.StreetNameId.HasValue
                        ? new CrabStreetnameId(e.RightSide.StreetNameId.Value)
                        : new CrabStreetnameId?())
                    .With(new MaintenanceAuthorityId(e.MaintenanceAuthority.Code));

                _acceptedNodes = _acceptedNodes
                    .TryReplaceValue(start, node => node.ConnectWith(id))
                    .TryReplaceValue(end, node => node.ConnectWith(id));

                var segment = new RoadSegment(
                    id,
                    GeometryTranslator.Translate(e.Geometry),
                    start,
                    end,
                    attributeHash);
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
                        case RoadNodeAdded e1:
                            {
                                var id = new RoadNodeId(e1.Id);
                                var node = new RoadNode(id, GeometryTranslator.Translate(e1.Geometry));
                                _acceptedNodes = _acceptedNodes.Add(id, node);
                                _maximumNodeId = RoadNodeId.Max(id, _maximumNodeId);
                            }
                            break;
                        case RoadSegmentAdded e2:
                            {
                                var id = new RoadSegmentId(e2.Id);
                                var start = new RoadNodeId(e2.StartNodeId);
                                var end = new RoadNodeId(e2.EndNodeId);
                                _acceptedNodes = _acceptedNodes
                                    .TryReplaceValue(start, node => node.ConnectWith(id))
                                    .TryReplaceValue(end, node => node.ConnectWith(id));
                                var attributeHash = AttributeHash.None
                                    .With(RoadSegmentAccessRestriction.Parse(e2.AccessRestriction))
                                    .With(RoadSegmentCategory.Parse(e2.Category))
                                    .With(RoadSegmentMorphology.Parse(e2.Morphology))
                                    .With(RoadSegmentStatus.Parse(e2.Status))
                                    .WithLeftSide(e2.LeftSide.StreetNameId.HasValue
                                        ? new CrabStreetnameId(e2.LeftSide.StreetNameId.Value)
                                        : new CrabStreetnameId?())
                                    .WithRightSide(e2.RightSide.StreetNameId.HasValue
                                        ? new CrabStreetnameId(e2.RightSide.StreetNameId.Value)
                                        : new CrabStreetnameId?())
                                    .With(new MaintenanceAuthorityId(e2.MaintenanceAuthority.Code));
                                var segment = new RoadSegment(
                                    id,
                                    GeometryTranslator.Translate(e2.Geometry),
                                    start,
                                    end,
                                    attributeHash);
                                _acceptedSegments = _acceptedSegments.Add(id, segment);
                                _maximumSegmentId = RoadSegmentId.Max(id, _maximumSegmentId);
                                if (e2.PartOfEuropeanRoads.Length > 0)
                                {
                                    _maximumEuropeanRoadAttributeId = AttributeId.Max(
                                        new AttributeId(e2.PartOfEuropeanRoads.Max(_ => _.AttributeId)),
                                        _maximumEuropeanRoadAttributeId);
                                }

                                if (e2.PartOfNationalRoads.Length > 0)
                                {
                                    _maximumNationalRoadAttributeId = AttributeId.Max(
                                        new AttributeId(e2.PartOfNationalRoads.Max(_ => _.AttributeId)),
                                        _maximumNationalRoadAttributeId);
                                }

                                if (e2.PartOfNumberedRoads.Length > 0)
                                {
                                    _maximumNumberedRoadAttributeId = AttributeId.Max(
                                        new AttributeId(e2.PartOfNumberedRoads.Max(_ => _.AttributeId)),
                                        _maximumNumberedRoadAttributeId);
                                }
                                if (e2.Lanes.Length > 0)
                                {
                                    _maximumLaneAttributeId = AttributeId.Max(
                                        new AttributeId(e2.Lanes.Max(_ => _.AttributeId)),
                                        _maximumLaneAttributeId);
                                }

                                if (e2.Widths.Length > 0)
                                {
                                    _maximumWidthAttributeId = AttributeId.Max(
                                        new AttributeId(e2.Widths.Max(_ => _.AttributeId)),
                                        _maximumWidthAttributeId);
                                }

                                if (e2.Surfaces.Length > 0)
                                {
                                    _maximumSurfaceAttributeId = AttributeId.Max(
                                        new AttributeId(e2.Surfaces.Max(_ => _.AttributeId)),
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
