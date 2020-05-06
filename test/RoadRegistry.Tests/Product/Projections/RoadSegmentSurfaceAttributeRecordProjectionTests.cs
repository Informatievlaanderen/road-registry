namespace RoadRegistry.Product.Projections
{
    using System;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using AutoFixture;
    using BackOffice;
    using BackOffice.Messages;
    using Editor.Projections;
    using Framework.Projections;
    using RoadRegistry.Projections;
    using Schema.RoadSegments;
    using Xunit;

    public class RoadSegmentSurfaceAttributeRecordProjectionTests : IClassFixture<ProjectionTestServices>
    {
        private readonly ProjectionTestServices _services;
        private readonly Fixture _fixture;

        public RoadSegmentSurfaceAttributeRecordProjectionTests(ProjectionTestServices services)
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));

            _fixture = new Fixture();
            _fixture.CustomizeAttributeId();
            _fixture.CustomizeRoadSegmentId();
            _fixture.CustomizeRoadNodeId();
            _fixture.CustomizeOrganizationId();
            _fixture.CustomizeOrganizationName();
            _fixture.CustomizePolylineM();
            _fixture.CustomizeEuropeanRoadNumber();
            _fixture.CustomizeNationalRoadNumber();
            _fixture.CustomizeNumberedRoadNumber();
            _fixture.CustomizeRoadSegmentNumberedRoadDirection();
            _fixture.CustomizeRoadSegmentNumberedRoadOrdinal();
            _fixture.CustomizeRoadSegmentLaneCount();
            _fixture.CustomizeRoadSegmentLaneDirection();
            _fixture.CustomizeRoadSegmentWidth();
            _fixture.CustomizeRoadSegmentSurfaceType();
            _fixture.CustomizeRoadSegmentGeometryDrawMethod();
            _fixture.CustomizeRoadSegmentMorphology();
            _fixture.CustomizeRoadSegmentStatus();
            _fixture.CustomizeRoadSegmentCategory();
            _fixture.CustomizeRoadSegmentAccessRestriction();
            _fixture.CustomizeRoadSegmentGeometryVersion();

            _fixture.CustomizeImportedRoadSegment();
            _fixture.CustomizeImportedRoadSegmentEuropeanRoadAttributes();
            _fixture.CustomizeImportedRoadSegmentNationalRoadAttributes();
            _fixture.CustomizeImportedRoadSegmentNumberedRoadAttributes();
            _fixture.CustomizeImportedRoadSegmentLaneAttributes();
            _fixture.CustomizeImportedRoadSegmentWidthAttributes();
            _fixture.CustomizeImportedRoadSegmentSurfaceAttributes();
            _fixture.CustomizeImportedRoadSegmentSideAttributes();
            _fixture.CustomizeOriginProperties();
        }

        [Fact]
        public Task When_importing_road_nodes()
        {
            var random = new Random();
            var data = _fixture
                .CreateMany<ImportedRoadSegment>(random.Next(1, 10))
                .Select(segment =>
                {
                    segment.Surfaces = _fixture
                        .CreateMany<ImportedRoadSegmentSurfaceAttributes>(random.Next(1, 10))
                        .ToArray();

                    var expected = segment
                        .Surfaces
                        .Select(surface => new RoadSegmentSurfaceAttributeRecord
                        {
                            Id = surface.AttributeId,
                            RoadSegmentId = segment.Id,
                            DbaseRecord = new RoadSegmentSurfaceAttributeDbaseRecord
                            {
                                WV_OIDN = { Value = surface.AttributeId },
                                WS_OIDN = { Value = segment.Id },
                                WS_GIDN = { Value = segment.Id + "_" + surface.AsOfGeometryVersion },
                                TYPE =  { Value = RoadSegmentSurfaceType.Parse(surface.Type).Translation.Identifier },
                                LBLTYPE =  { Value = RoadSegmentSurfaceType.Parse(surface.Type).Translation.Name },
                                VANPOS = { Value = (double)surface.FromPosition },
                                TOTPOS = { Value = (double)surface.ToPosition },
                                BEGINTIJD = { Value = surface.Origin.Since },
                                BEGINORG = { Value = surface.Origin.OrganizationId },
                                LBLBGNORG = { Value = surface.Origin.Organization }
                            }.ToBytes(_services.MemoryStreamManager, Encoding.UTF8)
                        });

                    return new
                    {
                        importedRoadSegment = segment,
                        expected
                    };

                }).ToList();

            return new RoadRegistry.Product.Projections.RoadSegmentSurfaceAttributeRecordProjection(_services.MemoryStreamManager, Encoding.UTF8)
                .Scenario()
                .Given(data.Select(d => d.importedRoadSegment))
                .Expect(data
                    .SelectMany(d => d.expected)
                    .Cast<object>()
                    .ToArray()
                );
        }

        [Fact]
        public Task When_importing_a_road_node_without_surfaces()
        {
            var importedRoadSegment = _fixture.Create<ImportedRoadSegment>();
            importedRoadSegment.Surfaces = new ImportedRoadSegmentSurfaceAttributes[0];

            return new RoadRegistry.Product.Projections.RoadSegmentSurfaceAttributeRecordProjection(_services.MemoryStreamManager, Encoding.UTF8)
                .Scenario()
                .Given(importedRoadSegment)
                .Expect(new object[0]);
        }
    }
}
