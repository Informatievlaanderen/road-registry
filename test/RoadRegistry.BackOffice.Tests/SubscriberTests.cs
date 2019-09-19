namespace RoadRegistry.BackOffice
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Threading.Tasks.Dataflow;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using Framework;
    using Framework.Reactions;
    using Messages;
    using Newtonsoft.Json;
    using SqlStreamStore;
    using SqlStreamStore.Streams;
    using SqlStreamStore.Subscriptions;
    using Xunit;
    using RecordedEvent = Framework.Reactions.RecordedEvent;

    public class SubscriberTests
    {
        [Fact]
        public void StoreCanNotBeNull()
        {
            Assert.Throws<ArgumentNullException>(() => new Subscriber(null, new StreamId("-")));
        }

        public class WithEmptyAllStream
        {
            private readonly IStreamStore _store;

            public WithEmptyAllStream()
            {
                _store = new InMemoryStreamStore();
            }

            [Fact(Skip = "Broken")]
            public async Task WhenMessageIsAppended()
            {
                var mapping = new EventMapping(EventMapping.DiscoverEventNamesInAssembly(typeof(RoadNetworkEvents).Assembly));
                var settings = EventsJsonSerializerSettingsProvider.CreateSerializerSettings();
                var archiveStream = new StreamName("archive-1");
                var commandStream = new StreamName("road-network-commands");
                var id = Guid.NewGuid();
                var reaction = new ReactionScenarioBuilder()
                    .Given(new RecordedEvent(archiveStream, new Messages.UploadRoadNetworkChangesArchive {ArchiveId = "123"}).WithMessageId(id))
                    .Then(new RecordedEvent(commandStream, new Messages.UploadRoadNetworkChangesArchive {ArchiveId = "123"}).WithMessageId(id).WithMetadata(new { Position = 1 }))
                    .Build();

                var sut = new Subscriber(_store, commandStream);

                using (sut)
                {
                    sut.Start();

                    //Act
                    foreach (var stream in reaction.Givens.GroupBy(given => given.Stream))
                    {
                        await _store.AppendToStream(
                            stream.Key,
                            ExpectedVersion.NoStream,
                            stream.Select((given, index) => new NewStreamMessage(
                                Deterministic.Create(Deterministic.Namespaces.Events,
                                    $"{given.Stream}-{index}"),
                                mapping.GetEventName(given.Event.GetType()),
                                JsonConvert.SerializeObject(given.Event, settings),
                                given.Metadata != null ? JsonConvert.SerializeObject(given.Metadata, settings) : null
                            )).ToArray());
                    }


                    //Await
                    var page = await _store.ReadStreamForwards(commandStream, StreamVersion.Start, 1);
                    while (page.Status == PageReadStatus.StreamNotFound)
                    {
                        page = await _store.ReadStreamForwards(commandStream, StreamVersion.Start, 1);
                    }

                    //Assert
                    //Assert.Equal(_messageId, page.Messages[0].MessageId);
                }

                await sut.Disposed;
            }
        }

        public class WithFilledAllStream
        {
        }
    }

    public class Subscriber : IDisposable
    {
        private CancellationTokenSource MessagePumpCancellation { get; }
        private BufferBlock<object> Mailbox { get; }
        private Lazy<Task> MessagePump { get; }

        public Subscriber(IStreamStore store, StreamId targetStream)
        {
            if (store == null) throw new ArgumentNullException(nameof(store));

            MessagePumpCancellation = new CancellationTokenSource();
            Mailbox = new BufferBlock<object>(
                new DataflowBlockOptions
                {
                    BoundedCapacity = int.MaxValue,
                    MaxMessagesPerTask = 1,
                    CancellationToken = MessagePumpCancellation.Token
                }
            );
            Disposed = Mailbox
                .Completion
                .ContinueWith(
                    task => { MessagePumpCancellation?.Dispose(); },
                    TaskContinuationOptions.ExecuteSynchronously);
            MessagePump = new Lazy<Task>(
                () => Task.Run(async () =>
                {
                    long? position = null;
                    var targetStreamVersion = ExpectedVersion.NoStream;
                    var page = await store.ReadStreamBackwards(targetStream, StreamVersion.End, 1,
                        MessagePumpCancellation.Token);
                    if (page.Status == PageReadStatus.Success)
                    {
                        targetStreamVersion = page.LastStreamVersion;
                        if (page.Messages.Length == 1)
                        {
                            position = JsonConvert
                                .DeserializeObject<MetaDataView>(page.Messages[0].JsonMetadata)
                                .CausedByPosition;

                        }
                    }

                    var subscription = store.SubscribeToAll(
                        position,
                        async (_, received, token) =>
                        {
                            if (received.StreamId != targetStream) // skip your own messages
                            {
                                var msg = new Messages.AllStreamSubscriptionReceivedMessage
                                {
                                    Position = received.Position,
                                    StreamId = received.StreamId,
                                    StreamVersion = received.StreamVersion,
                                    MessageId = received.MessageId,
                                    Type = received.Type,
                                    JsonData = await received.GetJsonData(token),
                                    JsonMetaData = received.JsonMetadata,
                                    CreatedUtc = received.CreatedUtc
                                };
                                await Mailbox.SendAsync(msg, token);
                            }
                        },
                        (_, reason, exception) =>
                            Mailbox.Post(new Messages.AllStreamSubscriptionDropped
                            {
                                Reason = reason, Exception = exception
                            }));

                    while (!MessagePumpCancellation.IsCancellationRequested)
                    {
                        var message = await Mailbox.ReceiveAsync(MessagePumpCancellation.Token);
                        switch (message)
                        {
                            case Messages.AllStreamSubscriptionReceivedMessage received:
                                var appendResult = await store.AppendToStream(
                                    targetStream,
                                    targetStreamVersion,
                                    new []
                                    {
                                        new NewStreamMessage(
                                            received.MessageId,
                                            received.Type,
                                            received.JsonData,
                                            received.JsonMetaData)
                                    },
                                    MessagePumpCancellation.Token);
                                targetStreamVersion = appendResult.CurrentVersion;
                                break;
                            case Messages.AllStreamSubscriptionDropped dropped:
                                break;
                        }
                    }
                }, MessagePumpCancellation.Token),
                LazyThreadSafetyMode.ExecutionAndPublication);
        }

        public void Start()
        {
            var task = MessagePump.Value;
            if (task.Status == TaskStatus.Created)
            {
                task.Start();
            }
        }

        public Task Disposed { get; }

        public void Dispose()
        {
            MessagePumpCancellation.Cancel();
            Mailbox.Complete();
        }

        private class Messages
        {
            public class AllStreamSubscriptionReceivedMessage
            {
                public long Position { get; set; }
                public string StreamId { get; set; }
                public int StreamVersion { get; set; }
                public Guid MessageId { get; set; }
                public string Type { get; set; }
                public string JsonData { get; set; }
                public string JsonMetaData { get; set; }
                public DateTime CreatedUtc { get; set; }

            }

            public class AllStreamSubscriptionDropped
            {
                public SubscriptionDroppedReason Reason { get; set; }
                public Exception Exception { get; set; }
            }
        }

        private class MetaDataView
        {
            public long CausedByPosition { get; set; }
        }
    }
}
