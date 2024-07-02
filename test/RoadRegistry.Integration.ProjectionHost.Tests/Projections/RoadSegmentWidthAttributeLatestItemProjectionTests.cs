namespace RoadRegistry.Integration.ProjectionHost.Tests.Projections;

using AutoFixture;
using BackOffice;
using BackOffice.Messages;
using Integration.Projections;
using RoadRegistry.Tests.BackOffice;
using Schema.RoadSegments;

public class RoadSegmentWidthAttributeLatestItemProjectionTests
{
    private readonly Fixture _fixture;

    public RoadSegmentWidthAttributeLatestItemProjectionTests()
    {
        _fixture = new Fixture();

        _fixture.CustomizeArchiveId();
        _fixture.CustomizeAttributeId();
        _fixture.CustomizeRoadSegmentId();
        _fixture.CustomizeOrganizationId();
        _fixture.CustomizePolylineM();
        _fixture.CustomizeRoadSegmentWidth();
        _fixture.CustomizeRoadSegmentGeometryDrawMethod();
        _fixture.CustomizeRoadSegmentMorphology();
        _fixture.CustomizeRoadSegmentStatus();
        _fixture.CustomizeRoadSegmentCategory();
        _fixture.CustomizeRoadSegmentAccessRestriction();

        _fixture.CustomizeImportedRoadSegmentWidthAttributes();

        _fixture.CustomizeRoadSegmentWidthAttributes();
        _fixture.CustomizeRoadSegmentAdded();
        _fixture.CustomizeRoadSegmentModified();
        _fixture.CustomizeRoadSegmentAttributesModified();
        _fixture.CustomizeRoadSegmentGeometryModified();
        _fixture.CustomizeRoadSegmentRemoved();
        _fixture.CustomizeRoadNetworkChangesAccepted();
    }

    [Fact]
    public Task When_adding_road_segment_with_widths()
    {
        var message = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.CreateMany<RoadSegmentAdded>());

        var expectedRecords = Array.ConvertAll(message.Changes, change =>
            {
                var segment = change.RoadSegmentAdded;
                return segment.Widths.Select(width => new RoadSegmentWidthAttributeLatestItem
                {
                    Id = width.AttributeId,
                    RoadSegmentId = segment.Id,
                    AsOfGeometryVersion = width.AsOfGeometryVersion,
                    Width = new RoadSegmentWidth(width.Width),
                    WidthLabel = new RoadSegmentWidth(width.Width).ToDutchString(),
                    FromPosition = (double)width.FromPosition,
                    ToPosition = (double)width.ToPosition,
                    OrganizationId = message.OrganizationId,
                    OrganizationName = message.Organization,
                    IsRemoved = false,
                    CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(message.When),
                    VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(message.When)
                });
            })
            .SelectMany(x => x);

