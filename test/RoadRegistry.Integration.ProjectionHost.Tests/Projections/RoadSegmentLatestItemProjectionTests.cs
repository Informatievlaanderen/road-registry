namespace RoadRegistry.Integration.ProjectionHost.Tests.Projections;

using AutoFixture;
using BackOffice;
using BackOffice.Messages;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Integration.Projections;
using RoadRegistry.Tests.BackOffice;
using RoadRegistry.Tests.BackOffice.Scenarios;
using Schema.RoadSegments;

public class RoadSegmentLatestItemProjectionTests
{
    private readonly Fixture _fixture;

    public RoadSegmentLatestItemProjectionTests()
    {
        _fixture = new RoadNetworkTestData().ObjectProvider;
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
        _fixture.CustomizeRoadSegmentAddedToEuropeanRoad();
        _fixture.CustomizeRoadSegmentRemovedFromEuropeanRoad();
        _fixture.CustomizeRoadSegmentAddedToNationalRoad();
        _fixture.CustomizeRoadSegmentRemovedFromNationalRoad();
        _fixture.CustomizeRoadSegmentAddedToNumberedRoad();
        _fixture.CustomizeRoadSegmentRemovedFromNumberedRoad();
    }

    [Fact]
    public Task When_adding_road_segments()
    {
        var message = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.CreateMany<RoadSegmentAdded>());

        var expectedRecords = Array.ConvertAll(message.Changes, change =>
        {
            return (object)BuildRoadSegmentRecord(message, change.RoadSegmentAdded, _ => { });
        });

