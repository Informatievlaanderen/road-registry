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
                        network.ProvidesNextRoadSegmentId(),
                        network.ProvidesNextEuropeanRoadAttributeId(),
                        network.ProvidesNextNationalRoadAttributeId(),
                        network.ProvidesNextNumberedRoadAttributeId(),
                        network.ProvidesNextLaneAttributeId(),
                        network.ProvidesNextWidthAttributeId(),
                        network.ProvidesNextSurfaceAttributeId()
                    );
                    var changeSet = translator.Translate(message.Body.Changes);
                    network.Change(changeSet);
                });
        }
    }
}
