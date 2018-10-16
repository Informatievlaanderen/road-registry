namespace RoadRegistry.Model
{
    using System;
    using Commands;
    using Framework;
    using Aiv.Vbr.Shaperon;
    using SqlStreamStore;

    public class RoadNetworkCommandHandlerModule : CommandHandlerModule
    {
        public RoadNetworkCommandHandlerModule(IStreamStore store, WellKnownBinaryReader reader)
        {
            if (store == null) throw new ArgumentNullException(nameof(store));
            if (reader == null) throw new ArgumentNullException(nameof(reader));

            var translator = new ChangeRoadNetworkTranslator(reader);

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
                            IRoadNetworkChange change;
                            switch (item.PickChange())
                            {
                                case Commands.AddRoadNode command:
                                    change = translator.Translate(command);
                                    break;
                                case Commands.AddRoadSegment command:
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
