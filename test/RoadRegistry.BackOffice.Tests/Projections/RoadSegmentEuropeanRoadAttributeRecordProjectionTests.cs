namespace RoadRegistry.BackOffice.Projections
{
    using System;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using AutoFixture;
    using Framework.Testing.Projections;
    using Messages;
    using Microsoft.IO;
    using Schema.RoadSegmentEuropeanRoadAttributes;
    using Xunit;

    public class RoadSegmentEuropeanRoadAttributeRecordProjectionTests : IClassFixture<ProjectionTestServices>
    {
        private readonly ProjectionTestServices _services;
        private readonly Fixture _fixture;

        public RoadSegmentEuropeanRoadAttributeRecordProjectionTests(ProjectionTestServices services)
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
                    segment.PartOfEuropeanRoads = _fixture
                        .CreateMany<ImportedRoadSegmentEuropeanRoadAttributes>(random.Next(1, 10))
                        .ToArray();

                    var expected = segment
                        .PartOfEuropeanRoads
                        .Select(europeanRoad => new RoadSegmentEuropeanRoadAttributeRecord
                        {
                            Id = europeanRoad.AttributeId,
                            RoadSegmentId = segment.Id,
                            DbaseRecord = new RoadSegmentEuropeanRoadAttributeDbaseRecord
                            {
                                EU_OIDN = { Value = europeanRoad.AttributeId },
                                WS_OIDN = { Value = segment.Id },
                                EUNUMMER = { Value = europeanRoad.Number },
                                BEGINTIJD = { Value = europeanRoad.Origin.Since },
                                BEGINORG = { Value = europeanRoad.Origin.OrganizationId },
                                LBLBGNORG = { Value = europeanRoad.Origin.Organization }
                            }.ToBytes(_services.MemoryStreamManager, Encoding.UTF8)
                        });

                    return new
                    {
                        importedRoadSegment = segment,
                        expected
                    };

                }).ToList();

            return new RoadSegmentEuropeanRoadAttributeRecordProjection(new RecyclableMemoryStreamManager(), Encoding.UTF8)
                .Scenario()
                .Given(data.Select(d => d.importedRoadSegment))
                .Expect(data
                    .SelectMany(d => d.expected)
                    .Cast<object>()
                    .ToArray()
                );
        }

        [Fact]
        public Task When_importing_a_road_node_without_european_road_links()
        {
            var importedRoadSegment = _fixture.Create<ImportedRoadSegment>();
            importedRoadSegment.PartOfEuropeanRoads = new ImportedRoadSegmentEuropeanRoadAttributes[0];

            return new RoadSegmentEuropeanRoadAttributeRecordProjection(new RecyclableMemoryStreamManager(), Encoding.UTF8)
                .Scenario()
                .Given(importedRoadSegment)
                .Expect(new object[0]);
        }
    }
}
