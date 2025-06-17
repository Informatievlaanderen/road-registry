namespace MartenPoc.Projections;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JasperFx.Events;
using Marten;

public class RoadNodeProjection : IRoadNetworkChangeProjection
{
    public async Task Project(ICollection<IEvent> events, IDocumentSession session)
    {
        foreach (var evt in events.OfType<IEvent<WegknoopWerdToegevoegd>>())
        {
            await Project(evt, session);
        }

        foreach (var evt in events.OfType<IEvent<WegknoopWerdGewijzigd>>())
        {
            await Project(evt, session);
        }
    }

    private async Task Project(IEvent<WegknoopWerdToegevoegd> e, IDocumentSession session)
    {
        session.Store(new RoadNode
        {
            Id = e.Data.Id,
            Version = e.Version
        });
    }

    private async Task Project(IEvent<WegknoopWerdGewijzigd> e, IDocumentSession session)
    {
        var roadNode = await session.LoadAsync<RoadNode>(e.Data.Id);

        roadNode.Version = e.Version;

        session.Store(roadNode);
    }
}

public sealed class RoadNode
{
    public Guid Id { get; set; }
    public long Version { get; set; }
}
