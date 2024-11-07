using RoadRegistry.Sync.StreetNameRegistry.Models;

namespace RoadRegistry.Wfs.ProjectionHost.Tests.Projections;

using AutoFixture;
using BackOffice;
using BackOffice.FeatureToggles;
using BackOffice.Messages;
using Framework;
using RoadRegistry.Tests.BackOffice;
using RoadRegistry.Tests.Framework.Projections;
using Schema;
using Wfs.Projections;
using StreetNameRecord = StreetNameRecord;

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

        _fixture.CustomizeImportedRoadSegment();
        _fixture.CustomizeRoadSegmentAdded();
        _fixture.CustomizeRoadSegmentModified();
        _fixture.CustomizeRoadSegmentAttributesModified();
        _fixture.CustomizeRoadSegmentGeometryModified();
        _fixture.CustomizeRoadSegmentRemoved();
        _fixture.CustomizeRoadNetworkChangesAccepted();
        _fixture.CustomizeStreetNameRecord();
        _fixture.CustomizeStreetNameModified();
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
                BeginTime = LocalDateTimeTranslator.TranslateFromWhen(message.When),

                MaintainerId = segment.MaintenanceAuthority.Code,
                MaintainerName = segment.MaintenanceAuthority.Name,

                MethodDutchName = RoadSegmentGeometryDrawMethod.Parse(segment.GeometryDrawMethod).Translation.Name,

                CategoryDutchName = RoadSegmentCategory.Parse(segment.Category).Translation.Name,

                Geometry2D = WfsGeometryTranslator.Translate2D(segment.Geometry),

                MorphologyDutchName = RoadSegmentMorphology.Parse(segment.Morphology).Translation.Name,

                StatusDutchName = RoadSegmentStatus.Parse(segment.Status).Translation.Name,

                AccessRestriction = RoadSegmentAccessRestriction.Parse(segment.AccessRestriction).Translation.Name,

                LeftSideStreetNameId = segment.LeftSide.StreetNameId,
                LeftSideStreetName = null,

                RightSideStreetNameId = segment.RightSide.StreetNameId,
                RightSideStreetName = null,

                BeginRoadNodeId = segment.StartNodeId,
                EndRoadNodeId = segment.EndNodeId
            };
        });

        return new RoadSegmentRecordProjection(new StreetNameCacheStub(), new UseRoadSegmentSoftDeleteFeatureToggle(true))
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

        await new RoadSegmentRecordProjection(new StreetNameCacheStub(), new UseRoadSegmentSoftDeleteFeatureToggle(true))
            .Scenario()
            .Given(importedRoadSegment)
            .Expect(new RoadSegmentRecord
            {
                Id = expectedRoadSegment.wegsegmentID,
                BeginTime = expectedRoadSegment.begintijd,

                MaintainerId = expectedRoadSegment.beheerder,
                MaintainerName = expectedRoadSegment.lblBeheerder,

                MethodDutchName = expectedRoadSegment.lblMethode,

                CategoryDutchName = expectedRoadSegment.lblCategorie,

                Geometry2D = expectedGeometry2D,

                MorphologyDutchName = expectedRoadSegment.lblMorfologie,

                StatusDutchName = expectedRoadSegment.lblStatus,

                AccessRestriction = expectedRoadSegment.lblToegangsbeperking,

                LeftSideStreetNameId = expectedRoadSegment.linksStraatnaamID,
                LeftSideStreetName = expectedRoadSegment.linksStraatnaam,

                RightSideStreetNameId = expectedRoadSegment.linksStraatnaamID,
                RightSideStreetName = expectedRoadSegment.linksStraatnaam,

                BeginRoadNodeId = expectedRoadSegment.beginWegknoopID,
                EndRoadNodeId = expectedRoadSegment.eindWegknoopID
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

        var streetNameCacheStub = new StreetNameCacheStub(streetNameRecord);

        await new RoadSegmentRecordProjection(streetNameCacheStub, new UseRoadSegmentSoftDeleteFeatureToggle(true))
            .Scenario()
            .Given(importedRoadSegment)
            .Expect(new RoadSegmentRecord
            {
                Id = expectedRoadSegment.wegsegmentID,
                BeginTime = expectedRoadSegment.begintijd,

                MaintainerId = expectedRoadSegment.beheerder,
                MaintainerName = expectedRoadSegment.lblBeheerder,

                MethodDutchName = expectedRoadSegment.lblMethode,

                CategoryDutchName = expectedRoadSegment.lblCategorie,

                Geometry2D = expectedGeometry2D,

                MorphologyDutchName = expectedRoadSegment.lblMorfologie,

                StatusDutchName = expectedRoadSegment.lblStatus,

                AccessRestriction = expectedRoadSegment.lblToegangsbeperking,

                LeftSideStreetNameId = expectedRoadSegment.linksStraatnaamID,
                LeftSideStreetName = streetNameRecord.DutchName,

                RightSideStreetNameId = expectedRoadSegment.linksStraatnaamID,
                RightSideStreetName = streetNameRecord.DutchName,

                BeginRoadNodeId = expectedRoadSegment.beginWegknoopID,
                EndRoadNodeId = expectedRoadSegment.eindWegknoopID
            });
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
                BeginTime = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentAttributesModified.When),

                MaintainerId = segment.MaintenanceAuthority.Code,
                MaintainerName = segment.MaintenanceAuthority.Name,

                MethodDutchName = RoadSegmentGeometryDrawMethod.Parse(segmentAdded.GeometryDrawMethod).Translation.Name,

                CategoryDutchName = RoadSegmentCategory.Parse(segment.Category).Translation.Name,

                Geometry2D = WfsGeometryTranslator.Translate2D(segmentAdded.Geometry),

                MorphologyDutchName = RoadSegmentMorphology.Parse(segment.Morphology).Translation.Name,

                StatusDutchName = RoadSegmentStatus.Parse(segment.Status).Translation.Name,

                AccessRestriction = RoadSegmentAccessRestriction.Parse(segment.AccessRestriction).Translation.Name,

                LeftSideStreetNameId = segment.LeftSide?.StreetNameId,
                LeftSideStreetName = null,

                RightSideStreetNameId = segment.RightSide?.StreetNameId,
                RightSideStreetName = null,

                BeginRoadNodeId = segmentAdded.StartNodeId,
                EndRoadNodeId = segmentAdded.EndNodeId
            };
        });

        return new RoadSegmentRecordProjection(new StreetNameCacheStub(), new UseRoadSegmentSoftDeleteFeatureToggle(true))
            .Scenario()
            .Given(acceptedRoadSegmentAdded, acceptedRoadSegmentAttributesModified)
            .Expect(expectedRecords);
    }

    [Fact]
    public Task When_modifying_road_segment_geometry()
    {
        _fixture.Freeze<RoadSegmentId>();

        var acceptedRoadSegmentAdded = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.Create<RoadSegmentAdded>());

        var acceptedRoadSegmentGeometryModified = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.Create<RoadSegmentGeometryModified>());

        var expectedRecords = Array.ConvertAll(acceptedRoadSegmentGeometryModified.Changes, change =>
        {
            var segmentAdded = acceptedRoadSegmentAdded.Changes[0].RoadSegmentAdded;
            var segment = change.RoadSegmentGeometryModified;

            return (object)new RoadSegmentRecord
            {
                Id = segment.Id,
                BeginTime = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentGeometryModified.When),

                MaintainerId = segmentAdded.MaintenanceAuthority.Code,
                MaintainerName = segmentAdded.MaintenanceAuthority.Name,

                MethodDutchName = RoadSegmentGeometryDrawMethod.Parse(segmentAdded.GeometryDrawMethod).Translation.Name,

                CategoryDutchName = RoadSegmentCategory.Parse(segmentAdded.Category).Translation.Name,

                Geometry2D = WfsGeometryTranslator.Translate2D(segment.Geometry),

                MorphologyDutchName = RoadSegmentMorphology.Parse(segmentAdded.Morphology).Translation.Name,

                StatusDutchName = RoadSegmentStatus.Parse(segmentAdded.Status).Translation.Name,

                AccessRestriction = RoadSegmentAccessRestriction.Parse(segmentAdded.AccessRestriction).Translation.Name,

                LeftSideStreetNameId = segmentAdded.LeftSide.StreetNameId,
                LeftSideStreetName = null,

                RightSideStreetNameId = segmentAdded.RightSide.StreetNameId,
                RightSideStreetName = null,

                BeginRoadNodeId = segmentAdded.StartNodeId,
                EndRoadNodeId = segmentAdded.EndNodeId
            };
        });

        return new RoadSegmentRecordProjection(new StreetNameCacheStub(), new UseRoadSegmentSoftDeleteFeatureToggle(true))
            .Scenario()
            .Given(acceptedRoadSegmentAdded, acceptedRoadSegmentGeometryModified)
            .Expect(expectedRecords);
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
                BeginTime = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentModified.When),

                MaintainerId = segment.MaintenanceAuthority.Code,
                MaintainerName = segment.MaintenanceAuthority.Name,

                MethodDutchName = RoadSegmentGeometryDrawMethod.Parse(segment.GeometryDrawMethod).Translation.Name,

                CategoryDutchName = RoadSegmentCategory.Parse(segment.Category).Translation.Name,

                Geometry2D = WfsGeometryTranslator.Translate2D(segment.Geometry),

                MorphologyDutchName = RoadSegmentMorphology.Parse(segment.Morphology).Translation.Name,

                StatusDutchName = RoadSegmentStatus.Parse(segment.Status).Translation.Name,

                AccessRestriction = RoadSegmentAccessRestriction.Parse(segment.AccessRestriction).Translation.Name,

                LeftSideStreetNameId = segment.LeftSide.StreetNameId,
                LeftSideStreetName = null,

                RightSideStreetNameId = segment.RightSide.StreetNameId,
                RightSideStreetName = null,

                BeginRoadNodeId = segment.StartNodeId,
                EndRoadNodeId = segment.EndNodeId
            };
        });

        return new RoadSegmentRecordProjection(new StreetNameCacheStub(), new UseRoadSegmentSoftDeleteFeatureToggle(true))
            .Scenario()
            .Given(acceptedRoadSegmentAdded, acceptedRoadSegmentModified)
            .Expect(expectedRecords);
    }

    [Fact]
    public Task When_removing_road_segments_harddelete()
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

        return new RoadSegmentRecordProjection(new StreetNameCacheStub(), new UseRoadSegmentSoftDeleteFeatureToggle(false))
            .Scenario()
            .Given(messages)
            .ExpectNone();
    }

    [Fact]
    public Task When_removing_road_segments_softdelete()
    {
        _fixture.Freeze<RoadSegmentId>();

        var acceptedRoadSegmentAdded = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.Create<RoadSegmentAdded>());

        var acceptedRoadSegmentRemoved = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.Create<RoadSegmentRemoved>());

        var messages = new object[]
        {
            acceptedRoadSegmentAdded,
            acceptedRoadSegmentRemoved
        };

        var expectedRecords = Array.ConvertAll(acceptedRoadSegmentAdded.Changes, change =>
        {
            var segment = change.RoadSegmentAdded;
            return (object)new RoadSegmentRecord
            {
                Id = segment.Id,
                BeginTime = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentRemoved.When),

                MaintainerId = segment.MaintenanceAuthority.Code,
                MaintainerName = segment.MaintenanceAuthority.Name,

                MethodDutchName = RoadSegmentGeometryDrawMethod.Parse(segment.GeometryDrawMethod).Translation.Name,

                CategoryDutchName = RoadSegmentCategory.Parse(segment.Category).Translation.Name,

                Geometry2D = WfsGeometryTranslator.Translate2D(segment.Geometry),

                MorphologyDutchName = RoadSegmentMorphology.Parse(segment.Morphology).Translation.Name,

                StatusDutchName = RoadSegmentStatus.Parse(segment.Status).Translation.Name,

                AccessRestriction = RoadSegmentAccessRestriction.Parse(segment.AccessRestriction).Translation.Name,

                LeftSideStreetNameId = segment.LeftSide.StreetNameId,
                LeftSideStreetName = null,

                RightSideStreetNameId = segment.RightSide.StreetNameId,
                RightSideStreetName = null,

                BeginRoadNodeId = segment.StartNodeId,
                EndRoadNodeId = segment.EndNodeId,

                IsRemoved = true
            };
        });

        return new RoadSegmentRecordProjection(new StreetNameCacheStub(), new UseRoadSegmentSoftDeleteFeatureToggle(true))
            .Scenario()
            .Given(messages)
            .Expect(expectedRecords);
    }

    [Fact]
    public Task When_organization_is_renamed()
    {
        _fixture.Freeze<RoadSegmentId>();
        _fixture.Freeze<OrganizationId>();

        var acceptedRoadSegmentAdded = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.Create<RoadSegmentAdded>());

        var renameOrganizationAccepted = new RenameOrganizationAccepted
        {
            Code = _fixture.Create<OrganizationId>(),
            Name = _fixture.CreateWhichIsDifferentThan(new OrganizationName(acceptedRoadSegmentAdded.Changes[0].RoadSegmentAdded.MaintenanceAuthority.Name))
        };

        var messages = new object[]
        {
            acceptedRoadSegmentAdded,
            renameOrganizationAccepted
        };

        var expectedRecords = Array.ConvertAll(acceptedRoadSegmentAdded.Changes, change =>
        {
            var segment = change.RoadSegmentAdded;
            return (object)new RoadSegmentRecord
            {
                Id = segment.Id,
                BeginTime = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentAdded.When),

                MaintainerId = segment.MaintenanceAuthority.Code,
                MaintainerName = renameOrganizationAccepted.Name,

                MethodDutchName = RoadSegmentGeometryDrawMethod.Parse(segment.GeometryDrawMethod).Translation.Name,

                CategoryDutchName = RoadSegmentCategory.Parse(segment.Category).Translation.Name,

                Geometry2D = WfsGeometryTranslator.Translate2D(segment.Geometry),

                MorphologyDutchName = RoadSegmentMorphology.Parse(segment.Morphology).Translation.Name,

                StatusDutchName = RoadSegmentStatus.Parse(segment.Status).Translation.Name,

                AccessRestriction = RoadSegmentAccessRestriction.Parse(segment.AccessRestriction).Translation.Name,

                LeftSideStreetNameId = segment.LeftSide.StreetNameId,
                LeftSideStreetName = null,

                RightSideStreetNameId = segment.RightSide.StreetNameId,
                RightSideStreetName = null,

                BeginRoadNodeId = segment.StartNodeId,
                EndRoadNodeId = segment.EndNodeId
            };
        });

        return new RoadSegmentRecordProjection(new StreetNameCacheStub(), new UseRoadSegmentSoftDeleteFeatureToggle(true))
            .Scenario()
            .Given(messages)
            .Expect(expectedRecords);
    }

    [Fact]
    public Task When_streetname_is_modified()
    {
        _fixture.Freeze<RoadSegmentId>();
        _fixture.Freeze<StreetNameLocalId>();

        var acceptedRoadSegmentAdded = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.CreateUntil<RoadSegmentAdded>(x => x.LeftSide.StreetNameId is not null));

        acceptedRoadSegmentAdded.Changes.Single().RoadSegmentAdded.RightSide = acceptedRoadSegmentAdded.Changes.Single().RoadSegmentAdded.LeftSide;

        var streetNameModified = _fixture.Create<StreetNameModified>();

        var messages = new object[]
        {
            acceptedRoadSegmentAdded,
            streetNameModified
        };

        var expectedRecords = Array.ConvertAll(acceptedRoadSegmentAdded.Changes, change =>
        {
            var segment = change.RoadSegmentAdded;
            return (object)new RoadSegmentRecord
            {
                Id = segment.Id,
                BeginTime = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentAdded.When),

                MaintainerId = segment.MaintenanceAuthority.Code,
                MaintainerName = segment.MaintenanceAuthority.Name,

                MethodDutchName = RoadSegmentGeometryDrawMethod.Parse(segment.GeometryDrawMethod).Translation.Name,

                CategoryDutchName = RoadSegmentCategory.Parse(segment.Category).Translation.Name,

                Geometry2D = WfsGeometryTranslator.Translate2D(segment.Geometry),

                MorphologyDutchName = RoadSegmentMorphology.Parse(segment.Morphology).Translation.Name,

                StatusDutchName = RoadSegmentStatus.Parse(segment.Status).Translation.Name,

                AccessRestriction = RoadSegmentAccessRestriction.Parse(segment.AccessRestriction).Translation.Name,

                LeftSideStreetNameId = segment.LeftSide.StreetNameId,
                LeftSideStreetName = streetNameModified.Record.DutchName,

                RightSideStreetNameId = segment.RightSide.StreetNameId,
                RightSideStreetName = streetNameModified.Record.DutchName,

                BeginRoadNodeId = segment.StartNodeId,
                EndRoadNodeId = segment.EndNodeId
            };
        });

        return new RoadSegmentRecordProjection(new StreetNameCacheStub(), new UseRoadSegmentSoftDeleteFeatureToggle(true))
            .Scenario()
            .Given(messages)
            .Expect(expectedRecords);
    }
}
