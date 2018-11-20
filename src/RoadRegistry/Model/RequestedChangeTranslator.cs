namespace RoadRegistry.Model
{
    using System;
    using Aiv.Vbr.Shaperon;
    using GeoAPI.Geometries;
    using NetTopologySuite.Geometries;

    internal static class RequestedChangeTranslator
    {
        public static IRequestedChange Translate(Messages.AddRoadNode command)
        {
            var id = new RoadNodeId(command.Id);
            return new AddRoadNode
            (
                id,
                RoadNodeType.Parse(command.Type),
                GeometryTranslator.Translate(command.Geometry2)
            );
        }

        public static IRequestedChange Translate(Messages.AddRoadSegment command)
        {
            var id = new RoadSegmentId(command.Id);
            var startNode = new RoadNodeId(command.StartNodeId);
            var endNode = new RoadNodeId(command.EndNodeId);
            var geometry = GeometryTranslator.Translate(command.Geometry2);
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
