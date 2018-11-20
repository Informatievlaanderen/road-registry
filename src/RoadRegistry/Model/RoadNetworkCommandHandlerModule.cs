namespace RoadRegistry.Model
{
    using System;
    using Framework;
    using Aiv.Vbr.Shaperon;
    using Messages;
    using SqlStreamStore;

    public class RoadNetworkCommandHandlerModule : CommandHandlerModule
    {
        public RoadNetworkCommandHandlerModule(IStreamStore store, WellKnownBinaryReader reader)
        {
            if (store == null) throw new ArgumentNullException(nameof(store));
            if (reader == null) throw new ArgumentNullException(nameof(reader));

            For<ChangeRoadNetwork>()
                .UseValidator(new ChangeRoadNetworkValidator())
                .UseRoadRegistryContext(store)
                .Handle(async (context, message, ct) =>
                {
                    var network = await context.RoadNetworks.Get(ct);
                    var changes = Array.ConvertAll(
                        message.Body.Changes,
                        item =>
                        {
                            IRequestedChange change;
                            switch (item.PickChange())
                            {
                                case Messages.AddRoadNode command:
                                    change = RequestedChangeTranslator.Translate(command);
                                    break;
                                case Messages.AddRoadSegment command:
                                    change = RequestedChangeTranslator.Translate(command);
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
