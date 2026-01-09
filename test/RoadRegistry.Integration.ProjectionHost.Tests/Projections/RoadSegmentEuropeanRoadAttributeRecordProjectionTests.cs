namespace RoadRegistry.Integration.ProjectionHost.Tests.Projections;

using AutoFixture;
using BackOffice;
using BackOffice.Messages;
using Integration.Projections;
using RoadRegistry.Tests.BackOffice;
using Schema.RoadSegments;

public class RoadSegmentEuropeanRoadAttributeLatestItemProjectionTests
{
    private readonly Fixture _fixture;

    public RoadSegmentEuropeanRoadAttributeLatestItemProjectionTests()
    {
        _fixture = FixtureFactory.Create();
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

        _fixture.CustomizeRoadNetworkChangesAccepted();

        _fixture.CustomizeRoadSegmentAddedToEuropeanRoad();
        _fixture.CustomizeRoadSegmentRemovedFromEuropeanRoad();
    }

    [Fact]
    public Task When_adding_road_segments()
    {
        var message = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.CreateMany<RoadSegmentAddedToEuropeanRoad>());

        var expectedRecords = Array.ConvertAll(message.Changes, change =>
        {
            var europeanRoad = change.RoadSegmentAddedToEuropeanRoad;

            return (object)new RoadSegmentEuropeanRoadAttributeLatestItem
            {
                Id = europeanRoad.AttributeId,
                RoadSegmentId = europeanRoad.SegmentId,
                Number = europeanRoad.Number,
                OrganizationId = message.OrganizationId,
                OrganizationName = message.Organization,
                CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(message.When),
                VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(message.When)
            };
        });

        return new RoadSegmentEuropeanRoadAttributeLatestItemProjection()
            .Scenario()
            .Given(message)
            .Expect(expectedRecords);
    }

    [Fact]
    public Task When_adding_same_road_segments_multiple_times_then_existing_record_is_reused()
    {
        var message = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.CreateMany<RoadSegmentAddedToEuropeanRoad>());

        var expectedRecords = Array.ConvertAll(message.Changes, change =>
        {
            var europeanRoad = change.RoadSegmentAddedToEuropeanRoad;

            return (object)new RoadSegmentEuropeanRoadAttributeLatestItem
            {
                Id = europeanRoad.AttributeId,
                RoadSegmentId = europeanRoad.SegmentId,
                Number = europeanRoad.Number,
                OrganizationId = message.OrganizationId,
                OrganizationName = message.Organization,
                CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(message.When),
                VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(message.When)
            };
        });

        return new RoadSegmentEuropeanRoadAttributeLatestItemProjection()
            .Scenario()
            .Given(message)
            .Given(message)
            .Expect(expectedRecords);
    }

    [Fact]
    public Task When_importing_a_road_segment_without_european_road_links()
    {
        var importedRoadSegment = _fixture.Create<ImportedRoadSegment>();
        importedRoadSegment.PartOfEuropeanRoads = [];

        return new RoadSegmentEuropeanRoadAttributeLatestItemProjection()
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
                segment.PartOfEuropeanRoads = _fixture
                    .CreateMany<ImportedRoadSegmentEuropeanRoadAttribute>(random.Next(1, 10))
                    .ToArray();

                var expected = segment
                    .PartOfEuropeanRoads
                    .Select(europeanRoad => new RoadSegmentEuropeanRoadAttributeLatestItem
                    {
                        Id = europeanRoad.AttributeId,
                        RoadSegmentId = segment.Id,
                        Number = europeanRoad.Number,
                        OrganizationId = europeanRoad.Origin.OrganizationId,
                        OrganizationName = europeanRoad.Origin.Organization,
                        CreatedOnTimestamp = europeanRoad.Origin.Since.ToBelgianInstant(),
                        VersionTimestamp = europeanRoad.Origin.Since.ToBelgianInstant()
                    });

                return new
                {
                    importedRoadSegment = segment,
                    expected
                };
            }).ToList();

        return new RoadSegmentEuropeanRoadAttributeLatestItemProjection()
            .Scenario()
            .Given(data.Select(d => d.importedRoadSegment))
            .Expect(data
                .SelectMany(d => d.expected)
                .Cast<object>()
                .ToArray()
            );
    }

    [Fact]
    public Task When_removing_road_segments()
    {
        var roadSegmentAddedToEuropeanRoad = _fixture.Create<RoadSegmentAddedToEuropeanRoad>();
        var anotherRoadSegmentAddedToEuropeanRoad = _fixture.Create<RoadSegmentAddedToEuropeanRoad>();

        var acceptedRoadSegmentsAdded = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(
                roadSegmentAddedToEuropeanRoad,
                anotherRoadSegmentAddedToEuropeanRoad);

        var acceptedRoadSegmentRemoved = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(new RoadSegmentRemoved
            {
                Id = anotherRoadSegmentAddedToEuropeanRoad.SegmentId
            });

        return new RoadSegmentEuropeanRoadAttributeLatestItemProjection()
            .Scenario()
            .Given(acceptedRoadSegmentsAdded, acceptedRoadSegmentRemoved)
            .Expect([
                new RoadSegmentEuropeanRoadAttributeLatestItem
                {
                    Id = roadSegmentAddedToEuropeanRoad.AttributeId,
                    RoadSegmentId = roadSegmentAddedToEuropeanRoad.SegmentId,
                    Number = roadSegmentAddedToEuropeanRoad.Number,
                    IsRemoved = false,
                    OrganizationId = acceptedRoadSegmentsAdded.OrganizationId,
                    OrganizationName = acceptedRoadSegmentsAdded.Organization,
                    CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentsAdded.When),
                    VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentsAdded.When)
                },
                new RoadSegmentEuropeanRoadAttributeLatestItem
                {
                    Id = anotherRoadSegmentAddedToEuropeanRoad.AttributeId,
                    RoadSegmentId = anotherRoadSegmentAddedToEuropeanRoad.SegmentId,
                    Number = anotherRoadSegmentAddedToEuropeanRoad.Number,
                    IsRemoved = true,
                    OrganizationId = acceptedRoadSegmentRemoved.OrganizationId,
                    OrganizationName = acceptedRoadSegmentRemoved.Organization,
                    CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentsAdded.When),
                    VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentRemoved.When)
                }]);
    }

    [Fact]
    public Task When_removing_road_segments_from_european_roads()
    {
        _fixture.Freeze<AttributeId>();

        var roadSegmentAddedToEuropeanRoad = _fixture.Create<RoadSegmentAddedToEuropeanRoad>();
        var acceptedRoadSegmentAdded = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(roadSegmentAddedToEuropeanRoad);

        var acceptedRoadSegmentRemoved = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.Create<RoadSegmentRemovedFromEuropeanRoad>());

        return new RoadSegmentEuropeanRoadAttributeLatestItemProjection()
            .Scenario()
            .Given(acceptedRoadSegmentAdded, acceptedRoadSegmentRemoved)
            .Expect([
                new RoadSegmentEuropeanRoadAttributeLatestItem
                {
                    Id = roadSegmentAddedToEuropeanRoad.AttributeId,
                    RoadSegmentId = roadSegmentAddedToEuropeanRoad.SegmentId,
                    Number = roadSegmentAddedToEuropeanRoad.Number,
                    IsRemoved = true,
                    OrganizationId = acceptedRoadSegmentRemoved.OrganizationId,
                    OrganizationName = acceptedRoadSegmentRemoved.Organization,
                    CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentAdded.When),
                    VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentRemoved.When)
                }
            ]);
    }
}
