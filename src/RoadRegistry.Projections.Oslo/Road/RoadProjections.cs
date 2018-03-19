namespace RoadRegistry.Projections.Oslo.Road
{
    using Aiv.Vbr.ProjectionHandling.Connector;
    using Aiv.Vbr.ProjectionHandling.SqlStreamStore;
    using RoadRegistry.Road.Events;

    public class RoadProjections : ConnectedProjection<OsloContext>
    {
        public RoadProjections()
        {
            When<Envelope<RoadWasRegistered>>(async (context, message) =>
            {
                await context
                    .Roads
                    .AddAsync(
                        new Road
                        {
                            RoadId = message.Message.RoadId
                        });
            });
        }
    }
}
