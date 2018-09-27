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
                .UseValidator(new ChangeRoadNetworkValidator())
                .UseRoadRegistryContext(store)
                .Handle(async (context, message, ct) =>
                {
                    var network = await context.RoadNetworks.Get(ct);
                    var changes = Array.ConvertAll(
                        message.Body.Changes,
                        item =>
                        {
                            IRoadNetworkChange result;
                            switch (item.PickChange())
                            {
                                case Commands.AddRoadNode change:
                                    result = translator.Translate(change);
                                    break;
                                case Commands.AddRoadSegment change:
                                    result = translator.Translate(change);
                                    break;
                                default:
                                    throw new InvalidOperationException("...");
                            }
                            return result;
                        });
                    network.Change(changes);
                });
        }
    }
}
