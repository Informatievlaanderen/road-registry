namespace RoadRegistry.Projections.Tests
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoFixture;
    using Events;
    using GeoAPI.Geometries;
    using Infrastructure;
    using NetTopologySuite;
    using NetTopologySuite.Geometries;
    using NetTopologySuite.IO;
    using Shaperon;
    using Xunit;

    public class RoadNodeProjectionTests
    {
        private readonly ScenarioFixture _fixture;
        private readonly RoadNodeTypeTranslator _roadNodeTypeTranslator;

        public RoadNodeProjectionTests()
        {
            _fixture = new ScenarioFixture();
            _roadNodeTypeTranslator = new RoadNodeTypeTranslator();
        }

        [Fact]
        public Task When_road_nodes_were_imported()
        {
            var data = _fixture
                .CreateMany<Point>(new Random().Next(1,10))
                .Select((point, index) =>
                {
                    var importedRoadNode = _fixture
                        .Build<ImportedRoadNode>()
                        .With(n => n.Geometry, point.ToBinary())
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
                                Value = _roadNodeTypeTranslator.TranslateToDutchName(importedRoadNode.Type)
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

            return new RoadNodeRecordProjection(
                    new WKBReader(new NtsGeometryServices())
                    {
                        HandleOrdinates = Ordinates.XYZM,
                        HandleSRID = true
                    },
                    _roadNodeTypeTranslator)
                .Scenario()
                .Given(data.Select(d => d.ImportedRoadNode))
                .Expect(data.Select(d => d.ExpectedRecord));

        }
    }
}
