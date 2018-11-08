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

    public class RoadSegmentSurfaceAttributeProjectionTests
    {
        private readonly ScenarioFixture _fixture;
        private readonly SurfaceTypeTranslator _surfaceTypeTranslator;

        public RoadSegmentSurfaceAttributeProjectionTests()
        {
            _fixture = new ScenarioFixture();
            _surfaceTypeTranslator = new SurfaceTypeTranslator();
        }

        [Fact]
        public Task When_importing_road_nodes()
        {
            var random = new Random();
            var data = _fixture
                .CreateMany<ImportedRoadSegment>(random.Next(1, 3))
                .Select(segment =>
                {
                    segment.Surfaces = _fixture
                        .CreateMany<ImportedRoadSegmentSurfaceAttributes>(random.Next(1, 5))
                        .ToArray();

                    var expected = segment
                        .Surfaces
                        .Select(surface => new RoadSegmentSurfaceAttributeRecord
                        {
                            Id = surface.AttributeId,
                            RoadSegmentId = segment.Id,
                            DbaseRecord = new RoadSegmentSurfaceAttributeDbaseRecord
                            {
                                WV_OIDN = { Value = surface.AttributeId },
                                WS_OIDN = { Value = segment.Id },
                                WS_GIDN = { Value = segment.Id + "_" + surface.AsOfGeometryVersion },
                                TYPE =  { Value = _surfaceTypeTranslator.TranslateToIdentifier(surface.Type) },
                                LBLTYPE =  { Value = _surfaceTypeTranslator.TranslateToDutchName(surface.Type) },
                                VANPOS = { Value = (double)surface.FromPosition },
                                TOTPOS = { Value = (double)surface.ToPosition },
                                BEGINTIJD = { Value = surface.Origin.Since },
                                BEGINORG = { Value = surface.Origin.OrganizationId },
                                LBLBGNORG = { Value = surface.Origin.Organization },
                            }.ToBytes(Encoding.UTF8)
                        });

                    return new
                    {
                        importedRoadSegment = segment,
                        expected,
                    };

                }).ToList();

            return new RoadSegmentSurfaceAttributeRecordProjection(_surfaceTypeTranslator, Encoding.UTF8)
                .Scenario()
                .Given(data.Select(d => d.importedRoadSegment))
                .Expect(data
                    .SelectMany(d => d.expected)
                    .Cast<object>()
                    .ToArray()
                );
        }

        [Fact]
        public Task When_importing_a_road_node_without_surfaces()
        {
            var importedRoadSegment = _fixture.Create<ImportedRoadSegment>();
            importedRoadSegment.Surfaces = new ImportedRoadSegmentSurfaceAttributes[0];

            return new RoadSegmentSurfaceAttributeRecordProjection(_surfaceTypeTranslator, Encoding.UTF8)
                .Scenario()
                .Given(importedRoadSegment)
                .Expect(new object[0]);
        }
    }
}
