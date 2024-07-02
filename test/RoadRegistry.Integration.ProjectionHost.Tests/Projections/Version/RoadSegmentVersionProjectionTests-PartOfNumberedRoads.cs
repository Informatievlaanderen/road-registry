namespace RoadRegistry.Integration.ProjectionHost.Tests.Projections.Version;

using AutoFixture;
using BackOffice;
using BackOffice.Messages;
using Integration.Projections;
using Schema.RoadSegments.Version;

public partial class RoadSegmentVersionProjectionTests
{
    [Fact]
    public Task When_importing_a_road_segment_without_numbered_road_links()
    {
        _fixture.Freeze<RoadSegmentId>();

        var importedRoadSegment = _fixture.Create<ImportedRoadSegment>();
        importedRoadSegment.PartOfNumberedRoads = [];
        
        var expectedRecords = Array.Empty<object>()
            .Concat(new[]
            {
                BuildRoadSegmentRecord(IntegrationContextScenarioExtensions.InitialPosition, importedRoadSegment)
            });

        return BuildProjection()
            .Scenario()
            .Given(importedRoadSegment)
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

                var position = 1;

                return BuildRoadSegmentRecord(
                    position,
                    acceptedRoadSegmentAdded,
                    acceptedRoadSegmentAdded.Changes.Single().RoadSegmentAdded,
                    roadSegment =>
                    {
                        roadSegment.Version = roadSegmentAddedToNumberedRoad.SegmentVersion!.Value;

                        roadSegment.OrganizationId = message.OrganizationId;
                        roadSegment.OrganizationName = message.Organization;
                        roadSegment.VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(message.When);

                        var numberedRoadDirectionTranslation = RoadSegmentNumberedRoadDirection.Parse(roadSegmentAddedToNumberedRoad.Direction).Translation;

                        roadSegment.PartOfNumberedRoads.Add(new RoadSegmentNumberedRoadAttributeVersion
                        {
                            Position = position,
                            Id = roadSegmentAddedToNumberedRoad.AttributeId,
                            RoadSegmentId = roadSegmentAddedToNumberedRoad.SegmentId,
                            Number = roadSegmentAddedToNumberedRoad.Number,
                            DirectionId = numberedRoadDirectionTranslation.Identifier,
                            DirectionLabel = numberedRoadDirectionTranslation.Name,
                            SequenceNumber = roadSegmentAddedToNumberedRoad.Ordinal,
                            OrganizationId = message.OrganizationId,
                            OrganizationName = message.Organization,
                            CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(message.When),
                            VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(message.When)
                        });
                    });
            }));

        return BuildProjection()
            .Scenario()
            .Given(acceptedRoadSegmentAdded, message)
            .Expect(expectedRecords);
    }

    [Fact]
    public Task When_adding_road_segment_to_multiple_numbered_roads()
    {
        _fixture.Freeze<RoadSegmentId>();

        var acceptedRoadSegmentAdded = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.Create<RoadSegmentAdded>());

        var roadSegmentAddedToNumberedRoad1 = _fixture.Create<RoadSegmentAddedToNumberedRoad>();
        var roadSegmentAddedToNumberedRoad2 = _fixture.Create<RoadSegmentAddedToNumberedRoad>();
        var message = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(roadSegmentAddedToNumberedRoad1, roadSegmentAddedToNumberedRoad2);

        Assert.Equal(2, message.Changes.Length);

        var position = 1;

        var expectedRecords = BuildInitialExpectedRoadSegmentRecords(acceptedRoadSegmentAdded)
            .Concat(new[]
            {
                BuildRoadSegmentRecord(
                    position,
                    acceptedRoadSegmentAdded,
                    acceptedRoadSegmentAdded.Changes.Single().RoadSegmentAdded,
                    roadSegment =>
                    {
                        roadSegment.Version = roadSegmentAddedToNumberedRoad2.SegmentVersion!.Value;

                        roadSegment.OrganizationId = message.OrganizationId;
                        roadSegment.OrganizationName = message.Organization;
                        roadSegment.VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(message.When);

                        roadSegment.PartOfNumberedRoads.Add(new RoadSegmentNumberedRoadAttributeVersion
                        {
                            Position = position,
                            Id = roadSegmentAddedToNumberedRoad1.AttributeId,
                            RoadSegmentId = roadSegmentAddedToNumberedRoad1.SegmentId,
                            Number = roadSegmentAddedToNumberedRoad1.Number,
                            DirectionId = RoadSegmentNumberedRoadDirection.Parse(roadSegmentAddedToNumberedRoad1.Direction).Translation.Identifier,
                            DirectionLabel = RoadSegmentNumberedRoadDirection.Parse(roadSegmentAddedToNumberedRoad1.Direction).Translation.Name,
                            SequenceNumber = roadSegmentAddedToNumberedRoad1.Ordinal,
                            OrganizationId = message.OrganizationId,
                            OrganizationName = message.Organization,
                            CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(message.When),
                            VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(message.When)
                        });

                        roadSegment.PartOfNumberedRoads.Add(new RoadSegmentNumberedRoadAttributeVersion
                        {
                            Position = position,
                            Id = roadSegmentAddedToNumberedRoad2.AttributeId,
                            RoadSegmentId = roadSegmentAddedToNumberedRoad2.SegmentId,
                            Number = roadSegmentAddedToNumberedRoad2.Number,
                            DirectionId = RoadSegmentNumberedRoadDirection.Parse(roadSegmentAddedToNumberedRoad2.Direction).Translation.Identifier,
                            DirectionLabel = RoadSegmentNumberedRoadDirection.Parse(roadSegmentAddedToNumberedRoad2.Direction).Translation.Name,
                            SequenceNumber = roadSegmentAddedToNumberedRoad2.Ordinal,
                            OrganizationId = message.OrganizationId,
                            OrganizationName = message.Organization,
                            CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(message.When),
                            VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(message.When)
                        });
                    })
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

        var importedRoadSegment = _fixture.Create<ImportedRoadSegment>();
        
        var message = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.CreateWith<RoadSegmentRemovedFromNumberedRoad>(x =>
            {
                x.AttributeId = importedRoadSegment.PartOfNumberedRoads.First().AttributeId;
            }));
        
        var expectedRecords = Array.Empty<object>()
            .Concat(new[]
            {
                BuildRoadSegmentRecord(IntegrationContextScenarioExtensions.InitialPosition, importedRoadSegment)
            })
            .Concat(message.Changes
                .Select(x => x.RoadSegmentRemovedFromNumberedRoad)
                .Select(roadSegmentRemovedFromNumberedRoad =>
                {
                    return BuildRoadSegmentRecord(
                        IntegrationContextScenarioExtensions.InitialPosition + 1,
                        importedRoadSegment,
                        roadSegment =>
                        {
                            roadSegment.Version = roadSegmentRemovedFromNumberedRoad.SegmentVersion!.Value;

                            roadSegment.OrganizationId = message.OrganizationId;
                            roadSegment.OrganizationName = message.Organization;
                            roadSegment.VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(message.When);

                            var removedNumberedRoad = roadSegment.PartOfNumberedRoads.First();
                            removedNumberedRoad.OrganizationId = message.OrganizationId;
                            removedNumberedRoad.OrganizationName = message.Organization;
                            removedNumberedRoad.VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(message.When);
                            removedNumberedRoad.IsRemoved = true;
                        });
                }));

        return BuildProjection()
            .Scenario()
            .Given(importedRoadSegment, message)
            .Expect(expectedRecords);
    }
}
