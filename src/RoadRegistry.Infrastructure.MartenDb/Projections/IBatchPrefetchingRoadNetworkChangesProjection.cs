namespace RoadRegistry.Infrastructure.MartenDb.Projections;

using JasperFx.Events;

// A sub-projection whose handlers read rows the batch could have loaded in one go. Handlers see one event at a time, so
// a read model keyed on something the events carry - a road segment, say - is read one key at a time too, and a batch
// of a hundred events becomes a hundred round trips per table, each returning a handful of rows. Against a database a
// network away that cost is latency, not work: the queries themselves are already as cheap as an index can make them.
//
// The driver offers the whole batch up front so those reads can be done once, leaving the handlers to work out of the
// change tracker. It is an optimisation and nothing more: a projection that prefetches nothing, or that is handed an
// event it cannot read a key from, must still produce exactly what it produced before - which is why the handlers keep
// their own lookups as the fallback rather than assuming the prefetch covered them.
public interface IBatchPrefetchingRoadNetworkChangesProjection<in TSession>
{
    Task PrefetchBatchAsync(TSession session, IReadOnlyList<IEvent> events, CancellationToken cancellationToken);
}
