namespace RoadRegistry.Projections.Tests
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoFixture;
    using Events;
    using Infrastructure;
    using Aiv.Vbr.Shaperon;
    using Xunit;
    using NetTopologySuite.Geometries;
    using System.Text;

    public class RoadSegmentProjectionTests
    {
        private readonly ScenarioFixture _fixture;
        private readonly RoadSegmentStatusTranslator _segmentStatusTranslator;
        private readonly RoadSegmentMorphologyTranslator _morphologyTranslator;
        private readonly RoadSegmentCategoryTranslator _categoryTranslator;
        private readonly RoadSegmentGeometryDrawMethodTranslator _geometryDrawMethodTranslator;
        private readonly RoadSegmentAccessRestrictionTranslator _accessRestrictionTranslator;


        public RoadSegmentProjectionTests()
        {
            _fixture = new ScenarioFixture();
            _segmentStatusTranslator = new RoadSegmentStatusTranslator();
            _morphologyTranslator = new RoadSegmentMorphologyTranslator();
            _categoryTranslator = new RoadSegmentCategoryTranslator();
            _geometryDrawMethodTranslator = new RoadSegmentGeometryDrawMethodTranslator();
            _accessRestrictionTranslator = new RoadSegmentAccessRestrictionTranslator();
        }

        [Fact]
        public Task When_road_segments_are_imported()
        {
            var wkbWriter = new WellKnownBinaryWriter();
            var data = _fixture
                .CreateMany<MultiLineString>(new Random().Next(1, 10))
                .Select(multiLineString =>
                {
                    var polyLineMShapeContent = new PolyLineMShapeContent(multiLineString);
                    var importedRoadSegment = _fixture
                        .Build<ImportedRoadSegment>()
                        .With(segment => segment.Geometry, wkbWriter.Write(multiLineString))
                        .Create();

                    var expected = new RoadSegmentRecord
                    {
                        Id = importedRoadSegment.Id,
                        ShapeRecordContent = polyLineMShapeContent.ToBytes(),
                        ShapeRecordContentLength = polyLineMShapeContent.ContentLength.ToInt32(),
                        DbaseRecord = new RoadSegmentDbaseRecord
                        {
                            WS_OIDN = { Value = importedRoadSegment.Id },
                            WS_UIDN = { Value = importedRoadSegment.Id + "_" + importedRoadSegment.Version },
                            WS_GIDN = { Value = importedRoadSegment.Id + "_" + importedRoadSegment.GeometryVersion },
                            B_WK_OIDN = { Value = importedRoadSegment.StartNodeId },
                            E_WK_OIDN = { Value = importedRoadSegment.EndNodeId },
                            STATUS = { Value = _segmentStatusTranslator.TranslateToIdentifier(importedRoadSegment.Status) },
                            LBLSTATUS = { Value = _segmentStatusTranslator.TranslateToDutchName(importedRoadSegment.Status) },
                            MORF = { Value = _morphologyTranslator.TranslateToIdentifier(importedRoadSegment.Morphology) },
                            LBLMORF = { Value = _morphologyTranslator.TranslateToDutchName(importedRoadSegment.Morphology) },
                            WEGCAT = { Value = _categoryTranslator.TranslateToIdentifier(importedRoadSegment.Category) },
                            LBLWEGCAT = { Value = _categoryTranslator.TranslateToDutchName(importedRoadSegment.Category) },
                            LSTRNMID = { Value = importedRoadSegment.LeftSide.StreetNameId },
                            LSTRNM = { Value = importedRoadSegment.LeftSide.StreetName },
                            RSTRNMID = { Value = importedRoadSegment.RightSide.StreetNameId },
                            RSTRNM = { Value = importedRoadSegment.RightSide.StreetName },
                            BEHEER = { Value = importedRoadSegment.Maintainer.Code },
                            LBLBEHEER = { Value = importedRoadSegment.Maintainer.Name },
                            METHODE = { Value = _geometryDrawMethodTranslator.TranslateToIdentifier(importedRoadSegment.GeometryDrawMethod) },
                            LBLMETHOD = { Value = _geometryDrawMethodTranslator.TranslateToDutchName(importedRoadSegment.GeometryDrawMethod) },
                            OPNDATUM = { Value = importedRoadSegment.RecordingDate },
                            BEGINTIJD = { Value = importedRoadSegment.Origin.Since },
                            BEGINORG = { Value = importedRoadSegment.Origin.OrganizationId },
                            LBLBGNORG = { Value = importedRoadSegment.Origin.Organization },
                            TGBEP = { Value = _accessRestrictionTranslator.TranslateToIdentifier(importedRoadSegment.AccessRestriction) },
                            LBLTGBEP = { Value = _accessRestrictionTranslator.TranslateToDutchName(importedRoadSegment.AccessRestriction) }
                        }.ToBytes(Encoding.UTF8),
                    };
                    return new {importedRoadSegment, expected};
                }).ToList();

            return new RoadSegmentRecordProjection(
                    new WellKnownBinaryReader(),
                    _segmentStatusTranslator,
                    _morphologyTranslator,
                    _categoryTranslator,
                    _geometryDrawMethodTranslator,
                    _accessRestrictionTranslator,
                    Encoding.UTF8)
                .Scenario()
                .Given(data.Select(d => d.importedRoadSegment))
                .Expect(data.Select(d => d.expected).ToArray());
        }
    }
}
