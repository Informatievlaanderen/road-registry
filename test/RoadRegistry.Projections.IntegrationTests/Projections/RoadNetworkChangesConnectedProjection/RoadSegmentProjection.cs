namespace RoadRegistry.Projections.IntegrationTests.Projections.RoadNetworkChangesConnectedProjection;

using System.Threading.Tasks;
using JasperFx.Events;
using Marten;
using Microsoft.Extensions.Logging;
using RoadRegistry.Infrastructure.MartenDb.Projections;
using RoadSegment.Events.V2;

public class RoadSegmentProjection : RoadNetworkChangesConnectedProjection
{
    public static void Configure(StoreOptions options)
    {
        options.Schema.For<RoadSegmentProjectionItem>()
            .DocumentAlias("projection_roadsegments")
            .Identity(x => x.Id);
    }

    public RoadSegmentProjection(ILogger logger)
    {
        When<IEvent<RoadSegmentWasAdded>>((session, e, ct) =>
        {
            logger.LogInformation($"{e.Data.GetType().Name} start");
            session.Store(new RoadSegmentProjectionItem
            {
                Id = e.Data.RoadSegmentId,
                GeometryDrawMethod = e.Data.GeometryDrawMethod
            });
            logger.LogInformation($"{e.Data.GetType().Name} end");
            return Task.CompletedTask;
        });

        When<IEvent<RoadSegmentWasModified>>(async (session, e, ct) =>
        {
            var roadSegment = await session.LoadAsync<RoadSegmentProjectionItem>(e.Data.RoadSegmentId);
            if (roadSegment is null)
            {
                throw new InvalidOperationException($"No document found for Id {e.Data.RoadSegmentId}");
            }

            roadSegment.GeometryDrawMethod = e.Data.GeometryDrawMethod;

            session.Store(roadSegment);
        });

        When<IEvent<RoadSegmentWasRemoved>>(async (session, e, ct) =>
        {
            logger.LogInformation($"{e.Data.GetType().Name} start");
            var roadSegment = await session.LoadAsync<RoadSegmentProjectionItem>(e.Data.RoadSegmentId);
            if (roadSegment is null)
            {
                throw new InvalidOperationException($"No document found for Id {e.Data.RoadSegmentId}");
            }

            session.Delete(roadSegment);
            logger.LogInformation($"{e.Data.GetType().Name} end");
        });
    }
}

public sealed class RoadSegmentProjectionItem
{
    public int Id { get; set; }
    public string GeometryDrawMethod { get; set; }
}
