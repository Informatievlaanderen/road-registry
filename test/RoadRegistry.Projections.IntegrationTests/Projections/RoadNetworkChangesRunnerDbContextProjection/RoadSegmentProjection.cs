namespace RoadRegistry.Projections.IntegrationTests.Projections.RoadNetworkChangesRunnerDbContextProjection;

using System.Threading.Tasks;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
using JasperFx.Events;
using RoadRegistry.RoadSegment.Events;

public class RoadSegmentProjection : ConnectedProjection<TestDbContext>
{
    public RoadSegmentProjection()
    {
        When<IEvent<RoadSegmentAdded>>((context, e, ct) =>
        {
            context.RoadSegments.Add(new RoadSegmentRecord(e.Data.RoadSegmentId));
            return Task.CompletedTask;
        });
    }
}

