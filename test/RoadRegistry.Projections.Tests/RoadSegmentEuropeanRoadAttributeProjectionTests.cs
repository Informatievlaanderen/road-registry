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
        private readonly ScenarioFixture _fixture;

        public RoadSegmentEuropeanRoadAttributeProjectionTests()
        {
            _fixture = new ScenarioFixture();
        }

        [Fact]
        public Task When_importing_road_nodes()
        {
            var random = new Random();
            var data = _fixture
                .CreateMany<ImportedRoadSegment>(random.Next(1, 3))
                .Select(segment =>
                {
                    segment.PartOfEuropeanRoads = _fixture
                        .CreateMany<ImportedRoadSegmentEuropeanRoadProperties>(random.Next(1, 5))
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
                                LBLBGNORG = { Value = europeanRoad.Origin.Organization },
                            }.ToBytes(Encoding.UTF8)
                        });

                    return new
                    {
                        importedRoadSegment = segment,
                        expected,
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
            importedRoadSegment.PartOfEuropeanRoads = new ImportedRoadSegmentEuropeanRoadProperties[0];

            return new RoadSegmentEuropeanRoadAttributeRecordProjection(Encoding.UTF8)
                .Scenario()
                .Given(importedRoadSegment)
                .Expect(new object[0]);
        }
    }
}
