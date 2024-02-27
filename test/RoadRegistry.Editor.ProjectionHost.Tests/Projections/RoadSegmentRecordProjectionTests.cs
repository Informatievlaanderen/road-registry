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
using Microsoft.Extensions.Logging.Abstractions;
using RoadRegistry.Tests.BackOffice;
using RoadRegistry.Tests.Framework.Projections;

public class RoadSegmentRecordProjectionTests : IClassFixture<ProjectionTestServices>
{
    private readonly Fixture _fixture;
    private readonly ProjectionTestServices _services;

    public RoadSegmentRecordProjectionTests(ProjectionTestServices services)
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
            var segment = change.RoadSegmentAdded;
            var geometry = GeometryTranslator.Translate(segment.Geometry);
            var polyLineMShapeContent = new PolyLineMShapeContent(Be.Vlaanderen.Basisregisters.Shaperon.Geometries.GeometryTranslator.FromGeometryMultiLineString(geometry));

            return (object)new RoadSegmentRecord
            {
                Id = segment.Id,
                StartNodeId = segment.StartNodeId,
                EndNodeId = segment.EndNodeId,
                ShapeRecordContent = polyLineMShapeContent.ToBytes(_services.MemoryStreamManager, Encoding.UTF8),
                ShapeRecordContentLength = polyLineMShapeContent.ContentLength.ToInt32(),
                Geometry = geometry,
                DbaseRecord = new RoadSegmentDbaseRecord
                {
                    WS_OIDN = { Value = segment.Id },
                    WS_UIDN = { Value = segment.Id + "_" + segment.Version },
                    WS_GIDN = { Value = segment.Id + "_" + segment.GeometryVersion },
                    B_WK_OIDN = { Value = segment.StartNodeId },
                    E_WK_OIDN = { Value = segment.EndNodeId },
                    STATUS = { Value = RoadSegmentStatus.Parse(segment.Status).Translation.Identifier },
                    LBLSTATUS = { Value = RoadSegmentStatus.Parse(segment.Status).Translation.Name },
                    MORF = { Value = RoadSegmentMorphology.Parse(segment.Morphology).Translation.Identifier },
                    LBLMORF = { Value = RoadSegmentMorphology.Parse(segment.Morphology).Translation.Name },
                    WEGCAT = { Value = RoadSegmentCategory.Parse(segment.Category).Translation.Identifier },
                    LBLWEGCAT = { Value = RoadSegmentCategory.Parse(segment.Category).Translation.Name },
                    LSTRNMID = { Value = segment.LeftSide.StreetNameId },
                    LSTRNM = { Value = null },
                    RSTRNMID = { Value = segment.RightSide.StreetNameId },
                    RSTRNM = { Value = null },
                    BEHEER = { Value = segment.MaintenanceAuthority.Code },
                    LBLBEHEER = { Value = segment.MaintenanceAuthority.Name },
                    METHODE = { Value = RoadSegmentGeometryDrawMethod.Parse(segment.GeometryDrawMethod).Translation.Identifier },
                    LBLMETHOD = { Value = RoadSegmentGeometryDrawMethod.Parse(segment.GeometryDrawMethod).Translation.Name },
                    OPNDATUM = { Value = LocalDateTimeTranslator.TranslateFromWhen(message.When) },
                    BEGINTIJD = { Value = LocalDateTimeTranslator.TranslateFromWhen(message.When) },
                    BEGINORG = { Value = message.OrganizationId },
                    LBLBGNORG = { Value = message.Organization },
                    TGBEP = { Value = RoadSegmentAccessRestriction.Parse(segment.AccessRestriction).Translation.Identifier },
                    LBLTGBEP = { Value = RoadSegmentAccessRestriction.Parse(segment.AccessRestriction).Translation.Name }
                }.ToBytes(_services.MemoryStreamManager, Encoding.UTF8),
                LastEventHash = segment.GetHash()
            }.WithBoundingBox(RoadSegmentBoundingBox.From(polyLineMShapeContent.Shape));
        });

        return new RoadSegmentRecordProjection(_services.MemoryStreamManager, Encoding.UTF8, new NullLogger<RoadSegmentRecordProjection>())
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
            var segmentAdded = acceptedRoadSegmentAdded.Changes[0].RoadSegmentAdded;

            var segment = change.RoadSegmentAttributesModified;
            var geometry = GeometryTranslator.Translate(segmentAdded.Geometry);
            var polyLineMShapeContent = new PolyLineMShapeContent(Be.Vlaanderen.Basisregisters.Shaperon.Geometries.GeometryTranslator.FromGeometryMultiLineString(geometry));

            return (object)new RoadSegmentRecord
            {
                Id = segment.Id,
                StartNodeId = segmentAdded.StartNodeId,
                EndNodeId = segmentAdded.EndNodeId,
                ShapeRecordContent = polyLineMShapeContent.ToBytes(_services.MemoryStreamManager, Encoding.UTF8),
                ShapeRecordContentLength = polyLineMShapeContent.ContentLength.ToInt32(),
                Geometry = geometry,
                DbaseRecord = new RoadSegmentDbaseRecord
                {
                    WS_OIDN = { Value = segment.Id },
                    WS_UIDN = { Value = segment.Id + "_" + segment.Version },
                    WS_GIDN = { Value = segment.Id + "_" + segmentAdded.GeometryVersion },
                    B_WK_OIDN = { Value = segmentAdded.StartNodeId },
                    E_WK_OIDN = { Value = segmentAdded.EndNodeId },
                    STATUS = { Value = RoadSegmentStatus.Parse(segment.Status).Translation.Identifier },
                    LBLSTATUS = { Value = RoadSegmentStatus.Parse(segment.Status).Translation.Name },
                    MORF = { Value = RoadSegmentMorphology.Parse(segment.Morphology).Translation.Identifier },
                    LBLMORF = { Value = RoadSegmentMorphology.Parse(segment.Morphology).Translation.Name },
                    WEGCAT = { Value = RoadSegmentCategory.Parse(segment.Category).Translation.Identifier },
                    LBLWEGCAT = { Value = RoadSegmentCategory.Parse(segment.Category).Translation.Name },
                    LSTRNMID = { Value = segmentAdded.LeftSide.StreetNameId },
                    LSTRNM = { Value = null },
                    RSTRNMID = { Value = segmentAdded.RightSide.StreetNameId },
                    RSTRNM = { Value = null },
                    BEHEER = { Value = segment.MaintenanceAuthority.Code },
                    LBLBEHEER = { Value = segment.MaintenanceAuthority.Name },
                    METHODE = { Value = RoadSegmentGeometryDrawMethod.Parse(segmentAdded.GeometryDrawMethod).Translation.Identifier },
                    LBLMETHOD = { Value = RoadSegmentGeometryDrawMethod.Parse(segmentAdded.GeometryDrawMethod).Translation.Name },
                    OPNDATUM = { Value = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentAdded.When) },
                    BEGINTIJD = { Value = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentAttributesModified.When) },
                    BEGINORG = { Value = acceptedRoadSegmentAdded.OrganizationId },
                    LBLBGNORG = { Value = acceptedRoadSegmentAdded.Organization },
                    TGBEP = { Value = RoadSegmentAccessRestriction.Parse(segment.AccessRestriction).Translation.Identifier },
                    LBLTGBEP = { Value = RoadSegmentAccessRestriction.Parse(segment.AccessRestriction).Translation.Name }
                }.ToBytes(_services.MemoryStreamManager, Encoding.UTF8),
                LastEventHash = segment.GetHash()
            }.WithBoundingBox(RoadSegmentBoundingBox.From(polyLineMShapeContent.Shape));
        });

        return new RoadSegmentRecordProjection(_services.MemoryStreamManager, Encoding.UTF8, new NullLogger<RoadSegmentRecordProjection>())
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
            var geometry = GeometryTranslator.Translate(segment.Geometry);
            var polyLineMShapeContent = new PolyLineMShapeContent(Be.Vlaanderen.Basisregisters.Shaperon.Geometries.GeometryTranslator.FromGeometryMultiLineString(geometry));

            return (object)new RoadSegmentRecord
            {
                Id = segment.Id,
                StartNodeId = segmentAdded.StartNodeId,
                EndNodeId = segmentAdded.EndNodeId,
                ShapeRecordContent = polyLineMShapeContent.ToBytes(_services.MemoryStreamManager, Encoding.UTF8),
                ShapeRecordContentLength = polyLineMShapeContent.ContentLength.ToInt32(),
                Geometry = geometry,
                DbaseRecord = new RoadSegmentDbaseRecord
                {
                    WS_OIDN = { Value = segment.Id },
                    WS_UIDN = { Value = segment.Id + "_" + segment.Version },
                    WS_GIDN = { Value = segment.Id + "_" + segment.GeometryVersion },
                    B_WK_OIDN = { Value = segmentAdded.StartNodeId },
                    E_WK_OIDN = { Value = segmentAdded.EndNodeId },
                    STATUS = { Value = RoadSegmentStatus.Parse(segmentAdded.Status).Translation.Identifier },
                    LBLSTATUS = { Value = RoadSegmentStatus.Parse(segmentAdded.Status).Translation.Name },
                    MORF = { Value = RoadSegmentMorphology.Parse(segmentAdded.Morphology).Translation.Identifier },
                    LBLMORF = { Value = RoadSegmentMorphology.Parse(segmentAdded.Morphology).Translation.Name },
                    WEGCAT = { Value = RoadSegmentCategory.Parse(segmentAdded.Category).Translation.Identifier },
                    LBLWEGCAT = { Value = RoadSegmentCategory.Parse(segmentAdded.Category).Translation.Name },
                    LSTRNMID = { Value = segmentAdded.LeftSide.StreetNameId },
                    LSTRNM = { Value = null },
                    RSTRNMID = { Value = segmentAdded.RightSide.StreetNameId },
                    RSTRNM = { Value = null },
                    BEHEER = { Value = segmentAdded.MaintenanceAuthority.Code },
                    LBLBEHEER = { Value = segmentAdded.MaintenanceAuthority.Name },
                    METHODE = { Value = RoadSegmentGeometryDrawMethod.Parse(segmentAdded.GeometryDrawMethod).Translation.Identifier },
                    LBLMETHOD = { Value = RoadSegmentGeometryDrawMethod.Parse(segmentAdded.GeometryDrawMethod).Translation.Name },
                    OPNDATUM = { Value = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentAdded.When) },
                    BEGINTIJD = { Value = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentGeometryModified.When) },
                    BEGINORG = { Value = acceptedRoadSegmentAdded.OrganizationId },
                    LBLBGNORG = { Value = acceptedRoadSegmentAdded.Organization },
                    TGBEP = { Value = RoadSegmentAccessRestriction.Parse(segmentAdded.AccessRestriction).Translation.Identifier },
                    LBLTGBEP = { Value = RoadSegmentAccessRestriction.Parse(segmentAdded.AccessRestriction).Translation.Name }
                }.ToBytes(_services.MemoryStreamManager, Encoding.UTF8),
                LastEventHash = segment.GetHash()
            }.WithBoundingBox(RoadSegmentBoundingBox.From(polyLineMShapeContent.Shape));
        });

        return new RoadSegmentRecordProjection(_services.MemoryStreamManager, Encoding.UTF8, new NullLogger<RoadSegmentRecordProjection>())
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
            var geometry = GeometryTranslator.Translate(segment.Geometry);
            var polyLineMShapeContent = new PolyLineMShapeContent(Be.Vlaanderen.Basisregisters.Shaperon.Geometries.GeometryTranslator.FromGeometryMultiLineString(geometry));

            return (object)new RoadSegmentRecord
            {
                Id = segment.Id,
                StartNodeId = segment.StartNodeId,
                EndNodeId = segment.EndNodeId,
                ShapeRecordContent = polyLineMShapeContent.ToBytes(_services.MemoryStreamManager, Encoding.UTF8),
                ShapeRecordContentLength = polyLineMShapeContent.ContentLength.ToInt32(),
                Geometry = geometry,
                DbaseRecord = new RoadSegmentDbaseRecord
                {
                    WS_OIDN = { Value = segment.Id },
                    WS_UIDN = { Value = segment.Id + "_" + segment.Version },
                    WS_GIDN = { Value = segment.Id + "_" + segment.GeometryVersion },
                    B_WK_OIDN = { Value = segment.StartNodeId },
                    E_WK_OIDN = { Value = segment.EndNodeId },
                    STATUS = { Value = RoadSegmentStatus.Parse(segment.Status).Translation.Identifier },
                    LBLSTATUS = { Value = RoadSegmentStatus.Parse(segment.Status).Translation.Name },
                    MORF = { Value = RoadSegmentMorphology.Parse(segment.Morphology).Translation.Identifier },
                    LBLMORF = { Value = RoadSegmentMorphology.Parse(segment.Morphology).Translation.Name },
                    WEGCAT = { Value = RoadSegmentCategory.Parse(segment.Category).Translation.Identifier },
                    LBLWEGCAT = { Value = RoadSegmentCategory.Parse(segment.Category).Translation.Name },
                    LSTRNMID = { Value = segment.LeftSide.StreetNameId },
                    LSTRNM = { Value = null },
                    RSTRNMID = { Value = segment.RightSide.StreetNameId },
                    RSTRNM = { Value = null },
                    BEHEER = { Value = segment.MaintenanceAuthority.Code },
                    LBLBEHEER = { Value = segment.MaintenanceAuthority.Name },
                    METHODE = { Value = RoadSegmentGeometryDrawMethod.Parse(segment.GeometryDrawMethod).Translation.Identifier },
                    LBLMETHOD = { Value = RoadSegmentGeometryDrawMethod.Parse(segment.GeometryDrawMethod).Translation.Name },
                    OPNDATUM = { Value = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentAdded.When) },
                    BEGINTIJD = { Value = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentModified.When) },
                    BEGINORG = { Value = acceptedRoadSegmentAdded.OrganizationId },
                    LBLBGNORG = { Value = acceptedRoadSegmentAdded.Organization },
                    TGBEP = { Value = RoadSegmentAccessRestriction.Parse(segment.AccessRestriction).Translation.Identifier },
                    LBLTGBEP = { Value = RoadSegmentAccessRestriction.Parse(segment.AccessRestriction).Translation.Name }
                }.ToBytes(_services.MemoryStreamManager, Encoding.UTF8),
                LastEventHash = segment.GetHash()
            }.WithBoundingBox(RoadSegmentBoundingBox.From(polyLineMShapeContent.Shape));
        });

        return new RoadSegmentRecordProjection(_services.MemoryStreamManager, Encoding.UTF8, new NullLogger<RoadSegmentRecordProjection>())
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
            var segment = change.RoadSegmentAdded;
            var geometry = GeometryTranslator.Translate(segment.Geometry);
            var polyLineMShapeContent = new PolyLineMShapeContent(Be.Vlaanderen.Basisregisters.Shaperon.Geometries.GeometryTranslator.FromGeometryMultiLineString(geometry));

            return (object)new RoadSegmentRecord
            {
                Id = segment.Id,
                StartNodeId = segment.StartNodeId,
                EndNodeId = segment.EndNodeId,
                ShapeRecordContent = polyLineMShapeContent.ToBytes(_services.MemoryStreamManager, Encoding.UTF8),
                ShapeRecordContentLength = polyLineMShapeContent.ContentLength.ToInt32(),
                Geometry = geometry,
                DbaseRecord = new RoadSegmentDbaseRecord
                {
                    WS_OIDN = { Value = segment.Id },
                    WS_UIDN = { Value = segment.Id + "_" + segment.Version },
                    WS_GIDN = { Value = segment.Id + "_" + segment.GeometryVersion },
                    B_WK_OIDN = { Value = segment.StartNodeId },
                    E_WK_OIDN = { Value = segment.EndNodeId },
                    STATUS = { Value = RoadSegmentStatus.Parse(segment.Status).Translation.Identifier },
                    LBLSTATUS = { Value = RoadSegmentStatus.Parse(segment.Status).Translation.Name },
                    MORF = { Value = RoadSegmentMorphology.Parse(segment.Morphology).Translation.Identifier },
                    LBLMORF = { Value = RoadSegmentMorphology.Parse(segment.Morphology).Translation.Name },
                    WEGCAT = { Value = RoadSegmentCategory.Parse(segment.Category).Translation.Identifier },
                    LBLWEGCAT = { Value = RoadSegmentCategory.Parse(segment.Category).Translation.Name },
                    LSTRNMID = { Value = segment.LeftSide.StreetNameId },
                    LSTRNM = { Value = null },
                    RSTRNMID = { Value = segment.RightSide.StreetNameId },
                    RSTRNM = { Value = null },
                    BEHEER = { Value = segment.MaintenanceAuthority.Code },
                    LBLBEHEER = { Value = segment.MaintenanceAuthority.Name },
                    METHODE = { Value = RoadSegmentGeometryDrawMethod.Parse(segment.GeometryDrawMethod).Translation.Identifier },
                    LBLMETHOD = { Value = RoadSegmentGeometryDrawMethod.Parse(segment.GeometryDrawMethod).Translation.Name },
                    OPNDATUM = { Value = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentAdded.When) },
                    BEGINTIJD = { Value = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentRemoved.When) },
                    BEGINORG = { Value = acceptedRoadSegmentAdded.OrganizationId },
                    LBLBGNORG = { Value = acceptedRoadSegmentAdded.Organization },
                    TGBEP = { Value = RoadSegmentAccessRestriction.Parse(segment.AccessRestriction).Translation.Identifier },
                    LBLTGBEP = { Value = RoadSegmentAccessRestriction.Parse(segment.AccessRestriction).Translation.Name }
                }.ToBytes(_services.MemoryStreamManager, Encoding.UTF8),
                LastEventHash = acceptedRoadSegmentRemoved.Changes[0].RoadSegmentRemoved.GetHash(),
                IsRemoved = true
            }.WithBoundingBox(RoadSegmentBoundingBox.From(polyLineMShapeContent.Shape));
        });

        return new RoadSegmentRecordProjection(_services.MemoryStreamManager, Encoding.UTF8, new NullLogger<RoadSegmentRecordProjection>())
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

                var expected = new RoadSegmentRecord
                {
                    Id = importedRoadSegment.Id,
                    StartNodeId = importedRoadSegment.StartNodeId,
                    EndNodeId = importedRoadSegment.EndNodeId,
                    ShapeRecordContent = polyLineMShapeContent.ToBytes(_services.MemoryStreamManager, Encoding.UTF8),
                    ShapeRecordContentLength = polyLineMShapeContent.ContentLength.ToInt32(),
                    Geometry = geometry,
                    DbaseRecord = new RoadSegmentDbaseRecord
                    {
                        WS_OIDN = { Value = importedRoadSegment.Id },
                        WS_UIDN = { Value = importedRoadSegment.Id + "_" + importedRoadSegment.Version },
                        WS_GIDN = { Value = importedRoadSegment.Id + "_" + importedRoadSegment.GeometryVersion },
                        B_WK_OIDN = { Value = importedRoadSegment.StartNodeId },
                        E_WK_OIDN = { Value = importedRoadSegment.EndNodeId },
                        STATUS = { Value = RoadSegmentStatus.Parse(importedRoadSegment.Status).Translation.Identifier },
                        LBLSTATUS = { Value = RoadSegmentStatus.Parse(importedRoadSegment.Status).Translation.Name },
                        MORF = { Value = RoadSegmentMorphology.Parse(importedRoadSegment.Morphology).Translation.Identifier },
                        LBLMORF = { Value = RoadSegmentMorphology.Parse(importedRoadSegment.Morphology).Translation.Name },
                        WEGCAT = { Value = RoadSegmentCategory.Parse(importedRoadSegment.Category).Translation.Identifier },
                        LBLWEGCAT = { Value = RoadSegmentCategory.Parse(importedRoadSegment.Category).Translation.Name },
                        LSTRNMID = { Value = importedRoadSegment.LeftSide.StreetNameId },
                        LSTRNM = { Value = importedRoadSegment.LeftSide.StreetName },
                        RSTRNMID = { Value = importedRoadSegment.RightSide.StreetNameId },
                        RSTRNM = { Value = importedRoadSegment.RightSide.StreetName },
                        BEHEER = { Value = importedRoadSegment.MaintenanceAuthority.Code },
                        LBLBEHEER = { Value = importedRoadSegment.MaintenanceAuthority.Name },
                        METHODE = { Value = RoadSegmentGeometryDrawMethod.Parse(importedRoadSegment.GeometryDrawMethod).Translation.Identifier },
                        LBLMETHOD = { Value = RoadSegmentGeometryDrawMethod.Parse(importedRoadSegment.GeometryDrawMethod).Translation.Name },
                        OPNDATUM = { Value = importedRoadSegment.RecordingDate },
                        BEGINTIJD = { Value = importedRoadSegment.Origin.Since },
                        BEGINORG = { Value = importedRoadSegment.Origin.OrganizationId },
                        LBLBGNORG = { Value = importedRoadSegment.Origin.Organization },
                        TGBEP = { Value = RoadSegmentAccessRestriction.Parse(importedRoadSegment.AccessRestriction).Translation.Identifier },
                        LBLTGBEP = { Value = RoadSegmentAccessRestriction.Parse(importedRoadSegment.AccessRestriction).Translation.Name }
                    }.ToBytes(_services.MemoryStreamManager, Encoding.UTF8),
                    LastEventHash = importedRoadSegment.GetHash()
                }.WithBoundingBox(RoadSegmentBoundingBox.From(polyLineMShapeContent.Shape));
                return new { importedRoadSegment, expected };
            }).ToList();

        return new RoadSegmentRecordProjection(_services.MemoryStreamManager, Encoding.UTF8, new NullLogger<RoadSegmentRecordProjection>())
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
        
        return new RoadSegmentRecordProjection(_services.MemoryStreamManager, Encoding.UTF8, new NullLogger<RoadSegmentRecordProjection>())
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
            var segment = change.RoadSegmentAdded;
            var geometry = GeometryTranslator.Translate(segment.Geometry);
            var polyLineMShapeContent = new PolyLineMShapeContent(Be.Vlaanderen.Basisregisters.Shaperon.Geometries.GeometryTranslator.FromGeometryMultiLineString(geometry));

            return (object)new RoadSegmentRecord
            {
                Id = segment.Id,
                StartNodeId = segment.StartNodeId,
                EndNodeId = segment.EndNodeId,
                ShapeRecordContent = polyLineMShapeContent.ToBytes(_services.MemoryStreamManager, Encoding.UTF8),
                ShapeRecordContentLength = polyLineMShapeContent.ContentLength.ToInt32(),
                Geometry = geometry,
                DbaseRecord = new RoadSegmentDbaseRecord
                {
                    WS_OIDN = { Value = segment.Id },
                    WS_UIDN = { Value = segment.Id + "_" + segment.Version },
                    WS_GIDN = { Value = segment.Id + "_" + segment.GeometryVersion },
                    B_WK_OIDN = { Value = segment.StartNodeId },
                    E_WK_OIDN = { Value = segment.EndNodeId },
                    STATUS = { Value = RoadSegmentStatus.Parse(segment.Status).Translation.Identifier },
                    LBLSTATUS = { Value = RoadSegmentStatus.Parse(segment.Status).Translation.Name },
                    MORF = { Value = RoadSegmentMorphology.Parse(segment.Morphology).Translation.Identifier },
                    LBLMORF = { Value = RoadSegmentMorphology.Parse(segment.Morphology).Translation.Name },
                    WEGCAT = { Value = RoadSegmentCategory.Parse(segment.Category).Translation.Identifier },
                    LBLWEGCAT = { Value = RoadSegmentCategory.Parse(segment.Category).Translation.Name },
                    LSTRNMID = { Value = segment.LeftSide.StreetNameId },
                    LSTRNM = { Value = null },
                    RSTRNMID = { Value = segment.RightSide.StreetNameId },
                    RSTRNM = { Value = null },
                    BEHEER = { Value = segment.MaintenanceAuthority.Code },
                    LBLBEHEER = { Value = renameOrganizationAccepted.Name },
                    METHODE = { Value = RoadSegmentGeometryDrawMethod.Parse(segment.GeometryDrawMethod).Translation.Identifier },
                    LBLMETHOD = { Value = RoadSegmentGeometryDrawMethod.Parse(segment.GeometryDrawMethod).Translation.Name },
                    OPNDATUM = { Value = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentAdded.When) },
                    BEGINTIJD = { Value = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentAdded.When) },
                    BEGINORG = { Value = acceptedRoadSegmentAdded.OrganizationId },
                    LBLBGNORG = { Value = acceptedRoadSegmentAdded.Organization },
                    TGBEP = { Value = RoadSegmentAccessRestriction.Parse(segment.AccessRestriction).Translation.Identifier },
                    LBLTGBEP = { Value = RoadSegmentAccessRestriction.Parse(segment.AccessRestriction).Translation.Name }
                }.ToBytes(_services.MemoryStreamManager, Encoding.UTF8),
                LastEventHash = segment.GetHash()
            }.WithBoundingBox(RoadSegmentBoundingBox.From(polyLineMShapeContent.Shape));
        });

        return new RoadSegmentRecordProjection(_services.MemoryStreamManager, Encoding.UTF8, new NullLogger<RoadSegmentRecordProjection>())
            .Scenario()
            .Given(messages)
            .Expect(expectedRecords);
    }
}
