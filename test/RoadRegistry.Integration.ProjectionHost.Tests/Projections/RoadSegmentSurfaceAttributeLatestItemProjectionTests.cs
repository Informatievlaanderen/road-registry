namespace RoadRegistry.Integration.ProjectionHost.Tests.Projections;

using AutoFixture;
using BackOffice;
using BackOffice.Messages;
using Integration.Projections;
using RoadRegistry.Tests.BackOffice;
using Schema.RoadSegments;

public class RoadSegmentSurfaceAttributeLatestItemProjectionTests
{
    private readonly Fixture _fixture;

    public RoadSegmentSurfaceAttributeLatestItemProjectionTests()
    {
        _fixture = new Fixture();

        _fixture.CustomizeArchiveId();
        _fixture.CustomizeAttributeId();
        _fixture.CustomizeRoadSegmentId();
        _fixture.CustomizeOrganizationId();
        _fixture.CustomizePolylineM();
        _fixture.CustomizeRoadSegmentSurfaceType();
        _fixture.CustomizeRoadSegmentGeometryDrawMethod();
        _fixture.CustomizeRoadSegmentMorphology();
        _fixture.CustomizeRoadSegmentStatus();
        _fixture.CustomizeRoadSegmentCategory();
        _fixture.CustomizeRoadSegmentAccessRestriction();

        _fixture.CustomizeImportedRoadSegmentSurfaceAttributes();

        _fixture.CustomizeRoadSegmentSurfaceAttributes();
        _fixture.CustomizeRoadSegmentAdded();
        _fixture.CustomizeRoadSegmentModified();
        _fixture.CustomizeRoadSegmentAttributesModified();
        _fixture.CustomizeRoadSegmentGeometryModified();
        _fixture.CustomizeRoadSegmentRemoved();
        _fixture.CustomizeRoadNetworkChangesAccepted();
    }

    [Fact]
    public Task When_adding_road_segment_with_surfaces()
    {
        var message = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.CreateMany<RoadSegmentAdded>());

        var expectedRecords = Array.ConvertAll(message.Changes, change =>
            {
                var segment = change.RoadSegmentAdded;
                return segment.Surfaces.Select(surface => new RoadSegmentSurfaceAttributeLatestItem
                {
                    Id = surface.AttributeId,
                    RoadSegmentId = segment.Id,
                    AsOfGeometryVersion = surface.AsOfGeometryVersion,
                    TypeId = RoadSegmentSurfaceType.Parse(surface.Type).Translation.Identifier,
                    TypeLabel = RoadSegmentSurfaceType.Parse(surface.Type).Translation.Name,
                    FromPosition = (double)surface.FromPosition,
                    ToPosition = (double)surface.ToPosition,
                    OrganizationId = message.OrganizationId,
                    OrganizationName = message.Organization,
                    IsRemoved = false,
                    CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(message.When),
                    VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(message.When)
                });
            })
            .SelectMany(x => x);

