using Aiv.Vbr.ProjectionHandling.Connector;
using RoadRegistry.Events;

namespace RoadRegistry.Projections
{
    public class RoadNodeRecordProjection : ConnectedProjection<ShapeContext>
    {
        public RoadNodeRecordProjection()
        {
            When<ImportedRoadNode>((context, @event, token) =>
            {
                return context.AddAsync(new RoadNodeRecord
                {
                    Id = @event.Id,
                    //...
                }, token);
            });
        }
    }
}