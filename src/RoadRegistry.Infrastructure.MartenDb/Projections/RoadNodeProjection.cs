namespace RoadRegistry.Infrastructure.MartenDb.Projections;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JasperFx.Events;
using Marten;
using RoadRegistry.RoadNode.Events;

public class RoadNodeProjection : IRoadNetworkChangesProjection
{
    public static void Configure(StoreOptions options)
    {
        options.Schema.For<RoadNodeProjectionItem>()
            .DocumentAlias("projection_roadnodes")
            .Identity(x => x.Id);
    }

    public async Task Project(ICollection<IEvent> events, IDocumentSession session)
    {
        foreach (var evt in events.OfType<IEvent<RoadNodeAdded>>())
        {
            Project(evt, session);
        }

        // foreach (var evt in events.OfType<IEvent<RoadNodeModified>>())
        // {
        //     await Project(evt, session);
        // }
    }

    private void Project(IEvent<RoadNodeAdded> e, IDocumentSession session)
    {
        session.Store(new RoadNodeProjectionItem
        {
            Id = e.Data.Id,
            Type = e.Data.Type
        });
    }

    // private async Task Project(IEvent<RoadNodeModified> e, IDocumentSession session)
    // {
    //     var roadNode = await session.LoadAsync<RoadNodeProjectionItem>(e.Data.Id);
    //
    //     roadNode.Version = e.Version;
    //
    //     session.Store(roadNode);
    // }
}

public sealed class RoadNodeProjectionItem
{
    public int Id { get; set; }
    public string Type { get; set; }
}
