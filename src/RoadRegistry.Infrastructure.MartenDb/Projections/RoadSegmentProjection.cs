namespace RoadRegistry.Infrastructure.MartenDb.Projections;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JasperFx.Events;
using Marten;
using RoadSegment.Events;

public class RoadSegmentProjection : IRoadNetworkChangesProjection
{
    public static void Configure(StoreOptions options)
    {
        options.Schema.For<RoadSegmentProjectionItem>()
            .DocumentAlias("projection_roadsegments")
            .Identity(x => x.Id);
    }

    public async Task Project(ICollection<IEvent> events, IDocumentSession session)
    {
        foreach (var evt in events.OfType<IEvent<RoadSegmentAdded>>())
        {
            Project(evt, session);
        }

        foreach (var evt in events.OfType<IEvent<RoadSegmentModified>>())
        {
            await Project(evt, session);
        }

        foreach (var evt in events.OfType<IEvent<RoadSegmentRemoved>>())
        {
            await Project(evt, session);
        }
    }

    private void Project(IEvent<RoadSegmentAdded> e, IDocumentSession session)
    {
        session.Store(new RoadSegmentProjectionItem
        {
            Id = e.Data.Id,
            GeometryDrawMethod = e.Data.GeometryDrawMethod
        });
    }

    private async Task Project(IEvent<RoadSegmentModified> e, IDocumentSession session)
    {
        var roadSegment = await session.LoadAsync<RoadSegmentProjectionItem>(e.Data.Id);

        roadSegment!.GeometryDrawMethod = e.Data.GeometryDrawMethod;

        session.Store(roadSegment);
    }

    private async Task Project(IEvent<RoadSegmentRemoved> e, IDocumentSession session)
    {
        var roadSegment = await session.LoadAsync<RoadSegmentProjectionItem>(e.Data.Id);

        session.Delete(roadSegment!);
    }
}

public sealed class RoadSegmentProjectionItem
{
    public int Id { get; set; }
    public string GeometryDrawMethod { get; set; }
}
