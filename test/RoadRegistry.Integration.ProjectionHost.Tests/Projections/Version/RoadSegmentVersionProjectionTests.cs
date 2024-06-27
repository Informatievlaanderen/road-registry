namespace RoadRegistry.Integration.ProjectionHost.Tests.Projections.Version;

using AutoFixture;
using BackOffice;
using BackOffice.Messages;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Integration.Projections;
using RoadRegistry.Integration.Projections.Version;
using RoadRegistry.Tests;
using RoadRegistry.Tests.BackOffice;
using RoadRegistry.Tests.BackOffice.Scenarios;
using Schema.RoadSegments;
using Schema.RoadSegments.Version;
using RoadSegmentVersion = Schema.RoadSegments.Version.RoadSegmentVersion;

public partial class RoadSegmentVersionProjectionTests
{
    private readonly Fixture _fixture;

    public RoadSegmentVersionProjectionTests()
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

        var expectedRecords = BuildInitialExpectedRoadSegmentRecords(message);

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

        var expectedRecords = BuildInitialExpectedRoadSegmentRecords(acceptedRoadSegmentAdded)
            .Concat(Array.ConvertAll(message.Changes, change =>
            {
                return BuildRoadSegmentRecord(
                    1,
                    acceptedRoadSegmentAdded,
                    acceptedRoadSegmentAdded.Changes.Single().RoadSegmentAdded,
                    record =>
                    {
                        record.Version = change.RoadSegmentAddedToEuropeanRoad.SegmentVersion!.Value;

                        record.OrganizationId = message.OrganizationId;
                        record.OrganizationName = message.Organization;
                        record.VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(message.When);
                    });
            }));

