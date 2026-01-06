namespace RoadRegistry.Integration.ProjectionHost.Tests.Projections;

using AutoFixture;
using BackOffice;
using BackOffice.Messages;
using Integration.Projections;
using RoadRegistry.Tests.BackOffice;
using Schema.RoadSegments;

public class RoadSegmentLaneAttributeLatestItemProjectionTests
{
    private readonly Fixture _fixture;

    public RoadSegmentLaneAttributeLatestItemProjectionTests()
    {
        _fixture = FixtureFactory.Create();

        _fixture.CustomizeArchiveId();
        _fixture.CustomizeAttributeId();
        _fixture.CustomizeRoadSegmentId();
        _fixture.CustomizeOrganizationId();
        _fixture.CustomizePolylineM();
        _fixture.CustomizeRoadSegmentLaneCount();
        _fixture.CustomizeRoadSegmentLaneDirection();
        _fixture.CustomizeRoadSegmentGeometryDrawMethod();
        _fixture.CustomizeRoadSegmentMorphology();
        _fixture.CustomizeRoadSegmentStatus();
        _fixture.CustomizeRoadSegmentCategory();
        _fixture.CustomizeRoadSegmentAccessRestriction();

        _fixture.CustomizeImportedRoadSegmentLaneAttributes();

        _fixture.CustomizeRoadSegmentLaneAttributes();
        _fixture.CustomizeRoadSegmentAdded();
        _fixture.CustomizeRoadSegmentModified();
        _fixture.CustomizeRoadSegmentAttributesModified();
        _fixture.CustomizeRoadSegmentGeometryModified();
        _fixture.CustomizeRoadSegmentRemoved();
        _fixture.CustomizeRoadNetworkChangesAccepted();
    }

    [Fact]
    public Task When_adding_road_segment_with_lanes()
    {
        var message = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.CreateMany<RoadSegmentAdded>());

        var expectedRecords = Array.ConvertAll(message.Changes, change =>
            {
                var segment = change.RoadSegmentAdded;
                return segment.Lanes.Select(lane => new RoadSegmentLaneAttributeLatestItem
                {
                    Id = lane.AttributeId,
                    RoadSegmentId = segment.Id,
                    AsOfGeometryVersion = lane.AsOfGeometryVersion,
                    Count = lane.Count,
                    DirectionId = RoadSegmentLaneDirection.Parse(lane.Direction).Translation.Identifier,
                    DirectionLabel = RoadSegmentLaneDirection.Parse(lane.Direction).Translation.Name,
                    FromPosition = (double)lane.FromPosition,
                    ToPosition = (double)lane.ToPosition,
                    OrganizationId = message.OrganizationId,
                    OrganizationName = message.Organization,
                    IsRemoved = false,
                    CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(message.When),
                    VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(message.When)
                });
            })
            .SelectMany(x => x);

