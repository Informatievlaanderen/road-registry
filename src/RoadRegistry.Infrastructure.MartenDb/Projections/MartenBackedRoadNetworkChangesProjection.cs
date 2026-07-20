namespace RoadRegistry.Infrastructure.MartenDb.Projections;

using JasperFx.Events;
using Marten;
using Microsoft.Extensions.Logging;

// Drives a set of Marten-backed sub-projections: each event's handlers write directly into the same IDocumentOperations
// that Marten's async daemon hands to ApplyAsync. Marten owns the read-side progression for these documents, so there is
// no extra projection-state bookkeeping here.
public abstract class MartenBackedRoadNetworkChangesProjection : RoadNetworkChangesProjection
{
    private readonly IReadOnlyCollection<IRoadNetworkChangesProjection<IDocumentOperations>> _projections;

    protected MartenBackedRoadNetworkChangesProjection(
        IReadOnlyCollection<IRoadNetworkChangesProjection<IDocumentOperations>> projections,
        ILoggerFactory loggerFactory,
        int batchSize = DefaultBatchSize)
        : base(loggerFactory, batchSize)
    {
        _projections = projections;
    }

    protected override async Task DispatchAsync(IDocumentOperations operations, IReadOnlyList<CorrelationWorkItem> correlationWork, CancellationToken cancellationToken)
    {
        foreach (var work in correlationWork)
        {
            foreach (var evt in work.ToProcess)
            {
                Logger.LogInformation("Processing event {Sequence}: {EventTypeName}", evt.Sequence, evt.EventTypeName);

                foreach (var projection in _projections)
                {
                    projection.IsCatchingUp = IsCatchingUp;

                    await projection.Project(operations, [evt], cancellationToken).ConfigureAwait(false);
                }
            }
        }
    }
}
