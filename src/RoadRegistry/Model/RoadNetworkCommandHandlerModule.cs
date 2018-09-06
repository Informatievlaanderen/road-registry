namespace RoadRegistry.Model
{
    using System;
    using System.Collections.Generic;
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
                    var changes = new IRoadNetworkChange[message.Body.Changeset.Length];
                    for (var index = 0; index < message.Body.Changeset.Length; index++)
                    {
                        switch (message.Body.Changeset[index].PickChange())
                        {
                            case Commands.AddRoadNode addRoadNode:
                                changes[index] = new AddRoadNode
                                (
                                    new RoadNodeId(addRoadNode.Id),
                                    RoadNodeType.Parse((int) addRoadNode.Type),
                                    addRoadNode.Geometry
                                );
                                break;
                        }
                    }
                    network.Change(changes);
                });
        }
    }
}
