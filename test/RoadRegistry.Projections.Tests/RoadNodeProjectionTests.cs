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
    using Model;
    using Xunit;
    using RoadNodeType = Model.RoadNodeType;

    public class RoadNodeProjectionTests
    {
        private readonly Fixture _fixture;

        public RoadNodeProjectionTests()
        {
            _fixture = new Fixture();

            _fixture.CustomizeRoadNodeId();
            _fixture.CustomizeRoadNodeType();
            _fixture.CustomizeMaintenanceAuthorityId();
            _fixture.CustomizeMaintenanceAuthorityName();
            _fixture.CustomizePointM();
            _fixture.CustomizeOriginProperties();
            _fixture.CustomizeImportedRoadNode();
        }

        [Fact]
        public Task When_road_nodes_were_imported()
        {
            var data = _fixture
                .CreateMany<ImportedRoadNode>(new Random().Next(1, 100))
                .Select(@event =>
                {
                    var point = GeometryTranslator.Translate(@event.Geometry2);
                    var pointShapeContent = new PointShapeContent(
                        new PointM(point.X, point.Y)
                    );
                    var expectedRecord = new RoadNodeRecord
                    {
                        Id = @event.Id,
                        DbaseRecord = new RoadNodeDbaseRecord
                        {
                            WK_OIDN = {Value = @event.Id},
                            WK_UIDN = {Value = @event.Id + "_" + @event.Version},
                            TYPE = {Value = RoadNodeType.Parse(@event.Type).Translation.Identifier},
                            LBLTYPE =
                            {
                                Value = RoadNodeType.Parse(@event.Type).Translation.Name
                            },
                            BEGINTIJD = {Value = @event.Origin.Since},
                            BEGINORG = {Value = @event.Origin.OrganizationId},
                            LBLBGNORG = {Value = @event.Origin.Organization}
                        }.ToBytes(Encoding.UTF8),
                        ShapeRecordContent = pointShapeContent.ToBytes(),
                        ShapeRecordContentLength = pointShapeContent.ContentLength.ToInt32(),
                        Envelope = BoundingBox2D.From(pointShapeContent.Shape.EnvelopeInternal)
                    };

                    return new
                    {
                        ImportedRoadNode = @event,
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
