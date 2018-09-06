namespace RoadRegistry.Model
{
    using System;
    using Commands;
    using Framework;

    public class RoadNetworkCommandHandlerModule : CommandHandlerModule<IRoadRegistryContext>
    {
        public RoadNetworkCommandHandlerModule()
        {
            For<ChangeRoadNetwork>()
                .ValidateUsing(new ChangeRoadNetworkValidator())
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