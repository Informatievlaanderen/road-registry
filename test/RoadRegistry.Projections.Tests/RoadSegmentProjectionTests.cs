namespace RoadRegistry.Projections.Tests
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoFixture;
    using Events;
    using Infrastructure;
    using Shaperon;
    using Wkx;
    using Xunit;
    using Moq;

    public class RoadSegmentProjectionTests
    {
        private readonly ScenarioFixture _fixture;
        private readonly RoadSegmentStatusTranslator _segmentStatusTranslator;
        private readonly RoadSegmentMorphologyTranslator _morphologyTranslator;
        private readonly RoadSegmentCategoryTranslator _categoryTranslator;
        private readonly RoadSegmentGeometryDrawMethodTranslator _geometryDrawMethodTranslator;
        private readonly RoadSegmentAccessRestrictionTranslator _accessRestrictionTranslator;
        private readonly Mock<IOrganizationRetriever> _organizationRetrieverMock;


        public RoadSegmentProjectionTests()
        {
            _fixture = new ScenarioFixture();
            _segmentStatusTranslator = new RoadSegmentStatusTranslator();
            _morphologyTranslator = new RoadSegmentMorphologyTranslator();
            _categoryTranslator = new RoadSegmentCategoryTranslator();
            _geometryDrawMethodTranslator = new RoadSegmentGeometryDrawMethodTranslator();
            _accessRestrictionTranslator = new RoadSegmentAccessRestrictionTranslator();
            _organizationRetrieverMock = new Mock<IOrganizationRetriever>();
        }

        [Fact]
        public Task When_road_segments_are_imported()
        {
            var data = _fixture
                .CreateMany<MultiLineString>(new Random().Next(1, 10))
                .Select((multiLineString, index) =>
                {
                    var polyLineMShapeContent = new PolyLineMShapeContent(multiLineString);
                    var geometry = _fixture
                        .Build<VersionedGeometry>()
                        .With(g => g.WellKnownBinary, multiLineString.SerializeByteArray<WkbSerializer>())
                        .Create();
                    var importedRoadSegment = _fixture
                        .Build<ImportedRoadSegment>()
                        .With(segment => segment.Id, index + 1)
                        .With(segment => segment.Geometry, geometry)
                        .Create();

                    var organization = _fixture.Build<Organization>()
                        .With(o => o.Id, importedRoadSegment.MaintainerId)
                        .Create();

                    _organizationRetrieverMock
                        .Setup(retriever => retriever.Get(importedRoadSegment.MaintainerId))
                        .Returns(organization);

                    var expected = new RoadSegmentRecord
                    {
                        Id = importedRoadSegment.Id,
                        ShapeRecordContent = polyLineMShapeContent.ToBytes(),
                        ShapeRecordContentLength = polyLineMShapeContent.ContentLength.ToInt32(),
                        DbaseRecord = new RoadSegmentDbaseRecord
                        {
                            WS_OIDN = { Value = importedRoadSegment.Id },
                            WS_UIDN = { Value = importedRoadSegment.Id + "_" + importedRoadSegment.Version },
                            WS_GIDN = { Value = importedRoadSegment.Id + "_" + importedRoadSegment.Geometry.Version },
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
                            BEHEER = { Value = importedRoadSegment.MaintainerId },
                            LBLBEHEER = { Value = organization.Name },
                            METHODE = { Value = _geometryDrawMethodTranslator.TranslateToIdentifier(importedRoadSegment.GeometryDrawMethod) },
                            LBLMETHOD = { Value = _geometryDrawMethodTranslator.TranslateToDutchName(importedRoadSegment.GeometryDrawMethod) },
                            OPNDATUM = { Value = importedRoadSegment.RecordingDate },
                            BEGINTIJD = { Value = importedRoadSegment.Origin.Since },
                            BEGINORG = { Value = importedRoadSegment.Origin.OrganizationId },
                            LBLBGNORG = { Value = importedRoadSegment.Origin.Organization },
                            TGBEP = { Value = _accessRestrictionTranslator.TranslateToIdentifier(importedRoadSegment.AccessRestriction) },
                            LBLTGBEP = { Value = _accessRestrictionTranslator.TranslateToDutchName(importedRoadSegment.AccessRestriction) }
                        }.ToBytes(),
                    };
                    return new {importedRoadSegment, expected};
                }).ToList();

            return new RoadSegmentRecordProjection(_organizationRetrieverMock.Object).Scenario()
                .Given(data.Select(d => d.importedRoadSegment))
                .Expect(data.Select(d => d.expected).ToArray());
        }
    }
}
