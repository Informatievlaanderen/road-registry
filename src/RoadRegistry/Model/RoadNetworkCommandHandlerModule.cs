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
                                var id = new RoadNodeId(addRoadNode.Id);
                                if (!reader.TryReadAs(addRoadNode.Geometry, out PointM point))
                                {
                                    throw new RoadNodeGeometryMismatchException(id);
                                }
                                changes[index] = new AddRoadNode
                                (
                                    id,
                                    RoadNodeType.Parse((int) addRoadNode.Type),
                                    point
                                );
                                break;
                        }
                    }
                    network.Change(changes);
                });
        }
    }
}
