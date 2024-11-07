namespace RoadRegistry.SyncHost.Tests.StreetName;

using Autofac;
using BackOffice;
using BackOffice.Core;
using BackOffice.Framework;
using BackOffice.Messages;
using Be.Vlaanderen.Basisregisters.GrAr.Contracts.StreetNameRegistry;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner.ProjectionStates;
using Editor.Schema;
using Editor.Schema.Organizations;
using Editor.Schema.RoadSegments;
using Extensions;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NodaTime;
using NodaTime.Testing;
using RoadRegistry.Tests.BackOffice.Scenarios;
using SqlStreamStore;
using SqlStreamStore.Streams;
using Sync.StreetNameRegistry;
using SyncHost.StreetName;

public class StreetNameEventConsumerTests
{
    private readonly ILoggerFactory _loggerFactory;

    public StreetNameEventConsumerTests(
        ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory;
    }

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

        var page = await store.ReadAllForwards(Position.Start, 1);
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

        var page = await store.ReadAllForwards(Position.Start, 2);
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

    [Fact]
    public async Task WhenStreetNameWasRetiredBecauseOfMunicipalityMerger_ThenRoadNetworkChangesAccepted()
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

        var @event = new StreetNameWasRetiredBecauseOfMunicipalityMerger(
            string.Empty,
            oldStreetNamePersistentLocalId,
            [newStreetNamePersistentLocalId1, newStreetNamePersistentLocalId2],
            new FakeProvenance());
        topicConsumer.SeedMessage(@event);

        await consumer.StartAsync(CancellationToken.None);

        var page = await store.ReadAllForwards(Position.Start, 1);
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
    }

    [Fact]
    public async Task WhenStreetNameWasRetiredBecauseOfMunicipalityMergerWithNoNewPersistentLocalIds_ThenRoadNetworkChangesAcceptedToDisconnectSegments()
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

        var @event = new StreetNameWasRetiredBecauseOfMunicipalityMerger(
            string.Empty,
            oldStreetNamePersistentLocalId,
            [],
            new FakeProvenance());
        topicConsumer.SeedMessage(@event);

        await consumer.StartAsync(CancellationToken.None);

        var page = await store.ReadAllForwards(Position.Start, 1);
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
    }

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

        var page = await store.ReadAllForwards(Position.Start, 1);
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

        var page = await store.ReadAllForwards(Position.Start, 1);
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
    }

    private (StreetNameEventConsumer, IStreamStore, InMemoryStreetNameEventTopicConsumer) BuildSetup(
        Action<StreetNameEventConsumerContext>? configureDbContext = null,
        Action<EditorContext>? configureEditorContext = null
    )
    {
        var containerBuilder = new ContainerBuilder();
        containerBuilder.Register(_ => new EventSourcedEntityMap());
        containerBuilder.Register(_ => new ConfigurationBuilder().Build()).As<IConfiguration>();
        containerBuilder.Register(_ => new LoggerFactory()).As<ILoggerFactory>();
        containerBuilder
            .Register(c => new FakeRoadNetworkIdGenerator())
            .As<IRoadNetworkIdGenerator>()
            .SingleInstance();

        containerBuilder
            .RegisterDbContext<StreetNameEventConsumerContext>(string.Empty,
                _ => { }
                , dbContextOptionsBuilder =>
                {
                    var context = new StreetNameEventConsumerContext(dbContextOptionsBuilder.Options);
                    configureDbContext?.Invoke(context);
                    return context;
                }
            );
        containerBuilder
            .RegisterDbContext<EditorContext>(string.Empty,
                _ => { }
                , dbContextOptionsBuilder =>
                {
                    var context = new EditorContext(dbContextOptionsBuilder.Options);

                    context.ProjectionStates.Add(new ProjectionStateItem
                    {
                        Name = WellKnownProjectionStateNames.RoadRegistryEditorRoadNetworkProjectionHost,
                        Position = -1
                    });
                    context.ProjectionStates.Add(new ProjectionStateItem
                    {
                        Name = WellKnownProjectionStateNames.RoadRegistryEditorOrganizationV2ProjectionHost,
                        Position = -1
                    });

                    context.OrganizationsV2.Add(new OrganizationRecordV2
                    {
                        Id = 1,
                        Code = OrganizationOvoCode.DigitaalVlaanderen,
                        OvoCode = OrganizationOvoCode.DigitaalVlaanderen,
                        Name = "Digitaal Vlaanderen"
                    });

                    configureEditorContext?.Invoke(context);
                    return context;
                }
            );

        var lifetimeScope = containerBuilder.Build();

        var store = new InMemoryStreamStore();
        var topicConsumer = new InMemoryStreetNameEventTopicConsumer(lifetimeScope.Resolve<StreetNameEventConsumerContext>);
        var eventEnricher = EnrichEvent.WithTime(new FakeClock(NodaConstants.UnixEpoch));

        return (new StreetNameEventConsumer(
            store,
            new StreetNameEventWriter(store, eventEnricher),
            new RoadNetworkEventWriter(store, eventEnricher),
            lifetimeScope.Resolve<IRoadNetworkIdGenerator>(),
            topicConsumer,
            lifetimeScope.Resolve<EditorContext>,
            _loggerFactory.CreateLogger<StreetNameEventConsumer>()
        ), store, topicConsumer);
    }
}
