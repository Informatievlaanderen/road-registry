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

    public class RoadSegmentNationalRoadAttributeProjectionTests
    {
        private readonly ScenarioFixture _fixture;

        public RoadSegmentNationalRoadAttributeProjectionTests()
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
                    segment.PartOfNationalRoads = _fixture
                        .CreateMany<ImportedRoadSegmentNationalRoadAttributes>(random.Next(1, 5))
                        .ToArray();

                    var expected = segment
                        .PartOfNationalRoads
                        .Select(nationalRoad => new RoadSegmentNationalRoadAttributeRecord
                        {
                            Id = nationalRoad.AttributeId,
                            RoadSegmentId = segment.Id,
                            DbaseRecord = new RoadSegmentNationalRoadAttributeDbaseRecord
                            {
                                NW_OIDN = { Value = nationalRoad.AttributeId },
                                WS_OIDN = { Value = segment.Id },
                                IDENT2 = { Value = nationalRoad.Ident2 },
                                BEGINTIJD = { Value = nationalRoad.Origin.Since },
                                BEGINORG = { Value = nationalRoad.Origin.OrganizationId },
                                LBLBGNORG = { Value = nationalRoad.Origin.Organization }
                            }.ToBytes(Encoding.UTF8)
                        });

                    return new
                    {
                        importedRoadSegment = segment,
                        expected
                    };

                }).ToList();

            return new RoadSegmentNationalRoadAttributeRecordProjection(Encoding.UTF8)
                .Scenario()
                .Given(data.Select(d => d.importedRoadSegment))
                .Expect(data
                    .SelectMany(d => d.expected)
                    .Cast<object>()
                    .ToArray()
                );
        }

        [Fact]
        public Task When_importing_a_road_node_without_national_road_links()
        {
            var importedRoadSegment = _fixture.Create<ImportedRoadSegment>();
            importedRoadSegment.PartOfNationalRoads = new ImportedRoadSegmentNationalRoadAttributes[0];

            return new RoadSegmentNationalRoadAttributeRecordProjection(Encoding.UTF8)
                .Scenario()
                .Given(importedRoadSegment)
                .Expect(new object[0]);
        }
    }
}
