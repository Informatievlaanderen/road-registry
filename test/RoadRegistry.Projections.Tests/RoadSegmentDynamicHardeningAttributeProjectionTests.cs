namespace RoadRegistry.Projections.Tests
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoFixture;
    using Events;
    using Infrastructure;
    using Xunit;

    public class RoadSegmentDynamicHardeningAttributeProjectionTests
    {
        private readonly ScenarioFixture _fixture;
        private readonly HardeningTypeTranslator _hardeningTypeTranslator;

        public RoadSegmentDynamicHardeningAttributeProjectionTests()
        {
            _fixture = new ScenarioFixture();
            _hardeningTypeTranslator = new HardeningTypeTranslator();
        }

        [Fact]
        public Task When_importing_road_nodes()
        {
            var random = new Random();
            var data = _fixture
                .CreateMany<ImportedRoadSegment>(random.Next(1, 3))
                .Select(segment =>
                {
                    segment.Hardenings = _fixture
                        .CreateMany<RoadSegmentHardeningProperties>(random.Next(0, 5))
                        .ToArray();

                    var expected = segment
                        .Hardenings
                        .Select(hardening => new RoadSegmentDynamicHardeningAttributeRecord
                        {
                            Id = hardening.AttributeId,
                            RoadSegmentId = segment.Id,
                            DbaseRecord = new RoadSegmentDynamicHardeningAttributeDbaseRecord
                            {
                                WV_OIDN = { Value = hardening.AttributeId },
                                WS_OIDN = { Value = segment.Id },
                                WS_GIDN = { Value = segment.Id + "_" + segment.GeometryVersion },
                                TYPE =  { Value = _hardeningTypeTranslator.TranslateToIdentifier(hardening.Type) },
                                LBLTYPE =  { Value = _hardeningTypeTranslator.TranslateToDutchName(hardening.Type) },
                                VANPOS = { Value = (double)hardening.FromPosition },
                                TOTPOS = { Value = (double)hardening.ToPosition },
                                BEGINTIJD = { Value = hardening.Origin.Since },
                                BEGINORG = { Value = hardening.Origin.OrganizationId },
                                LBLBGNORG = { Value = hardening.Origin.Organization },
                            }.ToBytes()
                        });

                    return new
                    {
                        importedRoadSegment = segment,
                        expected,
                    };

                }).ToList();

            return new RoadSegmentDynamicHardeningAttributeProjection(_hardeningTypeTranslator)
                .Scenario()
                .Given(data.Select(d => d.importedRoadSegment))
                .Expect(data
                    .SelectMany(d => d.expected)
                    .Cast<object>()
                    .ToArray()
                );
        }
    }
}
