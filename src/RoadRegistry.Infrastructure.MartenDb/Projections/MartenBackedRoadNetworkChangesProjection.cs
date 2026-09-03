namespace RoadRegistry.Infrastructure.MartenDb.Projections;

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

    // pageMaxSequence is unused here: these sub-projections write into the same Marten session as the progressions,
    // so there is no second position to keep - Marten's own transaction is the idempotency boundary.
    protected override async Task DispatchAsync(IDocumentOperations operations, IReadOnlyList<CorrelationWorkItem> correlationWork, long pageMaxSequence, CancellationToken cancellationToken)
    {
        foreach (var work in correlationWork)
        {
            foreach (var evt in work.ToProcess)
            {
                cancellationToken.ThrowIfCancellationRequested();

                using var eventScope = Logger.BeginScope(new Dictionary<string, object> { ["EventTypeName"] = evt.EventTypeName, ["EventSequence"] = evt.Sequence });

                foreach (var projection in _projections)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    projection.IsCatchingUp = IsCatchingUp;
                    projection.Logger ??= Logger;

                    using var childProjectionScope = Logger.BeginScope(new Dictionary<string, object> { ["ChildProjectionName"] = projection.GetType().Name });
                    await projection.Project(operations, [evt], cancellationToken).ConfigureAwait(false);
                }
            }
        }
    }
}
