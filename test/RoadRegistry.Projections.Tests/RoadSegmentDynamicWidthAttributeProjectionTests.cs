namespace RoadRegistry.Projections.Tests
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoFixture;
    using Events;
    using Infrastructure;
    using Xunit;

    public class RoadSegmentDynamicWidthAttributeProjectionTests
    {
        private readonly ScenarioFixture _fixture;

        public RoadSegmentDynamicWidthAttributeProjectionTests()
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
                    segment.Widths = _fixture
                        .CreateMany<RoadSegmentWidthProperties>(random.Next(1, 5))
                        .ToArray();

                    var expected = segment
                        .Widths
                        .Select(width => new RoadSegmentDynamicWidthAttributeRecord
                        {
                            Id = width.AttributeId,
                            RoadSegmentId = segment.Id,
                            DbaseRecord = new RoadSegmentDynamicWidthAttributeDbaseRecord
                            {
                                WB_OIDN = { Value = width.AttributeId },
                                WS_OIDN = { Value = segment.Id },
                                WS_GIDN = { Value = segment.Id + "_" + segment.GeometryVersion },
                                BREEDTE =  { Value = width.Width },
                                VANPOS = { Value = (double)width.FromPosition },
                                TOTPOS = { Value = (double)width.ToPosition },
                                BEGINTIJD = { Value = width.Origin.Since },
                                BEGINORG = { Value = width.Origin.OrganizationId },
                                LBLBGNORG = { Value = width.Origin.Organization },
                            }.ToBytes()
                        });

                    return new
                    {
                        importedRoadSegment = segment,
                        expected,
                    };

                }).ToList();

            return new RoadSegmentDynamicWidthAttributeProjection()
                .Scenario()
                .Given(data.Select(d => d.importedRoadSegment))
                .Expect(data
                    .SelectMany(d => d.expected)
                    .Cast<object>()
                    .ToArray()
                );
        }

        [Fact]
        public Task When_importing_a_road_node_without_widths()
        {
            var importedRoadSegment = _fixture.Create<ImportedRoadSegment>();
            importedRoadSegment.Widths = new RoadSegmentWidthProperties[0];

            return new RoadSegmentDynamicWidthAttributeProjection()
                .Scenario()
                .Given(importedRoadSegment)
                .Expect(new object[0]);
        }
    }
}
