namespace RoadRegistry.Projections.Tests
{
    using System;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using AutoFixture;
    using Events;
    using Xunit;

    public class RoadSegmentDynamicLaneAttributeProjectionTests
    {
        private readonly ScenarioFixture _fixture;
        private readonly LaneDirectionTranslator _laneDirectionTranslator;

        public RoadSegmentDynamicLaneAttributeProjectionTests()
        {
            _fixture = new ScenarioFixture();
            _laneDirectionTranslator = new LaneDirectionTranslator();
        }

        [Fact]
        public Task When_importing_road_nodes()
        {
            var random = new Random();
            var data = _fixture
                .CreateMany<ImportedRoadSegment>(random.Next(1, 3))
                .Select(segment =>
                {
                    segment.Lanes = _fixture
                        .CreateMany<RoadSegmentLaneProperties>(random.Next(1, 5))
                        .ToArray();

                    var expected = segment
                        .Lanes
                        .Select(lane => new RoadSegmentDynamicLaneAttributeRecord
                        {
                            Id = lane.AttributeId,
                            RoadSegmentId = segment.Id,
                            DbaseRecord = new RoadSegmentDynamicLaneAttributeDbaseRecord
                            {
                                RS_OIDN = { Value = lane.AttributeId },
                                WS_OIDN = { Value = segment.Id },
                                WS_GIDN = { Value = segment.Id + "_" + lane.AsOfGeometryVersion },
                                AANTAL =  { Value = lane.Count },
                                RICHTING = { Value = _laneDirectionTranslator.TranslateToIdentifier(lane.Direction) },
                                LBLRICHT = { Value = _laneDirectionTranslator.TranslateToDutchName(lane.Direction) },
                                VANPOS = { Value = (double)lane.FromPosition },
                                TOTPOS = { Value = (double)lane.ToPosition },
                                BEGINTIJD = { Value = lane.Origin.Since },
                                BEGINORG = { Value = lane.Origin.OrganizationId },
                                LBLBGNORG = { Value = lane.Origin.Organization },
                            }.ToBytes(Encoding.UTF8)
                        });

                    return new
                    {
                        importedRoadSegment = segment,
                        expected,
                    };

                }).ToList();

            return new RoadSegmentDynamicLaneAttributeRecordProjection(_laneDirectionTranslator, Encoding.UTF8)
                .Scenario()
                .Given(data.Select(d => d.importedRoadSegment))
                .Expect(data
                    .SelectMany(d => d.expected)
                    .Cast<object>()
                    .ToArray()
                );
        }

        [Fact]
        public Task When_importing_a_road_node_without_lanes()
        {
            var importedRoadSegment = _fixture.Create<ImportedRoadSegment>();
            importedRoadSegment.Lanes = new RoadSegmentLaneProperties[0];

            return new RoadSegmentDynamicLaneAttributeRecordProjection(_laneDirectionTranslator, Encoding.UTF8)
                .Scenario()
                .Given(importedRoadSegment)
                .Expect(new object[0]);
        }
    }
}
