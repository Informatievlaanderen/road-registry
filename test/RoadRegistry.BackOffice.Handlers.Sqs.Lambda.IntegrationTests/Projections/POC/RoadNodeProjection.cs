namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.IntegrationTests.Projections.POC;

using System.Collections.Generic;
using System.Threading.Tasks;
using JasperFx.Events;
using Marten;
using RoadRegistry.Infrastructure.MartenDb.Projections;
using RoadRegistry.RoadNode.Events;

public class RoadNodeProjection : IRoadNetworkChangesProjection
{
    public static void Configure(StoreOptions options)
    {
        options.Schema.For<RoadNodeProjectionItem>()
            .DocumentAlias("projection_roadnodes")
            .Identity(x => x.Id);
    }

    public async Task Project(IReadOnlyList<IEvent> events, IDocumentSession session, CancellationToken cancellationToken)
    {
        foreach (var evt in events)
        {
            cancellationToken.ThrowIfCancellationRequested();

            switch (evt)
            {
                case IEvent<RoadNodeAdded> added:
                    Project(added, session);
                    break;
                // case IEvent<RoadNodeModified> modified:
                //     Project(modified, session);
                //     break;
            }
        }
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
