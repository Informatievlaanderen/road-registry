namespace RoadRegistry.Integration.ProjectionHost.Tests.Projections.Version;

using AutoFixture;
using BackOffice;
using BackOffice.Messages;
using Integration.Projections;
using Schema.RoadSegments.Version;

public partial class RoadSegmentVersionProjectionTests
{
    [Fact]
    public Task When_importing_a_road_segment_without_national_road_links()
    {
        _fixture.Freeze<RoadSegmentId>();

        var importedRoadSegment = _fixture.Create<ImportedRoadSegment>();
        importedRoadSegment.PartOfNationalRoads = [];

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

                var position = 1;

                return BuildRoadSegmentRecord(
                    position,
                    acceptedRoadSegmentAdded,
                    acceptedRoadSegmentAdded.Changes.Single().RoadSegmentAdded,
                    roadSegment =>
                    {
                        roadSegment.Version = roadSegmentAddedToNationalRoad.SegmentVersion!.Value;

                        roadSegment.OrganizationId = message.OrganizationId;
                        roadSegment.OrganizationName = message.Organization;
                        roadSegment.VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(message.When);

                        roadSegment.PartOfNationalRoads.Add(new RoadSegmentNationalRoadAttributeVersion
                        {
                            Position = position,
                            Id = roadSegmentAddedToNationalRoad.AttributeId,
                            RoadSegmentId = roadSegmentAddedToNationalRoad.SegmentId,
                            Number = roadSegmentAddedToNationalRoad.Number,
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
    public Task When_adding_road_segment_to_national_road_multiple_times_then_existing_record_is_reused()
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

                var position = 1;

                return BuildRoadSegmentRecord(
                    position,
                    acceptedRoadSegmentAdded,
                    acceptedRoadSegmentAdded.Changes.Single().RoadSegmentAdded,
                    roadSegment =>
                    {
                        roadSegment.Version = roadSegmentAddedToNationalRoad.SegmentVersion!.Value;

                        roadSegment.OrganizationId = message.OrganizationId;
                        roadSegment.OrganizationName = message.Organization;
                        roadSegment.VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(message.When);

                        roadSegment.PartOfNationalRoads.Add(new RoadSegmentNationalRoadAttributeVersion
                        {
                            Position = position,
                            Id = roadSegmentAddedToNationalRoad.AttributeId,
                            RoadSegmentId = roadSegmentAddedToNationalRoad.SegmentId,
                            Number = roadSegmentAddedToNationalRoad.Number,
                            OrganizationId = message.OrganizationId,
                            OrganizationName = message.Organization,
                            CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(message.When),
                            VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(message.When)
                        });
                    });
            }))
            .Concat(Array.ConvertAll(message.Changes, change =>
            {
                var roadSegmentAddedToNationalRoad = change.RoadSegmentAddedToNationalRoad;

                var position = 2;

                return BuildRoadSegmentRecord(
                    position,
                    acceptedRoadSegmentAdded,
                    acceptedRoadSegmentAdded.Changes.Single().RoadSegmentAdded,
                    roadSegment =>
                    {
                        roadSegment.Version = roadSegmentAddedToNationalRoad.SegmentVersion!.Value;

                        roadSegment.OrganizationId = message.OrganizationId;
                        roadSegment.OrganizationName = message.Organization;
                        roadSegment.VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(message.When);

                        roadSegment.PartOfNationalRoads.Add(new RoadSegmentNationalRoadAttributeVersion
                        {
                            Position = position,
                            Id = roadSegmentAddedToNationalRoad.AttributeId,
                            RoadSegmentId = roadSegmentAddedToNationalRoad.SegmentId,
                            Number = roadSegmentAddedToNationalRoad.Number,
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
    public Task When_adding_road_segment_to_multiple_national_roads()
    {
        _fixture.Freeze<RoadSegmentId>();

        var acceptedRoadSegmentAdded = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.Create<RoadSegmentAdded>());

        var roadSegmentAddedToNationalRoad1 = _fixture.Create<RoadSegmentAddedToNationalRoad>();
        var roadSegmentAddedToNationalRoad2 = _fixture.Create<RoadSegmentAddedToNationalRoad>();
        var message = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(roadSegmentAddedToNationalRoad1, roadSegmentAddedToNationalRoad2);

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
                        roadSegment.Version = roadSegmentAddedToNationalRoad2.SegmentVersion!.Value;

                        roadSegment.OrganizationId = message.OrganizationId;
                        roadSegment.OrganizationName = message.Organization;
                        roadSegment.VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(message.When);

                        roadSegment.PartOfNationalRoads.Add(new RoadSegmentNationalRoadAttributeVersion
                        {
                            Position = position,
                            Id = roadSegmentAddedToNationalRoad1.AttributeId,
                            RoadSegmentId = roadSegmentAddedToNationalRoad1.SegmentId,
                            Number = roadSegmentAddedToNationalRoad1.Number,
                            OrganizationId = message.OrganizationId,
                            OrganizationName = message.Organization,
                            CreatedOnTimestamp = LocalDateTimeTranslator.TranslateFromWhen(message.When),
                            VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(message.When)
                        });
                        roadSegment.PartOfNationalRoads.Add(new RoadSegmentNationalRoadAttributeVersion
                        {
                            Position = position,
                            Id = roadSegmentAddedToNationalRoad2.AttributeId,
                            RoadSegmentId = roadSegmentAddedToNationalRoad2.SegmentId,
                            Number = roadSegmentAddedToNationalRoad2.Number,
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
    public Task When_removing_road_segment_from_national_road()
    {
        _fixture.Freeze<RoadSegmentId>();

        var importedRoadSegment = _fixture.Create<ImportedRoadSegment>();

        var message = _fixture
            .Create<RoadNetworkChangesAccepted>()
            .WithAcceptedChanges(_fixture.CreateWith<RoadSegmentRemovedFromNationalRoad>(x =>
            {
                x.AttributeId = importedRoadSegment.PartOfNationalRoads.First().AttributeId;
            }));

        var expectedRecords = Array.Empty<object>()
            .Concat(new[]
            {
                BuildRoadSegmentRecord(IntegrationContextScenarioExtensions.InitialPosition, importedRoadSegment)
            })
            .Concat(message.Changes
                .Select(x => x.RoadSegmentRemovedFromNationalRoad)
                .Select(roadSegmentRemovedFromNationalRoad =>
                {
                    return BuildRoadSegmentRecord(
                        IntegrationContextScenarioExtensions.InitialPosition + 1,
                        importedRoadSegment,
                        roadSegment =>
                        {
                            roadSegment.Version = roadSegmentRemovedFromNationalRoad.SegmentVersion!.Value;

                            roadSegment.OrganizationId = message.OrganizationId;
                            roadSegment.OrganizationName = message.Organization;
                            roadSegment.VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(message.When);

                            var removedNationalRoad = roadSegment.PartOfNationalRoads.First();
                            removedNationalRoad.OrganizationId = message.OrganizationId;
                            removedNationalRoad.OrganizationName = message.Organization;
                            removedNationalRoad.VersionTimestamp = LocalDateTimeTranslator.TranslateFromWhen(message.When);
                            removedNationalRoad.IsRemoved = true;
                        });
                }));

        return BuildProjection()
            .Scenario()
            .Given(importedRoadSegment, message)
            .Expect(expectedRecords);
    }
}
