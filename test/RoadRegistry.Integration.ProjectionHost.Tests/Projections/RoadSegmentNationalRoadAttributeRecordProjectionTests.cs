namespace RoadRegistry.Integration.ProjectionHost.Tests.Projections;

using AutoFixture;
using BackOffice;
using BackOffice.Messages;
using Integration.Projections;
using RoadRegistry.Tests.BackOffice;
using Schema.RoadSegments;

public class RoadSegmentNationalRoadAttributeLatestItemProjectionTests
{
    private readonly Fixture _fixture;

    public RoadSegmentNationalRoadAttributeLatestItemProjectionTests()
    {
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

        _fixture.CustomizeRoadNetworkChangesAccepted();

        _fixture.CustomizeRoadSegmentAddedToNationalRoad();
        _fixture.CustomizeRoadSegmentRemovedFromNationalRoad();
        _fixture.CustomizeRoadSegmentRemoved();
    }

    [Fact]
    public Task When_adding_road_segments()
    {
        var message = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.CreateMany<RoadSegmentAddedToNationalRoad>());

        var expectedRecords = Array.ConvertAll(message.Changes, change =>
        {
            var nationalRoad = change.RoadSegmentAddedToNationalRoad;

            return (object)new RoadSegmentNationalRoadAttributeLatestItem
            {
                Id = nationalRoad.AttributeId,
                RoadSegmentId = nationalRoad.SegmentId,
                Number = nationalRoad.Number,
                OrganizationId = message.OrganizationId,
                OrganizationName = message.Organization,
                CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(message.When),
                VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(message.When)
            };
        });

        return new RoadSegmentNationalRoadAttributeLatestItemProjection()
            .Scenario()
            .Given(message)
            .Expect(expectedRecords);
    }

    [Fact]
    public Task When_importing_a_road_segment_without_national_road_links()
    {
        var importedRoadSegment = _fixture.Create<ImportedRoadSegment>();
        importedRoadSegment.PartOfNationalRoads = [];

        return new RoadSegmentNationalRoadAttributeLatestItemProjection()
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
                segment.PartOfNationalRoads = _fixture
                    .CreateMany<ImportedRoadSegmentNationalRoadAttribute>(random.Next(1, 10))
                    .ToArray();

                var expected = segment
                    .PartOfNationalRoads
                    .Select(nationalRoad => new RoadSegmentNationalRoadAttributeLatestItem
                    {
                        Id = nationalRoad.AttributeId,
                        RoadSegmentId = segment.Id,
                        Number = nationalRoad.Number,
                        OrganizationId = nationalRoad.Origin.OrganizationId,
                        OrganizationName = nationalRoad.Origin.Organization,
                        CreatedOnTimestamp = new DateTimeOffset(nationalRoad.Origin.Since),
                        VersionTimestamp = new DateTimeOffset(nationalRoad.Origin.Since)
                    });

                return new
                {
                    importedRoadSegment = segment,
                    expected
                };
            }).ToList();

        return new RoadSegmentNationalRoadAttributeLatestItemProjection()
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
        var roadSegmentAddedToNationalRoad = _fixture.Create<RoadSegmentAddedToNationalRoad>();
        var anotherRoadSegmentAddedToNationalRoad = _fixture.Create<RoadSegmentAddedToNationalRoad>();

        var acceptedRoadSegmentsAdded = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(
                roadSegmentAddedToNationalRoad,
                anotherRoadSegmentAddedToNationalRoad);

        var acceptedRoadSegmentRemoved = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(new RoadSegmentRemoved
            {
                Id = anotherRoadSegmentAddedToNationalRoad.SegmentId
            });

        return new RoadSegmentNationalRoadAttributeLatestItemProjection()
            .Scenario()
            .Given(acceptedRoadSegmentsAdded, acceptedRoadSegmentRemoved)
            .Expect([
                new RoadSegmentNationalRoadAttributeLatestItem
                {
                    Id = roadSegmentAddedToNationalRoad.AttributeId,
                    RoadSegmentId = roadSegmentAddedToNationalRoad.SegmentId,
                    Number = roadSegmentAddedToNationalRoad.Number,
                    IsRemoved = false,
                    OrganizationId = acceptedRoadSegmentsAdded.OrganizationId,
                    OrganizationName = acceptedRoadSegmentsAdded.Organization,
                    CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentsAdded.When),
                    VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentsAdded.When)
                },
                new RoadSegmentNationalRoadAttributeLatestItem
                {
                    Id = anotherRoadSegmentAddedToNationalRoad.AttributeId,
                    RoadSegmentId = anotherRoadSegmentAddedToNationalRoad.SegmentId,
                    Number = anotherRoadSegmentAddedToNationalRoad.Number,
                    IsRemoved = true,
                    OrganizationId = acceptedRoadSegmentRemoved.OrganizationId,
                    OrganizationName = acceptedRoadSegmentRemoved.Organization,
                    CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentsAdded.When),
                    VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentRemoved.When)
                }]);
    }

    [Fact]
    public Task When_removing_road_segments_from_national_roads()
    {
        _fixture.Freeze<AttributeId>();

        var roadSegmentAddedToNationalRoad = _fixture.Create<RoadSegmentAddedToNationalRoad>();
        var acceptedRoadSegmentAdded = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(roadSegmentAddedToNationalRoad);

        var acceptedRoadSegmentRemoved = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.Create<RoadSegmentRemovedFromNationalRoad>());

        return new RoadSegmentNationalRoadAttributeLatestItemProjection()
            .Scenario()
            .Given(acceptedRoadSegmentAdded, acceptedRoadSegmentRemoved)
            .Expect([
                new RoadSegmentNationalRoadAttributeLatestItem
                {
                    Id = roadSegmentAddedToNationalRoad.AttributeId,
                    RoadSegmentId = roadSegmentAddedToNationalRoad.SegmentId,
                    Number = roadSegmentAddedToNationalRoad.Number,
                    IsRemoved = true,
                    OrganizationId = acceptedRoadSegmentRemoved.OrganizationId,
                    OrganizationName = acceptedRoadSegmentRemoved.Organization,
                    CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentAdded.When),
                    VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentRemoved.When)
                }
            ]);
    }
}
