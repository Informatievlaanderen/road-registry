namespace RoadRegistry.Projections.Tests
{
    using System;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using AutoFixture;
    using Infrastructure;
    using Messages;
    using Xunit;

    public class RoadSegmentEuropeanRoadAttributeProjectionTests
    {
        private readonly Fixture _fixture;

        public RoadSegmentEuropeanRoadAttributeProjectionTests()
        {
            _fixture = new Fixture();
            _fixture.CustomizeAttributeId();
            _fixture.CustomizeRoadSegmentId();
            _fixture.CustomizeRoadNodeId();
            _fixture.CustomizeMaintenanceAuthorityId();
            _fixture.CustomizeMaintenanceAuthorityName();
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
                                EUNUMMER = { Value = europeanRoad.RoadNumber },
                                BEGINTIJD = { Value = europeanRoad.Origin.Since },
                                BEGINORG = { Value = europeanRoad.Origin.OrganizationId },
                                LBLBGNORG = { Value = europeanRoad.Origin.Organization }
                            }.ToBytes(Encoding.UTF8)
                        });

                    return new
                    {
                        importedRoadSegment = segment,
                        expected
                    };

                }).ToList();

            return new RoadSegmentEuropeanRoadAttributeRecordProjection(Encoding.UTF8)
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

            return new RoadSegmentEuropeanRoadAttributeRecordProjection(Encoding.UTF8)
                .Scenario()
                .Given(importedRoadSegment)
                .Expect(new object[0]);
        }
    }
}
