namespace RoadRegistry.Projections.Tests
{
    using System;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using AutoFixture;
    using Infrastructure;
    using Messages;
    using Model;
    using Xunit;

    public class RoadSegmentLaneAttributeProjectionTests
    {
        private readonly ScenarioFixture _fixture;

        public RoadSegmentLaneAttributeProjectionTests()
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
                    segment.Lanes = _fixture
                        .CreateMany<ImportedRoadSegmentLaneAttributes>(random.Next(1, 5))
                        .ToArray();

                    var expected = segment
                        .Lanes
                        .Select(lane => new RoadSegmentLaneAttributeRecord
                        {
                            Id = lane.AttributeId,
                            RoadSegmentId = segment.Id,
                            DbaseRecord = new RoadSegmentLaneAttributeDbaseRecord
                            {
                                RS_OIDN = { Value = lane.AttributeId },
                                WS_OIDN = { Value = segment.Id },
                                WS_GIDN = { Value = segment.Id + "_" + lane.AsOfGeometryVersion },
                                AANTAL =  { Value = lane.Count },
                                RICHTING = { Value = RoadSegmentLaneDirection.Parse(lane.Direction).Translation.Identifier },
                                LBLRICHT = { Value = RoadSegmentLaneDirection.Parse(lane.Direction).Translation.Name },
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

            return new RoadSegmentLaneAttributeRecordProjection(Encoding.UTF8)
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
            importedRoadSegment.Lanes = new ImportedRoadSegmentLaneAttributes[0];

            return new RoadSegmentLaneAttributeRecordProjection(Encoding.UTF8)
                .Scenario()
                .Given(importedRoadSegment)
                .Expect(new object[0]);
        }
    }
}
