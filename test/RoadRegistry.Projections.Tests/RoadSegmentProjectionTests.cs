namespace RoadRegistry.Projections.Tests
{
    using System.Threading.Tasks;
    using AutoFixture;
    using Events;
    using Infrastucture;
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
        private readonly Mock<IOrganisationRetreiver> _organisationRetrieverMock;


        public RoadSegmentProjectionTests()
        {
            _fixture = new ScenarioFixture();
            _segmentStatusTranslator = new RoadSegmentStatusTranslator();
            _morphologyTranslator = new RoadSegmentMorphologyTranslator();
            _categoryTranslator = new RoadSegmentCategoryTranslator();
            _geometryDrawMethodTranslator = new RoadSegmentGeometryDrawMethodTranslator();
            _accessRestrictionTranslator = new RoadSegmentAccessRestrictionTranslator();
            _organisationRetrieverMock = new Mock<IOrganisationRetreiver>();
        }

        [Fact]
        public Task When_a_road_segment_was_imported()
        {

            var multiLineString = _fixture.Create<MultiLineString>();
            var polyLineMShapeContent = new PolyLineMShapeContent(multiLineString);
            var geometry = _fixture
                .Build<Events.VersionedGeometry>()
                .With(g => g.WellKnownBinary, multiLineString.SerializeByteArray<WkbSerializer>())
                .Create();
            var importedRoadSegment = _fixture
                .Build<ImportedRoadSegment>()
                .With(segment => segment.Geometry, geometry)
                .Create();

            var organisation = _fixture.Build<Organisation>()
                .With(o => o.Id, importedRoadSegment.MaintainerId)
                .Create();

            _organisationRetrieverMock
                .Setup(retreiver => retreiver.Get(importedRoadSegment.MaintainerId))
                .ReturnsAsync(organisation);
            var organisationLookup = _organisationRetrieverMock.Object;

            return new RoadSegmentRecordProjection(organisationLookup).Scenario()
                .Given(importedRoadSegment)
                .Expect(new object[]
                {
                    new RoadSegmentRecord
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
                            LBLBEHEER = { Value = organisationLookup.Get(importedRoadSegment.MaintainerId).Result.Name },
                            METHODE = { Value = _geometryDrawMethodTranslator.TranslateToIdentifier(importedRoadSegment.GeometryDrawMethod) },
                            LBLMETHOD = { Value = _geometryDrawMethodTranslator.TranslateToDutchName(importedRoadSegment.GeometryDrawMethod) },
                            OPNDATUM = { Value = importedRoadSegment.RecordingDate },
                            BEGINTIJD = { Value = importedRoadSegment.Origin.Since },
                            BEGINORG = { Value = importedRoadSegment.Origin.OrganizationId },
                            LBLBGNORG = { Value = importedRoadSegment.Origin.Organization },
                            TGBEP = { Value = _accessRestrictionTranslator.TranslateToIdentifier(importedRoadSegment.AccessRestriction) },
                            LBLTGBEP = { Value = _accessRestrictionTranslator.TranslateToDutchName(importedRoadSegment.AccessRestriction) }
                        }.ToBytes(),
                    }
                });
        }
    }
}
