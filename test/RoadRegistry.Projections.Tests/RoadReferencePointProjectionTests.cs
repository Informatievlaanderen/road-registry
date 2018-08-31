namespace RoadRegistry.Projections.Tests
{
    using System;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using AutoFixture;
    using Events;
    using GeoAPI.Geometries;
    using Infrastructure;
    using NetTopologySuite;
    using NetTopologySuite.Geometries;
    using NetTopologySuite.IO;
    using Shaperon;
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
                .Select(point =>
                {
                    var pointShapeContent = new PointShapeContent(point);

                    var importedReferencePoint = _fixture
                        .Build<ImportedReferencePoint>()
                        .With(referencePoint => referencePoint.Geometry, point.ToBinary())
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
                            LBLBGNORG = { Value = importedReferencePoint.Origin.Organization },
                        }.ToBytes(Encoding.UTF8),
                    };

                    return new {
                        ImportedReferencePoint = importedReferencePoint,
                        Expected = expected
                    };
                }).ToList();

            return new RoadReferencePointRecordProjection(
                    new WellKnownBinaryReader(),
                    _referencePointTypeTranslator,
                    Encoding.UTF8)
                .Scenario()
                .Given(data.Select(d => d.ImportedReferencePoint))
                .Expect(data.Select(d => d.Expected).ToArray());
        }
    }
}
