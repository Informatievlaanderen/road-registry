namespace RoadRegistry.SyncHost.Tests.StreetName.StreetNameEventConsumer.WhenStreetNameWasRemovedV2;

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
    public async Task WhenStreetNameWasRemovedV2_ThenRoadNetworkChangesAccepted()
    {
        var testData = new RoadNetworkTestData();
        var streetName1LocalId = 1;
        var streetName1WasRemoved = new StreetNameWasRemovedV2(string.Empty, streetName1LocalId, new FakeProvenance());

        var (consumer, store, topicConsumer) = BuildSetup(configureEditorContext: editorContext =>
        {
            var segment = testData.Segment1Added;

            var statusTranslation = RoadSegmentStatus.Parse(segment.Status).Translation;
            var morphologyTranslation = RoadSegmentMorphology.Parse(segment.Morphology).Translation;
            var categoryTranslation = RoadSegmentCategory.Parse(segment.Category).Translation;
            var geometryDrawMethodTranslation = RoadSegmentGeometryDrawMethod.Parse(segment.GeometryDrawMethod).Translation;
            var accessRestrictionTranslation = RoadSegmentAccessRestriction.Parse(segment.AccessRestriction).Translation;

            editorContext.RoadSegments.Add(
                new RoadSegmentRecord
                {
                    Id = segment.Id,
                    StartNodeId = segment.StartNodeId,
                    EndNodeId = segment.EndNodeId,
                    Geometry = GeometryTranslator.Translate(segment.Geometry),
                    Version = segment.Version,
                    GeometryVersion = segment.GeometryVersion,
                    StatusId = statusTranslation.Identifier,
                    MorphologyId = morphologyTranslation.Identifier,
                    CategoryId = categoryTranslation.Identifier,
                    LeftSideStreetNameId = streetName1LocalId,
                    RightSideStreetNameId = streetName1LocalId,
                    MaintainerId = segment.MaintenanceAuthority.Code,
                    MaintainerName = segment.MaintenanceAuthority.Name,
                    MethodId = geometryDrawMethodTranslation.Identifier,
                    AccessRestrictionId = accessRestrictionTranslation.Identifier
                });
        });
        topicConsumer.SeedMessage(streetName1WasRemoved);

        await consumer.StartAsync(CancellationToken.None);

        var page = await store.ReadAllForwards(Position.Start, int.MaxValue);
        page.Messages.Should().HaveCount(1);

        {
            var streamMessage = page.Messages[0];
            Assert.Equal(nameof(RoadNetworkChangesAccepted), streamMessage.Type);
            Assert.Equal("roadnetwork", streamMessage.StreamId);

            var message = JsonConvert.DeserializeObject<RoadNetworkChangesAccepted>(await streamMessage.GetJsonData());
            var roadSegmentAttributesModified = Assert.Single(message.Changes).RoadSegmentAttributesModified;
            Assert.Equal(testData.Segment1Added.Id, roadSegmentAttributesModified.Id);
            Assert.Equal(-9, roadSegmentAttributesModified.LeftSide.StreetNameId);
            Assert.Equal(-9, roadSegmentAttributesModified.RightSide.StreetNameId);
        }
    }

    [Fact]
    public async Task WhenStreetNameWasRemovedV2LinkedToMeasuredAndOutlinedRoadSegments_ThenRoadNetworkChangesAccepted()
    {
        var testData = new RoadNetworkTestData();
        var streetName1LocalId = 1;
        var streetName1WasRemoved = new StreetNameWasRemovedV2(string.Empty, streetName1LocalId, new FakeProvenance());

        var (consumer, store, topicConsumer) = BuildSetup(configureEditorContext: editorContext =>
        {
            var segment = testData.Segment1Added;

            var statusTranslation = RoadSegmentStatus.Parse(segment.Status).Translation;
            var morphologyTranslation = RoadSegmentMorphology.Parse(segment.Morphology).Translation;
            var categoryTranslation = RoadSegmentCategory.Parse(segment.Category).Translation;
            var measuredGeometryDrawMethodTranslation = RoadSegmentGeometryDrawMethod.Measured.Translation;
            var outlinedGeometryDrawMethodTranslation = RoadSegmentGeometryDrawMethod.Outlined.Translation;
            var accessRestrictionTranslation = RoadSegmentAccessRestriction.Parse(segment.AccessRestriction).Translation;

            editorContext.RoadSegments.Add(
                new RoadSegmentRecord
                {
                    Id = 1,
                    StartNodeId = segment.StartNodeId,
                    EndNodeId = segment.EndNodeId,
                    Geometry = GeometryTranslator.Translate(segment.Geometry),
                    Version = segment.Version,
                    GeometryVersion = segment.GeometryVersion,
                    StatusId = statusTranslation.Identifier,
                    MorphologyId = morphologyTranslation.Identifier,
                    CategoryId = categoryTranslation.Identifier,
                    LeftSideStreetNameId = streetName1LocalId,
                    RightSideStreetNameId = streetName1LocalId,
                    MaintainerId = segment.MaintenanceAuthority.Code,
                    MaintainerName = segment.MaintenanceAuthority.Name,
                    MethodId = measuredGeometryDrawMethodTranslation.Identifier,
                    AccessRestrictionId = accessRestrictionTranslation.Identifier
                });
            editorContext.RoadSegments.Add(
                new RoadSegmentRecord
                {
                    Id = 2,
                    StartNodeId = segment.StartNodeId,
                    EndNodeId = segment.EndNodeId,
                    Geometry = GeometryTranslator.Translate(segment.Geometry),
                    Version = segment.Version,
                    GeometryVersion = segment.GeometryVersion,
                    StatusId = statusTranslation.Identifier,
                    MorphologyId = morphologyTranslation.Identifier,
                    CategoryId = categoryTranslation.Identifier,
                    LeftSideStreetNameId = streetName1LocalId,
                    RightSideStreetNameId = streetName1LocalId,
                    MaintainerId = segment.MaintenanceAuthority.Code,
                    MaintainerName = segment.MaintenanceAuthority.Name,
                    MethodId = measuredGeometryDrawMethodTranslation.Identifier,
                    AccessRestrictionId = accessRestrictionTranslation.Identifier
                });
            editorContext.RoadSegments.Add(
                new RoadSegmentRecord
                {
                    Id = 3,
                    StartNodeId = 0,
                    EndNodeId = 0,
                    Geometry = GeometryTranslator.Translate(segment.Geometry),
                    Version = segment.Version,
                    GeometryVersion = segment.GeometryVersion,
                    StatusId = statusTranslation.Identifier,
                    MorphologyId = morphologyTranslation.Identifier,
                    CategoryId = categoryTranslation.Identifier,
                    LeftSideStreetNameId = streetName1LocalId,
                    RightSideStreetNameId = streetName1LocalId,
                    MaintainerId = segment.MaintenanceAuthority.Code,
                    MaintainerName = segment.MaintenanceAuthority.Name,
                    MethodId = outlinedGeometryDrawMethodTranslation.Identifier,
                    AccessRestrictionId = accessRestrictionTranslation.Identifier
                });
            editorContext.RoadSegments.Add(
                new RoadSegmentRecord
                {
                    Id = 4,
                    StartNodeId = 0,
                    EndNodeId = 0,
                    Geometry = GeometryTranslator.Translate(segment.Geometry),
                    Version = segment.Version,
                    GeometryVersion = segment.GeometryVersion,
                    StatusId = statusTranslation.Identifier,
                    MorphologyId = morphologyTranslation.Identifier,
                    CategoryId = categoryTranslation.Identifier,
                    LeftSideStreetNameId = streetName1LocalId,
                    RightSideStreetNameId = streetName1LocalId,
                    MaintainerId = segment.MaintenanceAuthority.Code,
                    MaintainerName = segment.MaintenanceAuthority.Name,
                    MethodId = outlinedGeometryDrawMethodTranslation.Identifier,
                    AccessRestrictionId = accessRestrictionTranslation.Identifier
                });
        });
        topicConsumer.SeedMessage(streetName1WasRemoved);

        await consumer.StartAsync(CancellationToken.None);

        var page = await store.ReadAllForwards(Position.Start, int.MaxValue);
        {
            page.Messages.Should().HaveCount(3);

            {
                var streamMessage = page.Messages[0];
                Assert.Equal(nameof(RoadNetworkChangesAccepted), streamMessage.Type);
                Assert.Equal("roadnetwork", streamMessage.StreamId);

                var message = JsonConvert.DeserializeObject<RoadNetworkChangesAccepted>(await streamMessage.GetJsonData());
                var roadSegment1AttributesModified = message.Changes[0].RoadSegmentAttributesModified;
                Assert.Equal(1, roadSegment1AttributesModified.Id);
                Assert.Equal(-9, roadSegment1AttributesModified.LeftSide.StreetNameId);
                Assert.Equal(-9, roadSegment1AttributesModified.RightSide.StreetNameId);

                var roadSegment2AttributesModified = message.Changes[1].RoadSegmentAttributesModified;
                Assert.Equal(2, roadSegment2AttributesModified.Id);
                Assert.Equal(-9, roadSegment2AttributesModified.LeftSide.StreetNameId);
                Assert.Equal(-9, roadSegment2AttributesModified.RightSide.StreetNameId);
            }

            {
                var streamMessage = page.Messages[1];
                Assert.Equal(nameof(RoadNetworkChangesAccepted), streamMessage.Type);
                Assert.Equal("roadsegment-outline-3", streamMessage.StreamId);

                var message = JsonConvert.DeserializeObject<RoadNetworkChangesAccepted>(await streamMessage.GetJsonData());
                var roadSegment3AttributesModified = message.Changes[0].RoadSegmentAttributesModified;
                Assert.Equal(3, roadSegment3AttributesModified.Id);
                Assert.Equal(-9, roadSegment3AttributesModified.LeftSide.StreetNameId);
                Assert.Equal(-9, roadSegment3AttributesModified.RightSide.StreetNameId);
            }

            {
                var streamMessage = page.Messages[2];
                Assert.Equal(nameof(RoadNetworkChangesAccepted), streamMessage.Type);
                Assert.Equal("roadsegment-outline-4", streamMessage.StreamId);

                var message = JsonConvert.DeserializeObject<RoadNetworkChangesAccepted>(await streamMessage.GetJsonData());
                var roadSegment4AttributesModified = message.Changes[0].RoadSegmentAttributesModified;
                Assert.Equal(4, roadSegment4AttributesModified.Id);
                Assert.Equal(-9, roadSegment4AttributesModified.LeftSide.StreetNameId);
                Assert.Equal(-9, roadSegment4AttributesModified.RightSide.StreetNameId);
            }
        }
    }
}
