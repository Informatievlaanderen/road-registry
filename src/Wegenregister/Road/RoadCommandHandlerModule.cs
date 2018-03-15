namespace Wegenregister.Road
{
    using System;
    using System.Threading.Tasks;
    using Aiv.Vbr.CommandHandling;
    using Commands;

    public class RoadCommandHandlerModule : CommandHandlerModule
    {
        public RoadCommandHandlerModule(
            Func<IRoads> getRoads,
            ReturnHandler<CommandMessage> finalHandler = null) : base(finalHandler)
        {
            For<RegisterRoad>()
                .Handle((message, ct) =>
                {
                    var roadId = message.Command.RoadId;
                    var road = Road.Register(roadId);

                    var roads = getRoads();
                    roads.Add(roadId, road);

                    return Task.CompletedTask;
                });
        }
    }
}
