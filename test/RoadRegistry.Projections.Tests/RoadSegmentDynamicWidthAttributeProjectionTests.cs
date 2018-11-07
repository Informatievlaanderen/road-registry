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
                        .CreateMany<ImportedRoadSegmentWidthProperties>(random.Next(1, 5))
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
                                WS_GIDN = { Value = segment.Id + "_" + width.AsOfGeometryVersion },
                                BREEDTE =  { Value = width.Width },
                                VANPOS = { Value = (double)width.FromPosition },
                                TOTPOS = { Value = (double)width.ToPosition },
                                BEGINTIJD = { Value = width.Origin.Since },
                                BEGINORG = { Value = width.Origin.OrganizationId },
                                LBLBGNORG = { Value = width.Origin.Organization },
                            }.ToBytes(Encoding.UTF8)
                        });

                    return new
                    {
                        importedRoadSegment = segment,
                        expected,
                    };

                }).ToList();

            return new RoadSegmentDynamicWidthAttributeRecordProjection(Encoding.UTF8)
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
            importedRoadSegment.Widths = new ImportedRoadSegmentWidthProperties[0];

            return new RoadSegmentDynamicWidthAttributeRecordProjection(Encoding.UTF8)
                .Scenario()
                .Given(importedRoadSegment)
                .Expect(new object[0]);
        }
    }
}
