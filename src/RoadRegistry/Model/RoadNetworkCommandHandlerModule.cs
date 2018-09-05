namespace RoadRegistry.Model
{
    using System;
    using Commands;
    using Framework;

    public class RoadNetworkCommandHandlerModule : CommandHandlerModule
    {
        public RoadNetworkCommandHandlerModule(IRoadNetworks networks)
        {
            if (networks == null) throw new ArgumentNullException(nameof(networks));

            For<ChangeRoadNetwork>()
                .ValidateUsing(new ChangeRoadNetworkValidator())
                .Handle(async (message, ct) =>
                {
                    var network = await networks.Get(ct);
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