        return BuildProjection()
            .Scenario()
            .Given(acceptedRoadSegmentAdded, message)
            .Expect(expectedRecords);
    }

    [Fact]
    public Task When_adding_road_segment_to_multiple_european_roads()
    {
        _fixture.Freeze<RoadSegmentId>();

        var acceptedRoadSegmentAdded = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.Create<RoadSegmentAdded>());

        var message = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.Create<RoadSegmentAddedToEuropeanRoad>(), _fixture.Create<RoadSegmentAddedToEuropeanRoad>());

        Assert.Equal(2, message.Changes.Length);

        var expectedRecords = BuildInitialExpectedRoadSegmentRecords(acceptedRoadSegmentAdded)
            .Concat(message.Changes.Skip(1).Select(change =>
            {
                return BuildRoadSegmentRecord(
                    1,
                    acceptedRoadSegmentAdded,
                    acceptedRoadSegmentAdded.Changes.Single().RoadSegmentAdded,
                    record =>
                    {
                        record.Version = change.RoadSegmentAddedToEuropeanRoad.SegmentVersion!.Value;

                        record.OrganizationId = message.OrganizationId;
                        record.OrganizationName = message.Organization;
                        record.VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(message.When);
                    });
            }));

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

        var expectedRecords = BuildInitialExpectedRoadSegmentRecords(acceptedRoadSegmentAdded)
            .Concat(Array.ConvertAll(message.Changes, change =>
            {
                var roadSegmentAddedToNationalRoad = change.RoadSegmentAddedToNationalRoad;

                return BuildRoadSegmentRecord(
                    1,
                    acceptedRoadSegmentAdded,
                    acceptedRoadSegmentAdded.Changes.Single().RoadSegmentAdded,
                    record =>
                    {
                        record.Version = roadSegmentAddedToNationalRoad.SegmentVersion!.Value;

                        record.OrganizationId = message.OrganizationId;
                        record.OrganizationName = message.Organization;
                        record.VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(message.When);
                    });
            }));

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

        var expectedRecords = BuildInitialExpectedRoadSegmentRecords(acceptedRoadSegmentAdded)
            .Concat(Array.ConvertAll(message.Changes, change =>
            {
                var roadSegmentAddedToNumberedRoad = change.RoadSegmentAddedToNumberedRoad;

                return BuildRoadSegmentRecord(
                    1,
                    acceptedRoadSegmentAdded,
                    acceptedRoadSegmentAdded.Changes.Single().RoadSegmentAdded,
                    record =>
                    {
                        record.Version = roadSegmentAddedToNumberedRoad.SegmentVersion!.Value;

                        record.OrganizationId = message.OrganizationId;
                        record.OrganizationName = message.Organization;
                        record.VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(message.When);
                    });
            }));

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

        var expectedRecords = BuildInitialExpectedRoadSegmentRecords(acceptedRoadSegmentAdded)
            .Concat(Array.ConvertAll(message.Changes, change =>
            {
                var roadSegmentRemovedFromEuropeanRoad = change.RoadSegmentRemovedFromEuropeanRoad;

                return BuildRoadSegmentRecord(
                    1,
                    acceptedRoadSegmentAdded,
                    acceptedRoadSegmentAdded.Changes.Single().RoadSegmentAdded,
                    record =>
                    {
                        record.Version = roadSegmentRemovedFromEuropeanRoad.SegmentVersion!.Value;

                        record.OrganizationId = message.OrganizationId;
                        record.OrganizationName = message.Organization;
                        record.VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(message.When);
                    });
            }));

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

        var expectedRecords = BuildInitialExpectedRoadSegmentRecords(acceptedRoadSegmentAdded)
            .Concat(Array.ConvertAll(message.Changes, change =>
            {
                var roadSegmentRemovedFromNationalRoad = change.RoadSegmentRemovedFromNationalRoad;

                return BuildRoadSegmentRecord(
                    1,
                    acceptedRoadSegmentAdded,
                    acceptedRoadSegmentAdded.Changes.Single().RoadSegmentAdded,
                    record =>
                    {
                        record.Version = roadSegmentRemovedFromNationalRoad.SegmentVersion!.Value;

                        record.OrganizationId = message.OrganizationId;
                        record.OrganizationName = message.Organization;
                        record.VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(message.When);
                    });
            }));

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

        var expectedRecords = BuildInitialExpectedRoadSegmentRecords(acceptedRoadSegmentAdded)
            .Concat(Array.ConvertAll(message.Changes, change =>
            {
                var roadSegmentRemovedFromNumberedRoad = change.RoadSegmentRemovedFromNumberedRoad;

                return BuildRoadSegmentRecord(
                    1,
                    acceptedRoadSegmentAdded,
                    acceptedRoadSegmentAdded.Changes.Single().RoadSegmentAdded,
                    record =>
                    {
                        record.Version = roadSegmentRemovedFromNumberedRoad.SegmentVersion!.Value;

                        record.OrganizationId = message.OrganizationId;
                        record.OrganizationName = message.Organization;
                        record.VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(message.When);
                    });
            }));

        return BuildProjection()
            .Scenario()
            .Given(acceptedRoadSegmentAdded, message)
            .Expect(expectedRecords);
    }

    [Fact]
    public Task When_modifying_road_segment_attributes()
    {
        return new ModifyRoadSegmentAttributesScenarioBuilder(_fixture)
            .Expect();
    }

    [Fact]
    public Task When_modifying_road_segment_geometry()
    {
        return new ModifyRoadSegmentGeometryScenarioBuilder(_fixture)
            .Expect();
    }

    [Fact]
    public Task When_modifying_road_segments()
    {
        return new ModifyRoadSegmentScenarioBuilder(_fixture)
            .Expect();
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

        var expectedRecords = BuildInitialExpectedRoadSegmentRecords(acceptedRoadSegmentAdded)
            .Concat(Array.ConvertAll(acceptedRoadSegmentAdded.Changes, change =>
            {
                var roadSegmentAdded = change.RoadSegmentAdded;

                var geometry = GeometryTranslator.Translate(roadSegmentAdded.Geometry);
                var polyLineMShapeContent = new PolyLineMShapeContent(Be.Vlaanderen.Basisregisters.Shaperon.Geometries.GeometryTranslator.FromGeometryMultiLineString(geometry));
                var statusTranslation = RoadSegmentStatus.Parse(roadSegmentAdded.Status).Translation;
                var morphologyTranslation = RoadSegmentMorphology.Parse(roadSegmentAdded.Morphology).Translation;
                var categoryTranslation = RoadSegmentCategory.Parse(roadSegmentAdded.Category).Translation;
                var geometryDrawMethodTranslation = RoadSegmentGeometryDrawMethod.Parse(roadSegmentAdded.GeometryDrawMethod).Translation;
                var accessRestrictionTranslation = RoadSegmentAccessRestriction.Parse(roadSegmentAdded.AccessRestriction).Translation;

                var position = 1;

                return new RoadSegmentVersion
                {
                    Position = position,
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

                    OrganizationId = acceptedRoadSegmentRemoved.OrganizationId,
                    OrganizationName = acceptedRoadSegmentRemoved.Organization,
                    CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentAdded.When),
                    VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentRemoved.When),

                    StatusLabel = statusTranslation.Name,
                    MorphologyLabel = morphologyTranslation.Name,
                    CategoryLabel = categoryTranslation.Name,
                    MethodLabel = geometryDrawMethodTranslation.Name,
                    AccessRestrictionLabel = accessRestrictionTranslation.Name,

                    IsRemoved = true,

                    Lanes = roadSegmentAdded.Lanes
                        .Select(x => new RoadSegmentLaneAttributeVersion
                        {
                            Position = position,
                            Id = x.AttributeId,
                            RoadSegmentId = roadSegmentAdded.Id,
                            AsOfGeometryVersion = x.AsOfGeometryVersion,
                            FromPosition = (double)x.FromPosition,
                            ToPosition = (double)x.ToPosition,
                            Count = x.Count,
                            DirectionId = RoadSegmentLaneDirection.Parse(x.Direction).Translation.Identifier,
                            DirectionLabel = RoadSegmentLaneDirection.Parse(x.Direction).Translation.Name,

                            OrganizationId = acceptedRoadSegmentRemoved.OrganizationId,
                            OrganizationName = acceptedRoadSegmentRemoved.Organization,
                            CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentAdded.When),
                            VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentRemoved.When),

                            IsRemoved = true
                        })
                        .ToList(),
                    Surfaces = roadSegmentAdded.Surfaces
                        .Select(x => new RoadSegmentSurfaceAttributeVersion
                        {
                            Position = position,
                            Id = x.AttributeId,
                            RoadSegmentId = roadSegmentAdded.Id,
                            AsOfGeometryVersion = x.AsOfGeometryVersion,
                            FromPosition = (double)x.FromPosition,
                            ToPosition = (double)x.ToPosition,
                            TypeId = RoadSegmentSurfaceType.Parse(x.Type).Translation.Identifier,
                            TypeLabel = RoadSegmentSurfaceType.Parse(x.Type).Translation.Name,

                            OrganizationId = acceptedRoadSegmentRemoved.OrganizationId,
                            OrganizationName = acceptedRoadSegmentRemoved.Organization,
                            CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentAdded.When),
                            VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentRemoved.When),

                            IsRemoved = true
                        })
                        .ToList(),
                    Widths = roadSegmentAdded.Widths
                        .Select(x => new RoadSegmentWidthAttributeVersion
                        {
                            Position = position,
                            Id = x.AttributeId,
                            RoadSegmentId = roadSegmentAdded.Id,
                            AsOfGeometryVersion = x.AsOfGeometryVersion,
                            FromPosition = (double)x.FromPosition,
                            ToPosition = (double)x.ToPosition,
                            Width = x.Width,
                            WidthLabel = new RoadSegmentWidth(x.Width).ToDutchString(),

                            OrganizationId = acceptedRoadSegmentRemoved.OrganizationId,
                            OrganizationName = acceptedRoadSegmentRemoved.Organization,
                            CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentAdded.When),
                            VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentRemoved.When),

                            IsRemoved = true
                        })
                        .ToList()
                }.WithBoundingBox(RoadSegmentBoundingBox.From(polyLineMShapeContent.Shape));
            }));

        return BuildProjection()
            .Scenario()
            .Given(messages)
            .Expect(expectedRecords);
    }

    [Fact]
    public Task When_importing_road_segments()
    {
        var random = new Random();
        var data = _fixture
            .CreateMany<ImportedRoadSegment>(random.Next(1, 10))
            .Select((importedRoadSegment, eventIndex) =>
            {
                var geometry = GeometryTranslator.Translate(importedRoadSegment.Geometry);
                var polyLineMShapeContent = new PolyLineMShapeContent(Be.Vlaanderen.Basisregisters.Shaperon.Geometries.GeometryTranslator.FromGeometryMultiLineString(geometry));
                var statusTranslation = RoadSegmentStatus.Parse(importedRoadSegment.Status).Translation;
                var morphologyTranslation = RoadSegmentMorphology.Parse(importedRoadSegment.Morphology).Translation;
                var categoryTranslation = RoadSegmentCategory.Parse(importedRoadSegment.Category).Translation;
                var geometryDrawMethodTranslation = RoadSegmentGeometryDrawMethod.Parse(importedRoadSegment.GeometryDrawMethod).Translation;
                var accessRestrictionTranslation = RoadSegmentAccessRestriction.Parse(importedRoadSegment.AccessRestriction).Translation;

                var expected = new RoadSegmentVersion
                {
                    Position = eventIndex,
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

                    OrganizationId = importedRoadSegment.Origin.OrganizationId,
                    OrganizationName = importedRoadSegment.Origin.Organization,
                    CreatedOnTimestamp = importedRoadSegment.RecordingDate,
                    VersionTimestamp = importedRoadSegment.Origin.Since,

                    StatusLabel = statusTranslation.Name,
                    MorphologyLabel = morphologyTranslation.Name,
                    CategoryLabel = categoryTranslation.Name,
                    MethodLabel = geometryDrawMethodTranslation.Name,
                    AccessRestrictionLabel = accessRestrictionTranslation.Name,

                    Lanes = importedRoadSegment.Lanes
                        .Select(x => new RoadSegmentLaneAttributeVersion
                        {
                            Position = eventIndex,
                            Id = x.AttributeId,
                            RoadSegmentId = importedRoadSegment.Id,
                            AsOfGeometryVersion = x.AsOfGeometryVersion,
                            FromPosition = (double)x.FromPosition,
                            ToPosition = (double)x.ToPosition,
                            Count = x.Count,
                            DirectionId = RoadSegmentLaneDirection.Parse(x.Direction).Translation.Identifier,
                            DirectionLabel = RoadSegmentLaneDirection.Parse(x.Direction).Translation.Name,

                            OrganizationId = x.Origin.OrganizationId,
                            OrganizationName = x.Origin.Organization,
                            CreatedOnTimestamp = new DateTimeOffset(x.Origin.Since),
                            VersionTimestamp = new DateTimeOffset(x.Origin.Since)
                        })
                        .ToList(),
                    Surfaces = importedRoadSegment.Surfaces
                        .Select(x => new RoadSegmentSurfaceAttributeVersion
                        {
                            Position = eventIndex,
                            Id = x.AttributeId,
                            RoadSegmentId = importedRoadSegment.Id,
                            AsOfGeometryVersion = x.AsOfGeometryVersion,
                            FromPosition = (double)x.FromPosition,
                            ToPosition = (double)x.ToPosition,
                            TypeId = RoadSegmentSurfaceType.Parse(x.Type).Translation.Identifier,
                            TypeLabel = RoadSegmentSurfaceType.Parse(x.Type).Translation.Name,

                            OrganizationId = x.Origin.OrganizationId,
                            OrganizationName = x.Origin.Organization,
                            CreatedOnTimestamp = new DateTimeOffset(x.Origin.Since),
                            VersionTimestamp = new DateTimeOffset(x.Origin.Since)
                        })
                        .ToList(),
                    Widths = importedRoadSegment.Widths
                        .Select(x => new RoadSegmentWidthAttributeVersion
                        {
                            Position = eventIndex,
                            Id = x.AttributeId,
                            RoadSegmentId = importedRoadSegment.Id,
                            AsOfGeometryVersion = x.AsOfGeometryVersion,
                            FromPosition = (double)x.FromPosition,
                            ToPosition = (double)x.ToPosition,
                            Width = x.Width,
                            WidthLabel = new RoadSegmentWidth(x.Width).ToDutchString(),

                            OrganizationId = x.Origin.OrganizationId,
                            OrganizationName = x.Origin.Organization,
                            CreatedOnTimestamp = new DateTimeOffset(x.Origin.Since),
                            VersionTimestamp = new DateTimeOffset(x.Origin.Since)
                        })
                        .ToList()
                }.WithBoundingBox(RoadSegmentBoundingBox.From(polyLineMShapeContent.Shape));
                return new { importedRoadSegment, expected };
            }).ToList();

        return BuildProjection()
            .Scenario()
            .Given(data.Select(d => d.importedRoadSegment))
            .Expect(data.Select(d => d.expected));
    }

    private object[] BuildInitialExpectedRoadSegmentRecords(RoadNetworkChangesAccepted message)
    {
        return message.Changes
            .Select(change => BuildRoadSegmentRecord(0, message, change.RoadSegmentAdded))
            .Cast<object>()
            .ToArray();
    }

    private RoadSegmentVersion BuildRoadSegmentRecord(
        long position,
        RoadNetworkChangesAccepted acceptedRoadSegmentAdded,
        RoadSegmentAdded roadSegmentAdded,
        Action<RoadSegmentVersion> changeRecord = null)
    {
        var geometry = GeometryTranslator.Translate(roadSegmentAdded.Geometry);
        var polyLineMShapeContent = new PolyLineMShapeContent(Be.Vlaanderen.Basisregisters.Shaperon.Geometries.GeometryTranslator.FromGeometryMultiLineString(geometry));
        var statusTranslation = RoadSegmentStatus.Parse(roadSegmentAdded.Status).Translation;
        var morphologyTranslation = RoadSegmentMorphology.Parse(roadSegmentAdded.Morphology).Translation;
        var categoryTranslation = RoadSegmentCategory.Parse(roadSegmentAdded.Category).Translation;
        var geometryDrawMethodTranslation = RoadSegmentGeometryDrawMethod.Parse(roadSegmentAdded.GeometryDrawMethod).Translation;
        var accessRestrictionTranslation = RoadSegmentAccessRestriction.Parse(roadSegmentAdded.AccessRestriction).Translation;

        var record = new RoadSegmentVersion
        {
            Position = position,
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

            OrganizationId = acceptedRoadSegmentAdded.OrganizationId,
            OrganizationName = acceptedRoadSegmentAdded.Organization,
            CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentAdded.When),
            VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentAdded.When),

            Lanes = roadSegmentAdded.Lanes
                .Select(x => new RoadSegmentLaneAttributeVersion
                {
                    Position = position,
                    Id = x.AttributeId,
                    RoadSegmentId = roadSegmentAdded.Id,
                    AsOfGeometryVersion = x.AsOfGeometryVersion,
                    FromPosition = (double)x.FromPosition,
                    ToPosition = (double)x.ToPosition,
                    Count = x.Count,
                    DirectionId = RoadSegmentLaneDirection.Parse(x.Direction).Translation.Identifier,
                    DirectionLabel = RoadSegmentLaneDirection.Parse(x.Direction).Translation.Name,

                    OrganizationId = acceptedRoadSegmentAdded.OrganizationId,
                    OrganizationName = acceptedRoadSegmentAdded.Organization,
                    CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentAdded.When),
                    VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentAdded.When)
                })
                .ToList(),
            Surfaces = roadSegmentAdded.Surfaces
                .Select(x => new RoadSegmentSurfaceAttributeVersion
                {
                    Position = position,
                    Id = x.AttributeId,
                    RoadSegmentId = roadSegmentAdded.Id,
                    AsOfGeometryVersion = x.AsOfGeometryVersion,
                    FromPosition = (double)x.FromPosition,
                    ToPosition = (double)x.ToPosition,
                    TypeId = RoadSegmentSurfaceType.Parse(x.Type).Translation.Identifier,
                    TypeLabel = RoadSegmentSurfaceType.Parse(x.Type).Translation.Name,

                    OrganizationId = acceptedRoadSegmentAdded.OrganizationId,
                    OrganizationName = acceptedRoadSegmentAdded.Organization,
                    CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentAdded.When),
                    VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentAdded.When)
                })
                .ToList(),
            Widths = roadSegmentAdded.Widths
                .Select(x => new RoadSegmentWidthAttributeVersion
                {
                    Position = position,
                    Id = x.AttributeId,
                    RoadSegmentId = roadSegmentAdded.Id,
                    AsOfGeometryVersion = x.AsOfGeometryVersion,
                    FromPosition = (double)x.FromPosition,
                    ToPosition = (double)x.ToPosition,
                    Width = x.Width,
                    WidthLabel = new RoadSegmentWidth(x.Width).ToDutchString(),

                    OrganizationId = acceptedRoadSegmentAdded.OrganizationId,
                    OrganizationName = acceptedRoadSegmentAdded.Organization,
                    CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentAdded.When),
                    VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentAdded.When)
                })
                .ToList()
        }.WithBoundingBox(RoadSegmentBoundingBox.From(polyLineMShapeContent.Shape));
        changeRecord?.Invoke(record);

        return record;
    }
    //TODO-rik add specific tests for lanes/surfaces/widths
    private RoadSegmentVersionProjection BuildProjection()
    {
        return new RoadSegmentVersionProjection();
    }
}
