namespace RoadRegistry.Projections.Tests
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoFixture;
    using Events;
    using Infrastructure;
    using Xunit;

    public class RoadSegmentNumberedRoadAttributeProjectionTests
    {
        private readonly ScenarioFixture _fixture;
        private readonly NumberedRoadSegmentDirectionTranslator _numberedRoadSegmentDirectionTranslator;

        public RoadSegmentNumberedRoadAttributeProjectionTests()
        {
            _fixture = new ScenarioFixture();
            _numberedRoadSegmentDirectionTranslator = new NumberedRoadSegmentDirectionTranslator();
        }

        [Fact]
        public Task When_importing_road_nodes()
        {
            var random = new Random();
            var data = _fixture
                .CreateMany<ImportedRoadSegment>(random.Next(1, 3))
                .Select(segment =>
                {
                    segment.PartOfNumberedRoads = _fixture
                        .CreateMany<RoadSegmentNumberedRoadProperties>(random.Next(1, 5))
                        .ToArray();

                    var expected = segment
                        .PartOfNumberedRoads
                        .Select(numberedRoad => new RoadSegmentNumberedRoadAttributeRecord
                        {
                            Id = numberedRoad.AttributeId,
                            RoadSegmentId = segment.Id,
                            DbaseRecord = new RoadSegmentNumberedRoadAttributeDbaseRecord
                            {
                                GW_OIDN = { Value = numberedRoad.AttributeId },
                                WS_OIDN = { Value = segment.Id },
                                IDENT8 = { Value = numberedRoad.Ident8 },
                                RICHTING = { Value = _numberedRoadSegmentDirectionTranslator.TranslateToIdentifier(numberedRoad.Direction) },
                                LBLRICHT = { Value = _numberedRoadSegmentDirectionTranslator.TranslateToDutchName(numberedRoad.Direction) },
                                VOLGNUMMER = { Value = numberedRoad.Ordinal },
                                BEGINTIJD = { Value = numberedRoad.Origin.Since },
                                BEGINORG = { Value = numberedRoad.Origin.OrganizationId },
                                LBLBGNORG = { Value = numberedRoad.Origin.Organization },
                            }.ToBytes()
                        });

                    return new
                    {
                        importedRoadSegment = segment,
                        expected,
                    };

                }).ToList();

            return new RoadSegmentNumberedRoadAttributeRecordProjection(_numberedRoadSegmentDirectionTranslator)
                .Scenario()
                .Given(data.Select(d => d.importedRoadSegment))
                .Expect(data
                    .SelectMany(d => d.expected)
                    .Cast<object>()
                    .ToArray()
                );
        }

        [Fact]
        public Task When_importing_a_road_node_without_numbered_road_links()
        {
            var importedRoadSegment = _fixture.Create<ImportedRoadSegment>();
            importedRoadSegment.PartOfNumberedRoads = new RoadSegmentNumberedRoadProperties[0];

            return new RoadSegmentNumberedRoadAttributeRecordProjection(_numberedRoadSegmentDirectionTranslator)
                .Scenario()
                .Given(importedRoadSegment)
                .Expect(new object[0]);
        }
    }
}
