namespace RoadRegistry.Projections.Oslo.RoadList
{
    using Aiv.Vbr.ProjectionHandling.Connector;
    using Aiv.Vbr.ProjectionHandling.SqlStreamStore;
    using RoadRegistry.Road.Events;

    public class RoadListProjections : ConnectedProjection<OsloContext>
    {
        public RoadListProjections()
        {
            When<Envelope<RoadWasRegistered>>(async (context, message) =>
            {
                await context
                    .RoadList
                    .AddAsync(
                        new RoadListItem
                        {
                            RoadId = message.Message.RoadId
                        });
            });
        }
    }
}
