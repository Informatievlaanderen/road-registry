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

            var translator = new RequestedChangeTranslator(reader);

            For<ChangeRoadNetwork>()
                .UseValidator(new ChangeRoadNetworkValidator(reader))
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
