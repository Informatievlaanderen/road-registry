namespace RoadRegistry.Integration.ProjectionHost.Tests.Projections;

using AutoFixture;
using BackOffice;
using BackOffice.Messages;
using Integration.Projections;
using RoadRegistry.Tests.BackOffice;
using Schema.RoadSegments;

public class RoadSegmentNumberedRoadAttributeLatestItemProjectionTests
{
    private readonly Fixture _fixture;

    public RoadSegmentNumberedRoadAttributeLatestItemProjectionTests()
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

        _fixture.CustomizeRoadSegmentAddedToNumberedRoad();
        _fixture.CustomizeRoadSegmentOnNumberedRoadModified();
        _fixture.CustomizeRoadSegmentRemovedFromNumberedRoad();
    }

    [Fact]
    public Task When_adding_road_segments()
    {
        var message = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.CreateMany<RoadSegmentAddedToNumberedRoad>());

        var expectedRecords = Array.ConvertAll(message.Changes, change =>
        {
            var numberedRoad = change.RoadSegmentAddedToNumberedRoad;

            return (object)new RoadSegmentNumberedRoadAttributeLatestItem
            {
                Id = numberedRoad.AttributeId,
                RoadSegmentId = numberedRoad.SegmentId,
                Number = numberedRoad.Number,
                DirectionId = RoadSegmentNumberedRoadDirection.Parse(numberedRoad.Direction).Translation.Identifier,
                DirectionLabel = RoadSegmentNumberedRoadDirection.Parse(numberedRoad.Direction).Translation.Name,
                SequenceNumber = numberedRoad.Ordinal,
                OrganizationId = message.OrganizationId,
                OrganizationName = message.Organization,
                CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(message.When),
                VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(message.When)
            };
        });

        return new RoadSegmentNumberedRoadAttributeLatestItemProjection()
            .Scenario()
            .Given(message)
            .Expect(expectedRecords);
    }

    [Fact]
    public Task When_importing_a_road_segment_without_numbered_road_links()
    {
        var importedRoadSegment = _fixture.Create<ImportedRoadSegment>();
        importedRoadSegment.PartOfNumberedRoads = [];

        return new RoadSegmentNumberedRoadAttributeLatestItemProjection()
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
                segment.PartOfNumberedRoads = _fixture
                    .CreateMany<ImportedRoadSegmentNumberedRoadAttribute>(random.Next(1, 10))
                    .ToArray();

                var expected = segment
                    .PartOfNumberedRoads
                    .Select(numberedRoad => new RoadSegmentNumberedRoadAttributeLatestItem
                    {
                        Id = numberedRoad.AttributeId,
                        RoadSegmentId = segment.Id,
                        Number = numberedRoad.Number,
                        DirectionId = RoadSegmentNumberedRoadDirection.Parse(numberedRoad.Direction).Translation.Identifier,
                        DirectionLabel = RoadSegmentNumberedRoadDirection.Parse(numberedRoad.Direction).Translation.Name,
                        SequenceNumber = numberedRoad.Ordinal,
                        OrganizationId = numberedRoad.Origin.OrganizationId,
                        OrganizationName = numberedRoad.Origin.Organization,
                        CreatedOnTimestamp = numberedRoad.Origin.Since.ToBelgianInstant(),
                        VersionTimestamp = numberedRoad.Origin.Since.ToBelgianInstant()
                    });

                return new
                {
                    importedRoadSegment = segment,
                    expected
                };
            }).ToList();

        return new RoadSegmentNumberedRoadAttributeLatestItemProjection()
            .Scenario()
            .Given(data.Select(d => d.importedRoadSegment))
            .Expect(data
                .SelectMany(d => d.expected)
                .Cast<object>()
                .ToArray()
            );
    }

    [Fact]
    public Task When_modifying_road_segments()
    {
        _fixture.Freeze<AttributeId>();

        var acceptedRoadSegmentAdded = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.Create<RoadSegmentAddedToNumberedRoad>());

        var acceptedRoadSegmentModified = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.Create<RoadSegmentOnNumberedRoadModified>());

        var expectedRecords = Array.ConvertAll(acceptedRoadSegmentModified.Changes, change =>
        {
            var numberedRoad = change.RoadSegmentOnNumberedRoadModified;

            return (object)new RoadSegmentNumberedRoadAttributeLatestItem
            {
                Id = numberedRoad.AttributeId,
                RoadSegmentId = numberedRoad.SegmentId,
                Number = numberedRoad.Number,
                DirectionId = RoadSegmentNumberedRoadDirection.Parse(numberedRoad.Direction).Translation.Identifier,
                DirectionLabel = RoadSegmentNumberedRoadDirection.Parse(numberedRoad.Direction).Translation.Name,
                SequenceNumber = numberedRoad.Ordinal,
                OrganizationId = acceptedRoadSegmentModified.OrganizationId,
                OrganizationName = acceptedRoadSegmentModified.Organization,
                CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentAdded.When),
                VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentModified.When)
            };
        });

        return new RoadSegmentNumberedRoadAttributeLatestItemProjection()
            .Scenario()
            .Given(acceptedRoadSegmentAdded, acceptedRoadSegmentModified)
            .Expect(expectedRecords);
    }

    [Fact]
    public Task When_removing_road_segments()
    {
        var roadSegmentAddedToNumberedRoad = _fixture.Create<RoadSegmentAddedToNumberedRoad>();
        var anotherRoadSegmentAddedToNumberedRoad = _fixture.Create<RoadSegmentAddedToNumberedRoad>();

        var acceptedRoadSegmentsAdded = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(
                roadSegmentAddedToNumberedRoad,
                anotherRoadSegmentAddedToNumberedRoad);

        var acceptedRoadSegmentRemoved = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(new RoadSegmentRemoved
            {
                Id = anotherRoadSegmentAddedToNumberedRoad.SegmentId
            });

        return new RoadSegmentNumberedRoadAttributeLatestItemProjection()
            .Scenario()
            .Given(acceptedRoadSegmentsAdded, acceptedRoadSegmentRemoved)
            .Expect(
            [
                new RoadSegmentNumberedRoadAttributeLatestItem
                {
                    Id = roadSegmentAddedToNumberedRoad.AttributeId,
                    RoadSegmentId = roadSegmentAddedToNumberedRoad.SegmentId,
                    Number = roadSegmentAddedToNumberedRoad.Number,
                    DirectionId = RoadSegmentNumberedRoadDirection.Parse(roadSegmentAddedToNumberedRoad.Direction).Translation.Identifier,
                    DirectionLabel = RoadSegmentNumberedRoadDirection.Parse(roadSegmentAddedToNumberedRoad.Direction).Translation.Name,
                    SequenceNumber =  roadSegmentAddedToNumberedRoad.Ordinal,
                    IsRemoved = false,
                    OrganizationId = acceptedRoadSegmentsAdded.OrganizationId,
                    OrganizationName = acceptedRoadSegmentsAdded.Organization,
                    CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentsAdded.When),
                    VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentsAdded.When)
                },
                new RoadSegmentNumberedRoadAttributeLatestItem
                {
                    Id = anotherRoadSegmentAddedToNumberedRoad.AttributeId,
                    RoadSegmentId = anotherRoadSegmentAddedToNumberedRoad.SegmentId,
                    Number = anotherRoadSegmentAddedToNumberedRoad.Number,
                    DirectionId = RoadSegmentNumberedRoadDirection.Parse(anotherRoadSegmentAddedToNumberedRoad.Direction).Translation.Identifier,
                    DirectionLabel = RoadSegmentNumberedRoadDirection.Parse(anotherRoadSegmentAddedToNumberedRoad.Direction).Translation.Name,
                    SequenceNumber =  anotherRoadSegmentAddedToNumberedRoad.Ordinal,
                    IsRemoved = true,
                    OrganizationId = acceptedRoadSegmentRemoved.OrganizationId,
                    OrganizationName = acceptedRoadSegmentRemoved.Organization,
                    CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentsAdded.When),
                    VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentRemoved.When)
                }
            ]);
    }

    [Fact]
    public Task When_removing_road_segments_from_numbered_roads()
    {
        _fixture.Freeze<AttributeId>();

        var roadSegmentAddedToNumberedRoad = _fixture.Create<RoadSegmentAddedToNumberedRoad>();
        var acceptedRoadSegmentAdded = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(roadSegmentAddedToNumberedRoad);

        var acceptedRoadSegmentRemoved = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.Create<RoadSegmentRemovedFromNumberedRoad>());

        return new RoadSegmentNumberedRoadAttributeLatestItemProjection()
            .Scenario()
            .Given(acceptedRoadSegmentAdded, acceptedRoadSegmentRemoved)
            .Expect([
                new RoadSegmentNumberedRoadAttributeLatestItem
                {
                    Id = roadSegmentAddedToNumberedRoad.AttributeId,
                    RoadSegmentId = roadSegmentAddedToNumberedRoad.SegmentId,
                    Number = roadSegmentAddedToNumberedRoad.Number,
                    DirectionId = RoadSegmentNumberedRoadDirection.Parse(roadSegmentAddedToNumberedRoad.Direction).Translation.Identifier,
                    DirectionLabel = RoadSegmentNumberedRoadDirection.Parse(roadSegmentAddedToNumberedRoad.Direction).Translation.Name,
                    SequenceNumber =  roadSegmentAddedToNumberedRoad.Ordinal,
                    IsRemoved = true,
                    OrganizationId = acceptedRoadSegmentRemoved.OrganizationId,
                    OrganizationName = acceptedRoadSegmentRemoved.Organization,
                    CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentAdded.When),
                    VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(acceptedRoadSegmentRemoved.When)
                }
            ]);
    }
}
