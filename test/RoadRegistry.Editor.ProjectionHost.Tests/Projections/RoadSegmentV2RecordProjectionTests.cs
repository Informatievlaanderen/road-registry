namespace RoadRegistry.Editor.ProjectionHost.Tests.Projections;

using System.Text;
using AutoFixture;
using BackOffice;
using BackOffice.Extracts.Dbase.RoadSegments;
using BackOffice.Messages;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Editor.Projections;
using Editor.Schema.Extensions;
using Editor.Schema.RoadSegments;
using RoadRegistry.Tests.BackOffice;
using RoadRegistry.Tests.Framework.Projections;

public class RoadSegmentV2RecordProjectionTests : IClassFixture<ProjectionTestServices>
{
    private readonly Fixture _fixture;
    private readonly ProjectionTestServices _services;

    public RoadSegmentV2RecordProjectionTests(ProjectionTestServices services)
    {
        _services = services ?? throw new ArgumentNullException(nameof(services));

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

        _fixture.CustomizeImportedRoadSegment();
        _fixture.CustomizeImportedRoadSegmentEuropeanRoadAttributes();
        _fixture.CustomizeImportedRoadSegmentNationalRoadAttributes();
        _fixture.CustomizeImportedRoadSegmentNumberedRoadAttributes();
        _fixture.CustomizeImportedRoadSegmentLaneAttributes();
        _fixture.CustomizeImportedRoadSegmentWidthAttributes();
        _fixture.CustomizeImportedRoadSegmentSurfaceAttributes();
        _fixture.CustomizeImportedRoadSegmentSideAttributes();
        _fixture.CustomizeOriginProperties();

        _fixture.CustomizeRoadSegmentAdded();
        _fixture.CustomizeRoadSegmentModified();
        _fixture.CustomizeRoadSegmentAttributesModified();
        _fixture.CustomizeRoadSegmentGeometryModified();
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
            var roadSegmentAdded = change.RoadSegmentAdded;

            var geometry = GeometryTranslator.Translate(roadSegmentAdded.Geometry);
            var polyLineMShapeContent = new PolyLineMShapeContent(Be.Vlaanderen.Basisregisters.Shaperon.Geometries.GeometryTranslator.FromGeometryMultiLineString(geometry));
            var statusTranslation = RoadSegmentStatus.Parse(roadSegmentAdded.Status).Translation;
            var morphologyTranslation = RoadSegmentMorphology.Parse(roadSegmentAdded.Morphology).Translation;
            var categoryTranslation = RoadSegmentCategory.Parse(roadSegmentAdded.Category).Translation;
            var geometryDrawMethodTranslation = RoadSegmentGeometryDrawMethod.Parse(roadSegmentAdded.GeometryDrawMethod).Translation;
            var accessRestrictionTranslation = RoadSegmentAccessRestriction.Parse(roadSegmentAdded.AccessRestriction).Translation;
            var transactionId = new TransactionId(message.TransactionId);

            return (object)new RoadSegmentV2Record
            {
                Id = roadSegmentAdded.Id,
                StartNodeId = roadSegmentAdded.StartNodeId,
                EndNodeId = roadSegmentAdded.EndNodeId,
                ShapeRecordContent = polyLineMShapeContent.ToBytes(_services.MemoryStreamManager, Encoding.UTF8),
                ShapeRecordContentLength = polyLineMShapeContent.ContentLength.ToInt32(),
                Geometry = geometry,
                
                Version = roadSegmentAdded.Version,
                GeometryVersion = roadSegmentAdded.GeometryVersion,
                StatusId = statusTranslation.Identifier,
                MorphologyId = morphologyTranslation.Identifier,
                CategoryId = categoryTranslation.Identifier,
                LeftSideStreetNameId = roadSegmentAdded.LeftSide.StreetNameId,
                RightSideStreetNameId = roadSegmentAdded.RightSide.StreetNameId,
                MaintainerId = roadSegmentAdded.MaintenanceAuthority.Code,
                MaintainerName = OrganizationName.FromValueWithFallback(roadSegmentAdded.MaintenanceAuthority.Name),
                MethodId = geometryDrawMethodTranslation.Identifier,
                AccessRestrictionId = accessRestrictionTranslation.Identifier,

                TransactionId = transactionId,
                RecordingDate = LocalDateTimeTranslator.TranslateFromWhen(message.When),
                BeginTime = LocalDateTimeTranslator.TranslateFromWhen(message.When),
                BeginOrganizationId = message.OrganizationId,
                BeginOrganizationName = message.Organization,
                
                DbaseRecord = new RoadSegmentDbaseRecord
                {
                    WS_OIDN = { Value = roadSegmentAdded.Id },
                    WS_UIDN = { Value = roadSegmentAdded.Id + "_" + roadSegmentAdded.Version },
                    WS_GIDN = { Value = roadSegmentAdded.Id + "_" + roadSegmentAdded.GeometryVersion },
                    B_WK_OIDN = { Value = roadSegmentAdded.StartNodeId },
                    E_WK_OIDN = { Value = roadSegmentAdded.EndNodeId },
                    STATUS = { Value = RoadSegmentStatus.Parse(roadSegmentAdded.Status).Translation.Identifier },
                    LBLSTATUS = { Value = RoadSegmentStatus.Parse(roadSegmentAdded.Status).Translation.Name },
                    MORF = { Value = RoadSegmentMorphology.Parse(roadSegmentAdded.Morphology).Translation.Identifier },
                    LBLMORF = { Value = RoadSegmentMorphology.Parse(roadSegmentAdded.Morphology).Translation.Name },
                    WEGCAT = { Value = RoadSegmentCategory.Parse(roadSegmentAdded.Category).Translation.Identifier },
                    LBLWEGCAT = { Value = RoadSegmentCategory.Parse(roadSegmentAdded.Category).Translation.Name },
                    LSTRNMID = { Value = roadSegmentAdded.LeftSide.StreetNameId },
                    LSTRNM = { Value = null },
                    RSTRNMID = { Value = roadSegmentAdded.RightSide.StreetNameId },
                    RSTRNM = { Value = null },
                    BEHEER = { Value = roadSegmentAdded.MaintenanceAuthority.Code },
                    LBLBEHEER = { Value = roadSegmentAdded.MaintenanceAuthority.Name },
                    METHODE = { Value = RoadSegmentGeometryDrawMethod.Parse(roadSegmentAdded.GeometryDrawMethod).Translation.Identifier },
                    LBLMETHOD = { Value = RoadSegmentGeometryDrawMethod.Parse(roadSegmentAdded.GeometryDrawMethod).Translation.Name },
                    OPNDATUM = { Value = LocalDateTimeTranslator.TranslateFromWhen(message.When) },
                    BEGINTIJD = { Value = LocalDateTimeTranslator.TranslateFromWhen(message.When) },
                    BEGINORG = { Value = message.OrganizationId },
                    LBLBGNORG = { Value = message.Organization },
                    TGBEP = { Value = RoadSegmentAccessRestriction.Parse(roadSegmentAdded.AccessRestriction).Translation.Identifier },
                    LBLTGBEP = { Value = RoadSegmentAccessRestriction.Parse(roadSegmentAdded.AccessRestriction).Translation.Name }
                }.ToBytes(_services.MemoryStreamManager, Encoding.UTF8),
                LastEventHash = roadSegmentAdded.GetHash()
            }.WithBoundingBox(RoadSegmentBoundingBox.From(polyLineMShapeContent.Shape));
        });

        return new RoadSegmentV2RecordProjection(_services.MemoryStreamManager, Encoding.UTF8)
            .Scenario()
            .Given(message)
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
            var roadSegmentAdded = acceptedRoadSegmentAdded.Changes[0].RoadSegmentAdded;
            var roadSegmentAttributesModified = change.RoadSegmentAttributesModified;
            
            var geometry = GeometryTranslator.Translate(roadSegmentAdded.Geometry);
            var polyLineMShapeContent = new PolyLineMShapeContent(Be.Vlaanderen.Basisregisters.Shaperon.Geometries.GeometryTranslator.FromGeometryMultiLineString(geometry));
            var statusTranslation = RoadSegmentStatus.Parse(roadSegmentAttributesModified.Status).Translation;
            var morphologyTranslation = RoadSegmentMorphology.Parse(roadSegmentAttributesModified.Morphology).Translation;
            var categoryTranslation = RoadSegmentCategory.Parse(roadSegmentAttributesModified.Category).Translation;
            var geometryDrawMethodTranslation = RoadSegmentGeometryDrawMethod.Parse(roadSegmentAdded.GeometryDrawMethod).Translation;
            var accessRestrictionTranslation = RoadSegmentAccessRestriction.Parse(roadSegmentAttributesModified.AccessRestriction).Translation;

            return (object)new RoadSegmentV2Record
            {
                Id = roadSegmentAttributesModified.Id,
                StartNodeId = roadSegmentAdded.StartNodeId,
                EndNodeId = roadSegmentAdded.EndNodeId,
                ShapeRecordContent = polyLineMShapeContent.ToBytes(_services.MemoryStreamManager, Encoding.UTF8),
                ShapeRecordContentLength = polyLineMShapeContent.ContentLength.ToInt32(),
                Geometry = geometry,

                Version = roadSegmentAttributesModified.Version,
                GeometryVersion = roadSegmentAdded.GeometryVersion,
                StatusId = statusTranslation.Identifier,
                MorphologyId = morphologyTranslation.Identifier,
                CategoryId = categoryTranslation.Identifier,
                LeftSideStreetNameId = roadSegmentAdded.LeftSide.StreetNameId,
                RightSideStreetNameId = roadSegmentAdded.RightSide.StreetNameId,
                MaintainerId = roadSegmentAttributesModified.MaintenanceAuthority.Code,
                MaintainerName = OrganizationName.FromValueWithFallback(roadSegmentAttributesModified.MaintenanceAuthority.Name),
                MethodId = geometryDrawMethodTranslation.Identifier,
                AccessRestrictionId = accessRestrictionTranslation.Identifier,

                TransactionId = new TransactionId(acceptedRoadSegmentAttributesModified.TransactionId),
                RecordingDate = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentAdded.When),
                BeginTime = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentAttributesModified.When),
                BeginOrganizationId = acceptedRoadSegmentAdded.OrganizationId,
                BeginOrganizationName = acceptedRoadSegmentAdded.Organization,

                DbaseRecord = new RoadSegmentDbaseRecord
                {
                    WS_OIDN = { Value = roadSegmentAttributesModified.Id },
                    WS_UIDN = { Value = roadSegmentAttributesModified.Id + "_" + roadSegmentAttributesModified.Version },
                    WS_GIDN = { Value = roadSegmentAttributesModified.Id + "_" + roadSegmentAdded.GeometryVersion },
                    B_WK_OIDN = { Value = roadSegmentAdded.StartNodeId },
                    E_WK_OIDN = { Value = roadSegmentAdded.EndNodeId },
                    STATUS = { Value = statusTranslation.Identifier },
                    LBLSTATUS = { Value = statusTranslation.Name },
                    MORF = { Value = morphologyTranslation.Identifier },
                    LBLMORF = { Value = morphologyTranslation.Name },
                    WEGCAT = { Value = categoryTranslation.Identifier },
                    LBLWEGCAT = { Value = categoryTranslation.Name },
                    LSTRNMID = { Value = roadSegmentAdded.LeftSide.StreetNameId },
                    LSTRNM = { Value = null },
                    RSTRNMID = { Value = roadSegmentAdded.RightSide.StreetNameId },
                    RSTRNM = { Value = null },
                    BEHEER = { Value = roadSegmentAttributesModified.MaintenanceAuthority.Code },
                    LBLBEHEER = { Value = roadSegmentAttributesModified.MaintenanceAuthority.Name },
                    METHODE = { Value = geometryDrawMethodTranslation.Identifier },
                    LBLMETHOD = { Value = geometryDrawMethodTranslation.Name },
                    OPNDATUM = { Value = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentAdded.When) },
                    BEGINTIJD = { Value = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentAttributesModified.When) },
                    BEGINORG = { Value = acceptedRoadSegmentAdded.OrganizationId },
                    LBLBGNORG = { Value = acceptedRoadSegmentAdded.Organization },
                    TGBEP = { Value = accessRestrictionTranslation.Identifier },
                    LBLTGBEP = { Value = accessRestrictionTranslation.Name }
                }.ToBytes(_services.MemoryStreamManager, Encoding.UTF8),
                LastEventHash = roadSegmentAttributesModified.GetHash()
            }.WithBoundingBox(RoadSegmentBoundingBox.From(polyLineMShapeContent.Shape));
        });

        return new RoadSegmentV2RecordProjection(_services.MemoryStreamManager, Encoding.UTF8)
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
            var roadSegmentAdded = acceptedRoadSegmentAdded.Changes[0].RoadSegmentAdded;
            var roadSegmentGeometryModified = change.RoadSegmentGeometryModified;

            var geometry = GeometryTranslator.Translate(roadSegmentGeometryModified.Geometry);
            var polyLineMShapeContent = new PolyLineMShapeContent(Be.Vlaanderen.Basisregisters.Shaperon.Geometries.GeometryTranslator.FromGeometryMultiLineString(geometry));
            var statusTranslation = RoadSegmentStatus.Parse(roadSegmentAdded.Status).Translation;
            var morphologyTranslation = RoadSegmentMorphology.Parse(roadSegmentAdded.Morphology).Translation;
            var categoryTranslation = RoadSegmentCategory.Parse(roadSegmentAdded.Category).Translation;
            var geometryDrawMethodTranslation = RoadSegmentGeometryDrawMethod.Parse(roadSegmentAdded.GeometryDrawMethod).Translation;
            var accessRestrictionTranslation = RoadSegmentAccessRestriction.Parse(roadSegmentAdded.AccessRestriction).Translation;

            return (object)new RoadSegmentV2Record
            {
                Id = roadSegmentGeometryModified.Id,
                StartNodeId = roadSegmentAdded.StartNodeId,
                EndNodeId = roadSegmentAdded.EndNodeId,
                ShapeRecordContent = polyLineMShapeContent.ToBytes(_services.MemoryStreamManager, Encoding.UTF8),
                ShapeRecordContentLength = polyLineMShapeContent.ContentLength.ToInt32(),
                Geometry = geometry,

                Version = roadSegmentGeometryModified.Version,
                GeometryVersion = roadSegmentGeometryModified.GeometryVersion,
                StatusId = statusTranslation.Identifier,
                MorphologyId = morphologyTranslation.Identifier,
                CategoryId = categoryTranslation.Identifier,
                LeftSideStreetNameId = roadSegmentAdded.LeftSide.StreetNameId,
                RightSideStreetNameId = roadSegmentAdded.RightSide.StreetNameId,
                MaintainerId = roadSegmentAdded.MaintenanceAuthority.Code,
                MaintainerName = OrganizationName.FromValueWithFallback(roadSegmentAdded.MaintenanceAuthority.Name),
                MethodId = geometryDrawMethodTranslation.Identifier,
                AccessRestrictionId = accessRestrictionTranslation.Identifier,

                TransactionId = new TransactionId(acceptedRoadSegmentGeometryModified.TransactionId),
                RecordingDate = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentAdded.When),
                BeginTime = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentGeometryModified.When),
                BeginOrganizationId = acceptedRoadSegmentAdded.OrganizationId,
                BeginOrganizationName = acceptedRoadSegmentAdded.Organization,

                DbaseRecord = new RoadSegmentDbaseRecord
                {
                    WS_OIDN = { Value = roadSegmentGeometryModified.Id },
                    WS_UIDN = { Value = roadSegmentGeometryModified.Id + "_" + roadSegmentGeometryModified.Version },
                    WS_GIDN = { Value = roadSegmentGeometryModified.Id + "_" + roadSegmentGeometryModified.GeometryVersion },
                    B_WK_OIDN = { Value = roadSegmentAdded.StartNodeId },
                    E_WK_OIDN = { Value = roadSegmentAdded.EndNodeId },
                    STATUS = { Value = RoadSegmentStatus.Parse(roadSegmentAdded.Status).Translation.Identifier },
                    LBLSTATUS = { Value = RoadSegmentStatus.Parse(roadSegmentAdded.Status).Translation.Name },
                    MORF = { Value = RoadSegmentMorphology.Parse(roadSegmentAdded.Morphology).Translation.Identifier },
                    LBLMORF = { Value = RoadSegmentMorphology.Parse(roadSegmentAdded.Morphology).Translation.Name },
                    WEGCAT = { Value = RoadSegmentCategory.Parse(roadSegmentAdded.Category).Translation.Identifier },
                    LBLWEGCAT = { Value = RoadSegmentCategory.Parse(roadSegmentAdded.Category).Translation.Name },
                    LSTRNMID = { Value = roadSegmentAdded.LeftSide.StreetNameId },
                    LSTRNM = { Value = null },
                    RSTRNMID = { Value = roadSegmentAdded.RightSide.StreetNameId },
                    RSTRNM = { Value = null },
                    BEHEER = { Value = roadSegmentAdded.MaintenanceAuthority.Code },
                    LBLBEHEER = { Value = roadSegmentAdded.MaintenanceAuthority.Name },
                    METHODE = { Value = RoadSegmentGeometryDrawMethod.Parse(roadSegmentAdded.GeometryDrawMethod).Translation.Identifier },
                    LBLMETHOD = { Value = RoadSegmentGeometryDrawMethod.Parse(roadSegmentAdded.GeometryDrawMethod).Translation.Name },
                    OPNDATUM = { Value = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentAdded.When) },
                    BEGINTIJD = { Value = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentGeometryModified.When) },
                    BEGINORG = { Value = acceptedRoadSegmentAdded.OrganizationId },
                    LBLBGNORG = { Value = acceptedRoadSegmentAdded.Organization },
                    TGBEP = { Value = RoadSegmentAccessRestriction.Parse(roadSegmentAdded.AccessRestriction).Translation.Identifier },
                    LBLTGBEP = { Value = RoadSegmentAccessRestriction.Parse(roadSegmentAdded.AccessRestriction).Translation.Name }
                }.ToBytes(_services.MemoryStreamManager, Encoding.UTF8),
                LastEventHash = roadSegmentGeometryModified.GetHash()
            }.WithBoundingBox(RoadSegmentBoundingBox.From(polyLineMShapeContent.Shape));
        });

        return new RoadSegmentV2RecordProjection(_services.MemoryStreamManager, Encoding.UTF8)
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
            var roadSegmentModified = change.RoadSegmentModified;

            var geometry = GeometryTranslator.Translate(roadSegmentModified.Geometry);
            var polyLineMShapeContent = new PolyLineMShapeContent(Be.Vlaanderen.Basisregisters.Shaperon.Geometries.GeometryTranslator.FromGeometryMultiLineString(geometry));
            var statusTranslation = RoadSegmentStatus.Parse(roadSegmentModified.Status).Translation;
            var morphologyTranslation = RoadSegmentMorphology.Parse(roadSegmentModified.Morphology).Translation;
            var categoryTranslation = RoadSegmentCategory.Parse(roadSegmentModified.Category).Translation;
            var geometryDrawMethodTranslation = RoadSegmentGeometryDrawMethod.Parse(roadSegmentModified.GeometryDrawMethod).Translation;
            var accessRestrictionTranslation = RoadSegmentAccessRestriction.Parse(roadSegmentModified.AccessRestriction).Translation;
            var transactionId = new TransactionId(acceptedRoadSegmentModified.TransactionId);

            return (object)new RoadSegmentV2Record
            {
                Id = roadSegmentModified.Id,
                StartNodeId = roadSegmentModified.StartNodeId,
                EndNodeId = roadSegmentModified.EndNodeId,
                ShapeRecordContent = polyLineMShapeContent.ToBytes(_services.MemoryStreamManager, Encoding.UTF8),
                ShapeRecordContentLength = polyLineMShapeContent.ContentLength.ToInt32(),
                Geometry = geometry,

                Version = roadSegmentModified.Version,
                GeometryVersion = roadSegmentModified.GeometryVersion,
                StatusId = statusTranslation.Identifier,
                MorphologyId = morphologyTranslation.Identifier,
                CategoryId = categoryTranslation.Identifier,
                LeftSideStreetNameId = roadSegmentModified.LeftSide.StreetNameId,
                RightSideStreetNameId = roadSegmentModified.RightSide.StreetNameId,
                MaintainerId = roadSegmentModified.MaintenanceAuthority.Code,
                MaintainerName = OrganizationName.FromValueWithFallback(roadSegmentModified.MaintenanceAuthority.Name),
                MethodId = geometryDrawMethodTranslation.Identifier,
                AccessRestrictionId = accessRestrictionTranslation.Identifier,

                TransactionId = transactionId,
                RecordingDate = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentAdded.When),
                BeginTime = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentModified.When),
                BeginOrganizationId = acceptedRoadSegmentAdded.OrganizationId,
                BeginOrganizationName = acceptedRoadSegmentAdded.Organization,

                DbaseRecord = new RoadSegmentDbaseRecord
                {
                    WS_OIDN = { Value = roadSegmentModified.Id },
                    WS_UIDN = { Value = roadSegmentModified.Id + "_" + roadSegmentModified.Version },
                    WS_GIDN = { Value = roadSegmentModified.Id + "_" + roadSegmentModified.GeometryVersion },
                    B_WK_OIDN = { Value = roadSegmentModified.StartNodeId },
                    E_WK_OIDN = { Value = roadSegmentModified.EndNodeId },
                    STATUS = { Value = statusTranslation.Identifier },
                    LBLSTATUS = { Value = statusTranslation.Name },
                    MORF = { Value = morphologyTranslation.Identifier },
                    LBLMORF = { Value = morphologyTranslation.Name },
                    WEGCAT = { Value = categoryTranslation.Identifier },
                    LBLWEGCAT = { Value = categoryTranslation.Name },
                    LSTRNMID = { Value = roadSegmentModified.LeftSide.StreetNameId },
                    LSTRNM = { Value = null },
                    RSTRNMID = { Value = roadSegmentModified.RightSide.StreetNameId },
                    RSTRNM = { Value = null },
                    BEHEER = { Value = roadSegmentModified.MaintenanceAuthority.Code },
                    LBLBEHEER = { Value = roadSegmentModified.MaintenanceAuthority.Name },
                    METHODE = { Value = geometryDrawMethodTranslation.Identifier },
                    LBLMETHOD = { Value = geometryDrawMethodTranslation.Name },
                    OPNDATUM = { Value = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentAdded.When) },
                    BEGINTIJD = { Value = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentModified.When) },
                    BEGINORG = { Value = acceptedRoadSegmentAdded.OrganizationId },
                    LBLBGNORG = { Value = acceptedRoadSegmentAdded.Organization },
                    TGBEP = { Value = accessRestrictionTranslation.Identifier },
                    LBLTGBEP = { Value = accessRestrictionTranslation.Name }
                }.ToBytes(_services.MemoryStreamManager, Encoding.UTF8),
                LastEventHash = roadSegmentModified.GetHash()
            }.WithBoundingBox(RoadSegmentBoundingBox.From(polyLineMShapeContent.Shape));
        });

        return new RoadSegmentV2RecordProjection(_services.MemoryStreamManager, Encoding.UTF8)
            .Scenario()
            .Given(acceptedRoadSegmentAdded, acceptedRoadSegmentModified)
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

        var messages = new object[]
        {
            acceptedRoadSegmentAdded,
            acceptedRoadSegmentRemoved
        };

        var expectedRecords = Array.ConvertAll(acceptedRoadSegmentAdded.Changes, change =>
        {
            var roadSegmentAdded = change.RoadSegmentAdded;

            var geometry = GeometryTranslator.Translate(roadSegmentAdded.Geometry);
            var polyLineMShapeContent = new PolyLineMShapeContent(Be.Vlaanderen.Basisregisters.Shaperon.Geometries.GeometryTranslator.FromGeometryMultiLineString(geometry));
            var statusTranslation = RoadSegmentStatus.Parse(roadSegmentAdded.Status).Translation;
            var morphologyTranslation = RoadSegmentMorphology.Parse(roadSegmentAdded.Morphology).Translation;
            var categoryTranslation = RoadSegmentCategory.Parse(roadSegmentAdded.Category).Translation;
            var geometryDrawMethodTranslation = RoadSegmentGeometryDrawMethod.Parse(roadSegmentAdded.GeometryDrawMethod).Translation;
            var accessRestrictionTranslation = RoadSegmentAccessRestriction.Parse(roadSegmentAdded.AccessRestriction).Translation;
            var transactionId = new TransactionId(acceptedRoadSegmentRemoved.TransactionId);
            
            return (object)new RoadSegmentV2Record
            {
                Id = roadSegmentAdded.Id,
                StartNodeId = roadSegmentAdded.StartNodeId,
                EndNodeId = roadSegmentAdded.EndNodeId,
                ShapeRecordContent = polyLineMShapeContent.ToBytes(_services.MemoryStreamManager, Encoding.UTF8),
                ShapeRecordContentLength = polyLineMShapeContent.ContentLength.ToInt32(),
                Geometry = geometry,

                Version = roadSegmentAdded.Version,
                GeometryVersion = roadSegmentAdded.GeometryVersion,
                StatusId = statusTranslation.Identifier,
                MorphologyId = morphologyTranslation.Identifier,
                CategoryId = categoryTranslation.Identifier,
                LeftSideStreetNameId = roadSegmentAdded.LeftSide.StreetNameId,
                RightSideStreetNameId = roadSegmentAdded.RightSide.StreetNameId,
                MaintainerId = roadSegmentAdded.MaintenanceAuthority.Code,
                MaintainerName = OrganizationName.FromValueWithFallback(roadSegmentAdded.MaintenanceAuthority.Name),
                MethodId = geometryDrawMethodTranslation.Identifier,
                AccessRestrictionId = accessRestrictionTranslation.Identifier,

                TransactionId = transactionId,
                RecordingDate = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentAdded.When),
                BeginTime = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentRemoved.When),
                BeginOrganizationId = acceptedRoadSegmentAdded.OrganizationId,
                BeginOrganizationName = acceptedRoadSegmentAdded.Organization,

                DbaseRecord = new RoadSegmentDbaseRecord
                {
                    WS_OIDN = { Value = roadSegmentAdded.Id },
                    WS_UIDN = { Value = roadSegmentAdded.Id + "_" + roadSegmentAdded.Version },
                    WS_GIDN = { Value = roadSegmentAdded.Id + "_" + roadSegmentAdded.GeometryVersion },
                    B_WK_OIDN = { Value = roadSegmentAdded.StartNodeId },
                    E_WK_OIDN = { Value = roadSegmentAdded.EndNodeId },
                    STATUS = { Value = statusTranslation.Identifier },
                    LBLSTATUS = { Value = statusTranslation.Name },
                    MORF = { Value = morphologyTranslation.Identifier },
                    LBLMORF = { Value = morphologyTranslation.Name },
                    WEGCAT = { Value = categoryTranslation.Identifier },
                    LBLWEGCAT = { Value = categoryTranslation.Name },
                    LSTRNMID = { Value = roadSegmentAdded.LeftSide.StreetNameId },
                    LSTRNM = { Value = null },
                    RSTRNMID = { Value = roadSegmentAdded.RightSide.StreetNameId },
                    RSTRNM = { Value = null },
                    BEHEER = { Value = roadSegmentAdded.MaintenanceAuthority.Code },
                    LBLBEHEER = { Value = roadSegmentAdded.MaintenanceAuthority.Name },
                    METHODE = { Value = geometryDrawMethodTranslation.Identifier },
                    LBLMETHOD = { Value = geometryDrawMethodTranslation.Name },
                    OPNDATUM = { Value = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentAdded.When) },
                    BEGINTIJD = { Value = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentRemoved.When) },
                    BEGINORG = { Value = acceptedRoadSegmentAdded.OrganizationId },
                    LBLBGNORG = { Value = acceptedRoadSegmentAdded.Organization },
                    TGBEP = { Value = accessRestrictionTranslation.Identifier },
                    LBLTGBEP = { Value = accessRestrictionTranslation.Name }
                }.ToBytes(_services.MemoryStreamManager, Encoding.UTF8),
                LastEventHash = acceptedRoadSegmentRemoved.Changes[0].RoadSegmentRemoved.GetHash(),
                IsRemoved = true
            }.WithBoundingBox(RoadSegmentBoundingBox.From(polyLineMShapeContent.Shape));
        });

        return new RoadSegmentV2RecordProjection(_services.MemoryStreamManager, Encoding.UTF8)
            .Scenario()
            .Given(messages)
            .ExpectWhileIgnoringQueryFilters(expectedRecords);
    }

    [Fact]
    public Task When_road_segments_are_imported()
    {
        var random = new Random();
        var data = _fixture
            .CreateMany<ImportedRoadSegment>(random.Next(1, 10))
            .Select(importedRoadSegment =>
            {
                var geometry = GeometryTranslator.Translate(importedRoadSegment.Geometry);
                var polyLineMShapeContent = new PolyLineMShapeContent(Be.Vlaanderen.Basisregisters.Shaperon.Geometries.GeometryTranslator.FromGeometryMultiLineString(geometry));
                var statusTranslation = RoadSegmentStatus.Parse(importedRoadSegment.Status).Translation;
                var morphologyTranslation = RoadSegmentMorphology.Parse(importedRoadSegment.Morphology).Translation;
                var categoryTranslation = RoadSegmentCategory.Parse(importedRoadSegment.Category).Translation;
                var geometryDrawMethodTranslation = RoadSegmentGeometryDrawMethod.Parse(importedRoadSegment.GeometryDrawMethod).Translation;
                var accessRestrictionTranslation = RoadSegmentAccessRestriction.Parse(importedRoadSegment.AccessRestriction).Translation;
                var transactionId = new TransactionId(importedRoadSegment.Origin.TransactionId);

                var expected = new RoadSegmentV2Record
                {
                    Id = importedRoadSegment.Id,
                    StartNodeId = importedRoadSegment.StartNodeId,
                    EndNodeId = importedRoadSegment.EndNodeId,
                    ShapeRecordContent = polyLineMShapeContent.ToBytes(_services.MemoryStreamManager, Encoding.UTF8),
                    ShapeRecordContentLength = polyLineMShapeContent.ContentLength.ToInt32(),
                    Geometry = geometry,
                    
                    Version = importedRoadSegment.Version,
                    GeometryVersion = importedRoadSegment.GeometryVersion,
                    StatusId = statusTranslation.Identifier,
                    MorphologyId = morphologyTranslation.Identifier,
                    CategoryId = categoryTranslation.Identifier,
                    LeftSideStreetNameId = importedRoadSegment.LeftSide.StreetNameId,
                    RightSideStreetNameId = importedRoadSegment.RightSide.StreetNameId,
                    MaintainerId = importedRoadSegment.MaintenanceAuthority.Code,
                    MaintainerName = OrganizationName.FromValueWithFallback(importedRoadSegment.MaintenanceAuthority.Name),
                    MethodId = geometryDrawMethodTranslation.Identifier,
                    AccessRestrictionId = accessRestrictionTranslation.Identifier,

                    TransactionId = transactionId,
                    RecordingDate = importedRoadSegment.RecordingDate,
                    BeginTime = importedRoadSegment.Origin.Since,
                    BeginOrganizationId = importedRoadSegment.Origin.OrganizationId,
                    BeginOrganizationName = importedRoadSegment.Origin.Organization,
                    
                    DbaseRecord = new RoadSegmentDbaseRecord
                    {
                        WS_OIDN = { Value = importedRoadSegment.Id },
                        WS_UIDN = { Value = importedRoadSegment.Id + "_" + importedRoadSegment.Version },
                        WS_GIDN = { Value = importedRoadSegment.Id + "_" + importedRoadSegment.GeometryVersion },
                        B_WK_OIDN = { Value = importedRoadSegment.StartNodeId },
                        E_WK_OIDN = { Value = importedRoadSegment.EndNodeId },
                        STATUS = { Value = statusTranslation.Identifier },
                        LBLSTATUS = { Value = statusTranslation.Name },
                        MORF = { Value = morphologyTranslation.Identifier },
                        LBLMORF = { Value = morphologyTranslation.Name },
                        WEGCAT = { Value = categoryTranslation.Identifier },
                        LBLWEGCAT = { Value = categoryTranslation.Name },
                        LSTRNMID = { Value = importedRoadSegment.LeftSide.StreetNameId },
                        LSTRNM = { Value = importedRoadSegment.LeftSide.StreetName },
                        RSTRNMID = { Value = importedRoadSegment.RightSide.StreetNameId },
                        RSTRNM = { Value = importedRoadSegment.RightSide.StreetName },
                        BEHEER = { Value = importedRoadSegment.MaintenanceAuthority.Code },
                        LBLBEHEER = { Value = importedRoadSegment.MaintenanceAuthority.Name },
                        METHODE = { Value = geometryDrawMethodTranslation.Identifier },
                        LBLMETHOD = { Value = geometryDrawMethodTranslation.Name },
                        OPNDATUM = { Value = importedRoadSegment.RecordingDate },
                        BEGINTIJD = { Value = importedRoadSegment.Origin.Since },
                        BEGINORG = { Value = importedRoadSegment.Origin.OrganizationId },
                        LBLBGNORG = { Value = importedRoadSegment.Origin.Organization },
                        TGBEP = { Value = accessRestrictionTranslation.Identifier },
                        LBLTGBEP = { Value = accessRestrictionTranslation.Name }
                    }.ToBytes(_services.MemoryStreamManager, Encoding.UTF8),
                    LastEventHash = importedRoadSegment.GetHash()
                }.WithBoundingBox(RoadSegmentBoundingBox.From(polyLineMShapeContent.Shape));
                return new { importedRoadSegment, expected };
            }).ToList();

        return new RoadSegmentV2RecordProjection(_services.MemoryStreamManager, Encoding.UTF8)
            .Scenario()
            .Given(data.Select(d => d.importedRoadSegment))
            .Expect(data.Select(d => d.expected));
    }

    [Fact]
    public Task When_retrieving_road_segments_isremoved_are_ignored()
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

        return new RoadSegmentV2RecordProjection(_services.MemoryStreamManager, Encoding.UTF8)
            .Scenario()
            .Given(messages)
            .ExpectNone();
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
            var roadSegmentAdded = change.RoadSegmentAdded;

            var geometry = GeometryTranslator.Translate(roadSegmentAdded.Geometry);
            var polyLineMShapeContent = new PolyLineMShapeContent(Be.Vlaanderen.Basisregisters.Shaperon.Geometries.GeometryTranslator.FromGeometryMultiLineString(geometry));
            var statusTranslation = RoadSegmentStatus.Parse(roadSegmentAdded.Status).Translation;
            var morphologyTranslation = RoadSegmentMorphology.Parse(roadSegmentAdded.Morphology).Translation;
            var categoryTranslation = RoadSegmentCategory.Parse(roadSegmentAdded.Category).Translation;
            var geometryDrawMethodTranslation = RoadSegmentGeometryDrawMethod.Parse(roadSegmentAdded.GeometryDrawMethod).Translation;
            var accessRestrictionTranslation = RoadSegmentAccessRestriction.Parse(roadSegmentAdded.AccessRestriction).Translation;
            var transactionId = new TransactionId(acceptedRoadSegmentAdded.TransactionId);

            return (object)new RoadSegmentV2Record
            {
                Id = roadSegmentAdded.Id,
                StartNodeId = roadSegmentAdded.StartNodeId,
                EndNodeId = roadSegmentAdded.EndNodeId,
                ShapeRecordContent = polyLineMShapeContent.ToBytes(_services.MemoryStreamManager, Encoding.UTF8),
                ShapeRecordContentLength = polyLineMShapeContent.ContentLength.ToInt32(),
                Geometry = geometry,

                Version = roadSegmentAdded.Version,
                GeometryVersion = roadSegmentAdded.GeometryVersion,
                StatusId = statusTranslation.Identifier,
                MorphologyId = morphologyTranslation.Identifier,
                CategoryId = categoryTranslation.Identifier,
                LeftSideStreetNameId = roadSegmentAdded.LeftSide.StreetNameId,
                RightSideStreetNameId = roadSegmentAdded.RightSide.StreetNameId,
                MaintainerId = roadSegmentAdded.MaintenanceAuthority.Code,
                MaintainerName = OrganizationName.FromValueWithFallback(roadSegmentAdded.MaintenanceAuthority.Name),
                MethodId = geometryDrawMethodTranslation.Identifier,
                AccessRestrictionId = accessRestrictionTranslation.Identifier,

                TransactionId = transactionId,
                RecordingDate = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentAdded.When),
                BeginTime = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentAdded.When),
                BeginOrganizationId = acceptedRoadSegmentAdded.OrganizationId,
                BeginOrganizationName = acceptedRoadSegmentAdded.Organization,

                DbaseRecord = new RoadSegmentDbaseRecord
                {
                    WS_OIDN = { Value = roadSegmentAdded.Id },
                    WS_UIDN = { Value = roadSegmentAdded.Id + "_" + roadSegmentAdded.Version },
                    WS_GIDN = { Value = roadSegmentAdded.Id + "_" + roadSegmentAdded.GeometryVersion },
                    B_WK_OIDN = { Value = roadSegmentAdded.StartNodeId },
                    E_WK_OIDN = { Value = roadSegmentAdded.EndNodeId },
                    STATUS = { Value = statusTranslation.Identifier },
                    LBLSTATUS = { Value = statusTranslation.Name },
                    MORF = { Value = morphologyTranslation.Identifier },
                    LBLMORF = { Value = morphologyTranslation.Name },
                    WEGCAT = { Value = categoryTranslation.Identifier },
                    LBLWEGCAT = { Value = categoryTranslation.Name },
                    LSTRNMID = { Value = roadSegmentAdded.LeftSide.StreetNameId },
                    LSTRNM = { Value = null },
                    RSTRNMID = { Value = roadSegmentAdded.RightSide.StreetNameId },
                    RSTRNM = { Value = null },
                    BEHEER = { Value = roadSegmentAdded.MaintenanceAuthority.Code },
                    LBLBEHEER = { Value = renameOrganizationAccepted.Name },
                    METHODE = { Value = geometryDrawMethodTranslation.Identifier },
                    LBLMETHOD = { Value = geometryDrawMethodTranslation.Name },
                    OPNDATUM = { Value = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentAdded.When) },
                    BEGINTIJD = { Value = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentAdded.When) },
                    BEGINORG = { Value = acceptedRoadSegmentAdded.OrganizationId },
                    LBLBGNORG = { Value = acceptedRoadSegmentAdded.Organization },
                    TGBEP = { Value = accessRestrictionTranslation.Identifier },
                    LBLTGBEP = { Value = accessRestrictionTranslation.Name }
                }.ToBytes(_services.MemoryStreamManager, Encoding.UTF8),
                LastEventHash = roadSegmentAdded.GetHash()
            }.WithBoundingBox(RoadSegmentBoundingBox.From(polyLineMShapeContent.Shape));
        });

        return new RoadSegmentV2RecordProjection(_services.MemoryStreamManager, Encoding.UTF8)
            .Scenario()
            .Given(messages)
            .Expect(expectedRecords);
    }
}