        return new RoadSegmentSurfaceAttributeLatestItemProjection()
            .Scenario()
            .Given(message)
            .Expect(expectedRecords);
    }

    [Fact]
    public Task When_importing_a_road_segment_without_surfaces()
    {
        var importedRoadSegment = _fixture.Create<ImportedRoadSegment>();
        importedRoadSegment.Surfaces = [];

        return new RoadSegmentSurfaceAttributeLatestItemProjection()
            .Scenario()
            .Given(importedRoadSegment)
            .Expect([]);
    }

    [Fact]
    public Task When_importing_road_segments()
    {
        var random = new Random();
        var data = _fixture
            .CreateMany<ImportedRoadSegment>(random.Next(1, 10))
            .Select(segment =>
            {
                segment.Surfaces = _fixture
                    .CreateMany<ImportedRoadSegmentSurfaceAttribute>(random.Next(1, 10))
                    .ToArray();

                var expected = segment
                    .Surfaces
                    .Select(surface => new RoadSegmentSurfaceAttributeLatestItem
                    {
                        Id = surface.AttributeId,
                        RoadSegmentId = segment.Id,
                        AsOfGeometryVersion = surface.AsOfGeometryVersion,
                        TypeId = RoadSegmentSurfaceType.Parse(surface.Type).Translation.Identifier,
                        TypeLabel = RoadSegmentSurfaceType.Parse(surface.Type).Translation.Name,
                        FromPosition = (double)surface.FromPosition,
                        ToPosition = (double)surface.ToPosition,
                        OrganizationId = surface.Origin.OrganizationId,
                        OrganizationName = surface.Origin.Organization,
                        IsRemoved = false,
                        CreatedOnTimestamp = new DateTimeOffset(surface.Origin.Since),
                        VersionTimestamp = new DateTimeOffset(surface.Origin.Since)
                    });

                return new
                {
                    importedRoadSegment = segment,
                    expected
                };
            }).ToList();

        return new RoadSegmentSurfaceAttributeLatestItemProjection()
            .Scenario()
            .Given(data.Select(d => d.importedRoadSegment))
            .Expect(data
                .SelectMany(d => d.expected)
                .Cast<object>()
                .ToArray()
            );
    }

    [Fact]
    public Task When_modifying_road_segments_with_modified_surfaces_only()
    {
        _fixture.Freeze<RoadSegmentId>();

        var roadSegmentAdded = _fixture.Create<RoadSegmentAdded>();
        var acceptedRoadSegmentAdded = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(roadSegmentAdded);

        var roadSegmentModified = _fixture.Create<RoadSegmentModified>();
        roadSegmentModified.Surfaces = roadSegmentAdded.Surfaces
            .Select(attributes =>
            {
                var roadSegmentSurfaceAttributes = _fixture.Create<RoadSegmentSurfaceAttributes>();
                roadSegmentSurfaceAttributes.AttributeId = attributes.AttributeId;
                return roadSegmentSurfaceAttributes;
            })
            .ToArray();

        var acceptedRoadSegmentModified = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(roadSegmentModified);

        var expectedRecords = Array.ConvertAll(acceptedRoadSegmentModified.Changes, change =>
        {
            var segment = change.RoadSegmentModified;

            return segment.Surfaces.Select(surface => (object)new RoadSegmentSurfaceAttributeLatestItem
            {
                Id = surface.AttributeId,
                RoadSegmentId = segment.Id,
                AsOfGeometryVersion = surface.AsOfGeometryVersion,
                TypeId = RoadSegmentSurfaceType.Parse(surface.Type).Translation.Identifier,
                TypeLabel = RoadSegmentSurfaceType.Parse(surface.Type).Translation.Name,
                FromPosition = (double)surface.FromPosition,
                ToPosition = (double)surface.ToPosition,
                OrganizationId = acceptedRoadSegmentModified.OrganizationId,
                OrganizationName = acceptedRoadSegmentModified.Organization,
                IsRemoved = false,
                CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentAdded.When),
                VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentModified.When)
            });
        }).SelectMany(x => x);

        return new RoadSegmentSurfaceAttributeLatestItemProjection()
            .Scenario()
            .Given(acceptedRoadSegmentAdded, acceptedRoadSegmentModified)
            .Expect(expectedRecords);
    }

    [Fact]
    public Task When_modifying_road_segments_with_removed_surfaces_only()
    {
        _fixture.Freeze<RoadSegmentId>();

        var roadSegmentAdded = _fixture.Create<RoadSegmentAdded>();
        var acceptedRoadSegmentAdded = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(roadSegmentAdded);

        var roadSegmentModified = _fixture.Create<RoadSegmentModified>();
        roadSegmentModified.Surfaces = [];

        var acceptedRoadSegmentModified = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(roadSegmentModified);

        var expectedRecords = Array.ConvertAll(acceptedRoadSegmentAdded.Changes, change =>
        {
            var segment = change.RoadSegmentAdded;

            return segment.Surfaces.Select(surface => (object)new RoadSegmentSurfaceAttributeLatestItem
            {
                Id = surface.AttributeId,
                RoadSegmentId = segment.Id,
                AsOfGeometryVersion = surface.AsOfGeometryVersion,
                TypeId = RoadSegmentSurfaceType.Parse(surface.Type).Translation.Identifier,
                TypeLabel = RoadSegmentSurfaceType.Parse(surface.Type).Translation.Name,
                FromPosition = (double)surface.FromPosition,
                ToPosition = (double)surface.ToPosition,
                OrganizationId = acceptedRoadSegmentModified.OrganizationId,
                OrganizationName = acceptedRoadSegmentModified.Organization,
                IsRemoved = true,
                CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentAdded.When),
                VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentModified.When)
            });
        }).SelectMany(x => x);

        return new RoadSegmentSurfaceAttributeLatestItemProjection()
            .Scenario()
            .Given(acceptedRoadSegmentAdded, acceptedRoadSegmentModified)
            .Expect(expectedRecords);
    }

    [Fact]
    public Task When_modifying_road_segments_with_some_added_surfaces()
    {
        _fixture.Freeze<RoadSegmentId>();

        var roadSegmentAdded = _fixture.Create<RoadSegmentAdded>();
        var acceptedRoadSegmentAdded = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(roadSegmentAdded);

        var roadSegmentModified = _fixture.Create<RoadSegmentModified>();
        roadSegmentModified.Surfaces = roadSegmentAdded.Surfaces
            .Append(_fixture.Create<RoadSegmentSurfaceAttributes>())
            .ToArray();

        var acceptedRoadSegmentModified = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(roadSegmentModified);

        var expectedRecords = Array.ConvertAll(acceptedRoadSegmentModified.Changes, change =>
        {
            var segment = change.RoadSegmentModified;

            return segment.Surfaces.Select(surface =>
            {
                var isAdded = roadSegmentAdded.Surfaces.All(x => x.AttributeId != surface.AttributeId);

                return (object)new RoadSegmentSurfaceAttributeLatestItem
                {
                    Id = surface.AttributeId,
                    RoadSegmentId = segment.Id,
                    AsOfGeometryVersion = surface.AsOfGeometryVersion,
                    TypeId = RoadSegmentSurfaceType.Parse(surface.Type).Translation.Identifier,
                    TypeLabel = RoadSegmentSurfaceType.Parse(surface.Type).Translation.Name,
                    FromPosition = (double)surface.FromPosition,
                    ToPosition = (double)surface.ToPosition,
                    OrganizationId = acceptedRoadSegmentModified.OrganizationId,
                    OrganizationName = acceptedRoadSegmentModified.Organization,
                    IsRemoved = false,
                    CreatedOnTimestamp = isAdded
                        ? LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentModified.When)
                        : LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentAdded.When),
                    VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentModified.When)
                };

            });
        }).SelectMany(x => x);

        return new RoadSegmentSurfaceAttributeLatestItemProjection()
            .Scenario()
            .Given(acceptedRoadSegmentAdded, acceptedRoadSegmentModified)
            .Expect(expectedRecords);
    }

    [Fact]
    public Task When_modifying_road_segments_with_some_modified_surfaces()
    {
        _fixture.Freeze<RoadSegmentId>();

        var roadSegmentAdded = _fixture.Create<RoadSegmentAdded>();
        var acceptedRoadSegmentAdded = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(roadSegmentAdded);

        var roadSegmentModified = _fixture.Create<RoadSegmentModified>();
        roadSegmentModified.Surfaces = roadSegmentAdded.Surfaces
            .Select((attributes, i) =>
            {
                if (i % 2 != 0)
                {
                    return attributes;
                }

                var roadSegmentSurfaceAttributes = _fixture.Create<RoadSegmentSurfaceAttributes>();
                roadSegmentSurfaceAttributes.AttributeId = attributes.AttributeId;
                return roadSegmentSurfaceAttributes;

            })
            .ToArray();

        var acceptedRoadSegmentModified = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(roadSegmentModified);

        var expectedRecords = Array.ConvertAll(acceptedRoadSegmentModified.Changes, change =>
        {
            var segment = change.RoadSegmentModified;

            return segment.Surfaces.Select(surface => (object)new RoadSegmentSurfaceAttributeLatestItem
            {
                Id = surface.AttributeId,
                RoadSegmentId = segment.Id,
                AsOfGeometryVersion = surface.AsOfGeometryVersion,
                TypeId = RoadSegmentSurfaceType.Parse(surface.Type).Translation.Identifier,
                TypeLabel = RoadSegmentSurfaceType.Parse(surface.Type).Translation.Name,
                FromPosition = (double)surface.FromPosition,
                ToPosition = (double)surface.ToPosition,
                OrganizationId = acceptedRoadSegmentModified.OrganizationId,
                OrganizationName = acceptedRoadSegmentModified.Organization,
                IsRemoved = false,
                CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentAdded.When),
                VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentModified.When)
            });
        }).SelectMany(x => x);

        return new RoadSegmentSurfaceAttributeLatestItemProjection()
            .Scenario()
            .Given(acceptedRoadSegmentAdded, acceptedRoadSegmentModified)
            .Expect(expectedRecords);
    }

    [Fact]
    public Task When_modifying_road_segments_with_some_removed_surfaces()
    {
        _fixture.Freeze<RoadSegmentId>();

        var roadSegmentAdded = _fixture.Create<RoadSegmentAdded>();
        var acceptedRoadSegmentAdded = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(roadSegmentAdded);

        var roadSegmentModified = _fixture.Create<RoadSegmentModified>();
        roadSegmentModified.Surfaces = roadSegmentAdded.Surfaces
            .Take(roadSegmentAdded.Surfaces.Length - 1)
            .ToArray();

        var removedSurface = roadSegmentAdded.Surfaces.Except(roadSegmentModified.Surfaces).Single();

        var acceptedRoadSegmentModified = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(roadSegmentModified);

        var expectedRecords = Array.ConvertAll(acceptedRoadSegmentModified.Changes, change =>
        {
            var segment = change.RoadSegmentModified;

            return segment.Surfaces.Select(surface => (object)new RoadSegmentSurfaceAttributeLatestItem
            {
                Id = surface.AttributeId,
                RoadSegmentId = segment.Id,
                AsOfGeometryVersion = surface.AsOfGeometryVersion,
                TypeId = RoadSegmentSurfaceType.Parse(surface.Type).Translation.Identifier,
                TypeLabel = RoadSegmentSurfaceType.Parse(surface.Type).Translation.Name,
                FromPosition = (double)surface.FromPosition,
                ToPosition = (double)surface.ToPosition,
                OrganizationId = acceptedRoadSegmentModified.OrganizationId,
                OrganizationName = acceptedRoadSegmentModified.Organization,
                IsRemoved = false,
                CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentAdded.When),
                VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentModified.When)
            });
        })
            .SelectMany(x => x)
            .Concat([
                new RoadSegmentSurfaceAttributeLatestItem
                {
                    Id = removedSurface.AttributeId,
                    RoadSegmentId = roadSegmentAdded.Id,
                    AsOfGeometryVersion = removedSurface.AsOfGeometryVersion,
                    TypeId = RoadSegmentSurfaceType.Parse(removedSurface.Type).Translation.Identifier,
                    TypeLabel = RoadSegmentSurfaceType.Parse(removedSurface.Type).Translation.Name,
                    FromPosition = (double)removedSurface.FromPosition,
                    ToPosition = (double)removedSurface.ToPosition,
                    OrganizationId = acceptedRoadSegmentModified.OrganizationId,
                    OrganizationName = acceptedRoadSegmentModified.Organization,
                    IsRemoved = true,
                    CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentAdded.When),
                    VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentModified.When)
                }
            ]);

        return new RoadSegmentSurfaceAttributeLatestItemProjection()
            .Scenario()
            .Given(acceptedRoadSegmentAdded, acceptedRoadSegmentModified)
            .Expect(expectedRecords);
    }

    [Fact]
    public Task When_modifying_road_segment_geometry_with_new_surfaces_only()
    {
        _fixture.Freeze<RoadSegmentId>();

        var roadSegmentAdded = _fixture.Create<RoadSegmentAdded>();
        var acceptedRoadSegmentAdded = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(roadSegmentAdded);

        var acceptedRoadSegmentGeometryModified = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.Create<RoadSegmentGeometryModified>());

        var expectedRecords = Array.ConvertAll(acceptedRoadSegmentGeometryModified.Changes, change =>
        {
            var segment = change.RoadSegmentGeometryModified;

            return segment.Surfaces.Select(surface => (object)new RoadSegmentSurfaceAttributeLatestItem
            {
                Id = surface.AttributeId,
                RoadSegmentId = segment.Id,
                AsOfGeometryVersion = surface.AsOfGeometryVersion,
                TypeId = RoadSegmentSurfaceType.Parse(surface.Type).Translation.Identifier,
                TypeLabel = RoadSegmentSurfaceType.Parse(surface.Type).Translation.Name,
                FromPosition = (double)surface.FromPosition,
                ToPosition = (double)surface.ToPosition,
                OrganizationId = acceptedRoadSegmentGeometryModified.OrganizationId,
                OrganizationName = acceptedRoadSegmentGeometryModified.Organization,
                IsRemoved = false,
                CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentGeometryModified.When),
                VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentGeometryModified.When)
            });
        })
            .SelectMany(x => x)
            .Concat(roadSegmentAdded.Surfaces
                .Select(surface =>
                    new RoadSegmentSurfaceAttributeLatestItem
                    {
                        Id = surface.AttributeId,
                        RoadSegmentId = roadSegmentAdded.Id,
                        AsOfGeometryVersion = surface.AsOfGeometryVersion,
                        TypeId = RoadSegmentSurfaceType.Parse(surface.Type).Translation.Identifier,
                        TypeLabel = RoadSegmentSurfaceType.Parse(surface.Type).Translation.Name,
                        FromPosition = (double)surface.FromPosition,
                        ToPosition = (double)surface.ToPosition,
                        OrganizationId = acceptedRoadSegmentGeometryModified.OrganizationId,
                        OrganizationName = acceptedRoadSegmentGeometryModified.Organization,
                        IsRemoved = true,
                        CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentAdded.When),
                        VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentGeometryModified.When)
                    }
                ));

        return new RoadSegmentSurfaceAttributeLatestItemProjection()
            .Scenario()
            .Given(acceptedRoadSegmentAdded, acceptedRoadSegmentGeometryModified)
            .ExpectInAnyOrder(expectedRecords);
    }

    [Fact]
    public Task When_modifying_road_segments_with_new_surfaces_only()
    {
        _fixture.Freeze<RoadSegmentId>();

        var roadSegmentAdded = _fixture.Create<RoadSegmentAdded>();
        var acceptedRoadSegmentAdded = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(roadSegmentAdded);

        var acceptedRoadSegmentModified = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.Create<RoadSegmentModified>());

        var expectedRecords = Array.ConvertAll(acceptedRoadSegmentModified.Changes, change =>
        {
            var segment = change.RoadSegmentModified;

            return segment.Surfaces.Select(surface => (object)new RoadSegmentSurfaceAttributeLatestItem
            {
                Id = surface.AttributeId,
                RoadSegmentId = segment.Id,
                AsOfGeometryVersion = surface.AsOfGeometryVersion,
                TypeId = RoadSegmentSurfaceType.Parse(surface.Type).Translation.Identifier,
                TypeLabel = RoadSegmentSurfaceType.Parse(surface.Type).Translation.Name,
                FromPosition = (double)surface.FromPosition,
                ToPosition = (double)surface.ToPosition,
                OrganizationId = acceptedRoadSegmentModified.OrganizationId,
                OrganizationName = acceptedRoadSegmentModified.Organization,
                IsRemoved = false,
                CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentModified.When),
                VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentModified.When)
            });
        })
            .SelectMany(x => x)
            .Concat(roadSegmentAdded.Surfaces
                .Select(surface =>
                    new RoadSegmentSurfaceAttributeLatestItem
                    {
                        Id = surface.AttributeId,
                        RoadSegmentId = roadSegmentAdded.Id,
                        AsOfGeometryVersion = surface.AsOfGeometryVersion,
                        TypeId = RoadSegmentSurfaceType.Parse(surface.Type).Translation.Identifier,
                        TypeLabel = RoadSegmentSurfaceType.Parse(surface.Type).Translation.Name,
                        FromPosition = (double)surface.FromPosition,
                        ToPosition = (double)surface.ToPosition,
                        OrganizationId = acceptedRoadSegmentModified.OrganizationId,
                        OrganizationName = acceptedRoadSegmentModified.Organization,
                        IsRemoved = true,
                        CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentAdded.When),
                        VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentModified.When)
                    }
                ));

        return new RoadSegmentSurfaceAttributeLatestItemProjection()
            .Scenario()
            .Given(acceptedRoadSegmentAdded, acceptedRoadSegmentModified)
            .ExpectInAnyOrder(expectedRecords);
    }

    [Fact]
    public Task When_modifying_road_segment_attributes_with_new_surfaces_only()
    {
        _fixture.Freeze<RoadSegmentId>();

        var roadSegmentAdded = _fixture.Create<RoadSegmentAdded>();
        var acceptedRoadSegmentAdded = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(roadSegmentAdded);

        var acceptedRoadSegmentModified = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.Create<RoadSegmentAttributesModified>());

        var expectedRecords = Array.ConvertAll(acceptedRoadSegmentModified.Changes, change =>
        {
            var segment = change.RoadSegmentAttributesModified;

            return segment.Surfaces.Select(surface => (object)new RoadSegmentSurfaceAttributeLatestItem
            {
                Id = surface.AttributeId,
                RoadSegmentId = segment.Id,
                AsOfGeometryVersion = surface.AsOfGeometryVersion,
                TypeId = RoadSegmentSurfaceType.Parse(surface.Type).Translation.Identifier,
                TypeLabel = RoadSegmentSurfaceType.Parse(surface.Type).Translation.Name,
                FromPosition = (double)surface.FromPosition,
                ToPosition = (double)surface.ToPosition,
                OrganizationId = acceptedRoadSegmentModified.OrganizationId,
                OrganizationName = acceptedRoadSegmentModified.Organization,
                IsRemoved = false,
                CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentModified.When),
                VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentModified.When)
            });
        })
            .SelectMany(x => x)
            .Concat(roadSegmentAdded.Surfaces
                .Select(surface =>
                    new RoadSegmentSurfaceAttributeLatestItem
                    {
                        Id = surface.AttributeId,
                        RoadSegmentId = roadSegmentAdded.Id,
                        AsOfGeometryVersion = surface.AsOfGeometryVersion,
                        TypeId = RoadSegmentSurfaceType.Parse(surface.Type).Translation.Identifier,
                        TypeLabel = RoadSegmentSurfaceType.Parse(surface.Type).Translation.Name,
                        FromPosition = (double)surface.FromPosition,
                        ToPosition = (double)surface.ToPosition,
                        OrganizationId = acceptedRoadSegmentModified.OrganizationId,
                        OrganizationName = acceptedRoadSegmentModified.Organization,
                        IsRemoved = true,
                        CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentAdded.When),
                        VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentModified.When)
                    }
                ));

        return new RoadSegmentSurfaceAttributeLatestItemProjection()
            .Scenario()
            .Given(acceptedRoadSegmentAdded, acceptedRoadSegmentModified)
            .ExpectInAnyOrder(expectedRecords);
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

        var expectedRecords = Array.ConvertAll(acceptedRoadSegmentAdded.Changes, change =>
        {
            var segment = change.RoadSegmentAdded;

            return segment.Surfaces.Select(surface => (object)new RoadSegmentSurfaceAttributeLatestItem
            {
                Id = surface.AttributeId,
                RoadSegmentId = segment.Id,
                AsOfGeometryVersion = surface.AsOfGeometryVersion,
                TypeId = RoadSegmentSurfaceType.Parse(surface.Type).Translation.Identifier,
                TypeLabel = RoadSegmentSurfaceType.Parse(surface.Type).Translation.Name,
                FromPosition = (double)surface.FromPosition,
                ToPosition = (double)surface.ToPosition,
                OrganizationId = acceptedRoadSegmentRemoved.OrganizationId,
                OrganizationName = acceptedRoadSegmentRemoved.Organization,
                IsRemoved = true,
                CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentAdded.When),
                VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentRemoved.When)
            });
        }).SelectMany(x => x);

        return new RoadSegmentSurfaceAttributeLatestItemProjection()
            .Scenario()
            .Given(acceptedRoadSegmentAdded, acceptedRoadSegmentRemoved)
            .Expect(expectedRecords);
    }
}