        return new RoadSegmentLaneAttributeLatestItemProjection()
            .Scenario()
            .Given(message)
            .Expect(expectedRecords);
    }

    [Fact]
    public Task When_importing_a_road_segment_without_lanes()
    {
        var importedRoadSegment = _fixture.Create<ImportedRoadSegment>();
        importedRoadSegment.Lanes = [];

        return new RoadSegmentLaneAttributeLatestItemProjection()
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
                segment.Lanes = _fixture
                    .CreateMany<ImportedRoadSegmentLaneAttribute>(random.Next(1, 10))
                    .ToArray();

                var expected = segment
                    .Lanes
                    .Select(lane => new RoadSegmentLaneAttributeLatestItem
                    {
                        Id = lane.AttributeId,
                        RoadSegmentId = segment.Id,
                        AsOfGeometryVersion = lane.AsOfGeometryVersion,
                        Count = lane.Count,
                        DirectionId = RoadSegmentLaneDirection.Parse(lane.Direction).Translation.Identifier,
                        DirectionLabel = RoadSegmentLaneDirection.Parse(lane.Direction).Translation.Name,
                        FromPosition = (double)lane.FromPosition,
                        ToPosition = (double)lane.ToPosition,
                        OrganizationId = lane.Origin.OrganizationId,
                        OrganizationName = lane.Origin.Organization,
                        IsRemoved = false,
                        CreatedOnTimestamp = lane.Origin.Since.ToBelgianInstant(),
                        VersionTimestamp = lane.Origin.Since.ToBelgianInstant()
                    });

                return new
                {
                    importedRoadSegment = segment,
                    expected
                };
            }).ToList();

        return new RoadSegmentLaneAttributeLatestItemProjection()
            .Scenario()
            .Given(data.Select(d => d.importedRoadSegment))
            .Expect(data
                .SelectMany(d => d.expected)
                .Cast<object>()
                .ToArray()
            );
    }

    [Fact]
    public Task When_modifying_road_segments_with_modified_lanes_only()
    {
        _fixture.Freeze<RoadSegmentId>();

        var roadSegmentAdded = _fixture.Create<RoadSegmentAdded>();
        var acceptedRoadSegmentAdded = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(roadSegmentAdded);

        var roadSegmentModified = _fixture.Create<RoadSegmentModified>();
        roadSegmentModified.Lanes = roadSegmentAdded.Lanes
            .Select(attributes =>
            {
                var roadSegmentLaneAttributes = _fixture.Create<RoadSegmentLaneAttributes>();
                roadSegmentLaneAttributes.AttributeId = attributes.AttributeId;
                return roadSegmentLaneAttributes;
            })
            .ToArray();

        var acceptedRoadSegmentModified = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(roadSegmentModified);

        var expectedRecords = Array.ConvertAll(acceptedRoadSegmentModified.Changes, change =>
        {
            var segment = change.RoadSegmentModified;

            return segment.Lanes.Select(lane => (object)new RoadSegmentLaneAttributeLatestItem
            {
                Id = lane.AttributeId,
                RoadSegmentId = segment.Id,
                AsOfGeometryVersion = lane.AsOfGeometryVersion,
                Count = lane.Count,
                DirectionId = RoadSegmentLaneDirection.Parse(lane.Direction).Translation.Identifier,
                DirectionLabel = RoadSegmentLaneDirection.Parse(lane.Direction).Translation.Name,
                FromPosition = (double)lane.FromPosition,
                ToPosition = (double)lane.ToPosition,
                OrganizationId = acceptedRoadSegmentModified.OrganizationId,
                OrganizationName = acceptedRoadSegmentModified.Organization,
                IsRemoved = false,
                CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentAdded.When),
                VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentModified.When)
            });
        }).SelectMany(x => x);

        return new RoadSegmentLaneAttributeLatestItemProjection()
            .Scenario()
            .Given(acceptedRoadSegmentAdded, acceptedRoadSegmentModified)
            .Expect(expectedRecords);
    }

    [Fact]
    public Task When_modifying_road_segments_with_removed_lanes_only()
    {
        _fixture.Freeze<RoadSegmentId>();

        var roadSegmentAdded = _fixture.Create<RoadSegmentAdded>();
        var acceptedRoadSegmentAdded = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(roadSegmentAdded);

        var roadSegmentModified = _fixture.Create<RoadSegmentModified>();
        roadSegmentModified.Lanes = [];

        var acceptedRoadSegmentModified = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(roadSegmentModified);

        var expectedRecords = Array.ConvertAll(acceptedRoadSegmentAdded.Changes, change =>
        {
            var segment = change.RoadSegmentAdded;

            return segment.Lanes.Select(lane => (object)new RoadSegmentLaneAttributeLatestItem
            {
                Id = lane.AttributeId,
                RoadSegmentId = segment.Id,
                AsOfGeometryVersion = lane.AsOfGeometryVersion,
                Count = lane.Count,
                DirectionId = RoadSegmentLaneDirection.Parse(lane.Direction).Translation.Identifier,
                DirectionLabel = RoadSegmentLaneDirection.Parse(lane.Direction).Translation.Name,
                FromPosition = (double)lane.FromPosition,
                ToPosition = (double)lane.ToPosition,
                OrganizationId = acceptedRoadSegmentModified.OrganizationId,
                OrganizationName = acceptedRoadSegmentModified.Organization,
                IsRemoved = true,
                CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentAdded.When),
                VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentModified.When)
            });
        }).SelectMany(x => x);

        return new RoadSegmentLaneAttributeLatestItemProjection()
            .Scenario()
            .Given(acceptedRoadSegmentAdded, acceptedRoadSegmentModified)
            .Expect(expectedRecords);
    }

    [Fact]
    public Task When_modifying_road_segments_with_some_added_lanes()
    {
        _fixture.Freeze<RoadSegmentId>();

        var roadSegmentAdded = _fixture.Create<RoadSegmentAdded>();
        var acceptedRoadSegmentAdded = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(roadSegmentAdded);

        var roadSegmentModified = _fixture.Create<RoadSegmentModified>();
        roadSegmentModified.Lanes = roadSegmentAdded.Lanes
            .Append(_fixture.Create<RoadSegmentLaneAttributes>())
            .ToArray();

        var acceptedRoadSegmentModified = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(roadSegmentModified);

        var expectedRecords = Array.ConvertAll(acceptedRoadSegmentModified.Changes, change =>
        {
            var segment = change.RoadSegmentModified;

            return segment.Lanes.Select(lane =>
            {
                var isAdded = roadSegmentAdded.Lanes.All(x => x.AttributeId != lane.AttributeId);

                return (object)new RoadSegmentLaneAttributeLatestItem
                {
                    Id = lane.AttributeId,
                    RoadSegmentId = segment.Id,
                    AsOfGeometryVersion = lane.AsOfGeometryVersion,
                    Count = lane.Count,
                    DirectionId = RoadSegmentLaneDirection.Parse(lane.Direction).Translation.Identifier,
                    DirectionLabel = RoadSegmentLaneDirection.Parse(lane.Direction).Translation.Name,
                    FromPosition = (double)lane.FromPosition,
                    ToPosition = (double)lane.ToPosition,
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

        return new RoadSegmentLaneAttributeLatestItemProjection()
            .Scenario()
            .Given(acceptedRoadSegmentAdded, acceptedRoadSegmentModified)
            .Expect(expectedRecords);
    }

    [Fact]
    public Task When_modifying_road_segments_with_some_modified_lanes()
    {
        _fixture.Freeze<RoadSegmentId>();

        var roadSegmentAdded = _fixture.Create<RoadSegmentAdded>();
        var acceptedRoadSegmentAdded = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(roadSegmentAdded);

        var roadSegmentModified = _fixture.Create<RoadSegmentModified>();
        roadSegmentModified.Lanes = roadSegmentAdded.Lanes
            .Select((attributes, i) =>
            {
                if (i % 2 != 0)
                {
                    return attributes;
                }

                var roadSegmentLaneAttributes = _fixture.Create<RoadSegmentLaneAttributes>();
                roadSegmentLaneAttributes.AttributeId = attributes.AttributeId;
                return roadSegmentLaneAttributes;

            })
            .ToArray();

        var acceptedRoadSegmentModified = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(roadSegmentModified);

        var expectedRecords = Array.ConvertAll(acceptedRoadSegmentModified.Changes, change =>
        {
            var segment = change.RoadSegmentModified;

            return segment.Lanes.Select(lane => (object)new RoadSegmentLaneAttributeLatestItem
            {
                Id = lane.AttributeId,
                RoadSegmentId = segment.Id,
                AsOfGeometryVersion = lane.AsOfGeometryVersion,
                Count = lane.Count,
                DirectionId = RoadSegmentLaneDirection.Parse(lane.Direction).Translation.Identifier,
                DirectionLabel = RoadSegmentLaneDirection.Parse(lane.Direction).Translation.Name,
                FromPosition = (double)lane.FromPosition,
                ToPosition = (double)lane.ToPosition,
                OrganizationId = acceptedRoadSegmentModified.OrganizationId,
                OrganizationName = acceptedRoadSegmentModified.Organization,
                IsRemoved = false,
                CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentAdded.When),
                VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentModified.When)
            });
        }).SelectMany(x => x);

        return new RoadSegmentLaneAttributeLatestItemProjection()
            .Scenario()
            .Given(acceptedRoadSegmentAdded, acceptedRoadSegmentModified)
            .Expect(expectedRecords);
    }

    [Fact]
    public Task When_modifying_road_segments_with_some_removed_lanes()
    {
        _fixture.Freeze<RoadSegmentId>();

        var roadSegmentAdded = _fixture.Create<RoadSegmentAdded>();
        var acceptedRoadSegmentAdded = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(roadSegmentAdded);

        var roadSegmentModified = _fixture.Create<RoadSegmentModified>();
        roadSegmentModified.Lanes = roadSegmentAdded.Lanes
            .Take(roadSegmentAdded.Lanes.Length - 1)
            .ToArray();

        var removedLane = roadSegmentAdded.Lanes.Except(roadSegmentModified.Lanes).Single();

        var acceptedRoadSegmentModified = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(roadSegmentModified);

        var expectedRecords = Array.ConvertAll(acceptedRoadSegmentModified.Changes, change =>
        {
            var segment = change.RoadSegmentModified;

            return segment.Lanes.Select(lane => (object)new RoadSegmentLaneAttributeLatestItem
            {
                Id = lane.AttributeId,
                RoadSegmentId = segment.Id,
                AsOfGeometryVersion = lane.AsOfGeometryVersion,
                Count = lane.Count,
                DirectionId = RoadSegmentLaneDirection.Parse(lane.Direction).Translation.Identifier,
                DirectionLabel = RoadSegmentLaneDirection.Parse(lane.Direction).Translation.Name,
                FromPosition = (double)lane.FromPosition,
                ToPosition = (double)lane.ToPosition,
                OrganizationId = acceptedRoadSegmentModified.OrganizationId,
                OrganizationName = acceptedRoadSegmentModified.Organization,
                IsRemoved = false,
                CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentAdded.When),
                VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentModified.When)
            });
        })
            .SelectMany(x => x)
            .Concat([
                new RoadSegmentLaneAttributeLatestItem
                {
                    Id = removedLane.AttributeId,
                    RoadSegmentId = roadSegmentAdded.Id,
                    AsOfGeometryVersion = removedLane.AsOfGeometryVersion,
                    Count = removedLane.Count,
                    DirectionId = RoadSegmentLaneDirection.Parse(removedLane.Direction).Translation.Identifier,
                    DirectionLabel = RoadSegmentLaneDirection.Parse(removedLane.Direction).Translation.Name,
                    FromPosition = (double)removedLane.FromPosition,
                    ToPosition = (double)removedLane.ToPosition,
                    OrganizationId = acceptedRoadSegmentModified.OrganizationId,
                    OrganizationName = acceptedRoadSegmentModified.Organization,
                    IsRemoved = true,
                    CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentAdded.When),
                    VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentModified.When)
                }
            ]);

        return new RoadSegmentLaneAttributeLatestItemProjection()
            .Scenario()
            .Given(acceptedRoadSegmentAdded, acceptedRoadSegmentModified)
            .Expect(expectedRecords);
    }

    [Fact]
    public Task When_modifying_road_segment_geometry_with_new_lanes_only()
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

            return segment.Lanes.Select(lane => (object)new RoadSegmentLaneAttributeLatestItem
            {
                Id = lane.AttributeId,
                RoadSegmentId = segment.Id,
                AsOfGeometryVersion = lane.AsOfGeometryVersion,
                Count = lane.Count,
                DirectionId = RoadSegmentLaneDirection.Parse(lane.Direction).Translation.Identifier,
                DirectionLabel = RoadSegmentLaneDirection.Parse(lane.Direction).Translation.Name,
                FromPosition = (double)lane.FromPosition,
                ToPosition = (double)lane.ToPosition,
                OrganizationId = acceptedRoadSegmentGeometryModified.OrganizationId,
                OrganizationName = acceptedRoadSegmentGeometryModified.Organization,
                IsRemoved = false,
                CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentGeometryModified.When),
                VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentGeometryModified.When)
            });
        })
            .SelectMany(x => x)
            .Concat(roadSegmentAdded.Lanes
                .Select(lane =>
                    new RoadSegmentLaneAttributeLatestItem
                    {
                        Id = lane.AttributeId,
                        RoadSegmentId = roadSegmentAdded.Id,
                        AsOfGeometryVersion = lane.AsOfGeometryVersion,
                        Count = lane.Count,
                        DirectionId = RoadSegmentLaneDirection.Parse(lane.Direction).Translation.Identifier,
                        DirectionLabel = RoadSegmentLaneDirection.Parse(lane.Direction).Translation.Name,
                        FromPosition = (double)lane.FromPosition,
                        ToPosition = (double)lane.ToPosition,
                        OrganizationId = acceptedRoadSegmentGeometryModified.OrganizationId,
                        OrganizationName = acceptedRoadSegmentGeometryModified.Organization,
                        IsRemoved = true,
                        CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentAdded.When),
                        VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentGeometryModified.When)
                    }
                ));

        return new RoadSegmentLaneAttributeLatestItemProjection()
            .Scenario()
            .Given(acceptedRoadSegmentAdded, acceptedRoadSegmentGeometryModified)
            .ExpectInAnyOrder(expectedRecords);
    }

    [Fact]
    public Task When_modifying_road_segments_with_new_lanes_only()
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

            return segment.Lanes.Select(lane => (object)new RoadSegmentLaneAttributeLatestItem
            {
                Id = lane.AttributeId,
                RoadSegmentId = segment.Id,
                AsOfGeometryVersion = lane.AsOfGeometryVersion,
                Count = lane.Count,
                DirectionId = RoadSegmentLaneDirection.Parse(lane.Direction).Translation.Identifier,
                DirectionLabel = RoadSegmentLaneDirection.Parse(lane.Direction).Translation.Name,
                FromPosition = (double)lane.FromPosition,
                ToPosition = (double)lane.ToPosition,
                OrganizationId = acceptedRoadSegmentModified.OrganizationId,
                OrganizationName = acceptedRoadSegmentModified.Organization,
                IsRemoved = false,
                CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentModified.When),
                VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentModified.When)
            });
        })
            .SelectMany(x => x)
            .Concat(roadSegmentAdded.Lanes
                .Select(lane =>
                    new RoadSegmentLaneAttributeLatestItem
                    {
                        Id = lane.AttributeId,
                        RoadSegmentId = roadSegmentAdded.Id,
                        AsOfGeometryVersion = lane.AsOfGeometryVersion,
                        Count = lane.Count,
                        DirectionId = RoadSegmentLaneDirection.Parse(lane.Direction).Translation.Identifier,
                        DirectionLabel = RoadSegmentLaneDirection.Parse(lane.Direction).Translation.Name,
                        FromPosition = (double)lane.FromPosition,
                        ToPosition = (double)lane.ToPosition,
                        OrganizationId = acceptedRoadSegmentModified.OrganizationId,
                        OrganizationName = acceptedRoadSegmentModified.Organization,
                        IsRemoved = true,
                        CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentAdded.When),
                        VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentModified.When)
                    }
                ));

        return new RoadSegmentLaneAttributeLatestItemProjection()
            .Scenario()
            .Given(acceptedRoadSegmentAdded, acceptedRoadSegmentModified)
            .ExpectInAnyOrder(expectedRecords);
    }

    [Fact]
    public Task When_modifying_road_segment_attributes_with_new_lanes_only()
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

            return segment.Lanes.Select(lane => (object)new RoadSegmentLaneAttributeLatestItem
            {
                Id = lane.AttributeId,
                RoadSegmentId = segment.Id,
                AsOfGeometryVersion = lane.AsOfGeometryVersion,
                Count = lane.Count,
                DirectionId = RoadSegmentLaneDirection.Parse(lane.Direction).Translation.Identifier,
                DirectionLabel = RoadSegmentLaneDirection.Parse(lane.Direction).Translation.Name,
                FromPosition = (double)lane.FromPosition,
                ToPosition = (double)lane.ToPosition,
                OrganizationId = acceptedRoadSegmentModified.OrganizationId,
                OrganizationName = acceptedRoadSegmentModified.Organization,
                IsRemoved = false,
                CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentModified.When),
                VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentModified.When)
            });
        })
            .SelectMany(x => x)
            .Concat(roadSegmentAdded.Lanes
                .Select(lane =>
                    new RoadSegmentLaneAttributeLatestItem
                    {
                        Id = lane.AttributeId,
                        RoadSegmentId = roadSegmentAdded.Id,
                        AsOfGeometryVersion = lane.AsOfGeometryVersion,
                        Count = lane.Count,
                        DirectionId = RoadSegmentLaneDirection.Parse(lane.Direction).Translation.Identifier,
                        DirectionLabel = RoadSegmentLaneDirection.Parse(lane.Direction).Translation.Name,
                        FromPosition = (double)lane.FromPosition,
                        ToPosition = (double)lane.ToPosition,
                        OrganizationId = acceptedRoadSegmentModified.OrganizationId,
                        OrganizationName = acceptedRoadSegmentModified.Organization,
                        IsRemoved = true,
                        CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentAdded.When),
                        VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentModified.When)
                    }
                ));

        return new RoadSegmentLaneAttributeLatestItemProjection()
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

            return segment.Lanes.Select(lane => (object)new RoadSegmentLaneAttributeLatestItem
            {
                Id = lane.AttributeId,
                RoadSegmentId = segment.Id,
                AsOfGeometryVersion = lane.AsOfGeometryVersion,
                Count = lane.Count,
                DirectionId = RoadSegmentLaneDirection.Parse(lane.Direction).Translation.Identifier,
                DirectionLabel = RoadSegmentLaneDirection.Parse(lane.Direction).Translation.Name,
                FromPosition = (double)lane.FromPosition,
                ToPosition = (double)lane.ToPosition,
                OrganizationId = acceptedRoadSegmentRemoved.OrganizationId,
                OrganizationName = acceptedRoadSegmentRemoved.Organization,
                IsRemoved = true,
                CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentAdded.When),
                VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentRemoved.When)
            });
        }).SelectMany(x => x);

        return new RoadSegmentLaneAttributeLatestItemProjection()
            .Scenario()
            .Given(acceptedRoadSegmentAdded, acceptedRoadSegmentRemoved)
            .Expect(expectedRecords);
    }
}
