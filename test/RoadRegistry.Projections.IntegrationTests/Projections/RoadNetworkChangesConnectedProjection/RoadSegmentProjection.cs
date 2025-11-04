namespace RoadRegistry.Projections.IntegrationTests.Projections.RoadNetworkChangesConnectedProjection;

using System.Threading.Tasks;
using JasperFx.Events;
using Marten;
using RoadRegistry.Infrastructure.MartenDb.Projections;
using RoadRegistry.RoadSegment.Events;

public class RoadSegmentProjection : RoadNetworkChangesConnectedProjection
{
    public static void Configure(StoreOptions options)
    {
        options.Schema.For<RoadSegmentProjectionItem>()
            .DocumentAlias("projection_roadsegments")
            .Identity(x => x.Id);
    }

    public RoadSegmentProjection()
    {
        When<IEvent<RoadSegmentAdded>>((session, e, ct) =>
        {
            session.Store(new RoadSegmentProjectionItem
            {
                Id = e.Data.Id,
                GeometryDrawMethod = e.Data.GeometryDrawMethod
            });
            return Task.CompletedTask;
        });

        When<IEvent<RoadSegmentModified>>(async (session, e, ct) =>
        {
            var roadSegment = await session.LoadAsync<RoadSegmentProjectionItem>(e.Data.Id);
            if (roadSegment is null)
            {
                throw new InvalidOperationException($"No document found for Id {e.Data.Id}");
            }

            roadSegment.GeometryDrawMethod = e.Data.GeometryDrawMethod;

            session.Store(roadSegment);
        });

        When<IEvent<RoadSegmentRemoved>>(async (session, e, ct) =>
        {
            var roadSegment = await session.LoadAsync<RoadSegmentProjectionItem>(e.Data.Id);
            if (roadSegment is null)
            {
                throw new InvalidOperationException($"No document found for Id {e.Data.Id}");
            }

            session.Delete(roadSegment);
        });
    }
}

public sealed class RoadSegmentProjectionItem
{
    public int Id { get; set; }
    public string GeometryDrawMethod { get; set; }
}
