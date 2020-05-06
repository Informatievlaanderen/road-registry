namespace RoadRegistry.BackOffice.Projections.Product
{
    using System;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Framework.Testing.Projections;
    using Messages;
    using Microsoft.IO;
    using RoadRegistry.Product.Schema.RoadNodes;
    using RoadRegistry.Product.Projections;
    using Xunit;
    using GeometryTranslator = Core.GeometryTranslator;
    using RoadNodeType = RoadNodeType;

    public class RoadNodeRecordProjectionTests : IClassFixture<ProjectionTestServices>
    {
        private readonly ProjectionTestServices _services;
        private readonly Fixture _fixture;

        public RoadNodeRecordProjectionTests(ProjectionTestServices services)
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));

            _fixture = new Fixture();

            _fixture.CustomizeRoadNodeId();
            _fixture.CustomizeRoadNodeType();
            _fixture.CustomizeOrganizationId();
            _fixture.CustomizeOrganizationName();
            _fixture.CustomizePoint();
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
                    var point = GeometryTranslator.Translate(@event.Geometry);
                    var pointShapeContent = new PointShapeContent(
                        Be.Vlaanderen.Basisregisters.Shaperon.Geometries.GeometryTranslator.FromGeometryPoint(
                            new NetTopologySuite.Geometries.Point(point.X, point.Y)
                        )
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
                        }.ToBytes(_services.MemoryStreamManager, Encoding.UTF8),
                        ShapeRecordContent = pointShapeContent.ToBytes(_services.MemoryStreamManager, Encoding.UTF8),
                        ShapeRecordContentLength = pointShapeContent.ContentLength.ToInt32(),
                        BoundingBox = RoadNodeBoundingBox.From(pointShapeContent.Shape)
                    };

                    return new
                    {
                        ImportedRoadNode = @event,
                        ExpectedRecord = expectedRecord
                    };
                }).ToList();

            return new RoadRegistry.Product.Projections.RoadNodeRecordProjection(new RecyclableMemoryStreamManager(), Encoding.UTF8)
                .Scenario()
                .Given(data.Select(d => d.ImportedRoadNode))
                .Expect(data.Select(d => d.ExpectedRecord));

        }
    }
}
