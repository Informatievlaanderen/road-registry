namespace RoadRegistry.Projections.Tests
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoFixture;
    using Events;
    using Infrastucture;
    using Shaperon;
    using Wkx;
    using Xunit;

    public class RoadReferencePointProjectionTests
    {
        private readonly ScenarioFixture _fixture;
        private readonly ReferencePointTypeTranslator _referencePointTypeTranslator;

        public RoadReferencePointProjectionTests()
        {
            _fixture = new ScenarioFixture();
            _referencePointTypeTranslator = new ReferencePointTypeTranslator();
        }

        [Fact]
        public Task When_a_road_reference_points_were_imported()
        {
            var data = _fixture
                .CreateMany<Point>(new Random().Next(1,10))
                .Select((point, i) =>
                {
                    var pointShapeContent = new PointShapeContent(point);
                    var geometry = _fixture.Build<Events.Geometry>()
                        .With(g => g.WellKnownBinary, point.SerializeByteArray<WkbSerializer>())
                        .Create();

                    var importedReferencePoint = _fixture
                        .Build<ImportedReferencePoint>()
                        .With(referencePoint => referencePoint.Id, i + 124)
                        .With(referencePoint => referencePoint.Geometry, geometry)
                        .Create();

                    var expected = new RoadReferencePointRecord
                    {
                        Id = importedReferencePoint.Id,
                        ShapeRecordContent = pointShapeContent.ToBytes(),
                        ShapeRecordContentLength = pointShapeContent.ContentLength.ToInt32(),
                        DbaseRecord = new RoadReferencePointDbaseRecord
                        {
                            RP_OIDN = { Value = importedReferencePoint.Id },
                            RP_UIDN = { Value = importedReferencePoint.Id + "_" + importedReferencePoint.Version },
                            IDENT8 = { Value = importedReferencePoint.Ident8 },
                            OPSCHRIFT = { Value = importedReferencePoint.Caption },
                            TYPE = { Value = _referencePointTypeTranslator.TranslateToIdentifier(importedReferencePoint.Type) },
                            LBLTYPE = { Value = _referencePointTypeTranslator.TranslateToDutchName(importedReferencePoint.Type) },
                            BEGINTIJD = { Value = importedReferencePoint.Origin.Since },
                            BEGINORG = { Value = importedReferencePoint.Origin.OrganizationId },
                            LBLBEGINORG = { Value = importedReferencePoint.Origin.Organization },
                        }.ToBytes(),
                    };

                    return new { importedReferencePoint, expected };
                }).ToList();

            return new RoadReferencePointRecordProjection().Scenario()
                .Given(data.Select(d => d.importedReferencePoint))
                .Expect(data.Select(d => d.expected).ToArray());
        }
    }
}
