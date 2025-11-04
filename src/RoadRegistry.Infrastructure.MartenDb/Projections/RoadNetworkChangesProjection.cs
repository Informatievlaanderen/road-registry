namespace RoadRegistry.Infrastructure.MartenDb.Projections;

using JasperFx.Events;
using Marten;
using Marten.Events.Projections;
using RoadNetwork.Events;

public class RoadNetworkChangeProjection : EventProjection
{
    private readonly IReadOnlyCollection<IRoadNetworkChangesProjection> _projections;

    public RoadNetworkChangeProjection(IReadOnlyCollection<IRoadNetworkChangesProjection> projections)
    {
        _projections = projections;
        IncludeType<RoadNetworkChanged>();
    }

    public async Task Project(IEvent<RoadNetworkChanged> e, IDocumentOperations operations, CancellationToken cancellation)
    {
        var causationId = e.Data.CausationId;

        await using var session = operations.DocumentStore.LightweightSession();

        var processEvents = operations.Events.QueryAllRawEvents()
            .Where(x => x.CausationId == causationId) //TODO-pr add index on causationId
            .ToList()
            .AsReadOnly();

        foreach (var projection in _projections)
        {
            await projection.Project(processEvents, session, cancellation);
        }

        await session.SaveChangesAsync(cancellation);
    }
}

