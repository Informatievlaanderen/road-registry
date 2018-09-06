namespace RoadRegistry.Model
{
    using System;
    using Commands;
    using Framework;
    using SqlStreamStore;

    public class RoadNetworkCommandHandlerModule : CommandHandlerModule
    {
        public RoadNetworkCommandHandlerModule(IStreamStore store)
        {
            if (store == null) throw new ArgumentNullException(nameof(store));

            For<ChangeRoadNetwork>()
                .UseValidator(new ChangeRoadNetworkValidator())
                .UseRoadRegistryContext(store)
                .Handle(async (context, message, ct) =>
                {
                    var network = await context.RoadNetworks.Get(ct);
                    foreach (var change in message.Body.Changeset)
                    {
                        if (change.AddRoadNode != null)
                        {
                            network.AddRoadNode(
                                new RoadNodeId(change.AddRoadNode.Id),
                                RoadNodeType.Parse((int)change.AddRoadNode.Type),
                                change.AddRoadNode.Geometry);
                        }
                    }
                });
        }
    }
}
