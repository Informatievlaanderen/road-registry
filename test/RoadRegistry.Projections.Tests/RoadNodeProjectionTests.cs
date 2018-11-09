namespace RoadRegistry.Projections.Tests
{
    using System;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using AutoFixture;
    using Infrastructure;
    using Aiv.Vbr.Shaperon;
    using Messages;
    using Xunit;
    using RoadNodeType = Model.RoadNodeType;

    public class RoadNodeProjectionTests
    {
        private readonly ScenarioFixture _fixture;

        public RoadNodeProjectionTests()
        {
            _fixture = new ScenarioFixture();
        }

        [Fact]
        public Task When_road_nodes_were_imported()
        {
            var wkbWriter = new WellKnownBinaryWriter();
            var data = _fixture
                .CreateMany<PointM>(new Random().Next(1,10))
                .Select(point =>
                {
                    var importedRoadNode = _fixture
                        .Build<ImportedRoadNode>()
                        .With(n => n.Geometry, wkbWriter.Write(point))
                        .Create();

                    var pointShapeContent = new PointShapeContent(point);
                    var expectedRecord = new RoadNodeRecord
                    {
                        Id = importedRoadNode.Id,
                        DbaseRecord = new RoadNodeDbaseRecord
                        {
                            WK_OIDN = {Value = importedRoadNode.Id},
                            WK_UIDN = {Value = importedRoadNode.Id + "_" + importedRoadNode.Version},
                            TYPE = {Value = RoadNodeType.Parse(importedRoadNode.Type).Translation.Identifier},
                            LBLTYPE =
                            {
                                Value = RoadNodeType.Parse(importedRoadNode.Type).Translation.Name
                            },
                            BEGINTIJD = {Value = importedRoadNode.Origin.Since},
                            BEGINORG = {Value = importedRoadNode.Origin.OrganizationId},
                            LBLBGNORG = {Value = importedRoadNode.Origin.Organization}
                        }.ToBytes(Encoding.UTF8),
                        ShapeRecordContent = pointShapeContent.ToBytes(),
                        ShapeRecordContentLength = pointShapeContent.ContentLength.ToInt32(),
                        Envelope = BoundingBox2D.From(pointShapeContent.Shape.EnvelopeInternal)
                    };

                    return new
                    {
                        ImportedRoadNode = importedRoadNode,
                        ExpectedRecord = expectedRecord
                    };
                }).ToList();

            return new RoadNodeRecordProjection(
                    new WellKnownBinaryReader(),
                    Encoding.UTF8)
                .Scenario()
                .Given(data.Select(d => d.ImportedRoadNode))
                .Expect(data.Select(d => d.ExpectedRecord));

        }
    }
}
