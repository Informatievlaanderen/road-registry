//namespace RoadRegistry.Model
//{
//    using System;
//    using System.Threading;
//    using System.Threading.Tasks;
//    using System.Threading.Tasks.Dataflow;
//    using Aiv.Vbr.EventHandling;
//    using Aiv.Vbr.Generators.Guid;
//    using Commands;
//    using FluentValidation;
//    using Framework;
//    using Newtonsoft.Json;
//    using SqlStreamStore;
//    using SqlStreamStore.Streams;
//
//    public class RoadNetworkModule : IDisposable
//    {
//        private readonly Task MessagePump;
//        private readonly CancellationTokenSource MailboxCancellationSource;
//        private readonly BufferBlock<Message> Mailbox;
//
//        public RoadNetworkModule(IStreamStore store, JsonSerializerSettings settings, EventMapping mapping)
//        {
//            if (store == null) throw new ArgumentNullException(nameof(store));
//            if (settings == null) throw new ArgumentNullException(nameof(settings));
//            if (mapping == null) throw new ArgumentNullException(nameof(mapping));
//
//            MailboxCancellationSource = new CancellationTokenSource();
//            Mailbox = new BufferBlock<Message>();
//            MessagePump = Task.Run(async () =>
//            {
//                var networks = new RoadNetworks2(store, settings, mapping);
//                var validator = new ChangeRoadNetworkValidator();
//                while (!MailboxCancellationSource.IsCancellationRequested)
//                {
//                    var message = await Mailbox.ReceiveAsync(MailboxCancellationSource.Token);
//                    switch (message.Body)
//                    {
//                        case ChangeRoadNetwork command:
//                            try
//                            {
//                                await validator.ValidateAndThrowAsync(command,
//                                    cancellationToken: MailboxCancellationSource.Token);
//                                var network = await networks.Get(MailboxCancellationSource.Token);
//                                foreach (var change in command.Changeset)
//                                {
//                                    if (change.AddRoadNode != null)
//                                    {
//                                        network.AddRoadNode(
//                                            new RoadNodeId(change.AddRoadNode.Id),
//                                            RoadNodeType.Parse((int) change.AddRoadNode.Type),
//                                            change.AddRoadNode.Geometry);
//                                    }
//                                }
//
//                                var entity = (IEventSourcedEntity) network;
//                                var version = entity.ExpectedVersion;
//                                var events = Array.ConvertAll(
//                                    entity.TakeEvents(),
//                                    @event =>
//                                        new NewStreamMessage(
//                                            Deterministic.Create(Deterministic.Namespaces.Events,
//                                                $"{RoadNetworks2.Stream}-{version++}"),
//                                            mapping.GetEventName(@event.GetType()),
//                                            JsonConvert.SerializeObject(@event, settings)
//                                        ));
//                                await store.AppendToStream(RoadNetworks2.Stream.ToString(), entity.ExpectedVersion, events, MailboxCancellationSource.Token);
//                            }
//                            catch (Exception exception)
//                            {
//                                // help
//                            }
//                            break;
//                    }
//                }
//            }, MailboxCancellationSource.Token);
//        }
//
//        public Task Completion => Mailbox.Completion;
//
//        public void Dispose()
//        {
//            MailboxCancellationSource?.Cancel();
//            MailboxCancellationSource?.Dispose();
//            Mailbox.Complete();
//            MessagePump?.Dispose();
//        }
//    }
//
//    public class HttpCommandMessage
//    {
//        public Message Message { get; }
//
//        public HttpCommandMessage(Message message)
//        {
//            Message = message ?? throw new ArgumentNullException(nameof(message));
//        }
//
//    }
//}
