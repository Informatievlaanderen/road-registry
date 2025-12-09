namespace RoadRegistry.Infrastructure.MartenDb.Projections;

using GradeSeparatedJunction.Events.V1;
using JasperFx.Events;
using Marten;
using Marten.Events.Projections;
using RoadNetwork.Events.V1;
using RoadNetwork.Events.V2;
using RoadNode.Events.V1;
using RoadSegment.Events.V1;

public class RoadNetworkChangeProjection : EventProjection
{
    private readonly IReadOnlyCollection<IRoadNetworkChangesProjection> _projections;

    public RoadNetworkChangeProjection(IReadOnlyCollection<IRoadNetworkChangesProjection> projections)
    {
        _projections = projections;

        // V1
        IncludeType<ImportedRoadNode>();
        IncludeType<ImportedRoadSegment>();
        IncludeType<ImportedGradeSeparatedJunction>();
        IncludeType<RoadNetworkChangesAccepted>();

        // V2
        IncludeType<RoadNetworkChanged>();
    }

    public Task Project(IEvent<ImportedRoadNode> e, IDocumentOperations operations, CancellationToken cancellation)
    {
        return ProjectCorrelatedEvents(e, operations, cancellation);
    }
    public Task Project(IEvent<ImportedRoadSegment> e, IDocumentOperations operations, CancellationToken cancellation)
    {
        return ProjectCorrelatedEvents(e, operations, cancellation);
    }
    public Task Project(IEvent<ImportedGradeSeparatedJunction> e, IDocumentOperations operations, CancellationToken cancellation)
    {
        return ProjectCorrelatedEvents(e, operations, cancellation);
    }
    public Task Project(IEvent<RoadNetworkChangesAccepted> e, IDocumentOperations operations, CancellationToken cancellation)
    {
        return ProjectCorrelatedEvents(e, operations, cancellation);
    }

    public Task Project(IEvent<RoadNetworkChanged> e, IDocumentOperations operations, CancellationToken cancellation)
    {
        return ProjectCorrelatedEvents(e, operations, cancellation);
    }

    private async Task ProjectCorrelatedEvents(IEvent e, IDocumentOperations operations, CancellationToken cancellation)
    {
        var correlationId = e.CorrelationId!;

        await using var session = operations.DocumentStore.LightweightSession();

        var processEvents = operations.Events.QueryAllRawEvents()
            .Where(x => x.CorrelationId == correlationId) //TODO-pr add index on correlationId
            .ToList()
            .AsReadOnly();

        foreach (var projection in _projections)
        {
            await projection.Project(processEvents, session, cancellation);
        }

        await session.SaveChangesAsync(cancellation);
    }
}

