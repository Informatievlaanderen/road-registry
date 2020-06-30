namespace RoadRegistry.Wms.Projections
{
    using System;
    using System.Threading.Tasks;
    using AutoFixture;
    using BackOffice;
    using BackOffice.Messages;
    using Framework;
    using RoadRegistry.Framework.Projections;
    using RoadRegistry.Projections;
    using Schema;
    using Xunit;

    public class RoadSegmentRecordProjectionTests
    {
        private readonly Fixture _fixture;
        private readonly TestDataHelper _testDataHelper;

        public RoadSegmentRecordProjectionTests()
        {
            _testDataHelper = new TestDataHelper();

            _fixture = new Fixture();
            _fixture.CustomizeArchiveId();
            _fixture.CustomizeAttributeId();
            _fixture.CustomizeRoadSegmentId();
            _fixture.CustomizeRoadNodeId();
            _fixture.CustomizeOrganizationId();
            _fixture.CustomizeOrganizationName();
            _fixture.CustomizePolylineM();
            _fixture.CustomizeEuropeanRoadNumber();
            _fixture.CustomizeNationalRoadNumber();
            _fixture.CustomizeNumberedRoadNumber();
            _fixture.CustomizeRoadSegmentNumberedRoadDirection();
            _fixture.CustomizeRoadSegmentNumberedRoadOrdinal();
            _fixture.CustomizeRoadSegmentLaneCount();
            _fixture.CustomizeRoadSegmentLaneDirection();
            _fixture.CustomizeRoadSegmentWidth();
            _fixture.CustomizeRoadSegmentSurfaceType();
            _fixture.CustomizeRoadSegmentGeometryDrawMethod();
            _fixture.CustomizeRoadSegmentMorphology();
            _fixture.CustomizeRoadSegmentStatus();
            _fixture.CustomizeRoadSegmentCategory();
            _fixture.CustomizeRoadSegmentAccessRestriction();
            _fixture.CustomizeRoadSegmentGeometryVersion();
            _fixture.CustomizeTransactionId();

            _fixture.CustomizeImportedRoadSegment();
            _fixture.CustomizeRoadSegmentAdded();
        }

        [Theory]
        [InlineData(904)]
        [InlineData(458)]
        [InlineData(4)]
        public async Task When_importing_road_segments(int wegSegmentId)
        {
            var importedRoadSegment = await _testDataHelper.EventFromFileAsync<ImportedRoadSegment>(wegSegmentId);

            var expectedGeometry2D = _testDataHelper.ExpectedGeometry2D(wegSegmentId);

            var expectedRoadSegment = _testDataHelper.ExpectedRoadSegment(wegSegmentId);

            await new RoadSegmentRecordProjection()
                .Scenario()
                .Given(importedRoadSegment)
                .Expect(new RoadSegmentRecord
                {
                    Id = expectedRoadSegment.wegsegmentID,
                    BeginOperator = expectedRoadSegment.beginoperator,
                    BeginOrganizationId = expectedRoadSegment.beginorganisatie,
                    BeginTime = expectedRoadSegment.begintijd,
                    BeginApplication = expectedRoadSegment.beginapplicatie,

                    MaintainerId = expectedRoadSegment.beheerder,
                    MaintainerName = expectedRoadSegment.lblBeheerder,

                    MethodId = expectedRoadSegment.methode,
                    MethodDutchName = expectedRoadSegment.lblMethode,

                    CategoryId = expectedRoadSegment.categorie,
                    CategoryDutchName = expectedRoadSegment.lblCategorie,

                    Geometry2D = expectedGeometry2D,
                    GeometryVersion = expectedRoadSegment.geometrieversie,

                    MorphologyId = expectedRoadSegment.morfologie,
                    MorphologyDutchName = expectedRoadSegment.lblMorfologie,

                    StatusId = expectedRoadSegment.status,
                    StatusDutchName = expectedRoadSegment.lblStatus,

                    AccessRestrictionId = expectedRoadSegment.toegangsbeperking,
                    AccessRestrictionDutchName = expectedRoadSegment.lblToegangsbeperking,

                    BeginOrganizationName = expectedRoadSegment.lblOrganisatie,
                    RecordingDate = expectedRoadSegment.opnamedatum,

                    TransactionId = expectedRoadSegment.transactieID,

                    LeftSideMunicipalityId = null,
                    LeftSideStreetNameId = expectedRoadSegment.linksStraatnaamID,
                    LeftSideStreetName = expectedRoadSegment.linksStraatnaam,

                    RightSideMunicipalityId = null,
                    RightSideStreetNameId = expectedRoadSegment.linksStraatnaamID,
                    RightSideStreetName = expectedRoadSegment.linksStraatnaam,

                    RoadSegmentVersion = expectedRoadSegment.wegsegmentversie,
                    BeginRoadNodeId = expectedRoadSegment.beginWegknoopID,
                    EndRoadNodeId = expectedRoadSegment.eindWegknoopID,
                });
        }

        [Fact]
        public Task When_adding_road_segments()
        {
            var message = _fixture.Create<RoadNetworkChangesAccepted>();
            var expectedRecords = Array.ConvertAll(message.Changes, change =>
            {
                var segment = change.RoadSegmentAdded;
                return (object)new RoadSegmentRecord
                {
                    Id = segment.Id,
                    BeginOperator = message.Operator,
                    BeginOrganizationId = message.OrganizationId,
                    BeginOrganizationName = message.Organization,
                    BeginTime = LocalDateTimeTranslator.TranslateFromWhen(message.When),
                    BeginApplication = null,

                    MaintainerId = segment.MaintenanceAuthority.Code,
                    MaintainerName = segment.MaintenanceAuthority.Name,

                    MethodId = RoadSegmentGeometryDrawMethod.Parse(segment.GeometryDrawMethod).Translation.Identifier,
                    MethodDutchName = RoadSegmentGeometryDrawMethod.Parse(segment.GeometryDrawMethod).Translation.Name,

                    CategoryId = RoadSegmentCategory.Parse(segment.Category).Translation.Identifier,
                    CategoryDutchName = RoadSegmentCategory.Parse(segment.Category).Translation.Name,

                    Geometry2D = WmsGeometryTranslator.Translate2D(segment.Geometry),
                    GeometryVersion = segment.GeometryVersion,

                    MorphologyId = RoadSegmentMorphology.Parse(segment.Morphology).Translation.Identifier,
                    MorphologyDutchName = RoadSegmentMorphology.Parse(segment.Morphology).Translation.Name,

                    StatusId = RoadSegmentStatus.Parse(segment.Status).Translation.Identifier,
                    StatusDutchName = RoadSegmentStatus.Parse(segment.Status).Translation.Name,

                    AccessRestrictionId = RoadSegmentAccessRestriction.Parse(segment.AccessRestriction).Translation.Identifier,
                    AccessRestrictionDutchName = RoadSegmentAccessRestriction.Parse(segment.AccessRestriction).Translation.Name,

                    RecordingDate = LocalDateTimeTranslator.TranslateFromWhen(message.When),

                    TransactionId = message.TransactionId,

                    LeftSideMunicipalityId = null,
                    LeftSideStreetNameId = segment.LeftSide.StreetNameId,
                    LeftSideStreetName = null,

                    RightSideMunicipalityId = null,
                    RightSideStreetNameId = segment.RightSide.StreetNameId,
                    RightSideStreetName = null,

                    RoadSegmentVersion = segment.Version,
                    BeginRoadNodeId = segment.StartNodeId,
                    EndRoadNodeId = segment.EndNodeId,
                };
            });

            return new RoadSegmentRecordProjection()
                .Scenario()
                .Given(message)
                .Expect(expectedRecords);
        }
    }
}
