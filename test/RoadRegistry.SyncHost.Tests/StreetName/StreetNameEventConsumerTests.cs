namespace RoadRegistry.SyncHost.Tests.StreetName
{
    using Autofac;
    using AutoFixture;
    using BackOffice;
    using BackOffice.FeatureToggles;
    using BackOffice.Framework;
    using BackOffice.Messages;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner.ProjectionStates;
    using Editor.Schema;
    using Editor.Schema.RoadSegments;
    using Extensions;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Microsoft.IO;
    using Newtonsoft.Json;
    using RoadRegistry.StreetName;
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

            var streetName1ObjectId = 1;

            var streetName1WasRemoved = new StreetNameWasRemovedV2
            {
                PersistentLocalId = streetName1ObjectId
            };

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

                editorContext.RoadSegmentsV2.Add(
                    new RoadSegmentV2Record
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
                        LeftSideStreetNameId = streetName1ObjectId,
                        RightSideStreetNameId = streetName1ObjectId,
                        MaintainerId = testData.Segment1Added.MaintenanceAuthority.Code,
                        MaintainerName = testData.Segment1Added.MaintenanceAuthority.Name,
                        MethodId = geometryDrawMethodTranslation.Identifier,
                        AccessRestrictionId = accessRestrictionTranslation.Identifier
                    });
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
            }
        }
        
        [Fact]
        public async Task CanConsumeSuccessfully_StreetNameWasRenamed()
        {
            var testData = new RoadNetworkTestData();

            var streetName1ObjectId = 1;
            var streetName2ObjectId = 2;

            var streetName1WasRemoved = new StreetNameWasRenamed
            {
                PersistentLocalId = streetName1ObjectId,
                DestinationPersistentLocalId = streetName2ObjectId
            };

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

                editorContext.RoadSegmentsV2.Add(
                    new RoadSegmentV2Record
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
                        LeftSideStreetNameId = streetName1ObjectId,
                        RightSideStreetNameId = streetName1ObjectId,
                        MaintainerId = testData.Segment1Added.MaintenanceAuthority.Code,
                        MaintainerName = testData.Segment1Added.MaintenanceAuthority.Name,
                        MethodId = geometryDrawMethodTranslation.Identifier,
                        AccessRestrictionId = accessRestrictionTranslation.Identifier
                    });
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
                Assert.Equal(streetName2ObjectId, modifyRoadSegment.LeftSideStreetNameId);
                Assert.Equal(streetName2ObjectId, modifyRoadSegment.RightSideStreetNameId);
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
                store,
                new RoadNetworkCommandQueue(store, new ApplicationMetadata(RoadRegistryApplication.BackOffice)),
                topicConsumer,
                lifetimeScope.Resolve<EditorContext>,
                _memoryStreamManager,
                _fileEncoding,
                new UseRoadSegmentV2EventProcessorFeatureToggle(false),
                _loggerFactory.CreateLogger<StreetNameEventConsumer>()
            ), store, topicConsumer);
        }
    }
}
