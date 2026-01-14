namespace RoadRegistry.Infrastructure.MartenDb.Projections;

using JasperFx.Events;
using Marten;
using Marten.Events.Projections;

public abstract class RoadNetworkChangesProjection : IProjection
{
    private readonly IReadOnlyCollection<IRoadNetworkChangesProjection> _projections;

    protected RoadNetworkChangesProjection(IReadOnlyCollection<IRoadNetworkChangesProjection> projections)
    {
        _projections = projections;
    }

    public virtual void Configure(StoreOptions options)
    {
    }

    public async Task ApplyAsync(IDocumentOperations operations, IReadOnlyList<IEvent> events, CancellationToken cancellation)
    {
        foreach (var projection in _projections)
        {
            await projection.Project(events, operations, cancellation).ConfigureAwait(false);
        }
    }
}
