namespace RoadRegistry.Wms.ProjectionHost.Tests.Projections;

using AutoFixture;
using BackOffice;
using BackOffice.Messages;
using Framework;
using RoadRegistry.Tests.BackOffice;
using RoadRegistry.Tests.Framework.Projections;
using Schema;
using Syndication.Schema;
using Wms.Projections;

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
        _fixture.CustomizeRoadSegmentModified();
        _fixture.CustomizeRoadSegmentAttributesModified();
        _fixture.CustomizeRoadSegmentRemoved();
        _fixture.CustomizeRoadNetworkChangesAccepted();
    }

    [Fact]
    public Task When_adding_road_segments()
    {
        var message = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.CreateMany<RoadSegmentAdded>());

        var expectedRecords = Array.ConvertAll(message.Changes, change =>
        {
            var segment = change.RoadSegmentAdded;
            return (object)new RoadSegmentRecord
            {
                Id = segment.Id,
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

                StreetNameCachePosition = -1L
            };
        });

        return new RoadSegmentRecordProjection(new StreetNameCacheStub())
            .Scenario()
            .Given(message)
            .Expect(expectedRecords);
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

        await new RoadSegmentRecordProjection(new StreetNameCacheStub())
            .Scenario()
            .Given(importedRoadSegment)
            .Expect(new RoadSegmentRecord
            {
                Id = expectedRoadSegment.wegsegmentID,
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

                StreetNameCachePosition = -1L
            });
    }

    [Theory]
    [InlineData(904)]
    [InlineData(458)]
    [InlineData(4)]
    public async Task When_importing_road_segments_with_street_name_in_cache(int wegSegmentId)
    {
        var importedRoadSegment = await _testDataHelper.EventFromFileAsync<ImportedRoadSegment>(wegSegmentId);

        var expectedGeometry2D = _testDataHelper.ExpectedGeometry2D(wegSegmentId);

        var expectedRoadSegment = _testDataHelper.ExpectedRoadSegment(wegSegmentId);

        var streetNameRecord = _fixture.Create<StreetNameRecord>();

        var streetNameCachePosition = _fixture.Create<long>();
        var streetNameCacheStub = new StreetNameCacheStub(streetNameRecord, streetNameCachePosition);

        await new RoadSegmentRecordProjection(streetNameCacheStub)
            .Scenario()
            .Given(importedRoadSegment)
            .Expect(new RoadSegmentRecord
            {
                Id = expectedRoadSegment.wegsegmentID,
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
                LeftSideMunicipalityNisCode = streetNameRecord.NisCode,
                LeftSideStreetNameId = expectedRoadSegment.linksStraatnaamID,
                LeftSideStreetName = streetNameRecord.DutchNameWithHomonymAddition,

                RightSideMunicipalityId = null,
                RightSideMunicipalityNisCode = streetNameRecord.NisCode,
                RightSideStreetNameId = expectedRoadSegment.linksStraatnaamID,
                RightSideStreetName = streetNameRecord.DutchNameWithHomonymAddition,

                RoadSegmentVersion = expectedRoadSegment.wegsegmentversie,
                BeginRoadNodeId = expectedRoadSegment.beginWegknoopID,
                EndRoadNodeId = expectedRoadSegment.eindWegknoopID,

                StreetNameCachePosition = streetNameCachePosition
            });
    }

    [Fact]
    public Task When_modifying_road_segments()
    {
        _fixture.Freeze<RoadSegmentId>();

        var acceptedRoadSegmentAdded = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.Create<RoadSegmentAdded>());

        var acceptedRoadSegmentModified = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.Create<RoadSegmentModified>());

        var expectedRecords = Array.ConvertAll(acceptedRoadSegmentModified.Changes, change =>
        {
            var segment = change.RoadSegmentModified;
            return (object)new RoadSegmentRecord
            {
                Id = segment.Id,
                BeginOrganizationId = acceptedRoadSegmentModified.OrganizationId,
                BeginOrganizationName = acceptedRoadSegmentModified.Organization,
                BeginTime = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentModified.When),
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

                RecordingDate = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentAdded.When),

                TransactionId = acceptedRoadSegmentModified.TransactionId,

                LeftSideMunicipalityId = null,
                LeftSideStreetNameId = segment.LeftSide.StreetNameId,
                LeftSideStreetName = null,

                RightSideMunicipalityId = null,
                RightSideStreetNameId = segment.RightSide.StreetNameId,
                RightSideStreetName = null,

                RoadSegmentVersion = segment.Version,
                BeginRoadNodeId = segment.StartNodeId,
                EndRoadNodeId = segment.EndNodeId,

                StreetNameCachePosition = -1L
            };
        });

        return new RoadSegmentRecordProjection(new StreetNameCacheStub())
            .Scenario()
            .Given(acceptedRoadSegmentAdded, acceptedRoadSegmentModified)
            .Expect(expectedRecords);
    }

    [Fact]
    public Task When_modifying_road_segment_attributes()
    {
        _fixture.Freeze<RoadSegmentId>();

        var acceptedRoadSegmentAdded = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.Create<RoadSegmentAdded>());

        var acceptedRoadSegmentAttributesModified = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.Create<RoadSegmentAttributesModified>());

        var expectedRecords = Array.ConvertAll(acceptedRoadSegmentAttributesModified.Changes, change =>
        {
            var segmentAdded = acceptedRoadSegmentAdded.Changes[0].RoadSegmentAdded;
            var segment = change.RoadSegmentAttributesModified;

            return (object)new RoadSegmentRecord
            {
                Id = segment.Id,
                BeginOrganizationId = acceptedRoadSegmentAttributesModified.OrganizationId,
                BeginOrganizationName = acceptedRoadSegmentAttributesModified.Organization,
                BeginTime = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentAttributesModified.When),
                BeginApplication = null,

                MaintainerId = segment.MaintenanceAuthority.Code,
                MaintainerName = segment.MaintenanceAuthority.Name,

                MethodId = RoadSegmentGeometryDrawMethod.Parse(segmentAdded.GeometryDrawMethod).Translation.Identifier,
                MethodDutchName = RoadSegmentGeometryDrawMethod.Parse(segmentAdded.GeometryDrawMethod).Translation.Name,

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

                RecordingDate = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentAdded.When),

                TransactionId = acceptedRoadSegmentAttributesModified.TransactionId,

                LeftSideMunicipalityId = null,
                LeftSideStreetNameId = segmentAdded.LeftSide.StreetNameId,
                LeftSideStreetName = null,

                RightSideMunicipalityId = null,
                RightSideStreetNameId = segmentAdded.RightSide.StreetNameId,
                RightSideStreetName = null,

                RoadSegmentVersion = segment.Version,
                BeginRoadNodeId = segmentAdded.StartNodeId,
                EndRoadNodeId = segmentAdded.EndNodeId,

                StreetNameCachePosition = -1L
            };
        });

        return new RoadSegmentRecordProjection(new StreetNameCacheStub())
            .Scenario()
            .Given(acceptedRoadSegmentAdded, acceptedRoadSegmentAttributesModified)
            .Expect(expectedRecords);
    }

    [Fact]
    public Task When_removing_road_segments()
    {
        _fixture.Freeze<RoadSegmentId>();

        var acceptedRoadSegmentAdded = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.Create<RoadSegmentAdded>());

        var acceptedRoadSegmentRemoved = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.Create<RoadSegmentRemoved>());

        var messages = new[]
        {
            acceptedRoadSegmentAdded,
            acceptedRoadSegmentRemoved
        };

        return new RoadSegmentRecordProjection(new StreetNameCacheStub())
            .Scenario()
            .Given(messages)
            .ExpectNone();
    }
}