        return new RoadSegmentWidthAttributeLatestItemProjection()
            .Scenario()
            .Given(message)
            .Expect(expectedRecords);
    }

    [Fact]
    public Task When_importing_a_road_segment_without_widths()
    {
        var importedRoadSegment = _fixture.Create<ImportedRoadSegment>();
        importedRoadSegment.Widths = [];

        return new RoadSegmentWidthAttributeLatestItemProjection()
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
                segment.Widths = _fixture
                    .CreateMany<ImportedRoadSegmentWidthAttribute>(random.Next(1, 10))
                    .ToArray();

                var expected = segment
                    .Widths
                    .Select(width => new RoadSegmentWidthAttributeLatestItem
                    {
                        Id = width.AttributeId,
                        RoadSegmentId = segment.Id,
                        AsOfGeometryVersion = width.AsOfGeometryVersion,
                        Width = new RoadSegmentWidth(width.Width),
                        WidthLabel = new RoadSegmentWidth(width.Width).ToDutchString(),
                        FromPosition = (double)width.FromPosition,
                        ToPosition = (double)width.ToPosition,
                        OrganizationId = width.Origin.OrganizationId,
                        OrganizationName = width.Origin.Organization,
                        IsRemoved = false,
                        CreatedOnTimestamp = width.Origin.Since.ToBelgianInstant(),
                        VersionTimestamp = width.Origin.Since.ToBelgianInstant()
                    });

                return new
                {
                    importedRoadSegment = segment,
                    expected
                };
            }).ToList();

        return new RoadSegmentWidthAttributeLatestItemProjection()
            .Scenario()
            .Given(data.Select(d => d.importedRoadSegment))
            .Expect(data
                .SelectMany(d => d.expected)
                .Cast<object>()
                .ToArray()
            );
    }

    [Fact]
    public Task When_modifying_road_segments_with_modified_widths_only()
    {
        _fixture.Freeze<RoadSegmentId>();

        var roadSegmentAdded = _fixture.Create<RoadSegmentAdded>();
        var acceptedRoadSegmentAdded = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(roadSegmentAdded);

        var roadSegmentModified = _fixture.Create<RoadSegmentModified>();
        roadSegmentModified.Widths = roadSegmentAdded.Widths
            .Select(attributes =>
            {
                var roadSegmentWidthAttributes = _fixture.Create<RoadSegmentWidthAttributes>();
                roadSegmentWidthAttributes.AttributeId = attributes.AttributeId;
                return roadSegmentWidthAttributes;
            })
            .ToArray();

        var acceptedRoadSegmentModified = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(roadSegmentModified);

        var expectedRecords = Array.ConvertAll(acceptedRoadSegmentModified.Changes, change =>
        {
            var segment = change.RoadSegmentModified;

            return segment.Widths.Select(width => (object)new RoadSegmentWidthAttributeLatestItem
            {
                Id = width.AttributeId,
                RoadSegmentId = segment.Id,
                AsOfGeometryVersion = width.AsOfGeometryVersion,
                Width = new RoadSegmentWidth(width.Width),
                WidthLabel = new RoadSegmentWidth(width.Width).ToDutchString(),
                FromPosition = (double)width.FromPosition,
                ToPosition = (double)width.ToPosition,
                OrganizationId = acceptedRoadSegmentModified.OrganizationId,
                OrganizationName = acceptedRoadSegmentModified.Organization,
                IsRemoved = false,
                CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentAdded.When),
                VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentModified.When)
            });
        }).SelectMany(x => x);

        return new RoadSegmentWidthAttributeLatestItemProjection()
            .Scenario()
            .Given(acceptedRoadSegmentAdded, acceptedRoadSegmentModified)
            .Expect(expectedRecords);
    }

    [Fact]
    public Task When_modifying_road_segments_with_removed_widths_only()
    {
        _fixture.Freeze<RoadSegmentId>();

        var roadSegmentAdded = _fixture.Create<RoadSegmentAdded>();
        var acceptedRoadSegmentAdded = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(roadSegmentAdded);

        var roadSegmentModified = _fixture.Create<RoadSegmentModified>();
        roadSegmentModified.Widths = [];

        var acceptedRoadSegmentModified = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(roadSegmentModified);

        var expectedRecords = Array.ConvertAll(acceptedRoadSegmentAdded.Changes, change =>
        {
            var segment = change.RoadSegmentAdded;

            return segment.Widths.Select(width => (object)new RoadSegmentWidthAttributeLatestItem
            {
                Id = width.AttributeId,
                RoadSegmentId = segment.Id,
                AsOfGeometryVersion = width.AsOfGeometryVersion,
                Width = new RoadSegmentWidth(width.Width),
                WidthLabel = new RoadSegmentWidth(width.Width).ToDutchString(),
                FromPosition = (double)width.FromPosition,
                ToPosition = (double)width.ToPosition,
                OrganizationId = acceptedRoadSegmentModified.OrganizationId,
                OrganizationName = acceptedRoadSegmentModified.Organization,
                IsRemoved = true,
                CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentAdded.When),
                VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentModified.When)
            });
        }).SelectMany(x => x);

        return new RoadSegmentWidthAttributeLatestItemProjection()
            .Scenario()
            .Given(acceptedRoadSegmentAdded, acceptedRoadSegmentModified)
            .Expect(expectedRecords);
    }

    [Fact]
    public Task When_modifying_road_segments_with_some_added_widths()
    {
        _fixture.Freeze<RoadSegmentId>();

        var roadSegmentAdded = _fixture.Create<RoadSegmentAdded>();
        var acceptedRoadSegmentAdded = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(roadSegmentAdded);

        var roadSegmentModified = _fixture.Create<RoadSegmentModified>();
        roadSegmentModified.Widths = roadSegmentAdded.Widths
            .Append(_fixture.Create<RoadSegmentWidthAttributes>())
            .ToArray();

        var acceptedRoadSegmentModified = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(roadSegmentModified);

        var expectedRecords = Array.ConvertAll(acceptedRoadSegmentModified.Changes, change =>
        {
            var segment = change.RoadSegmentModified;

            return segment.Widths.Select(width =>
            {
                var isAdded = roadSegmentAdded.Widths.All(x => x.AttributeId != width.AttributeId);

                return (object)new RoadSegmentWidthAttributeLatestItem
                {
                    Id = width.AttributeId,
                    RoadSegmentId = segment.Id,
                    AsOfGeometryVersion = width.AsOfGeometryVersion,
                    Width = new RoadSegmentWidth(width.Width),
                    WidthLabel = new RoadSegmentWidth(width.Width).ToDutchString(),
                    FromPosition = (double)width.FromPosition,
                    ToPosition = (double)width.ToPosition,
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

        return new RoadSegmentWidthAttributeLatestItemProjection()
            .Scenario()
            .Given(acceptedRoadSegmentAdded, acceptedRoadSegmentModified)
            .Expect(expectedRecords);
    }

    [Fact]
    public Task When_modifying_road_segments_with_some_modified_widths()
    {
        _fixture.Freeze<RoadSegmentId>();

        var roadSegmentAdded = _fixture.Create<RoadSegmentAdded>();
        var acceptedRoadSegmentAdded = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(roadSegmentAdded);

        var roadSegmentModified = _fixture.Create<RoadSegmentModified>();
        roadSegmentModified.Widths = roadSegmentAdded.Widths
            .Select((attributes, i) =>
            {
                if (i % 2 != 0)
                {
                    return attributes;
                }

                var roadSegmentWidthAttributes = _fixture.Create<RoadSegmentWidthAttributes>();
                roadSegmentWidthAttributes.AttributeId = attributes.AttributeId;
                return roadSegmentWidthAttributes;

            })
            .ToArray();

        var acceptedRoadSegmentModified = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(roadSegmentModified);

        var expectedRecords = Array.ConvertAll(acceptedRoadSegmentModified.Changes, change =>
        {
            var segment = change.RoadSegmentModified;

            return segment.Widths.Select(width => (object)new RoadSegmentWidthAttributeLatestItem
            {
                Id = width.AttributeId,
                RoadSegmentId = segment.Id,
                AsOfGeometryVersion = width.AsOfGeometryVersion,
                Width = new RoadSegmentWidth(width.Width),
                WidthLabel = new RoadSegmentWidth(width.Width).ToDutchString(),
                FromPosition = (double)width.FromPosition,
                ToPosition = (double)width.ToPosition,
                OrganizationId = acceptedRoadSegmentModified.OrganizationId,
                OrganizationName = acceptedRoadSegmentModified.Organization,
                IsRemoved = false,
                CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentAdded.When),
                VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentModified.When)
            });
        }).SelectMany(x => x);

        return new RoadSegmentWidthAttributeLatestItemProjection()
            .Scenario()
            .Given(acceptedRoadSegmentAdded, acceptedRoadSegmentModified)
            .Expect(expectedRecords);
    }

    [Fact]
    public Task When_modifying_road_segments_with_some_removed_widths()
    {
        _fixture.Freeze<RoadSegmentId>();

        var roadSegmentAdded = _fixture.Create<RoadSegmentAdded>();
        var acceptedRoadSegmentAdded = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(roadSegmentAdded);

        var roadSegmentModified = _fixture.Create<RoadSegmentModified>();
        roadSegmentModified.Widths = roadSegmentAdded.Widths
            .Take(roadSegmentAdded.Widths.Length - 1)
            .ToArray();

        var removedWidth = roadSegmentAdded.Widths.Except(roadSegmentModified.Widths).Single();

        var acceptedRoadSegmentModified = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(roadSegmentModified);

        var expectedRecords = Array.ConvertAll(acceptedRoadSegmentModified.Changes, change =>
        {
            var segment = change.RoadSegmentModified;

            return segment.Widths.Select(width => (object)new RoadSegmentWidthAttributeLatestItem
            {
                Id = width.AttributeId,
                RoadSegmentId = segment.Id,
                AsOfGeometryVersion = width.AsOfGeometryVersion,
                Width = new RoadSegmentWidth(width.Width),
                WidthLabel = new RoadSegmentWidth(width.Width).ToDutchString(),
                FromPosition = (double)width.FromPosition,
                ToPosition = (double)width.ToPosition,
                OrganizationId = acceptedRoadSegmentModified.OrganizationId,
                OrganizationName = acceptedRoadSegmentModified.Organization,
                IsRemoved = false,
                CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentAdded.When),
                VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentModified.When)
            });
        })
            .SelectMany(x => x)
            .Concat([
                new RoadSegmentWidthAttributeLatestItem
                {
                    Id = removedWidth.AttributeId,
                    RoadSegmentId = roadSegmentAdded.Id,
                    AsOfGeometryVersion = removedWidth.AsOfGeometryVersion,
                    Width = new RoadSegmentWidth(removedWidth.Width),
                    WidthLabel = new RoadSegmentWidth(removedWidth.Width).ToDutchString(),
                    FromPosition = (double)removedWidth.FromPosition,
                    ToPosition = (double)removedWidth.ToPosition,
                    OrganizationId = acceptedRoadSegmentModified.OrganizationId,
                    OrganizationName = acceptedRoadSegmentModified.Organization,
                    IsRemoved = true,
                    CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentAdded.When),
                    VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentModified.When)
                }
            ]);

        return new RoadSegmentWidthAttributeLatestItemProjection()
            .Scenario()
            .Given(acceptedRoadSegmentAdded, acceptedRoadSegmentModified)
            .Expect(expectedRecords);
    }

    [Fact]
    public Task When_modifying_road_segment_geometry_with_new_widths_only()
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

            return segment.Widths.Select(width => (object)new RoadSegmentWidthAttributeLatestItem
            {
                Id = width.AttributeId,
                RoadSegmentId = segment.Id,
                AsOfGeometryVersion = width.AsOfGeometryVersion,
                Width = new RoadSegmentWidth(width.Width),
                WidthLabel = new RoadSegmentWidth(width.Width).ToDutchString(),
                FromPosition = (double)width.FromPosition,
                ToPosition = (double)width.ToPosition,
                OrganizationId = acceptedRoadSegmentGeometryModified.OrganizationId,
                OrganizationName = acceptedRoadSegmentGeometryModified.Organization,
                IsRemoved = false,
                CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentGeometryModified.When),
                VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentGeometryModified.When)
            });
        })
            .SelectMany(x => x)
            .Concat(roadSegmentAdded.Widths
                .Select(width =>
                    new RoadSegmentWidthAttributeLatestItem
                    {
                        Id = width.AttributeId,
                        RoadSegmentId = roadSegmentAdded.Id,
                        AsOfGeometryVersion = width.AsOfGeometryVersion,
                        Width = new RoadSegmentWidth(width.Width),
                        WidthLabel = new RoadSegmentWidth(width.Width).ToDutchString(),
                        FromPosition = (double)width.FromPosition,
                        ToPosition = (double)width.ToPosition,
                        OrganizationId = acceptedRoadSegmentGeometryModified.OrganizationId,
                        OrganizationName = acceptedRoadSegmentGeometryModified.Organization,
                        IsRemoved = true,
                        CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentAdded.When),
                        VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentGeometryModified.When)
                    }
                ));

        return new RoadSegmentWidthAttributeLatestItemProjection()
            .Scenario()
            .Given(acceptedRoadSegmentAdded, acceptedRoadSegmentGeometryModified)
            .ExpectInAnyOrder(expectedRecords);
    }

    [Fact]
    public Task When_modifying_road_segments_with_new_widths_only()
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

            return segment.Widths.Select(width => (object)new RoadSegmentWidthAttributeLatestItem
            {
                Id = width.AttributeId,
                RoadSegmentId = segment.Id,
                AsOfGeometryVersion = width.AsOfGeometryVersion,
                Width = new RoadSegmentWidth(width.Width),
                WidthLabel = new RoadSegmentWidth(width.Width).ToDutchString(),
                FromPosition = (double)width.FromPosition,
                ToPosition = (double)width.ToPosition,
                OrganizationId = acceptedRoadSegmentModified.OrganizationId,
                OrganizationName = acceptedRoadSegmentModified.Organization,
                IsRemoved = false,
                CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentModified.When),
                VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentModified.When)
            });
        })
            .SelectMany(x => x)
            .Concat(roadSegmentAdded.Widths
                .Select(width =>
                    new RoadSegmentWidthAttributeLatestItem
                    {
                        Id = width.AttributeId,
                        RoadSegmentId = roadSegmentAdded.Id,
                        AsOfGeometryVersion = width.AsOfGeometryVersion,
                        Width = new RoadSegmentWidth(width.Width),
                        WidthLabel = new RoadSegmentWidth(width.Width).ToDutchString(),
                        FromPosition = (double)width.FromPosition,
                        ToPosition = (double)width.ToPosition,
                        OrganizationId = acceptedRoadSegmentModified.OrganizationId,
                        OrganizationName = acceptedRoadSegmentModified.Organization,
                        IsRemoved = true,
                        CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentAdded.When),
                        VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentModified.When)
                    }
                ));

        return new RoadSegmentWidthAttributeLatestItemProjection()
            .Scenario()
            .Given(acceptedRoadSegmentAdded, acceptedRoadSegmentModified)
            .ExpectInAnyOrder(expectedRecords);
    }

    [Fact]
    public Task When_modifying_road_segment_attributes_with_new_widths_only()
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

            return segment.Widths.Select(width => (object)new RoadSegmentWidthAttributeLatestItem
            {
                Id = width.AttributeId,
                RoadSegmentId = segment.Id,
                AsOfGeometryVersion = width.AsOfGeometryVersion,
                Width = new RoadSegmentWidth(width.Width),
                WidthLabel = new RoadSegmentWidth(width.Width).ToDutchString(),
                FromPosition = (double)width.FromPosition,
                ToPosition = (double)width.ToPosition,
                OrganizationId = acceptedRoadSegmentModified.OrganizationId,
                OrganizationName = acceptedRoadSegmentModified.Organization,
                IsRemoved = false,
                CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentModified.When),
                VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentModified.When)
            });
        })
            .SelectMany(x => x)
            .Concat(roadSegmentAdded.Widths
                .Select(width =>
                    new RoadSegmentWidthAttributeLatestItem
                    {
                        Id = width.AttributeId,
                        RoadSegmentId = roadSegmentAdded.Id,
                        AsOfGeometryVersion = width.AsOfGeometryVersion,
                        Width = new RoadSegmentWidth(width.Width),
                        WidthLabel = new RoadSegmentWidth(width.Width).ToDutchString(),
                        FromPosition = (double)width.FromPosition,
                        ToPosition = (double)width.ToPosition,
                        OrganizationId = acceptedRoadSegmentModified.OrganizationId,
                        OrganizationName = acceptedRoadSegmentModified.Organization,
                        IsRemoved = true,
                        CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentAdded.When),
                        VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentModified.When)
                    }
                ));

        return new RoadSegmentWidthAttributeLatestItemProjection()
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

            return segment.Widths.Select(width => (object)new RoadSegmentWidthAttributeLatestItem
            {
                Id = width.AttributeId,
                RoadSegmentId = segment.Id,
                AsOfGeometryVersion = width.AsOfGeometryVersion,
                Width = new RoadSegmentWidth(width.Width),
                WidthLabel = new RoadSegmentWidth(width.Width).ToDutchString(),
                FromPosition = (double)width.FromPosition,
                ToPosition = (double)width.ToPosition,
                OrganizationId = acceptedRoadSegmentRemoved.OrganizationId,
                OrganizationName = acceptedRoadSegmentRemoved.Organization,
                IsRemoved = true,
                CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentAdded.When),
                VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentRemoved.When)
            });
        }).SelectMany(x => x);

        return new RoadSegmentWidthAttributeLatestItemProjection()
            .Scenario()
            .Given(acceptedRoadSegmentAdded, acceptedRoadSegmentRemoved)
            .Expect(expectedRecords);
    }
}
