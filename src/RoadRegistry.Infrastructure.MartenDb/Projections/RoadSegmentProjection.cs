namespace RoadRegistry.Infrastructure.MartenDb.Projections;

using System.Collections.Generic;
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

    public async Task Project(IReadOnlyList<IEvent> events, IDocumentSession session, CancellationToken cancellationToken)
    {
        foreach (var evt in events)
        {
            cancellationToken.ThrowIfCancellationRequested();

            switch (evt)
            {
                case IEvent<RoadSegmentAdded> added:
                    Project(added, session);
                    break;
                case IEvent<RoadSegmentModified> modified:
                    await Project(modified, session);
                    break;
                case IEvent<RoadSegmentRemoved> removed:
                    await Project(removed, session);
                    break;
            }
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
