namespace RoadRegistry.SyncHost.Tests.StreetName.StreetNameEventConsumer.WhenStreetNameBecameInactive;

using RoadRegistry.Tests;
using BackOffice;
using BackOffice.Messages;
using Be.Vlaanderen.Basisregisters.GrAr.Contracts.StreetNameRegistry;
using Editor.Schema.RoadSegments;
using FluentAssertions;
using Newtonsoft.Json;
using RoadRegistry.Tests.BackOffice.Scenarios;
using SqlStreamStore.Streams;

// GAWR-7784: a street name that is retired or rejected is let go by every road segment, the same as a removed one.
public class GivenRoadNetwork : StreetNameEventConsumerTestsBase
{
    public static IEnumerable<object[]> InactiveStreetNameEvents()
    {
        yield return [new StreetNameWasRetiredV2(string.Empty, StreetNameLocalId, new FakeKafkaProvenance())];
        yield return [new StreetNameWasRejected(string.Empty, StreetNameLocalId, new FakeKafkaProvenance())];
    }

    private const int StreetNameLocalId = 1;
    private const int OtherStreetNameLocalId = 2;

    [Theory]
    [MemberData(nameof(InactiveStreetNameEvents))]
    public async Task ThenTheRoadSegmentsLetGoOfThatStreetName(object streetNameBecameInactive)
    {
        var testData = new RoadNetworkTestData();

        var (consumer, store, topicConsumer) = BuildSetup(configureEditorContext: editorContext =>
        {
            var segment = testData.Segment1Added;

            editorContext.RoadSegments.Add(
                new RoadSegmentRecord
                {
                    Id = segment.Id,
                    StartNodeId = segment.StartNodeId,
                    EndNodeId = segment.EndNodeId,
                    Geometry = GeometryTranslator.Translate(segment.Geometry),
                    Version = segment.Version,
                    GeometryVersion = segment.GeometryVersion,
                    StatusId = RoadSegmentStatus.Parse(segment.Status).Translation.Identifier,
                    MorphologyId = RoadSegmentMorphology.Parse(segment.Morphology).Translation.Identifier,
                    CategoryId = RoadSegmentCategory.Parse(segment.Category).Translation.Identifier,
                    LeftSideStreetNameId = StreetNameLocalId,
                    RightSideStreetNameId = OtherStreetNameLocalId,
                    MaintainerId = segment.MaintenanceAuthority.Code,
                    MaintainerName = segment.MaintenanceAuthority.Name,
                    MethodId = RoadSegmentGeometryDrawMethod.Parse(segment.GeometryDrawMethod).Translation.Identifier,
                    AccessRestrictionId = RoadSegmentAccessRestriction.Parse(segment.AccessRestriction).Translation.Identifier
                });
        });
        topicConsumer.SeedMessage(streetNameBecameInactive);

        await consumer.RunOnceAsync(CancellationToken.None);

        var page = await store.ReadAllForwards(Position.Start, int.MaxValue);
        page.Messages.Should().HaveCount(1);

        var streamMessage = page.Messages[0];
        Assert.Equal(nameof(RoadSegmentsStreetNamesChanged), streamMessage.Type);
        Assert.Equal("roadnetwork", streamMessage.StreamId);

        // Only the side that had the inactive street name changes; the other side keeps its own.
        var message = JsonConvert.DeserializeObject<RoadSegmentsStreetNamesChanged>(await streamMessage.GetJsonData());
        var roadSegment = Assert.Single(message.RoadSegments);
        Assert.Equal(testData.Segment1Added.Id, roadSegment.Id);
        Assert.Equal(-9, roadSegment.LeftSideStreetNameId);
        Assert.Null(roadSegment.RightSideStreetNameId);
    }
}
