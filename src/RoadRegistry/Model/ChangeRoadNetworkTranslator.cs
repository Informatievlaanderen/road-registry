namespace RoadRegistry.Model
{
    using System;
    using Aiv.Vbr.Shaperon;
    using NetTopologySuite.Geometries;

    internal class ChangeRoadNetworkTranslator
    {
        public ChangeRoadNetworkTranslator(WellKnownBinaryReader reader)
        {
            Reader = reader ?? throw new ArgumentNullException(nameof(reader));
        }

        public WellKnownBinaryReader Reader { get; }

        public IRoadNetworkChange Translate(Commands.AddRoadNode command)
        {
            var id = new RoadNodeId(command.Id);
            var geometry = Reader.ReadAs<PointM>(command.Geometry);
            return new AddRoadNode
            (
                id,
                RoadNodeType.Parse((int) command.Type),
                geometry
            );
        }

        public IRoadNetworkChange Translate(Commands.AddRoadSegment command)
        {
            var id = new RoadSegmentId(command.Id);
            var startNode = new RoadNodeId(command.StartNodeId);
            var endNode = new RoadNodeId(command.EndNodeId);
            var geometry = Reader.ReadAs<MultiLineString>(command.Geometry);
            var maintainer = new MaintainerId(command.Maintainer);
            var geometryDrawMethod = RoadSegmentGeometryDrawMethod.Parse((int)command.GeometryDrawMethod);
            var morphology = RoadSegmentMorphology.Parse((int)command.Morphology);
            var status = RoadSegmentStatus.Parse((int)command.Status);
            var category = RoadSegmentCategory.Parse(command.Category.ToString()); // won't work
            var accessRestriction = RoadSegmentAccessRestriction.Parse((int)command.AccessRestriction);
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
                    RoadSegmentNumberedRoadDirection.Parse((int)item.Direction),
                    new RoadSegmentNumberedRoadOrdinal(item.Ordinal)
                )
            );
            var laneAttributes = new RoadSegmentLaneAttributes(
                Array.ConvertAll(
                    command.Lanes,
                    item => new RoadSegmentLaneAttribute(
                        new RoadSegmentLaneCount(item.Count),
                        RoadSegmentLaneDirection.Parse((int)item.Direction),
                        new RoadSegmentPosition(item.FromPosition),
                        new RoadSegmentPosition(item.ToPosition),
                        new GeometryVersion(0)
                    )
                )
            );
            var widthAttributes = new RoadSegmentWidthAttributes(
                Array.ConvertAll(
                    command.Widths,
                    item => new RoadSegmentWidthAttribute(
                        new RoadSegmentWidth(item.Width),
                        new RoadSegmentPosition(item.FromPosition),
                        new RoadSegmentPosition(item.ToPosition),
                        new GeometryVersion(0)
                    )
                )
            );
            var hardeningAttributes = new RoadSegmentHardeningAttributes(
                Array.ConvertAll(
                    command.Hardenings,
                    item => new RoadSegmentHardeningAttribute(
                        RoadSegmentHardeningType.Parse((int)item.Type),
                        new RoadSegmentPosition(item.FromPosition),
                        new RoadSegmentPosition(item.ToPosition),
                        new GeometryVersion(0)
                    )
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
                hardeningAttributes
            );
        }
    }
}