        return BuildProjection()
            .Scenario()
            .Given(message)
            .Expect(expectedRecords);
    }

    [Fact]
    public Task When_adding_road_segment_to_european_road()
    {
        _fixture.Freeze<RoadSegmentId>();

        var acceptedRoadSegmentAdded = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.Create<RoadSegmentAdded>());

        var message = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.Create<RoadSegmentAddedToEuropeanRoad>());

        var expectedRecords = Array.ConvertAll(message.Changes, change =>
        {
            var roadSegmentAddedToEuropeanRoad = change.RoadSegmentAddedToEuropeanRoad;

            return (object)BuildRoadSegmentRecord(
                acceptedRoadSegmentAdded,
                acceptedRoadSegmentAdded.Changes.Single().RoadSegmentAdded,
                record =>
                {
                    record.Version = roadSegmentAddedToEuropeanRoad.SegmentVersion!.Value;

                    record.OrganizationId = message.OrganizationId;
                    record.OrganizationName = message.Organization;
                    record.VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(message.When);
                });
        });

        return BuildProjection()
            .Scenario()
            .Given(acceptedRoadSegmentAdded, message)
            .Expect(expectedRecords);
    }

    [Fact]
    public Task When_adding_road_segment_to_national_road()
    {
        _fixture.Freeze<RoadSegmentId>();

        var acceptedRoadSegmentAdded = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.Create<RoadSegmentAdded>());

        var message = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.Create<RoadSegmentAddedToNationalRoad>());

        var expectedRecords = Array.ConvertAll(message.Changes, change =>
        {
            var roadSegmentAddedToNationalRoad = change.RoadSegmentAddedToNationalRoad;

            return (object)BuildRoadSegmentRecord(
                acceptedRoadSegmentAdded,
                acceptedRoadSegmentAdded.Changes.Single().RoadSegmentAdded,
                record =>
                {
                    record.Version = roadSegmentAddedToNationalRoad.SegmentVersion!.Value;

                    record.OrganizationId = message.OrganizationId;
                    record.OrganizationName = message.Organization;
                    record.VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(message.When);
                });
        });

        return BuildProjection()
            .Scenario()
            .Given(acceptedRoadSegmentAdded, message)
            .Expect(expectedRecords);
    }

    [Fact]
    public Task When_adding_road_segment_to_numbered_road()
    {
        _fixture.Freeze<RoadSegmentId>();

        var acceptedRoadSegmentAdded = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.Create<RoadSegmentAdded>());

        var message = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.Create<RoadSegmentAddedToNumberedRoad>());

        var expectedRecords = Array.ConvertAll(message.Changes, change =>
        {
            var roadSegmentAddedToNumberedRoad = change.RoadSegmentAddedToNumberedRoad;

            return (object)BuildRoadSegmentRecord(
                acceptedRoadSegmentAdded,
                acceptedRoadSegmentAdded.Changes.Single().RoadSegmentAdded,
                record =>
                {
                    record.Version = roadSegmentAddedToNumberedRoad.SegmentVersion!.Value;

                    record.OrganizationId = message.OrganizationId;
                    record.OrganizationName = message.Organization;
                    record.VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(message.When);
                });
        });

        return BuildProjection()
            .Scenario()
            .Given(acceptedRoadSegmentAdded, message)
            .Expect(expectedRecords);
    }

    [Fact]
    public Task When_removing_road_segment_from_european_road()
    {
        _fixture.Freeze<RoadSegmentId>();

        var acceptedRoadSegmentAdded = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.Create<RoadSegmentAdded>());

        var message = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.Create<RoadSegmentRemovedFromEuropeanRoad>());

        var expectedRecords = Array.ConvertAll(message.Changes, change =>
        {
            var roadSegmentRemovedFromEuropeanRoad = change.RoadSegmentRemovedFromEuropeanRoad;

            return (object)BuildRoadSegmentRecord(
                acceptedRoadSegmentAdded,
                acceptedRoadSegmentAdded.Changes.Single().RoadSegmentAdded,
                record =>
                {
                    record.Version = roadSegmentRemovedFromEuropeanRoad.SegmentVersion!.Value;

                    record.OrganizationId = message.OrganizationId;
                    record.OrganizationName = message.Organization;
                    record.VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(message.When);
                });
        });

        return BuildProjection()
            .Scenario()
            .Given(acceptedRoadSegmentAdded, message)
            .Expect(expectedRecords);
    }

    [Fact]
    public Task When_removing_road_segment_from_national_road()
    {
        _fixture.Freeze<RoadSegmentId>();

        var acceptedRoadSegmentAdded = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.Create<RoadSegmentAdded>());

        var message = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.Create<RoadSegmentRemovedFromNationalRoad>());

        var expectedRecords = Array.ConvertAll(message.Changes, change =>
        {
            var roadSegmentRemovedFromNationalRoad = change.RoadSegmentRemovedFromNationalRoad;

            return (object)BuildRoadSegmentRecord(
                acceptedRoadSegmentAdded,
                acceptedRoadSegmentAdded.Changes.Single().RoadSegmentAdded,
                record =>
                {
                    record.Version = roadSegmentRemovedFromNationalRoad.SegmentVersion!.Value;

                    record.OrganizationId = message.OrganizationId;
                    record.OrganizationName = message.Organization;
                    record.VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(message.When);
                });
        });

        return BuildProjection()
            .Scenario()
            .Given(acceptedRoadSegmentAdded, message)
            .Expect(expectedRecords);
    }

    [Fact]
    public Task When_removing_road_segment_from_numbered_road()
    {
        _fixture.Freeze<RoadSegmentId>();

        var acceptedRoadSegmentAdded = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.Create<RoadSegmentAdded>());

        var message = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.Create<RoadSegmentRemovedFromNumberedRoad>());

        var expectedRecords = Array.ConvertAll(message.Changes, change =>
        {
            var roadSegmentRemovedFromNumberedRoad = change.RoadSegmentRemovedFromNumberedRoad;

            return (object)BuildRoadSegmentRecord(
                acceptedRoadSegmentAdded,
                acceptedRoadSegmentAdded.Changes.Single().RoadSegmentAdded,
                record =>
                {
                    record.Version = roadSegmentRemovedFromNumberedRoad.SegmentVersion!.Value;

                    record.OrganizationId = message.OrganizationId;
                    record.OrganizationName = message.Organization;
                    record.VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(message.When);
                });
        });

        return BuildProjection()
            .Scenario()
            .Given(acceptedRoadSegmentAdded, message)
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

            return (object)new RoadSegmentLatestItem
            {
                Id = roadSegmentAttributesModified.Id,
                StartNodeId = roadSegmentAdded.StartNodeId,
                EndNodeId = roadSegmentAdded.EndNodeId,
                Geometry = geometry,

                Version = roadSegmentAttributesModified.Version,
                GeometryVersion = roadSegmentAdded.GeometryVersion,
                StatusId = statusTranslation.Identifier,
                MorphologyId = morphologyTranslation.Identifier,
                CategoryId = categoryTranslation.Identifier,
                LeftSideStreetNameId = roadSegmentAdded.LeftSide.StreetNameId,
                RightSideStreetNameId = roadSegmentAdded.RightSide.StreetNameId,
                MaintainerId = roadSegmentAttributesModified.MaintenanceAuthority.Code,
                MethodId = geometryDrawMethodTranslation.Identifier,
                AccessRestrictionId = accessRestrictionTranslation.Identifier,

                CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentAdded.When),
                VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentAttributesModified.When),
                OrganizationId = acceptedRoadSegmentAttributesModified.OrganizationId,
                OrganizationName = acceptedRoadSegmentAttributesModified.Organization,

                StatusLabel = statusTranslation.Name,
                MorphologyLabel = morphologyTranslation.Name,
                CategoryLabel = categoryTranslation.Name,
                MethodLabel = geometryDrawMethodTranslation.Name,
                AccessRestrictionLabel = accessRestrictionTranslation.Name
            }.WithBoundingBox(RoadSegmentBoundingBox.From(polyLineMShapeContent.Shape));
        });

        return BuildProjection()
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

            return (object)new RoadSegmentLatestItem
            {
                Id = roadSegmentGeometryModified.Id,
                StartNodeId = roadSegmentAdded.StartNodeId,
                EndNodeId = roadSegmentAdded.EndNodeId,
                Geometry = geometry,

                Version = roadSegmentGeometryModified.Version,
                GeometryVersion = roadSegmentGeometryModified.GeometryVersion,
                StatusId = statusTranslation.Identifier,
                MorphologyId = morphologyTranslation.Identifier,
                CategoryId = categoryTranslation.Identifier,
                LeftSideStreetNameId = roadSegmentAdded.LeftSide.StreetNameId,
                RightSideStreetNameId = roadSegmentAdded.RightSide.StreetNameId,
                MaintainerId = roadSegmentAdded.MaintenanceAuthority.Code,
                MethodId = geometryDrawMethodTranslation.Identifier,
                AccessRestrictionId = accessRestrictionTranslation.Identifier,

                CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentAdded.When),
                VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentGeometryModified.When),
                OrganizationId = acceptedRoadSegmentGeometryModified.OrganizationId,
                OrganizationName = acceptedRoadSegmentGeometryModified.Organization,

                StatusLabel = RoadSegmentStatus.Parse(roadSegmentAdded.Status).Translation.Name,
                MorphologyLabel = RoadSegmentMorphology.Parse(roadSegmentAdded.Morphology).Translation.Name,
                CategoryLabel = RoadSegmentCategory.Parse(roadSegmentAdded.Category).Translation.Name,
                MethodLabel = RoadSegmentGeometryDrawMethod.Parse(roadSegmentAdded.GeometryDrawMethod).Translation.Name,
                AccessRestrictionLabel = RoadSegmentAccessRestriction.Parse(roadSegmentAdded.AccessRestriction).Translation.Name
            }.WithBoundingBox(RoadSegmentBoundingBox.From(polyLineMShapeContent.Shape));
        });

        return BuildProjection()
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

            return (object)new RoadSegmentLatestItem
            {
                Id = roadSegmentModified.Id,
                StartNodeId = roadSegmentModified.StartNodeId,
                EndNodeId = roadSegmentModified.EndNodeId,
                Geometry = geometry,

                Version = roadSegmentModified.Version,
                GeometryVersion = roadSegmentModified.GeometryVersion,
                StatusId = statusTranslation.Identifier,
                MorphologyId = morphologyTranslation.Identifier,
                CategoryId = categoryTranslation.Identifier,
                LeftSideStreetNameId = roadSegmentModified.LeftSide.StreetNameId,
                RightSideStreetNameId = roadSegmentModified.RightSide.StreetNameId,
                MaintainerId = roadSegmentModified.MaintenanceAuthority.Code,
                MethodId = geometryDrawMethodTranslation.Identifier,
                AccessRestrictionId = accessRestrictionTranslation.Identifier,

                CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentAdded.When),
                VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentModified.When),
                OrganizationId = acceptedRoadSegmentModified.OrganizationId,
                OrganizationName = acceptedRoadSegmentModified.Organization,

                StatusLabel = statusTranslation.Name,
                MorphologyLabel = morphologyTranslation.Name,
                CategoryLabel = categoryTranslation.Name,
                MethodLabel = geometryDrawMethodTranslation.Name,
                AccessRestrictionLabel = accessRestrictionTranslation.Name
            }.WithBoundingBox(RoadSegmentBoundingBox.From(polyLineMShapeContent.Shape));
        });

        return BuildProjection()
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

            return (object)new RoadSegmentLatestItem
            {
                Id = roadSegmentAdded.Id,
                StartNodeId = roadSegmentAdded.StartNodeId,
                EndNodeId = roadSegmentAdded.EndNodeId,
                Geometry = geometry,

                Version = roadSegmentAdded.Version,
                GeometryVersion = roadSegmentAdded.GeometryVersion,
                StatusId = statusTranslation.Identifier,
                MorphologyId = morphologyTranslation.Identifier,
                CategoryId = categoryTranslation.Identifier,
                LeftSideStreetNameId = roadSegmentAdded.LeftSide.StreetNameId,
                RightSideStreetNameId = roadSegmentAdded.RightSide.StreetNameId,
                MaintainerId = roadSegmentAdded.MaintenanceAuthority.Code,
                MethodId = geometryDrawMethodTranslation.Identifier,
                AccessRestrictionId = accessRestrictionTranslation.Identifier,

                CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentAdded.When),
                VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentRemoved.When),
                OrganizationId = acceptedRoadSegmentRemoved.OrganizationId,
                OrganizationName = acceptedRoadSegmentRemoved.Organization,

                StatusLabel = statusTranslation.Name,
                MorphologyLabel = morphologyTranslation.Name,
                CategoryLabel = categoryTranslation.Name,
                MethodLabel = geometryDrawMethodTranslation.Name,
                AccessRestrictionLabel = accessRestrictionTranslation.Name,

                IsRemoved = true
            }.WithBoundingBox(RoadSegmentBoundingBox.From(polyLineMShapeContent.Shape));
        });

        return BuildProjection()
            .Scenario()
            .Given(messages)
            .Expect(expectedRecords);
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

                var expected = new RoadSegmentLatestItem
                {
                    Id = importedRoadSegment.Id,
                    StartNodeId = importedRoadSegment.StartNodeId,
                    EndNodeId = importedRoadSegment.EndNodeId,
                    Geometry = geometry,

                    Version = importedRoadSegment.Version,
                    GeometryVersion = importedRoadSegment.GeometryVersion,
                    StatusId = statusTranslation.Identifier,
                    MorphologyId = morphologyTranslation.Identifier,
                    CategoryId = categoryTranslation.Identifier,
                    LeftSideStreetNameId = importedRoadSegment.LeftSide.StreetNameId,
                    RightSideStreetNameId = importedRoadSegment.RightSide.StreetNameId,
                    MaintainerId = importedRoadSegment.MaintenanceAuthority.Code,
                    MethodId = geometryDrawMethodTranslation.Identifier,
                    AccessRestrictionId = accessRestrictionTranslation.Identifier,

                    CreatedOnTimestamp = importedRoadSegment.RecordingDate,
                    VersionTimestamp = importedRoadSegment.Origin.Since,
                    OrganizationId = importedRoadSegment.Origin.OrganizationId,
                    OrganizationName = importedRoadSegment.Origin.Organization,

                    StatusLabel = statusTranslation.Name,
                    MorphologyLabel = morphologyTranslation.Name,
                    CategoryLabel = categoryTranslation.Name,
                    MethodLabel = geometryDrawMethodTranslation.Name,
                    AccessRestrictionLabel = accessRestrictionTranslation.Name
                }.WithBoundingBox(RoadSegmentBoundingBox.From(polyLineMShapeContent.Shape));
                return new { importedRoadSegment, expected };
            }).ToList();

        return BuildProjection()
            .Scenario()
            .Given(data.Select(d => d.importedRoadSegment))
            .Expect(data.Select(d => d.expected));
    }

    private RoadSegmentLatestItem BuildRoadSegmentRecord(
        RoadNetworkChangesAccepted acceptedRoadSegmentAdded,
        RoadSegmentAdded roadSegmentAdded,
        Action<RoadSegmentLatestItem> changeRecord)
    {
        var geometry = GeometryTranslator.Translate(roadSegmentAdded.Geometry);
        var polyLineMShapeContent = new PolyLineMShapeContent(Be.Vlaanderen.Basisregisters.Shaperon.Geometries.GeometryTranslator.FromGeometryMultiLineString(geometry));
        var statusTranslation = RoadSegmentStatus.Parse(roadSegmentAdded.Status).Translation;
        var morphologyTranslation = RoadSegmentMorphology.Parse(roadSegmentAdded.Morphology).Translation;
        var categoryTranslation = RoadSegmentCategory.Parse(roadSegmentAdded.Category).Translation;
        var geometryDrawMethodTranslation = RoadSegmentGeometryDrawMethod.Parse(roadSegmentAdded.GeometryDrawMethod).Translation;
        var accessRestrictionTranslation = RoadSegmentAccessRestriction.Parse(roadSegmentAdded.AccessRestriction).Translation;

        var record = new RoadSegmentLatestItem
        {
            Id = roadSegmentAdded.Id,
            StartNodeId = roadSegmentAdded.StartNodeId,
            EndNodeId = roadSegmentAdded.EndNodeId,
            Geometry = geometry,

            Version = roadSegmentAdded.Version,
            GeometryVersion = roadSegmentAdded.GeometryVersion,
            StatusId = statusTranslation.Identifier,
            StatusLabel = statusTranslation.Name,
            MorphologyId = morphologyTranslation.Identifier,
            MorphologyLabel = morphologyTranslation.Name,
            CategoryId = categoryTranslation.Identifier,
            CategoryLabel = categoryTranslation.Name,
            LeftSideStreetNameId = roadSegmentAdded.LeftSide.StreetNameId,
            RightSideStreetNameId = roadSegmentAdded.RightSide.StreetNameId,
            MaintainerId = roadSegmentAdded.MaintenanceAuthority.Code,
            MethodId = geometryDrawMethodTranslation.Identifier,
            MethodLabel = geometryDrawMethodTranslation.Name,
            AccessRestrictionId = accessRestrictionTranslation.Identifier,
            AccessRestrictionLabel = accessRestrictionTranslation.Name,

            CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentAdded.When),
            VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentAdded.When),
            OrganizationId = acceptedRoadSegmentAdded.OrganizationId,
            OrganizationName = acceptedRoadSegmentAdded.Organization
        }.WithBoundingBox(RoadSegmentBoundingBox.From(polyLineMShapeContent.Shape));
        changeRecord(record);

        return record;
    }

    private RoadSegmentLatestItemProjection BuildProjection()
    {
        return new RoadSegmentLatestItemProjection();
    }
}
