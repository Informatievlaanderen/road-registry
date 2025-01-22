namespace RoadRegistry.SyncHost.Tests.StreetName.StreetNameEventConsumer.WhenStreetNameWasRejectedBecauseOfMunicipalityMerger;

using Be.Vlaanderen.Basisregisters.GrAr.Contracts.StreetNameRegistry;
using FluentAssertions;
using Newtonsoft.Json;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Messages;
using RoadRegistry.Editor.Schema.RoadSegments;
using RoadRegistry.Tests.BackOffice.Scenarios;
using SqlStreamStore.Streams;

public class GivenRoadNetwork : StreetNameEventConsumerTestsBase
{
    [Fact]
    public async Task WhenStreetNameWasRejectedBecauseOfMunicipalityMerger_ThenRoadNetworkChangesAccepted()
    {
        var testData = new RoadNetworkTestData();
        var oldStreetNamePersistentLocalId = 1;
        var newStreetNamePersistentLocalId1 = 2;
        var newStreetNamePersistentLocalId2 = 3;

        var (consumer, store, topicConsumer) = BuildSetup(configureEditorContext: editorContext =>
        {
            var segment = testData.Segment1Added;

            var statusTranslation = RoadSegmentStatus.Parse(segment.Status).Translation;
            var morphologyTranslation = RoadSegmentMorphology.Parse(segment.Morphology).Translation;
            var categoryTranslation = RoadSegmentCategory.Parse(segment.Category).Translation;
            var geometryDrawMethodTranslation = RoadSegmentGeometryDrawMethod.Parse(segment.GeometryDrawMethod).Translation;
            var accessRestrictionTranslation = RoadSegmentAccessRestriction.Parse(segment.AccessRestriction).Translation;

            editorContext.RoadSegments.AddRange(
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
                    LeftSideStreetNameId = oldStreetNamePersistentLocalId,
                    RightSideStreetNameId = StreetNameLocalId.Unknown,
                    MaintainerId = segment.MaintenanceAuthority.Code,
                    MaintainerName = segment.MaintenanceAuthority.Name,
                    MethodId = geometryDrawMethodTranslation.Identifier,
                    AccessRestrictionId = accessRestrictionTranslation.Identifier
                },
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
                    LeftSideStreetNameId = StreetNameLocalId.Unknown,
                    RightSideStreetNameId = oldStreetNamePersistentLocalId,
                    MaintainerId = segment.MaintenanceAuthority.Code,
                    MaintainerName = segment.MaintenanceAuthority.Name,
                    MethodId = geometryDrawMethodTranslation.Identifier,
                    AccessRestrictionId = accessRestrictionTranslation.Identifier
                },
                new RoadSegmentRecord
                {
                    Id = 3,
                    StartNodeId = segment.StartNodeId,
                    EndNodeId = segment.EndNodeId,
                    Geometry = GeometryTranslator.Translate(segment.Geometry),
                    Version = segment.Version,
                    GeometryVersion = segment.GeometryVersion,
                    StatusId = statusTranslation.Identifier,
                    MorphologyId = morphologyTranslation.Identifier,
                    CategoryId = categoryTranslation.Identifier,
                    LeftSideStreetNameId = StreetNameLocalId.Unknown,
                    RightSideStreetNameId = StreetNameLocalId.Unknown,
                    MaintainerId = segment.MaintenanceAuthority.Code,
                    MaintainerName = segment.MaintenanceAuthority.Name,
                    MethodId = geometryDrawMethodTranslation.Identifier,
                    AccessRestrictionId = accessRestrictionTranslation.Identifier
                });
        });

        var @event = new StreetNameWasRejectedBecauseOfMunicipalityMerger(
            string.Empty,
            oldStreetNamePersistentLocalId,
            [newStreetNamePersistentLocalId1, newStreetNamePersistentLocalId2],
            new FakeProvenance());
        topicConsumer.SeedMessage(@event);

        await consumer.StartAsync(CancellationToken.None);

        var page = await store.ReadAllForwards(Position.Start, int.MaxValue);
        page.Messages.Should().HaveCount(2);

        {
            var streamMessage = page.Messages[0];
            Assert.Equal(nameof(RoadNetworkChangesAccepted), streamMessage.Type);
            Assert.Equal("roadnetwork", streamMessage.StreamId);

            var message = JsonConvert.DeserializeObject<RoadNetworkChangesAccepted>(await streamMessage.GetJsonData());
            message.Changes.Should().HaveCount(2);

            var roadSegmentAttributesModified1 = message.Changes[0].RoadSegmentAttributesModified;
            roadSegmentAttributesModified1.Id.Should().Be(1);
            roadSegmentAttributesModified1.LeftSide.StreetNameId.Should().Be(newStreetNamePersistentLocalId1);
            roadSegmentAttributesModified1.RightSide.Should().BeNull();

            var roadSegmentAttributesModified2 = message.Changes[1].RoadSegmentAttributesModified;
            roadSegmentAttributesModified2.Id.Should().Be(2);
            roadSegmentAttributesModified2.LeftSide.Should().BeNull();
            roadSegmentAttributesModified2.RightSide.StreetNameId.Should().Be(newStreetNamePersistentLocalId1);
        }

        {
            var streamMessage = page.Messages[1];
            Assert.Equal(nameof(StreetNameRenamed), streamMessage.Type);
            Assert.Equal("streetnames", streamMessage.StreamId);

            var message = JsonConvert.DeserializeObject<StreetNameRenamed>(await streamMessage.GetJsonData());
            message.StreetNameLocalId.Should().Be(oldStreetNamePersistentLocalId);
            message.DestinationStreetNameLocalId.Should().Be(newStreetNamePersistentLocalId1);
        }
    }

    [Fact]
    public async Task WhenStreetNameWasRejectedBecauseOfMunicipalityMergerWithNoNewPersistentLocalIds_ThenRoadNetworkChangesAcceptedToDisconnectSegments()
    {
        var testData = new RoadNetworkTestData();
        var oldStreetNamePersistentLocalId = 1;

        var (consumer, store, topicConsumer) = BuildSetup(configureEditorContext: editorContext =>
        {
            var segment = testData.Segment1Added;

            var statusTranslation = RoadSegmentStatus.Parse(segment.Status).Translation;
            var morphologyTranslation = RoadSegmentMorphology.Parse(segment.Morphology).Translation;
            var categoryTranslation = RoadSegmentCategory.Parse(segment.Category).Translation;
            var geometryDrawMethodTranslation = RoadSegmentGeometryDrawMethod.Parse(segment.GeometryDrawMethod).Translation;
            var accessRestrictionTranslation = RoadSegmentAccessRestriction.Parse(segment.AccessRestriction).Translation;

            editorContext.RoadSegments.AddRange(
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
                    LeftSideStreetNameId = oldStreetNamePersistentLocalId,
                    RightSideStreetNameId = StreetNameLocalId.Unknown,
                    MaintainerId = segment.MaintenanceAuthority.Code,
                    MaintainerName = segment.MaintenanceAuthority.Name,
                    MethodId = geometryDrawMethodTranslation.Identifier,
                    AccessRestrictionId = accessRestrictionTranslation.Identifier
                },
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
                    LeftSideStreetNameId = StreetNameLocalId.Unknown,
                    RightSideStreetNameId = oldStreetNamePersistentLocalId,
                    MaintainerId = segment.MaintenanceAuthority.Code,
                    MaintainerName = segment.MaintenanceAuthority.Name,
                    MethodId = geometryDrawMethodTranslation.Identifier,
                    AccessRestrictionId = accessRestrictionTranslation.Identifier
                },
                new RoadSegmentRecord
                {
                    Id = 3,
                    StartNodeId = segment.StartNodeId,
                    EndNodeId = segment.EndNodeId,
                    Geometry = GeometryTranslator.Translate(segment.Geometry),
                    Version = segment.Version,
                    GeometryVersion = segment.GeometryVersion,
                    StatusId = statusTranslation.Identifier,
                    MorphologyId = morphologyTranslation.Identifier,
                    CategoryId = categoryTranslation.Identifier,
                    LeftSideStreetNameId = StreetNameLocalId.Unknown,
                    RightSideStreetNameId = StreetNameLocalId.Unknown,
                    MaintainerId = segment.MaintenanceAuthority.Code,
                    MaintainerName = segment.MaintenanceAuthority.Name,
                    MethodId = geometryDrawMethodTranslation.Identifier,
                    AccessRestrictionId = accessRestrictionTranslation.Identifier
                });
        });

        var @event = new StreetNameWasRejectedBecauseOfMunicipalityMerger(
            string.Empty,
            oldStreetNamePersistentLocalId,
            [],
            new FakeProvenance());
        topicConsumer.SeedMessage(@event);

        await consumer.StartAsync(CancellationToken.None);

        var page = await store.ReadAllForwards(Position.Start, int.MaxValue);
        page.Messages.Should().HaveCount(2);

        {
            var streamMessage = page.Messages[0];
            Assert.Equal(nameof(RoadNetworkChangesAccepted), streamMessage.Type);
            Assert.Equal("roadnetwork", streamMessage.StreamId);

            var message = JsonConvert.DeserializeObject<RoadNetworkChangesAccepted>(await streamMessage.GetJsonData());
            message.Changes.Should().HaveCount(2);

            var roadSegmentAttributesModified1 = message.Changes[0].RoadSegmentAttributesModified;
            roadSegmentAttributesModified1.Id.Should().Be(1);
            roadSegmentAttributesModified1.LeftSide.StreetNameId.Should().Be(-9);
            roadSegmentAttributesModified1.RightSide.Should().BeNull();

            var roadSegmentAttributesModified2 = message.Changes[1].RoadSegmentAttributesModified;
            roadSegmentAttributesModified2.Id.Should().Be(2);
            roadSegmentAttributesModified2.LeftSide.Should().BeNull();
            roadSegmentAttributesModified2.RightSide.StreetNameId.Should().Be(-9);
        }

        {
            var streamMessage = page.Messages[1];
            Assert.Equal(nameof(StreetNameRenamed), streamMessage.Type);
            Assert.Equal("streetnames", streamMessage.StreamId);

            var message = JsonConvert.DeserializeObject<StreetNameRenamed>(await streamMessage.GetJsonData());
            message.StreetNameLocalId.Should().Be(oldStreetNamePersistentLocalId);
            message.DestinationStreetNameLocalId.Should().Be(-9);
        }
    }
}
