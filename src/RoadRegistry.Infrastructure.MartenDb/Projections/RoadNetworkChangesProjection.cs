namespace RoadRegistry.Infrastructure.MartenDb.Projections;

using GradeSeparatedJunction.Events.V1;
using JasperFx.Events;
using Marten;
using Marten.Events.Projections;
using Microsoft.Extensions.Logging;
using RoadNetwork.Events.V1;
using RoadNetwork.Events.V2;
using RoadNode.Events.V1;
using RoadSegment.Events.V1;

public abstract class RoadNetworkChangesProjection : EventProjection
{
    private readonly IReadOnlyCollection<IRoadNetworkChangesProjection> _projections;
    private readonly ILogger? _logger;

    protected RoadNetworkChangesProjection(IReadOnlyCollection<IRoadNetworkChangesProjection> projections, ILogger? logger = null)
    {
        _projections = projections;
        _logger = logger;

        // V1
        IncludeType<ImportedRoadNode>();
        IncludeType<ImportedRoadSegment>();
        IncludeType<ImportedGradeSeparatedJunction>();
        IncludeType<RoadNetworkChangesAccepted>();

        // V2
        IncludeType<RoadNetworkWasChanged>();
    }

    public virtual void Configure(StoreOptions options)
    {
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

    public Task Project(IEvent<RoadNetworkWasChanged> e, IDocumentOperations operations, CancellationToken cancellation)
    {
        return ProjectCorrelatedEvents(e, operations, cancellation);
    }

    private async Task ProjectCorrelatedEvents(IEvent e, IDocumentOperations operations, CancellationToken cancellation)
    {
        var correlationId = e.CorrelationId!;

        var processEvents = operations.Events.QueryAllRawEvents()
            .Where(x => x.CorrelationId == correlationId) //TODO-pr add index on correlationId
            .ToList()
            .AsReadOnly();

        foreach (var projection in _projections)
        {
            await projection.Project(processEvents, operations, cancellation);
        }
    }
}

