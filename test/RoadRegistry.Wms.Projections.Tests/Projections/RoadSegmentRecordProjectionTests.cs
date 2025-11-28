namespace RoadRegistry.Wms.ProjectionHost.Tests.Projections;

using AutoFixture;
using Infrastructure;
using RoadRegistry.BackOffice.FeatureToggles;
using RoadRegistry.RoadSegment.Events;
using RoadRegistry.Tests.BackOffice;
using RoadRegistry.Tests.Framework.Projections;
using RoadRegistry.Wms.ProjectionHost.Tests.Projections.Framework;
using RoadRegistry.Wms.Projections;
using RoadRegistry.Wms.Schema;

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
        _fixture.CustomizeStreetNameLocalId();

        _fixture.CustomizeRoadNetworkChangesAccepted();
        _fixture.CustomizeImportedRoadSegment();
        _fixture.CustomizeRoadSegmentAdded();
        _fixture.CustomizeRoadSegmentModified();
        _fixture.CustomizeRoadSegmentAttributesModified();
        _fixture.CustomizeRoadSegmentGeometryModified();
        _fixture.CustomizeRoadSegmentRemoved();
        _fixture.CustomizeStreetNameRecord();
        _fixture.CustomizeStreetNameModified();
        _fixture.CustomizeRoadSegmentAddedToEuropeanRoad();
        _fixture.CustomizeRoadSegmentRemovedFromEuropeanRoad();
        _fixture.CustomizeRoadSegmentAddedToNationalRoad();
        _fixture.CustomizeRoadSegmentRemovedFromNationalRoad();
        _fixture.CustomizeRoadSegmentAddedToNumberedRoad();
        _fixture.CustomizeRoadSegmentRemovedFromNumberedRoad();
        _fixture.CustomizeRoadSegmentsStreetNamesChanged();
    }

    [Fact]
    public Task When_adding_road_segments()
    {
        var message = _fixture.CreateMany<RoadSegmentAdded>().ToArray();

        var expectedRecords = Array.ConvertAll(message, change => { return BuildRoadSegmentRecord(change, _ => { }); });

        return BuildProjection()
            .Scenario()
            .Given(message.Cast<object>().ToArray())
            .Expect(expectedRecords.Cast<object>().ToArray());
    }

    // [Fact]
    // public Task When_adding_road_segment_to_european_road()
    // {
    //     _fixture.Freeze<RoadSegmentId>();
    //
    //     var acceptedRoadSegmentAdded = _fixture
    //         .Create<RoadNetworkChangesAccepted>()
    //         .WithAcceptedChanges(_fixture.Create<RoadSegmentAdded>());
    //
    //     var message = _fixture
    //         .Create<RoadNetworkChangesAccepted>()
    //         .WithAcceptedChanges(_fixture.Create<RoadSegmentAddedToEuropeanRoad>());
    //
    //     var expectedRecords = Array.ConvertAll(message.Changes, change =>
    //     {
    //         var roadSegmentAddedToEuropeanRoad = change.RoadSegmentAddedToEuropeanRoad;
    //
    //         return (object)BuildRoadSegmentRecord(
    //             acceptedRoadSegmentAdded,
    //             acceptedRoadSegmentAdded.Changes.Single().RoadSegmentAdded,
    //             record =>
    //             {
    //                 record.RoadSegmentVersion = roadSegmentAddedToEuropeanRoad.SegmentVersion!.Value;
    //
    //                 record.TransactionId = new TransactionId(message.TransactionId);
    //                 record.BeginTime = LocalDateTimeTranslator.TranslateFromWhen(message.When);
    //             });
    //     });
    //
    //     return BuildProjection()
    //         .Scenario()
    //         .Given(acceptedRoadSegmentAdded, message)
    //         .Expect(expectedRecords);
    // }
    //
    // [Fact]
    // public Task When_adding_road_segment_to_national_road()
    // {
    //     _fixture.Freeze<RoadSegmentId>();
    //
    //     var acceptedRoadSegmentAdded = _fixture
    //         .Create<RoadNetworkChangesAccepted>()
    //         .WithAcceptedChanges(_fixture.Create<RoadSegmentAdded>());
    //
    //     var message = _fixture
    //         .Create<RoadNetworkChangesAccepted>()
    //         .WithAcceptedChanges(_fixture.Create<RoadSegmentAddedToNationalRoad>());
    //
    //     var expectedRecords = Array.ConvertAll(message.Changes, change =>
    //     {
    //         var roadSegmentAddedToNationalRoad = change.RoadSegmentAddedToNationalRoad;
    //
    //         return (object)BuildRoadSegmentRecord(
    //             acceptedRoadSegmentAdded,
    //             acceptedRoadSegmentAdded.Changes.Single().RoadSegmentAdded,
    //             record =>
    //             {
    //                 record.RoadSegmentVersion = roadSegmentAddedToNationalRoad.SegmentVersion!.Value;
    //
    //                 record.TransactionId = new TransactionId(message.TransactionId);
    //                 record.BeginTime = LocalDateTimeTranslator.TranslateFromWhen(message.When);
    //             });
    //     });
    //
    //     return BuildProjection()
    //         .Scenario()
    //         .Given(acceptedRoadSegmentAdded, message)
    //         .Expect(expectedRecords);
    // }
    //
    // [Fact]
    // public Task When_adding_road_segment_to_numbered_road()
    // {
    //     _fixture.Freeze<RoadSegmentId>();
    //
    //     var acceptedRoadSegmentAdded = _fixture
    //         .Create<RoadNetworkChangesAccepted>()
    //         .WithAcceptedChanges(_fixture.Create<RoadSegmentAdded>());
    //
    //     var message = _fixture
    //         .Create<RoadNetworkChangesAccepted>()
    //         .WithAcceptedChanges(_fixture.Create<RoadSegmentAddedToNumberedRoad>());
    //
    //     var expectedRecords = Array.ConvertAll(message.Changes, change =>
    //     {
    //         var roadSegmentAddedToNumberedRoad = change.RoadSegmentAddedToNumberedRoad;
    //
    //         return (object)BuildRoadSegmentRecord(
    //             acceptedRoadSegmentAdded,
    //             acceptedRoadSegmentAdded.Changes.Single().RoadSegmentAdded,
    //             record =>
    //             {
    //                 record.RoadSegmentVersion = roadSegmentAddedToNumberedRoad.SegmentVersion!.Value;
    //
    //                 record.TransactionId = new TransactionId(message.TransactionId);
    //                 record.BeginTime = LocalDateTimeTranslator.TranslateFromWhen(message.When);
    //             });
    //     });
    //
    //     return BuildProjection()
    //         .Scenario()
    //         .Given(acceptedRoadSegmentAdded, message)
    //         .Expect(expectedRecords);
    // }
    //
    // [Fact]
    // public Task When_removing_road_segment_from_european_road()
    // {
    //     _fixture.Freeze<RoadSegmentId>();
    //
    //     var acceptedRoadSegmentAdded = _fixture
    //         .Create<RoadNetworkChangesAccepted>()
    //         .WithAcceptedChanges(_fixture.Create<RoadSegmentAdded>());
    //
    //     var message = _fixture
    //         .Create<RoadNetworkChangesAccepted>()
    //         .WithAcceptedChanges(_fixture.Create<RoadSegmentRemovedFromEuropeanRoad>());
    //
    //     var expectedRecords = Array.ConvertAll(message.Changes, change =>
    //     {
    //         var roadSegmentRemovedFromEuropeanRoad = change.RoadSegmentRemovedFromEuropeanRoad;
    //
    //         return (object)BuildRoadSegmentRecord(
    //             acceptedRoadSegmentAdded,
    //             acceptedRoadSegmentAdded.Changes.Single().RoadSegmentAdded,
    //             record =>
    //             {
    //                 record.RoadSegmentVersion = roadSegmentRemovedFromEuropeanRoad.SegmentVersion!.Value;
    //
    //                 record.TransactionId = new TransactionId(message.TransactionId);
    //                 record.BeginTime = LocalDateTimeTranslator.TranslateFromWhen(message.When);
    //             });
    //     });
    //
    //     return BuildProjection()
    //         .Scenario()
    //         .Given(acceptedRoadSegmentAdded, message)
    //         .Expect(expectedRecords);
    // }
    //
    // [Fact]
    // public Task When_removing_road_segment_from_national_road()
    // {
    //     _fixture.Freeze<RoadSegmentId>();
    //
    //     var acceptedRoadSegmentAdded = _fixture
    //         .Create<RoadNetworkChangesAccepted>()
    //         .WithAcceptedChanges(_fixture.Create<RoadSegmentAdded>());
    //
    //     var message = _fixture
    //         .Create<RoadNetworkChangesAccepted>()
    //         .WithAcceptedChanges(_fixture.Create<RoadSegmentRemovedFromNationalRoad>());
    //
    //     var expectedRecords = Array.ConvertAll(message.Changes, change =>
    //     {
    //         var roadSegmentRemovedFromNationalRoad = change.RoadSegmentRemovedFromNationalRoad;
    //
    //         return (object)BuildRoadSegmentRecord(
    //             acceptedRoadSegmentAdded,
    //             acceptedRoadSegmentAdded.Changes.Single().RoadSegmentAdded,
    //             record =>
    //             {
    //                 record.RoadSegmentVersion = roadSegmentRemovedFromNationalRoad.SegmentVersion!.Value;
    //
    //                 record.TransactionId = new TransactionId(message.TransactionId);
    //                 record.BeginTime = LocalDateTimeTranslator.TranslateFromWhen(message.When);
    //             });
    //     });
    //
    //     return BuildProjection()
    //         .Scenario()
    //         .Given(acceptedRoadSegmentAdded, message)
    //         .Expect(expectedRecords);
    // }
    //
    // [Fact]
    // public Task When_removing_road_segment_from_numbered_road()
    // {
    //     _fixture.Freeze<RoadSegmentId>();
    //
    //     var acceptedRoadSegmentAdded = _fixture
    //         .Create<RoadNetworkChangesAccepted>()
    //         .WithAcceptedChanges(_fixture.Create<RoadSegmentAdded>());
    //
    //     var message = _fixture
    //         .Create<RoadNetworkChangesAccepted>()
    //         .WithAcceptedChanges(_fixture.Create<RoadSegmentRemovedFromNumberedRoad>());
    //
    //     var expectedRecords = Array.ConvertAll(message.Changes, change =>
    //     {
    //         var roadSegmentRemovedFromNumberedRoad = change.RoadSegmentRemovedFromNumberedRoad;
    //
    //         return (object)BuildRoadSegmentRecord(
    //             acceptedRoadSegmentAdded,
    //             acceptedRoadSegmentAdded.Changes.Single().RoadSegmentAdded,
    //             record =>
    //             {
    //                 record.RoadSegmentVersion = roadSegmentRemovedFromNumberedRoad.SegmentVersion!.Value;
    //
    //                 record.TransactionId = new TransactionId(message.TransactionId);
    //                 record.BeginTime = LocalDateTimeTranslator.TranslateFromWhen(message.When);
    //             });
    //     });
    //
    //     return BuildProjection()
    //         .Scenario()
    //         .Given(acceptedRoadSegmentAdded, message)
    //         .Expect(expectedRecords);
    // }
    //
    // [Theory]
    // [InlineData(904)]
    // [InlineData(458)]
    // [InlineData(4)]
    // public async Task When_importing_road_segments(int wegSegmentId)
    // {
    //     var importedRoadSegment = await _testDataHelper.EventFromFileAsync<ImportedRoadSegment>(wegSegmentId);
    //
    //     var expectedGeometry2D = _testDataHelper.ExpectedGeometry2D(wegSegmentId);
    //
    //     var expectedRoadSegment = _testDataHelper.ExpectedRoadSegment(wegSegmentId);
    //
    //     await new RoadSegmentRecordProjection(new StreetNameCacheStub(), new UseRoadSegmentSoftDeleteFeatureToggle(true))
    //         .Scenario()
    //         .Given(importedRoadSegment)
    //         .Expect(new RoadSegmentRecord
    //         {
    //             Id = expectedRoadSegment.wegsegmentID,
    //             BeginOrganizationId = expectedRoadSegment.beginorganisatie,
    //             BeginTime = expectedRoadSegment.begintijd,
    //             BeginApplication = expectedRoadSegment.beginapplicatie,
    //
    //             MaintainerId = expectedRoadSegment.beheerder,
    //             MaintainerName = expectedRoadSegment.lblBeheerder,
    //
    //             MethodId = expectedRoadSegment.methode,
    //             MethodDutchName = expectedRoadSegment.lblMethode,
    //
    //             CategoryId = expectedRoadSegment.categorie,
    //             CategoryDutchName = expectedRoadSegment.lblCategorie,
    //
    //             Geometry2D = expectedGeometry2D,
    //             GeometryVersion = expectedRoadSegment.geometrieversie,
    //
    //             MorphologyId = expectedRoadSegment.morfologie,
    //             MorphologyDutchName = expectedRoadSegment.lblMorfologie,
    //
    //             StatusId = expectedRoadSegment.status,
    //             StatusDutchName = expectedRoadSegment.lblStatus,
    //
    //             AccessRestrictionId = expectedRoadSegment.toegangsbeperking,
    //             AccessRestrictionDutchName = expectedRoadSegment.lblToegangsbeperking,
    //
    //             BeginOrganizationName = expectedRoadSegment.lblOrganisatie,
    //             RecordingDate = expectedRoadSegment.opnamedatum,
    //
    //             TransactionId = expectedRoadSegment.transactieID,
    //
    //             LeftSideMunicipalityId = null,
    //             LeftSideStreetNameId = expectedRoadSegment.linksStraatnaamID,
    //             LeftSideStreetName = expectedRoadSegment.linksStraatnaam,
    //
    //             RightSideMunicipalityId = null,
    //             RightSideStreetNameId = expectedRoadSegment.linksStraatnaamID,
    //             RightSideStreetName = expectedRoadSegment.linksStraatnaam,
    //
    //             RoadSegmentVersion = expectedRoadSegment.wegsegmentversie,
    //             BeginRoadNodeId = expectedRoadSegment.beginWegknoopID,
    //             EndRoadNodeId = expectedRoadSegment.eindWegknoopID
    //         });
    // }
    //
    // [Theory]
    // [InlineData(904)]
    // [InlineData(458)]
    // [InlineData(4)]
    // public async Task When_importing_road_segments_with_street_name_in_cache(int wegSegmentId)
    // {
    //     var importedRoadSegment = await _testDataHelper.EventFromFileAsync<ImportedRoadSegment>(wegSegmentId);
    //
    //     var expectedGeometry2D = _testDataHelper.ExpectedGeometry2D(wegSegmentId);
    //
    //     var expectedRoadSegment = _testDataHelper.ExpectedRoadSegment(wegSegmentId);
    //
    //     var streetNameRecord = _fixture.Create<StreetNameRecord>();
    //
    //     var streetNameCacheStub = new StreetNameCacheStub(streetNameRecord);
    //
    //     await new RoadSegmentRecordProjection(streetNameCacheStub, new UseRoadSegmentSoftDeleteFeatureToggle(true))
    //         .Scenario()
    //         .Given(importedRoadSegment)
    //         .Expect(new RoadSegmentRecord
    //         {
    //             Id = expectedRoadSegment.wegsegmentID,
    //             BeginOrganizationId = expectedRoadSegment.beginorganisatie,
    //             BeginTime = expectedRoadSegment.begintijd,
    //             BeginApplication = expectedRoadSegment.beginapplicatie,
    //
    //             MaintainerId = expectedRoadSegment.beheerder,
    //             MaintainerName = expectedRoadSegment.lblBeheerder,
    //
    //             MethodId = expectedRoadSegment.methode,
    //             MethodDutchName = expectedRoadSegment.lblMethode,
    //
    //             CategoryId = expectedRoadSegment.categorie,
    //             CategoryDutchName = expectedRoadSegment.lblCategorie,
    //
    //             Geometry2D = expectedGeometry2D,
    //             GeometryVersion = expectedRoadSegment.geometrieversie,
    //
    //             MorphologyId = expectedRoadSegment.morfologie,
    //             MorphologyDutchName = expectedRoadSegment.lblMorfologie,
    //
    //             StatusId = expectedRoadSegment.status,
    //             StatusDutchName = expectedRoadSegment.lblStatus,
    //
    //             AccessRestrictionId = expectedRoadSegment.toegangsbeperking,
    //             AccessRestrictionDutchName = expectedRoadSegment.lblToegangsbeperking,
    //
    //             BeginOrganizationName = expectedRoadSegment.lblOrganisatie,
    //             RecordingDate = expectedRoadSegment.opnamedatum,
    //
    //             TransactionId = expectedRoadSegment.transactieID,
    //
    //             LeftSideMunicipalityId = null,
    //             LeftSideMunicipalityNisCode = streetNameRecord.NisCode,
    //             LeftSideStreetNameId = expectedRoadSegment.linksStraatnaamID,
    //             LeftSideStreetName = streetNameRecord.DutchName,
    //
    //             RightSideMunicipalityId = null,
    //             RightSideMunicipalityNisCode = streetNameRecord.NisCode,
    //             RightSideStreetNameId = expectedRoadSegment.linksStraatnaamID,
    //             RightSideStreetName = streetNameRecord.DutchName,
    //
    //             RoadSegmentVersion = expectedRoadSegment.wegsegmentversie,
    //             BeginRoadNodeId = expectedRoadSegment.beginWegknoopID,
    //             EndRoadNodeId = expectedRoadSegment.eindWegknoopID
    //         });
    // }
    //
    // [Fact]
    // public Task When_modifying_road_segment_attributes()
    // {
    //     _fixture.Freeze<RoadSegmentId>();
    //
    //     var acceptedRoadSegmentAdded = _fixture
    //         .Create<RoadNetworkChangesAccepted>()
    //         .WithAcceptedChanges(_fixture.Create<RoadSegmentAdded>());
    //
    //     var acceptedRoadSegmentAttributesModified = _fixture
    //         .Create<RoadNetworkChangesAccepted>()
    //         .WithAcceptedChanges(_fixture.Create<RoadSegmentAttributesModified>());
    //
    //     var expectedRecords = Array.ConvertAll(acceptedRoadSegmentAttributesModified.Changes, change =>
    //     {
    //         var segmentAdded = acceptedRoadSegmentAdded.Changes[0].RoadSegmentAdded;
    //         var segment = change.RoadSegmentAttributesModified;
    //
    //         return (object)new RoadSegmentRecord
    //         {
    //             Id = segment.Id,
    //             BeginOrganizationId = acceptedRoadSegmentAdded.OrganizationId,
    //             BeginOrganizationName = acceptedRoadSegmentAdded.Organization,
    //             BeginTime = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentAttributesModified.When),
    //             BeginApplication = null,
    //
    //             MaintainerId = segment.MaintenanceAuthority.Code,
    //             MaintainerName = segment.MaintenanceAuthority.Name,
    //
    //             MethodId = RoadSegmentGeometryDrawMethod.Parse(segmentAdded.GeometryDrawMethod).Translation.Identifier,
    //             MethodDutchName = RoadSegmentGeometryDrawMethod.Parse(segmentAdded.GeometryDrawMethod).Translation.Name,
    //
    //             CategoryId = RoadSegmentCategory.Parse(segment.Category).Translation.Identifier,
    //             CategoryDutchName = RoadSegmentCategory.Parse(segment.Category).Translation.Name,
    //
    //             Geometry2D = WmsGeometryTranslator.Translate2D(segmentAdded.Geometry),
    //             GeometryVersion = segmentAdded.GeometryVersion,
    //
    //             MorphologyId = RoadSegmentMorphology.Parse(segment.Morphology).Translation.Identifier,
    //             MorphologyDutchName = RoadSegmentMorphology.Parse(segment.Morphology).Translation.Name,
    //
    //             StatusId = RoadSegmentStatus.Parse(segment.Status).Translation.Identifier,
    //             StatusDutchName = RoadSegmentStatus.Parse(segment.Status).Translation.Name,
    //
    //             AccessRestrictionId = RoadSegmentAccessRestriction.Parse(segment.AccessRestriction).Translation.Identifier,
    //             AccessRestrictionDutchName = RoadSegmentAccessRestriction.Parse(segment.AccessRestriction).Translation.Name,
    //
    //             RecordingDate = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentAdded.When),
    //
    //             TransactionId = acceptedRoadSegmentAttributesModified.TransactionId,
    //
    //             LeftSideMunicipalityId = null,
    //             LeftSideStreetNameId = segment.LeftSide?.StreetNameId,
    //             LeftSideStreetName = null,
    //
    //             RightSideMunicipalityId = null,
    //             RightSideStreetNameId = segment.RightSide?.StreetNameId,
    //             RightSideStreetName = null,
    //
    //             RoadSegmentVersion = segment.Version,
    //             BeginRoadNodeId = segmentAdded.StartNodeId,
    //             EndRoadNodeId = segmentAdded.EndNodeId
    //         };
    //     });
    //
    //     return new RoadSegmentRecordProjection(new StreetNameCacheStub(), new UseRoadSegmentSoftDeleteFeatureToggle(true))
    //         .Scenario()
    //         .Given(acceptedRoadSegmentAdded, acceptedRoadSegmentAttributesModified)
    //         .Expect(expectedRecords);
    // }
    //
    // [Fact]
    // public Task When_modifying_road_segment_geometry()
    // {
    //     _fixture.Freeze<RoadSegmentId>();
    //
    //     var acceptedRoadSegmentAdded = _fixture
    //         .Create<RoadNetworkChangesAccepted>()
    //         .WithAcceptedChanges(_fixture.Create<RoadSegmentAdded>());
    //
    //     var acceptedRoadSegmentGeometryModified = _fixture
    //         .Create<RoadNetworkChangesAccepted>()
    //         .WithAcceptedChanges(_fixture.Create<RoadSegmentGeometryModified>());
    //
    //     var expectedRecords = Array.ConvertAll(acceptedRoadSegmentGeometryModified.Changes, change =>
    //     {
    //         var segmentAdded = acceptedRoadSegmentAdded.Changes[0].RoadSegmentAdded;
    //         var segment = change.RoadSegmentGeometryModified;
    //
    //         return (object)new RoadSegmentRecord
    //         {
    //             Id = segment.Id,
    //             BeginOrganizationId = acceptedRoadSegmentAdded.OrganizationId,
    //             BeginOrganizationName = acceptedRoadSegmentAdded.Organization,
    //             BeginTime = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentGeometryModified.When),
    //             BeginApplication = null,
    //
    //             MaintainerId = segmentAdded.MaintenanceAuthority.Code,
    //             MaintainerName = segmentAdded.MaintenanceAuthority.Name,
    //
    //             MethodId = RoadSegmentGeometryDrawMethod.Parse(segmentAdded.GeometryDrawMethod).Translation.Identifier,
    //             MethodDutchName = RoadSegmentGeometryDrawMethod.Parse(segmentAdded.GeometryDrawMethod).Translation.Name,
    //
    //             CategoryId = RoadSegmentCategory.Parse(segmentAdded.Category).Translation.Identifier,
    //             CategoryDutchName = RoadSegmentCategory.Parse(segmentAdded.Category).Translation.Name,
    //
    //             Geometry2D = WmsGeometryTranslator.Translate2D(segment.Geometry),
    //             GeometryVersion = segment.GeometryVersion,
    //
    //             MorphologyId = RoadSegmentMorphology.Parse(segmentAdded.Morphology).Translation.Identifier,
    //             MorphologyDutchName = RoadSegmentMorphology.Parse(segmentAdded.Morphology).Translation.Name,
    //
    //             StatusId = RoadSegmentStatus.Parse(segmentAdded.Status).Translation.Identifier,
    //             StatusDutchName = RoadSegmentStatus.Parse(segmentAdded.Status).Translation.Name,
    //
    //             AccessRestrictionId = RoadSegmentAccessRestriction.Parse(segmentAdded.AccessRestriction).Translation.Identifier,
    //             AccessRestrictionDutchName = RoadSegmentAccessRestriction.Parse(segmentAdded.AccessRestriction).Translation.Name,
    //
    //             RecordingDate = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentAdded.When),
    //
    //             TransactionId = acceptedRoadSegmentGeometryModified.TransactionId,
    //
    //             LeftSideMunicipalityId = null,
    //             LeftSideStreetNameId = segmentAdded.LeftSide.StreetNameId,
    //             LeftSideStreetName = null,
    //
    //             RightSideMunicipalityId = null,
    //             RightSideStreetNameId = segmentAdded.RightSide.StreetNameId,
    //             RightSideStreetName = null,
    //
    //             RoadSegmentVersion = segment.Version,
    //             BeginRoadNodeId = segmentAdded.StartNodeId,
    //             EndRoadNodeId = segmentAdded.EndNodeId
    //         };
    //     });
    //
    //     return new RoadSegmentRecordProjection(new StreetNameCacheStub(), new UseRoadSegmentSoftDeleteFeatureToggle(true))
    //         .Scenario()
    //         .Given(acceptedRoadSegmentAdded, acceptedRoadSegmentGeometryModified)
    //         .Expect(expectedRecords);
    // }
    //
    // [Fact]
    // public Task WhenRoadSegmentsStreetNamesChanged()
    // {
    //     _fixture.Freeze<RoadSegmentId>();
    //
    //     var acceptedRoadSegmentAdded = _fixture
    //         .Create<RoadNetworkChangesAccepted>()
    //         .WithAcceptedChanges(_fixture.Create<RoadSegmentAdded>());
    //
    //     var roadSegmentsStreetNamesChanged = _fixture
    //         .Create<RoadSegmentsStreetNamesChanged>();
    //
    //     var expectedRecords = Array.ConvertAll(roadSegmentsStreetNamesChanged.RoadSegments, roadSegmentStreetNamesChanged =>
    //     {
    //         var segmentAdded = acceptedRoadSegmentAdded.Changes[0].RoadSegmentAdded;
    //
    //         return (object)new RoadSegmentRecord
    //         {
    //             Id = roadSegmentStreetNamesChanged.Id,
    //             BeginOrganizationId = acceptedRoadSegmentAdded.OrganizationId,
    //             BeginOrganizationName = acceptedRoadSegmentAdded.Organization,
    //             BeginTime = LocalDateTimeTranslator.TranslateFromWhen(roadSegmentsStreetNamesChanged.When),
    //             BeginApplication = null,
    //
    //             MaintainerId = segmentAdded.MaintenanceAuthority.Code,
    //             MaintainerName = segmentAdded.MaintenanceAuthority.Name,
    //
    //             MethodId = RoadSegmentGeometryDrawMethod.Parse(segmentAdded.GeometryDrawMethod).Translation.Identifier,
    //             MethodDutchName = RoadSegmentGeometryDrawMethod.Parse(segmentAdded.GeometryDrawMethod).Translation.Name,
    //
    //             CategoryId = RoadSegmentCategory.Parse(segmentAdded.Category).Translation.Identifier,
    //             CategoryDutchName = RoadSegmentCategory.Parse(segmentAdded.Category).Translation.Name,
    //
    //             Geometry2D = WmsGeometryTranslator.Translate2D(segmentAdded.Geometry),
    //             GeometryVersion = segmentAdded.GeometryVersion,
    //
    //             MorphologyId = RoadSegmentMorphology.Parse(segmentAdded.Morphology).Translation.Identifier,
    //             MorphologyDutchName = RoadSegmentMorphology.Parse(segmentAdded.Morphology).Translation.Name,
    //
    //             StatusId = RoadSegmentStatus.Parse(segmentAdded.Status).Translation.Identifier,
    //             StatusDutchName = RoadSegmentStatus.Parse(segmentAdded.Status).Translation.Name,
    //
    //             AccessRestrictionId = RoadSegmentAccessRestriction.Parse(segmentAdded.AccessRestriction).Translation.Identifier,
    //             AccessRestrictionDutchName = RoadSegmentAccessRestriction.Parse(segmentAdded.AccessRestriction).Translation.Name,
    //
    //             RecordingDate = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentAdded.When),
    //
    //             TransactionId = acceptedRoadSegmentAdded.TransactionId,
    //
    //             LeftSideMunicipalityId = null,
    //             LeftSideStreetNameId = roadSegmentStreetNamesChanged.LeftSideStreetNameId,
    //             LeftSideStreetName = null,
    //
    //             RightSideMunicipalityId = null,
    //             RightSideStreetNameId = roadSegmentStreetNamesChanged.RightSideStreetNameId,
    //             RightSideStreetName = null,
    //
    //             RoadSegmentVersion = roadSegmentStreetNamesChanged.Version,
    //             BeginRoadNodeId = segmentAdded.StartNodeId,
    //             EndRoadNodeId = segmentAdded.EndNodeId
    //         };
    //     });
    //
    //     return new RoadSegmentRecordProjection(new StreetNameCacheStub(), new UseRoadSegmentSoftDeleteFeatureToggle(true))
    //         .Scenario()
    //         .Given(acceptedRoadSegmentAdded, roadSegmentsStreetNamesChanged)
    //         .Expect(expectedRecords);
    // }
    //
    // [Fact]
    // public Task When_modifying_road_segments()
    // {
    //     _fixture.Freeze<RoadSegmentId>();
    //
    //     var acceptedRoadSegmentAdded = _fixture
    //         .Create<RoadNetworkChangesAccepted>()
    //         .WithAcceptedChanges(_fixture.Create<RoadSegmentAdded>());
    //
    //     var acceptedRoadSegmentModified = _fixture
    //         .Create<RoadNetworkChangesAccepted>()
    //         .WithAcceptedChanges(_fixture.Create<RoadSegmentModified>());
    //
    //     var expectedRecords = Array.ConvertAll(acceptedRoadSegmentModified.Changes, change =>
    //     {
    //         var segment = change.RoadSegmentModified;
    //         return (object)new RoadSegmentRecord
    //         {
    //             Id = segment.Id,
    //             BeginOrganizationId = acceptedRoadSegmentAdded.OrganizationId,
    //             BeginOrganizationName = acceptedRoadSegmentAdded.Organization,
    //             BeginTime = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentModified.When),
    //             BeginApplication = null,
    //
    //             MaintainerId = segment.MaintenanceAuthority.Code,
    //             MaintainerName = segment.MaintenanceAuthority.Name,
    //
    //             MethodId = RoadSegmentGeometryDrawMethod.Parse(segment.GeometryDrawMethod).Translation.Identifier,
    //             MethodDutchName = RoadSegmentGeometryDrawMethod.Parse(segment.GeometryDrawMethod).Translation.Name,
    //
    //             CategoryId = RoadSegmentCategory.Parse(segment.Category).Translation.Identifier,
    //             CategoryDutchName = RoadSegmentCategory.Parse(segment.Category).Translation.Name,
    //
    //             Geometry2D = WmsGeometryTranslator.Translate2D(segment.Geometry),
    //             GeometryVersion = segment.GeometryVersion,
    //
    //             MorphologyId = RoadSegmentMorphology.Parse(segment.Morphology).Translation.Identifier,
    //             MorphologyDutchName = RoadSegmentMorphology.Parse(segment.Morphology).Translation.Name,
    //
    //             StatusId = RoadSegmentStatus.Parse(segment.Status).Translation.Identifier,
    //             StatusDutchName = RoadSegmentStatus.Parse(segment.Status).Translation.Name,
    //
    //             AccessRestrictionId = RoadSegmentAccessRestriction.Parse(segment.AccessRestriction).Translation.Identifier,
    //             AccessRestrictionDutchName = RoadSegmentAccessRestriction.Parse(segment.AccessRestriction).Translation.Name,
    //
    //             RecordingDate = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentAdded.When),
    //
    //             TransactionId = acceptedRoadSegmentModified.TransactionId,
    //
    //             LeftSideMunicipalityId = null,
    //             LeftSideStreetNameId = segment.LeftSide.StreetNameId,
    //             LeftSideStreetName = null,
    //
    //             RightSideMunicipalityId = null,
    //             RightSideStreetNameId = segment.RightSide.StreetNameId,
    //             RightSideStreetName = null,
    //
    //             RoadSegmentVersion = segment.Version,
    //             BeginRoadNodeId = segment.StartNodeId,
    //             EndRoadNodeId = segment.EndNodeId
    //         };
    //     });
    //
    //     return new RoadSegmentRecordProjection(new StreetNameCacheStub(), new UseRoadSegmentSoftDeleteFeatureToggle(true))
    //         .Scenario()
    //         .Given(acceptedRoadSegmentAdded, acceptedRoadSegmentModified)
    //         .Expect(expectedRecords);
    // }
    //
    // [Fact]
    // public Task When_removing_road_segments_harddelete()
    // {
    //     _fixture.Freeze<RoadSegmentId>();
    //
    //     var acceptedRoadSegmentAdded = _fixture
    //         .Create<RoadNetworkChangesAccepted>()
    //         .WithAcceptedChanges(_fixture.Create<RoadSegmentAdded>());
    //
    //     var acceptedRoadSegmentRemoved = _fixture
    //         .Create<RoadNetworkChangesAccepted>()
    //         .WithAcceptedChanges(_fixture.Create<RoadSegmentRemoved>());
    //
    //     var messages = new[]
    //     {
    //         acceptedRoadSegmentAdded,
    //         acceptedRoadSegmentRemoved
    //     };
    //
    //     return new RoadSegmentRecordProjection(new StreetNameCacheStub(), new UseRoadSegmentSoftDeleteFeatureToggle(false))
    //         .Scenario()
    //         .Given(messages)
    //         .ExpectNone();
    // }
    //
    // [Fact]
    // public Task When_removing_road_segments_softdelete()
    // {
    //     _fixture.Freeze<RoadSegmentId>();
    //
    //     var acceptedRoadSegmentAdded = _fixture
    //         .Create<RoadNetworkChangesAccepted>()
    //         .WithAcceptedChanges(_fixture.Create<RoadSegmentAdded>());
    //
    //     var acceptedRoadSegmentRemoved = _fixture
    //         .Create<RoadNetworkChangesAccepted>()
    //         .WithAcceptedChanges(_fixture.Create<RoadSegmentRemoved>());
    //
    //     var messages = new object[]
    //     {
    //         acceptedRoadSegmentAdded,
    //         acceptedRoadSegmentRemoved
    //     };
    //
    //     var expectedRecords = Array.ConvertAll(acceptedRoadSegmentAdded.Changes, change =>
    //     {
    //         var segment = change.RoadSegmentAdded;
    //         return (object)new RoadSegmentRecord
    //         {
    //             Id = segment.Id,
    //             BeginOrganizationId = acceptedRoadSegmentAdded.OrganizationId,
    //             BeginOrganizationName = acceptedRoadSegmentAdded.Organization,
    //             BeginTime = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentRemoved.When),
    //             BeginApplication = null,
    //
    //             MaintainerId = segment.MaintenanceAuthority.Code,
    //             MaintainerName = segment.MaintenanceAuthority.Name,
    //
    //             MethodId = RoadSegmentGeometryDrawMethod.Parse(segment.GeometryDrawMethod).Translation.Identifier,
    //             MethodDutchName = RoadSegmentGeometryDrawMethod.Parse(segment.GeometryDrawMethod).Translation.Name,
    //
    //             CategoryId = RoadSegmentCategory.Parse(segment.Category).Translation.Identifier,
    //             CategoryDutchName = RoadSegmentCategory.Parse(segment.Category).Translation.Name,
    //
    //             Geometry2D = WmsGeometryTranslator.Translate2D(segment.Geometry),
    //             GeometryVersion = segment.GeometryVersion,
    //
    //             MorphologyId = RoadSegmentMorphology.Parse(segment.Morphology).Translation.Identifier,
    //             MorphologyDutchName = RoadSegmentMorphology.Parse(segment.Morphology).Translation.Name,
    //
    //             StatusId = RoadSegmentStatus.Parse(segment.Status).Translation.Identifier,
    //             StatusDutchName = RoadSegmentStatus.Parse(segment.Status).Translation.Name,
    //
    //             AccessRestrictionId = RoadSegmentAccessRestriction.Parse(segment.AccessRestriction).Translation.Identifier,
    //             AccessRestrictionDutchName = RoadSegmentAccessRestriction.Parse(segment.AccessRestriction).Translation.Name,
    //
    //             RecordingDate = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentAdded.When),
    //
    //             TransactionId = acceptedRoadSegmentAdded.TransactionId,
    //
    //             LeftSideMunicipalityId = null,
    //             LeftSideStreetNameId = segment.LeftSide.StreetNameId,
    //             LeftSideStreetName = null,
    //
    //             RightSideMunicipalityId = null,
    //             RightSideStreetNameId = segment.RightSide.StreetNameId,
    //             RightSideStreetName = null,
    //
    //             RoadSegmentVersion = segment.Version,
    //             BeginRoadNodeId = segment.StartNodeId,
    //             EndRoadNodeId = segment.EndNodeId,
    //
    //             IsRemoved = true
    //         };
    //     });
    //
    //     return new RoadSegmentRecordProjection(new StreetNameCacheStub(), new UseRoadSegmentSoftDeleteFeatureToggle(true))
    //         .Scenario()
    //         .Given(messages)
    //         .Expect(expectedRecords);
    // }
    //
    // [Fact]
    // public Task When_organization_is_renamed()
    // {
    //     _fixture.Freeze<RoadSegmentId>();
    //     _fixture.Freeze<OrganizationId>();
    //
    //     var acceptedRoadSegmentAdded = _fixture
    //         .Create<RoadNetworkChangesAccepted>()
    //         .WithAcceptedChanges(_fixture.Create<RoadSegmentAdded>());
    //
    //     var renameOrganizationAccepted = new RenameOrganizationAccepted
    //     {
    //         Code = _fixture.Create<OrganizationId>(),
    //         Name = _fixture.CreateWhichIsDifferentThan(new OrganizationName(acceptedRoadSegmentAdded.Changes[0].RoadSegmentAdded.MaintenanceAuthority.Name))
    //     };
    //
    //     var messages = new object[]
    //     {
    //         acceptedRoadSegmentAdded,
    //         renameOrganizationAccepted
    //     };
    //
    //     var expectedRecords = Array.ConvertAll(acceptedRoadSegmentAdded.Changes, change =>
    //     {
    //         var segment = change.RoadSegmentAdded;
    //         return (object)new RoadSegmentRecord
    //         {
    //             Id = segment.Id,
    //             BeginOrganizationId = acceptedRoadSegmentAdded.OrganizationId,
    //             BeginOrganizationName = acceptedRoadSegmentAdded.Organization,
    //             BeginTime = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentAdded.When),
    //             BeginApplication = null,
    //
    //             MaintainerId = segment.MaintenanceAuthority.Code,
    //             MaintainerName = renameOrganizationAccepted.Name,
    //
    //             MethodId = RoadSegmentGeometryDrawMethod.Parse(segment.GeometryDrawMethod).Translation.Identifier,
    //             MethodDutchName = RoadSegmentGeometryDrawMethod.Parse(segment.GeometryDrawMethod).Translation.Name,
    //
    //             CategoryId = RoadSegmentCategory.Parse(segment.Category).Translation.Identifier,
    //             CategoryDutchName = RoadSegmentCategory.Parse(segment.Category).Translation.Name,
    //
    //             Geometry2D = WmsGeometryTranslator.Translate2D(segment.Geometry),
    //             GeometryVersion = segment.GeometryVersion,
    //
    //             MorphologyId = RoadSegmentMorphology.Parse(segment.Morphology).Translation.Identifier,
    //             MorphologyDutchName = RoadSegmentMorphology.Parse(segment.Morphology).Translation.Name,
    //
    //             StatusId = RoadSegmentStatus.Parse(segment.Status).Translation.Identifier,
    //             StatusDutchName = RoadSegmentStatus.Parse(segment.Status).Translation.Name,
    //
    //             AccessRestrictionId = RoadSegmentAccessRestriction.Parse(segment.AccessRestriction).Translation.Identifier,
    //             AccessRestrictionDutchName = RoadSegmentAccessRestriction.Parse(segment.AccessRestriction).Translation.Name,
    //
    //             RecordingDate = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentAdded.When),
    //
    //             TransactionId = acceptedRoadSegmentAdded.TransactionId,
    //
    //             LeftSideMunicipalityId = null,
    //             LeftSideStreetNameId = segment.LeftSide.StreetNameId,
    //             LeftSideStreetName = null,
    //
    //             RightSideMunicipalityId = null,
    //             RightSideStreetNameId = segment.RightSide.StreetNameId,
    //             RightSideStreetName = null,
    //
    //             RoadSegmentVersion = segment.Version,
    //             BeginRoadNodeId = segment.StartNodeId,
    //             EndRoadNodeId = segment.EndNodeId
    //         };
    //     });
    //
    //     return new RoadSegmentRecordProjection(new StreetNameCacheStub(), new UseRoadSegmentSoftDeleteFeatureToggle(true))
    //         .Scenario()
    //         .Given(messages)
    //         .Expect(expectedRecords);
    // }
    //
    // [Fact]
    // public Task When_streetname_is_modified()
    // {
    //     _fixture.Freeze<RoadSegmentId>();
    //     _fixture.Freeze<StreetNameLocalId>();
    //
    //     var acceptedRoadSegmentAdded = _fixture
    //         .Create<RoadNetworkChangesAccepted>()
    //         .WithAcceptedChanges(_fixture.CreateUntil<RoadSegmentAdded>(x => x.LeftSide.StreetNameId is not null));
    //
    //     acceptedRoadSegmentAdded.Changes.Single().RoadSegmentAdded.RightSide = acceptedRoadSegmentAdded.Changes.Single().RoadSegmentAdded.LeftSide;
    //
    //     var streetNameModified = _fixture.Create<StreetNameModified>();
    //
    //     var messages = new object[]
    //     {
    //         acceptedRoadSegmentAdded,
    //         streetNameModified
    //     };
    //
    //     var expectedRecords = Array.ConvertAll(acceptedRoadSegmentAdded.Changes, change =>
    //     {
    //         var segment = change.RoadSegmentAdded;
    //         return (object)new RoadSegmentRecord
    //         {
    //             Id = segment.Id,
    //             BeginOrganizationId = acceptedRoadSegmentAdded.OrganizationId,
    //             BeginOrganizationName = acceptedRoadSegmentAdded.Organization,
    //             BeginTime = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentAdded.When),
    //             BeginApplication = null,
    //
    //             MaintainerId = segment.MaintenanceAuthority.Code,
    //             MaintainerName = segment.MaintenanceAuthority.Name,
    //
    //             MethodId = RoadSegmentGeometryDrawMethod.Parse(segment.GeometryDrawMethod).Translation.Identifier,
    //             MethodDutchName = RoadSegmentGeometryDrawMethod.Parse(segment.GeometryDrawMethod).Translation.Name,
    //
    //             CategoryId = RoadSegmentCategory.Parse(segment.Category).Translation.Identifier,
    //             CategoryDutchName = RoadSegmentCategory.Parse(segment.Category).Translation.Name,
    //
    //             Geometry2D = WmsGeometryTranslator.Translate2D(segment.Geometry),
    //             GeometryVersion = segment.GeometryVersion,
    //
    //             MorphologyId = RoadSegmentMorphology.Parse(segment.Morphology).Translation.Identifier,
    //             MorphologyDutchName = RoadSegmentMorphology.Parse(segment.Morphology).Translation.Name,
    //
    //             StatusId = RoadSegmentStatus.Parse(segment.Status).Translation.Identifier,
    //             StatusDutchName = RoadSegmentStatus.Parse(segment.Status).Translation.Name,
    //
    //             AccessRestrictionId = RoadSegmentAccessRestriction.Parse(segment.AccessRestriction).Translation.Identifier,
    //             AccessRestrictionDutchName = RoadSegmentAccessRestriction.Parse(segment.AccessRestriction).Translation.Name,
    //
    //             RecordingDate = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentAdded.When),
    //
    //             TransactionId = acceptedRoadSegmentAdded.TransactionId,
    //
    //             LeftSideMunicipalityId = null,
    //             LeftSideStreetNameId = segment.LeftSide.StreetNameId,
    //             LeftSideStreetName = streetNameModified.Record.DutchName,
    //
    //             RightSideMunicipalityId = null,
    //             RightSideStreetNameId = segment.RightSide.StreetNameId,
    //             RightSideStreetName = streetNameModified.Record.DutchName,
    //
    //             RoadSegmentVersion = segment.Version,
    //             BeginRoadNodeId = segment.StartNodeId,
    //             EndRoadNodeId = segment.EndNodeId
    //         };
    //     });
    //
    //     return new RoadSegmentRecordProjection(new StreetNameCacheStub(), new UseRoadSegmentSoftDeleteFeatureToggle(true))
    //         .Scenario()
    //         .Given(messages)
    //         .Expect(expectedRecords);
    // }

    private RoadSegmentRecord BuildRoadSegmentRecord(
        RoadSegmentAdded roadSegmentAdded,
        Action<RoadSegmentRecord> changeRecord)
    {
        // var geometry = WmsGeometryTranslator.Translate2D(roadSegmentAdded.Geometry);
        // var statusTranslation = RoadSegmentStatus.Parse(roadSegmentAdded.Status).Translation;
        // var morphologyTranslation = RoadSegmentMorphology.Parse(roadSegmentAdded.Morphology).Translation;
        // var categoryTranslation = RoadSegmentCategory.Parse(roadSegmentAdded.Category).Translation;
        // var geometryDrawMethodTranslation = RoadSegmentGeometryDrawMethod.Parse(roadSegmentAdded.GeometryDrawMethod).Translation;
        // var accessRestrictionTranslation = RoadSegmentAccessRestriction.Parse(roadSegmentAdded.AccessRestriction).Translation;
        // var transactionId = new TransactionId(acceptedRoadSegmentAdded.TransactionId);

        var record = new RoadSegmentRecord
        {
            Id = roadSegmentAdded.RoadSegmentId,
            // BeginRoadNodeId = roadSegmentAdded.StartNodeId,
            // EndRoadNodeId = roadSegmentAdded.EndNodeId,
            // Geometry2D = geometry,
            //
            // RoadSegmentVersion = roadSegmentAdded.Version,
            // GeometryVersion = roadSegmentAdded.GeometryVersion,
            // StatusId = statusTranslation.Identifier,
            // StatusDutchName = statusTranslation.Name,
            // MorphologyId = morphologyTranslation.Identifier,
            // MorphologyDutchName = morphologyTranslation.Name,
            // CategoryId = categoryTranslation.Identifier,
            // CategoryDutchName = categoryTranslation.Name,
            // LeftSideStreetNameId = roadSegmentAdded.LeftSide.StreetNameId,
            // RightSideStreetNameId = roadSegmentAdded.RightSide.StreetNameId,
            // MaintainerId = roadSegmentAdded.MaintenanceAuthority.Code,
            // MaintainerName = OrganizationName.FromValueWithFallback(roadSegmentAdded.MaintenanceAuthority.Name),
            // MethodId = geometryDrawMethodTranslation.Identifier,
            // MethodDutchName = geometryDrawMethodTranslation.Name,
            // AccessRestrictionId = accessRestrictionTranslation.Identifier,
            // AccessRestrictionDutchName = accessRestrictionTranslation.Name,
            //
            // TransactionId = transactionId,
            // RecordingDate = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentAdded.When),
            // BeginTime = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentAdded.When),
            // BeginOrganizationId = acceptedRoadSegmentAdded.OrganizationId,
            // BeginOrganizationName = acceptedRoadSegmentAdded.Organization
        };
        changeRecord(record);

        return record;
    }

    private RoadSegmentRecordProjection BuildProjection()
    {
        return new RoadSegmentRecordProjection(new StreetNameCacheStub(), new UseRoadSegmentSoftDeleteFeatureToggle(true));
    }
}
