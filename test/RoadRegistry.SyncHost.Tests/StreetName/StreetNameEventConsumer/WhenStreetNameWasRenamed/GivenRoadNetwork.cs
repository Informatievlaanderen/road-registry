namespace RoadRegistry.SyncHost.Tests.StreetName.StreetNameEventConsumer.WhenStreetNameWasRenamed;

using BackOffice;
using BackOffice.Messages;
using Be.Vlaanderen.Basisregisters.GrAr.Contracts.StreetNameRegistry;
using Editor.Schema.RoadSegments;
using FluentAssertions;
using Newtonsoft.Json;
using RoadRegistry.Tests.BackOffice.Scenarios;
using SqlStreamStore.Streams;

public class GivenRoadNetwork : StreetNameEventConsumerTestsBase
{
    [Fact]
    public async Task WhenStreetNameWasRenamed_ThenRoadNetworkChangesAccepted()
    {
        var testData = new RoadNetworkTestData();
        var streetName1LocalId = 1;
        var streetName2LocalId = 2;
        var streetName1WasRenamed = new StreetNameWasRenamed(string.Empty, streetName1LocalId, streetName2LocalId, new FakeProvenance());

        var (consumer, store, topicConsumer) = BuildSetup(configureEditorContext: editorContext =>
        {
            var statusTranslation = RoadSegmentStatus.Parse(testData.Segment1Added.Status).Translation;
            var morphologyTranslation = RoadSegmentMorphology.Parse(testData.Segment1Added.Morphology).Translation;
            var categoryTranslation = RoadSegmentCategory.Parse(testData.Segment1Added.Category).Translation;
            var geometryDrawMethodTranslation = RoadSegmentGeometryDrawMethod.Parse(testData.Segment1Added.GeometryDrawMethod).Translation;
            var accessRestrictionTranslation = RoadSegmentAccessRestriction.Parse(testData.Segment1Added.AccessRestriction).Translation;

            editorContext.RoadSegments.Add(
                new RoadSegmentRecord
                {
                    Id = testData.Segment1Added.Id,
                    StartNodeId = testData.Segment1Added.StartNodeId,
                    EndNodeId = testData.Segment1Added.EndNodeId,
                    Geometry = GeometryTranslator.Translate(testData.Segment1Added.Geometry),
                    Version = testData.Segment1Added.Version,
                    GeometryVersion = testData.Segment1Added.GeometryVersion,
                    StatusId = statusTranslation.Identifier,
                    MorphologyId = morphologyTranslation.Identifier,
                    CategoryId = categoryTranslation.Identifier,
                    LeftSideStreetNameId = streetName1LocalId,
                    RightSideStreetNameId = streetName1LocalId,
                    MaintainerId = testData.Segment1Added.MaintenanceAuthority.Code,
                    MaintainerName = testData.Segment1Added.MaintenanceAuthority.Name,
                    MethodId = geometryDrawMethodTranslation.Identifier,
                    AccessRestrictionId = accessRestrictionTranslation.Identifier
                });
        });
        topicConsumer.SeedMessage(streetName1WasRenamed);

        await consumer.StartAsync(CancellationToken.None);

        var page = await store.ReadAllForwards(Position.Start, int.MaxValue);
        page.Messages.Should().HaveCount(2);

        {
            var streamMessage = page.Messages[0];
            Assert.Equal(nameof(RoadNetworkChangesAccepted), streamMessage.Type);
            Assert.Equal("roadnetwork", streamMessage.StreamId);

            var message = JsonConvert.DeserializeObject<RoadNetworkChangesAccepted>(await streamMessage.GetJsonData());
            var roadSegmentAttributesModified = Assert.Single(message.Changes).RoadSegmentAttributesModified;
            roadSegmentAttributesModified.Id.Should().Be(testData.Segment1Added.Id);
            roadSegmentAttributesModified.LeftSide.StreetNameId.Should().Be(streetName2LocalId);
            roadSegmentAttributesModified.RightSide.StreetNameId.Should().Be(streetName2LocalId);
        }
        {
            var streamMessage = page.Messages[1];
            Assert.Equal(nameof(StreetNameRenamed), streamMessage.Type);
            Assert.Equal("streetnames", streamMessage.StreamId);

            var message = JsonConvert.DeserializeObject<StreetNameRenamed>(await streamMessage.GetJsonData());
            Assert.Equal(streetName1LocalId, message.StreetNameLocalId);
            Assert.Equal(streetName2LocalId, message.DestinationStreetNameLocalId);
        }
    }
}
