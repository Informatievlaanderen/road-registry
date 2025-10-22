namespace RoadRegistry.Integration.ProjectionHost.Tests.Projections.Version;

using AutoFixture;
using BackOffice;
using BackOffice.Messages;
using Integration.Projections;
using RoadSegment.ValueObjects;
using Schema.RoadSegments.Version;

public partial class RoadSegmentVersionProjectionTests
{
    [Fact]
    public Task When_importing_a_road_segment_without_european_road_links()
    {
        _fixture.Freeze<RoadSegmentId>();

        var importedRoadSegment = _fixture.Create<ImportedRoadSegment>();
        importedRoadSegment.PartOfEuropeanRoads = [];

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
                var roadSegmentAddedToEuropeanRoad = change.RoadSegmentAddedToEuropeanRoad;

                var position = 1;

                return BuildRoadSegmentRecord(
                    position,
                    acceptedRoadSegmentAdded,
                    acceptedRoadSegmentAdded.Changes.Single().RoadSegmentAdded,
                    roadSegment =>
                    {
                        roadSegment.Version = roadSegmentAddedToEuropeanRoad.SegmentVersion!.Value;

                        roadSegment.OrganizationId = message.OrganizationId;
                        roadSegment.OrganizationName = message.Organization;
                        roadSegment.VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(message.When);

                        roadSegment.PartOfEuropeanRoads.Add(new RoadSegmentEuropeanRoadAttributeVersion
                        {
                            Position = position,
                            Id = roadSegmentAddedToEuropeanRoad.AttributeId,
                            RoadSegmentId = roadSegmentAddedToEuropeanRoad.SegmentId,
                            Number = roadSegmentAddedToEuropeanRoad.Number,
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
    public Task When_adding_road_segment_to_european_road_multiple_times_then_existing_record_is_reused()
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
                var roadSegmentAddedToEuropeanRoad = change.RoadSegmentAddedToEuropeanRoad;

                var position = 1;

                return BuildRoadSegmentRecord(
                    position,
                    acceptedRoadSegmentAdded,
                    acceptedRoadSegmentAdded.Changes.Single().RoadSegmentAdded,
                    roadSegment =>
                    {
                        roadSegment.Version = roadSegmentAddedToEuropeanRoad.SegmentVersion!.Value;

                        roadSegment.OrganizationId = message.OrganizationId;
                        roadSegment.OrganizationName = message.Organization;
                        roadSegment.VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(message.When);

                        roadSegment.PartOfEuropeanRoads.Add(new RoadSegmentEuropeanRoadAttributeVersion
                        {
                            Position = position,
                            Id = roadSegmentAddedToEuropeanRoad.AttributeId,
                            RoadSegmentId = roadSegmentAddedToEuropeanRoad.SegmentId,
                            Number = roadSegmentAddedToEuropeanRoad.Number,
                            OrganizationId = message.OrganizationId,
                            OrganizationName = message.Organization,
                            CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(message.When),
                            VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(message.When)
                        });
                    });
            }))
            .Concat(Array.ConvertAll(message.Changes, change =>
            {
                var roadSegmentAddedToEuropeanRoad = change.RoadSegmentAddedToEuropeanRoad;

                var position = 2;

                return BuildRoadSegmentRecord(
                    position,
                    acceptedRoadSegmentAdded,
                    acceptedRoadSegmentAdded.Changes.Single().RoadSegmentAdded,
                    roadSegment =>
                    {
                        roadSegment.Version = roadSegmentAddedToEuropeanRoad.SegmentVersion!.Value;

                        roadSegment.OrganizationId = message.OrganizationId;
                        roadSegment.OrganizationName = message.Organization;
                        roadSegment.VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(message.When);

                        roadSegment.PartOfEuropeanRoads.Add(new RoadSegmentEuropeanRoadAttributeVersion
                        {
                            Position = position,
                            Id = roadSegmentAddedToEuropeanRoad.AttributeId,
                            RoadSegmentId = roadSegmentAddedToEuropeanRoad.SegmentId,
                            Number = roadSegmentAddedToEuropeanRoad.Number,
                            OrganizationId = message.OrganizationId,
                            OrganizationName = message.Organization,
                            CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(message.When),
                            VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(message.When)
                        });
                    });
            }));

        return BuildProjection()
            .Scenario()
            .Given(acceptedRoadSegmentAdded, message, message)
            .Expect(expectedRecords);
    }

    [Fact]
    public Task When_adding_road_segment_to_multiple_european_roads()
    {
        _fixture.Freeze<RoadSegmentId>();

        var acceptedRoadSegmentAdded = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.Create<RoadSegmentAdded>());

        var roadSegmentAddedToEuropeanRoad1 = _fixture.Create<RoadSegmentAddedToEuropeanRoad>();
        var roadSegmentAddedToEuropeanRoad2 = _fixture.Create<RoadSegmentAddedToEuropeanRoad>();
        var message = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(roadSegmentAddedToEuropeanRoad1, roadSegmentAddedToEuropeanRoad2);

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
                        roadSegment.Version = roadSegmentAddedToEuropeanRoad2.SegmentVersion!.Value;

                        roadSegment.OrganizationId = message.OrganizationId;
                        roadSegment.OrganizationName = message.Organization;
                        roadSegment.VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(message.When);

                        roadSegment.PartOfEuropeanRoads.Add(new RoadSegmentEuropeanRoadAttributeVersion
                        {
                            Position = position,
                            Id = roadSegmentAddedToEuropeanRoad1.AttributeId,
                            RoadSegmentId = roadSegmentAddedToEuropeanRoad1.SegmentId,
                            Number = roadSegmentAddedToEuropeanRoad1.Number,
                            OrganizationId = message.OrganizationId,
                            OrganizationName = message.Organization,
                            CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(message.When),
                            VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(message.When)
                        });
                        roadSegment.PartOfEuropeanRoads.Add(new RoadSegmentEuropeanRoadAttributeVersion
                        {
                            Position = position,
                            Id = roadSegmentAddedToEuropeanRoad2.AttributeId,
                            RoadSegmentId = roadSegmentAddedToEuropeanRoad2.SegmentId,
                            Number = roadSegmentAddedToEuropeanRoad2.Number,
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
    public Task When_removing_road_segment_from_european_road()
    {
        _fixture.Freeze<RoadSegmentId>();

        var importedRoadSegment = _fixture.Create<ImportedRoadSegment>();

        var message = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.CreateWith<RoadSegmentRemovedFromEuropeanRoad>(x =>
            {
                x.AttributeId = importedRoadSegment.PartOfEuropeanRoads.First().AttributeId;
            }));

        var expectedRecords = Array.Empty<object>()
            .Concat(new[]
            {
                BuildRoadSegmentRecord(IntegrationContextScenarioExtensions.InitialPosition, importedRoadSegment)
            })
            .Concat(message.Changes
                .Select(x => x.RoadSegmentRemovedFromEuropeanRoad)
                .Select(roadSegmentRemovedFromEuropeanRoad =>
                {
                    return BuildRoadSegmentRecord(
                        IntegrationContextScenarioExtensions.InitialPosition + 1,
                        importedRoadSegment,
                        roadSegment =>
                        {
                            roadSegment.Version = roadSegmentRemovedFromEuropeanRoad.SegmentVersion!.Value;

                            roadSegment.OrganizationId = message.OrganizationId;
                            roadSegment.OrganizationName = message.Organization;
                            roadSegment.VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(message.When);

                            var removedEuropeanRoad = roadSegment.PartOfEuropeanRoads.First();
                            removedEuropeanRoad.OrganizationId = message.OrganizationId;
                            removedEuropeanRoad.OrganizationName = message.Organization;
                            removedEuropeanRoad.VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(message.When);
                            removedEuropeanRoad.IsRemoved = true;
                        });
                }));

        return BuildProjection()
            .Scenario()
            .Given(importedRoadSegment, message)
            .Expect(expectedRecords);
    }
}
