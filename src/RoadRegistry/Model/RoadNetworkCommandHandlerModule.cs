namespace RoadRegistry.Model
{
    using System;
    using Framework;
    using Messages;
    using NodaTime;
    using SqlStreamStore;

    public class RoadNetworkCommandHandlerModule : CommandHandlerModule
    {
        public RoadNetworkCommandHandlerModule(IStreamStore store, IClock clock)
        {
            if (store == null) throw new ArgumentNullException(nameof(store));
            if (clock == null) throw new ArgumentNullException(nameof(clock));

            For<ChangeRoadNetwork>()
                .UseValidator(new ChangeRoadNetworkValidator())
                .UseRoadRegistryContext(store)
                .Handle(async (context, message, ct) =>
                {
                    var network = await context.RoadNetworks.Get(ct);
                    var translator = new RequestedChangeTranslator(
                        network.ProvidesNextRoadNodeId(),
                        network.ProvidesNextRoadSegmentId()
                    );
                    //var changes = translator.Translate(message.Body.Changes); this way we can control the order
                    var changes = Array.ConvertAll(
                        message.Body.Changes,
                        item =>
                        {
                            IRequestedChange change;
                            switch (item.PickChange())
                            {
                                case Messages.AddRoadNode command:
                                    change = translator.Translate(command);
                                    break;
                                case Messages.AddRoadSegment command:
                                    change = translator.Translate(command);
                                    break;
                                default:
                                    throw new InvalidOperationException("...");
                            }
                            return change;
                        });
                    network.Change(changes);
                });
        }
    }
}
