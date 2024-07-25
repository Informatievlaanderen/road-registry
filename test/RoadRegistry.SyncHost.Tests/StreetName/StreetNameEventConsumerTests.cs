namespace RoadRegistry.SyncHost.Tests.StreetName
{
    using Autofac;
    using BackOffice;
    using BackOffice.Extracts.Dbase.RoadSegments;
    using BackOffice.Framework;
    using BackOffice.Messages;
    using Be.Vlaanderen.Basisregisters.GrAr.Contracts.StreetNameRegistry;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner.ProjectionStates;
    using Editor.Schema;
    using Editor.Schema.Extensions;
    using Editor.Schema.RoadSegments;
    using Extensions;
    using FluentAssertions;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Microsoft.IO;
    using Newtonsoft.Json;
    using NodaTime;
    using NodaTime.Testing;
    using RoadRegistry.Tests.BackOffice.Scenarios;
    using SqlStreamStore;
    using SqlStreamStore.Streams;
    using Sync.StreetNameRegistry;

    public class StreetNameEventConsumerTests
    {
        private readonly RecyclableMemoryStreamManager _memoryStreamManager;
        private readonly FileEncoding _fileEncoding;
        private readonly ILoggerFactory _loggerFactory;

        public StreetNameEventConsumerTests(
            RecyclableMemoryStreamManager memoryStreamManager,
            FileEncoding fileEncoding,
            ILoggerFactory loggerFactory)
        {
            _memoryStreamManager = memoryStreamManager;
            _fileEncoding = fileEncoding;
            _loggerFactory = loggerFactory;
        }

        [Fact]
        public async Task CanConsumeSuccessfully_StreetNameWasRemovedV2()
        {
            var testData = new RoadNetworkTestData();

            var streetName1LocalId = 1;

            var streetName1WasRemoved = new StreetNameWasRemovedV2(string.Empty, streetName1LocalId, new FakeProvenance());

            var (consumer, store, topicConsumer) = BuildSetup(configureEditorContext: editorContext =>
            {
                editorContext.ProjectionStates.Add(new ProjectionStateItem
                {
                    Name = WellKnownProjectionStateNames.RoadRegistryEditorRoadNetworkProjectionHost,
                    Position = -1
                });

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

                editorContext.RoadSegmentLaneAttributes.AddRange(segment.Lanes
                    .Select(lane => new RoadSegmentLaneAttributeRecord
                    {
                        Id = lane.AttributeId,
                        RoadSegmentId = segment.Id,
                        DbaseRecord = new RoadSegmentLaneAttributeDbaseRecord
                        {
                            RS_OIDN = { Value = lane.AttributeId },
                            WS_OIDN = { Value = segment.Id },
                            WS_GIDN = { Value = segment.Id + "_" + lane.AsOfGeometryVersion },
                            AANTAL = { Value = lane.Count },
                            RICHTING = { Value = RoadSegmentLaneDirection.Parse(lane.Direction).Translation.Identifier },
                            LBLRICHT = { Value = RoadSegmentLaneDirection.Parse(lane.Direction).Translation.Name },
                            VANPOS = { Value = new RoadSegmentPosition(lane.FromPosition) },
                            TOTPOS = { Value = new RoadSegmentPosition(lane.ToPosition) }
                        }.ToBytes(_memoryStreamManager, _fileEncoding)
                    }));

                editorContext.RoadSegmentSurfaceAttributes.AddRange(segment.Surfaces
                    .Select(surface => new RoadSegmentSurfaceAttributeRecord
                    {
                        Id = surface.AttributeId,
                        RoadSegmentId = segment.Id,
                        DbaseRecord = new RoadSegmentSurfaceAttributeDbaseRecord
                        {
                            WV_OIDN = { Value = surface.AttributeId },
                            WS_OIDN = { Value = segment.Id },
                            WS_GIDN = { Value = segment.Id + "_" + surface.AsOfGeometryVersion },
                            TYPE = { Value = RoadSegmentSurfaceType.Parse(surface.Type).Translation.Identifier },
                            LBLTYPE = { Value = RoadSegmentSurfaceType.Parse(surface.Type).Translation.Name },
                            VANPOS = { Value = new RoadSegmentPosition(surface.FromPosition) },
                            TOTPOS = { Value = new RoadSegmentPosition(surface.ToPosition) }
                        }.ToBytes(_memoryStreamManager, _fileEncoding)
                    }));

                editorContext.RoadSegmentWidthAttributes.AddRange(segment.Widths
                    .Select(width => new RoadSegmentWidthAttributeRecord
                    {
                        Id = width.AttributeId,
                        RoadSegmentId = segment.Id,
                        DbaseRecord = new RoadSegmentWidthAttributeDbaseRecord
                        {
                            WB_OIDN = { Value = width.AttributeId },
                            WS_OIDN = { Value = segment.Id },
                            WS_GIDN = { Value = $"{segment.Id}_{width.AsOfGeometryVersion}" },
                            BREEDTE = { Value = width.Width },
                            VANPOS = { Value = new RoadSegmentPosition(width.FromPosition) },
                            TOTPOS = { Value = new RoadSegmentPosition(width.ToPosition) }
                        }.ToBytes(_memoryStreamManager, _fileEncoding)
                    }));
            });

            topicConsumer
                .SeedMessage(streetName1WasRemoved)
                ;

            await consumer.StartAsync(CancellationToken.None);

            var page = await store.ReadAllForwards(Position.Start, 1);
            {
                var streamMessage = page.Messages[0];
                Assert.Equal(nameof(ChangeRoadNetwork), streamMessage.Type);
                Assert.Equal("roadnetwork-command-queue", streamMessage.StreamId);

                var message = JsonConvert.DeserializeObject<ChangeRoadNetwork>(await streamMessage.GetJsonData());
                var modifyRoadSegment = Assert.Single(message.Changes).ModifyRoadSegment;
                Assert.Equal(testData.Segment1Added.Id, modifyRoadSegment.Id);
                Assert.Equal(-9, modifyRoadSegment.LeftSideStreetNameId);
                Assert.Equal(-9, modifyRoadSegment.RightSideStreetNameId);

                Assert.Equal(testData.Segment1Added.Lanes.Length, modifyRoadSegment.Lanes.Length);
                for (var i = 0; i < testData.Segment1Added.Lanes.Length; i++)
                {
                    Assert.Equal(testData.Segment1Added.Lanes[i].FromPosition, modifyRoadSegment.Lanes[i].FromPosition);
                    Assert.Equal(testData.Segment1Added.Lanes[i].ToPosition, modifyRoadSegment.Lanes[i].ToPosition);
                    Assert.Equal(testData.Segment1Added.Lanes[i].Count, modifyRoadSegment.Lanes[i].Count);
                    Assert.Equal(testData.Segment1Added.Lanes[i].Direction, modifyRoadSegment.Lanes[i].Direction);
                }

                Assert.Equal(testData.Segment1Added.Surfaces.Length, modifyRoadSegment.Surfaces.Length);
                for (var i = 0; i < testData.Segment1Added.Surfaces.Length; i++)
                {
                    Assert.Equal(testData.Segment1Added.Surfaces[i].FromPosition, modifyRoadSegment.Surfaces[i].FromPosition);
                    Assert.Equal(testData.Segment1Added.Surfaces[i].ToPosition, modifyRoadSegment.Surfaces[i].ToPosition);
                    Assert.Equal(testData.Segment1Added.Surfaces[i].Type, modifyRoadSegment.Surfaces[i].Type);
                }

                Assert.Equal(testData.Segment1Added.Widths.Length, modifyRoadSegment.Widths.Length);
                for (var i = 0; i < testData.Segment1Added.Widths.Length; i++)
                {
                    Assert.Equal(testData.Segment1Added.Widths[i].FromPosition, modifyRoadSegment.Widths[i].FromPosition);
                    Assert.Equal(testData.Segment1Added.Widths[i].ToPosition, modifyRoadSegment.Widths[i].ToPosition);
                    Assert.Equal(testData.Segment1Added.Widths[i].Width, modifyRoadSegment.Widths[i].Width);
                }
            }
        }

        [Fact]
        public async Task CanConsumeSuccessfully_StreetNameWasRenamed()
        {
            var testData = new RoadNetworkTestData();

            var streetName1LocalId = 1;
            var streetName2LocalId = 2;
            var streetName1WasRenamed = new StreetNameWasRenamed(string.Empty, streetName1LocalId, streetName2LocalId, new FakeProvenance());

            var (consumer, store, topicConsumer) = BuildSetup(configureEditorContext: editorContext =>
            {
                editorContext.ProjectionStates.Add(new ProjectionStateItem
                {
                    Name = WellKnownProjectionStateNames.RoadRegistryEditorRoadNetworkProjectionHost,
                    Position = -1
                });

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

            topicConsumer
                .SeedMessage(streetName1WasRenamed)
                ;

            await consumer.StartAsync(CancellationToken.None);

            var page = await store.ReadAllForwards(Position.Start, 2);
            {
                var streamMessage = page.Messages[0];
                Assert.Equal(nameof(StreetNameRenamed), streamMessage.Type);
                Assert.Equal("streetnames", streamMessage.StreamId);

                var message = JsonConvert.DeserializeObject<StreetNameRenamed>(await streamMessage.GetJsonData());
                Assert.Equal(streetName1LocalId, message.StreetNameLocalId);
                Assert.Equal(streetName2LocalId, message.DestinationStreetNameLocalId);
            }
            {
                var streamMessage = page.Messages[1];
                Assert.Equal(nameof(ChangeRoadNetwork), streamMessage.Type);
                Assert.Equal("roadnetwork-command-queue", streamMessage.StreamId);

                var message = JsonConvert.DeserializeObject<ChangeRoadNetwork>(await streamMessage.GetJsonData());
                var modifyRoadSegment = Assert.Single(message.Changes).ModifyRoadSegment;
                Assert.Equal(testData.Segment1Added.Id, modifyRoadSegment.Id);
                Assert.Equal(streetName2LocalId, modifyRoadSegment.LeftSideStreetNameId);
                Assert.Equal(streetName2LocalId, modifyRoadSegment.RightSideStreetNameId);
            }
        }

        [Fact]
        public async Task WhenStreetNameWasRetiredBecauseOfMunicipalityMerger_ThenChangeRoadNetwork()
        {
            var testData = new RoadNetworkTestData();

            var oldStreetNamePersistentLocalId = 1;
            var newStreetNamePersistentLocalId1 = 2;
            var newStreetNamePersistentLocalId2 = 3;

            var (consumer, store, topicConsumer) = BuildSetup(configureEditorContext: editorContext =>
            {
                editorContext.ProjectionStates.Add(new ProjectionStateItem
                {
                    Name = WellKnownProjectionStateNames.RoadRegistryEditorRoadNetworkProjectionHost,
                    Position = -1
                });

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
                new[] { newStreetNamePersistentLocalId1, newStreetNamePersistentLocalId2 },
                new FakeProvenance());
            topicConsumer
                .SeedMessage(@event);

            await consumer.StartAsync(CancellationToken.None);

            var page = await store.ReadAllForwards(Position.Start, 1);
            {
                var streamMessage = page.Messages[0];
                Assert.Equal(nameof(ChangeRoadNetwork), streamMessage.Type);
                Assert.Equal("roadnetwork-command-queue", streamMessage.StreamId);

                var message = JsonConvert.DeserializeObject<ChangeRoadNetwork>(await streamMessage.GetJsonData());
                message.Changes.Should().HaveCount(2);

                var modifyRoadSegment1 = message.Changes[0].ModifyRoadSegment;
                Assert.Equal(1, modifyRoadSegment1.Id);
                Assert.Equal(newStreetNamePersistentLocalId1, modifyRoadSegment1.LeftSideStreetNameId);
                Assert.Equal(StreetNameLocalId.Unknown, modifyRoadSegment1.RightSideStreetNameId);

                var modifyRoadSegment2 = message.Changes[1].ModifyRoadSegment;
                Assert.Equal(2, modifyRoadSegment2.Id);
                Assert.Equal(StreetNameLocalId.Unknown, modifyRoadSegment2.LeftSideStreetNameId);
                Assert.Equal(newStreetNamePersistentLocalId1, modifyRoadSegment2.RightSideStreetNameId);
            }
        }

        [Fact]
        public async Task WhenStreetNameWasRejectedBecauseOfMunicipalityMerger_ThenChangeRoadNetwork()
        {
            var testData = new RoadNetworkTestData();

            var oldStreetNamePersistentLocalId = 1;
            var newStreetNamePersistentLocalId1 = 2;
            var newStreetNamePersistentLocalId2 = 3;

            var (consumer, store, topicConsumer) = BuildSetup(configureEditorContext: editorContext =>
            {
                editorContext.ProjectionStates.Add(new ProjectionStateItem
                {
                    Name = WellKnownProjectionStateNames.RoadRegistryEditorRoadNetworkProjectionHost,
                    Position = -1
                });

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
                new[] { newStreetNamePersistentLocalId1, newStreetNamePersistentLocalId2 },
                new FakeProvenance());
            topicConsumer
                .SeedMessage(@event);

            await consumer.StartAsync(CancellationToken.None);

            var page = await store.ReadAllForwards(Position.Start, 1);
            {
                var streamMessage = page.Messages[0];
                Assert.Equal(nameof(ChangeRoadNetwork), streamMessage.Type);
                Assert.Equal("roadnetwork-command-queue", streamMessage.StreamId);

                var message = JsonConvert.DeserializeObject<ChangeRoadNetwork>(await streamMessage.GetJsonData());
                message.Changes.Should().HaveCount(2);

                var modifyRoadSegment1 = message.Changes[0].ModifyRoadSegment;
                Assert.Equal(1, modifyRoadSegment1.Id);
                Assert.Equal(newStreetNamePersistentLocalId1, modifyRoadSegment1.LeftSideStreetNameId);
                Assert.Equal(StreetNameLocalId.Unknown, modifyRoadSegment1.RightSideStreetNameId);

                var modifyRoadSegment2 = message.Changes[1].ModifyRoadSegment;
                Assert.Equal(2, modifyRoadSegment2.Id);
                Assert.Equal(StreetNameLocalId.Unknown, modifyRoadSegment2.LeftSideStreetNameId);
                Assert.Equal(newStreetNamePersistentLocalId1, modifyRoadSegment2.RightSideStreetNameId);
            }
        }

        private (StreetNameEventConsumer, IStreamStore, InMemoryStreetNameEventTopicConsumer) BuildSetup(
            Action<StreetNameEventConsumerContext> configureDbContext = null,
            Action<EditorContext> configureEditorContext = null
        )
        {
            var containerBuilder = new ContainerBuilder();
            containerBuilder.Register(_ => new EventSourcedEntityMap());
            containerBuilder.Register(_ => new ConfigurationBuilder().Build()).As<IConfiguration>();
            containerBuilder.Register(_ => new LoggerFactory()).As<ILoggerFactory>();

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
                        configureEditorContext?.Invoke(context);
                        return context;
                    }
                );

            var lifetimeScope = containerBuilder.Build();

            var store = new InMemoryStreamStore();
            var topicConsumer = new InMemoryStreetNameEventTopicConsumer(lifetimeScope.Resolve<StreetNameEventConsumerContext>);

            return (new StreetNameEventConsumer(
                lifetimeScope,
                store,
                new StreetNameEventWriter(store, EnrichEvent.WithTime(new FakeClock(NodaConstants.UnixEpoch))),
                new RoadNetworkCommandQueue(store, new ApplicationMetadata(RoadRegistryApplication.BackOffice)),
                topicConsumer,
                lifetimeScope.Resolve<EditorContext>,
                _memoryStreamManager,
                _fileEncoding,
                _loggerFactory.CreateLogger<StreetNameEventConsumer>()
            ), store, topicConsumer);
        }
    }
}
