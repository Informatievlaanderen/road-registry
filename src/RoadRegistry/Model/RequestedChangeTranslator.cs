namespace RoadRegistry.Model
{
    using System;
    using System.Collections.Generic;
    using Aiv.Vbr.Shaperon;
    using GeoAPI.Geometries;
    using NetTopologySuite.Geometries;

    internal class RequestedChangeTranslator
    {
        private readonly Dictionary<RoadNodeId, RoadNodeId> _temporaryToPermanentNodeIdMap;
        //private readonly Dictionary<RoadSegmentId, RoadSegmentId> _temporaryToPermanentSegmentIdMap;
        private readonly Func<RoadNodeId> _nextRoadNodeId;
        private readonly Func<RoadSegmentId> _nextRoadSegmentId;

        public RequestedChangeTranslator(Func<RoadNodeId> nextRoadNodeId, Func<RoadSegmentId> nextRoadSegmentId)
        {
            _nextRoadNodeId = nextRoadNodeId ?? throw new ArgumentNullException(nameof(nextRoadNodeId));
            _nextRoadSegmentId = nextRoadSegmentId ?? throw new ArgumentNullException(nameof(nextRoadSegmentId));
            _temporaryToPermanentNodeIdMap = new Dictionary<RoadNodeId, RoadNodeId>();
            //_temporaryToPermanentSegmentIdMap = new Dictionary<RoadSegmentId, RoadSegmentId>();
        }

        public IRequestedChange Translate(Messages.AddRoadNode command)
        {
            var id = _nextRoadNodeId();
            var temporaryId = new RoadNodeId(command.TemporaryId);

            _temporaryToPermanentNodeIdMap.Add(temporaryId, id);

            return new AddRoadNode
            (
                id,
                temporaryId,
                RoadNodeType.Parse(command.Type),
                GeometryTranslator.Translate(command.Geometry)
            );
        }

        public IRequestedChange Translate(Messages.AddRoadSegment command)
        {
            var id = _nextRoadSegmentId();
            var temporaryId = new RoadSegmentId(command.TemporaryId);

            //_temporaryToPermanentSegmentIdMap.Add(temporaryId, id);

            var startNode = new RoadNodeId(command.StartNodeId);
            if (_temporaryToPermanentNodeIdMap.TryGetValue(startNode, out var permanentStartNodeId))
            {
                startNode = permanentStartNodeId;
            }

            var endNode = new RoadNodeId(command.EndNodeId);
            if (_temporaryToPermanentNodeIdMap.TryGetValue(endNode, out var permanentEndNodeId))
            {
                endNode = permanentEndNodeId;
            }

            var geometry = GeometryTranslator.Translate(command.Geometry);
            var maintainer = new MaintenanceAuthorityId(command.MaintenanceAuthority);
            var geometryDrawMethod = RoadSegmentGeometryDrawMethod.Parse(command.GeometryDrawMethod);
            var morphology = RoadSegmentMorphology.Parse(command.Morphology);
            var status = RoadSegmentStatus.Parse(command.Status);
            var category = RoadSegmentCategory.Parse(command.Category);
            var accessRestriction = RoadSegmentAccessRestriction.Parse(command.AccessRestriction);
            var leftSideStreetNameId = command.LeftSideStreetNameId.HasValue
                ? new CrabStreetnameId(command.LeftSideStreetNameId.Value)
                : new CrabStreetnameId?();
            var rightSideStreetNameId = command.RightSideStreetNameId.HasValue
                ? new CrabStreetnameId(command.RightSideStreetNameId.Value)
                : new CrabStreetnameId?();
            var partOfEuropeanRoads = Array.ConvertAll(
                command.PartOfEuropeanRoads,
                item => EuropeanRoadNumber.Parse(item.RoadNumber)
            );
            var partOfNationalRoads = Array.ConvertAll(
                command.PartOfNationalRoads,
                item => NationalRoadNumber.Parse(item.Ident2)
            );
            var partOfNumberedRoads = Array.ConvertAll(
                command.PartOfNumberedRoads,
                item => new RoadSegmentNumberedRoadAttribute(
                    NumberedRoadNumber.Parse(item.Ident8),
                    RoadSegmentNumberedRoadDirection.Parse(item.Direction),
                    new RoadSegmentNumberedRoadOrdinal(item.Ordinal)
                )
            );
            var laneAttributes = Array.ConvertAll(
                command.Lanes,
                item => new RoadSegmentLaneAttribute(
                    new RoadSegmentLaneCount(item.Count),
                    RoadSegmentLaneDirection.Parse(item.Direction),
                    new RoadSegmentPosition(item.FromPosition),
                    new RoadSegmentPosition(item.ToPosition),
                    new GeometryVersion(0)
                )
            );
            var widthAttributes = Array.ConvertAll(
                command.Widths,
                item => new RoadSegmentWidthAttribute(
                    new RoadSegmentWidth(item.Width),
                    new RoadSegmentPosition(item.FromPosition),
                    new RoadSegmentPosition(item.ToPosition),
                    new GeometryVersion(0)
                )
            );
            var surfaceAttributes = Array.ConvertAll(
                command.Surfaces,
                item => new RoadSegmentSurfaceAttribute(
                    RoadSegmentSurfaceType.Parse(item.Type),
                    new RoadSegmentPosition(item.FromPosition),
                    new RoadSegmentPosition(item.ToPosition),
                    new GeometryVersion(0)
                )
            );

            return new AddRoadSegment
            (
                id,
                temporaryId,
                startNode,
                endNode,
                geometry,
                maintainer,
                geometryDrawMethod,
                morphology,
                status,
                category,
                accessRestriction,
                leftSideStreetNameId,
                rightSideStreetNameId,
                partOfEuropeanRoads,
                partOfNationalRoads,
                partOfNumberedRoads,
                laneAttributes,
                widthAttributes,
                surfaceAttributes
            );
        }
    }
}
