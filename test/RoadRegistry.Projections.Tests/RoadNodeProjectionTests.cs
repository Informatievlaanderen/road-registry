namespace RoadRegistry.Projections.Tests
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoFixture;
    using Events;
    using Infrastucture;
    using Shaperon;
    using Wkx;
    using Xunit;

    public class RoadNodeProjectionTests
    {
        private readonly ScenarioFixture _fixture;

        public RoadNodeProjectionTests()
        {
            _fixture = new ScenarioFixture();
        }

        [Fact]
        public Task When_road_node_was_imported()
        {
            var point = _fixture.Create<Point>();
            var pointShapeContent = new PointShapeContent(point);
            var geometry = _fixture.Build<Events.Geometry>().With(g => g.WellKnownBinary, point.SerializeByteArray<WkbSerializer>()).Create();
            var importedRoadNode = _fixture.Build<ImportedRoadNode>().With(n => n.Geometry, geometry).Create();

            return new RoadNodeRecordProjection().Scenario()
                .Given(importedRoadNode)
                .Expect(new object[]
                {
                    new RoadNodeRecord
                    {
                        Id = importedRoadNode.Id,
                        DbaseRecord = new RoadNodeDbaseRecord
                        {
                            WK_OIDN = {Value = importedRoadNode.Id},
                            WK_UIDN = {Value = importedRoadNode.Id + "_" + importedRoadNode.Version},
                            TYPE = {Value = (int) importedRoadNode.Type},
                            LBLTYPE =
                            {
                                Value = new RoadNodeTypeTranslator().TranslateToDutchName(importedRoadNode.Type)
                            },
                            BEGINTIJD = {Value = importedRoadNode.Origin.Since},
                            BEGINORG = {Value = importedRoadNode.Origin.OrganizationId},
                            LBLBGNORG = {Value = importedRoadNode.Origin.Organization}
                        }.ToBytes(),
                        ShapeRecordContent = pointShapeContent.ToBytes(),
                        ShapeRecordContentLength = pointShapeContent.ContentLength.ToInt32()
                    }
                });
        }

        [Fact]
        public Task When_multiple_road_nodes_were_imported()
        {
            var data = _fixture
                .CreateMany<Point>(new Random().Next(10))
                .Select((point, index) =>
                {
                    var geometry = _fixture
                        .Build<Events.Geometry>()
                        .With(g => g.WellKnownBinary, point.SerializeByteArray<WkbSerializer>())
                        .Create();

                    var importedRoadNode = _fixture
                        .Build<ImportedRoadNode>()
                        .With(n => n.Geometry, geometry)
                        .With(node => node.Id, index + 1)
                        .Create();

                    var pointShapeContent = new PointShapeContent(point);
                    var expectedRecord = new RoadNodeRecord
                    {
                        Id = importedRoadNode.Id,
                        DbaseRecord = new RoadNodeDbaseRecord
                        {
                            WK_OIDN = {Value = importedRoadNode.Id},
                            WK_UIDN = {Value = importedRoadNode.Id + "_" + importedRoadNode.Version},
                            TYPE = {Value = (int) importedRoadNode.Type},
                            LBLTYPE =
                            {
                                Value = new RoadNodeTypeTranslator().TranslateToDutchName(importedRoadNode.Type)
                            },
                            BEGINTIJD = {Value = importedRoadNode.Origin.Since},
                            BEGINORG = {Value = importedRoadNode.Origin.OrganizationId},
                            LBLBGNORG = {Value = importedRoadNode.Origin.Organization}
                        }.ToBytes(),
                        ShapeRecordContent = pointShapeContent.ToBytes(),
                        ShapeRecordContentLength = pointShapeContent.ContentLength.ToInt32()
                    };

                    return new
                    {
                        ImportedRoadNode = importedRoadNode,
                        ExpectedRecord = expectedRecord
                    };
                }).ToList();

            return new RoadNodeRecordProjection().Scenario()
                .Given(data.Select(d => d.ImportedRoadNode))
                .Expect(data.Select(d => d.ExpectedRecord));

        }
    }
}
