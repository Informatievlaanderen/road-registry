namespace RoadRegistry.Infrastructure.MartenDb.Projections;

// Tuning for the DbContext-backed projections while they are catching up (a rebuild, or a long replay after downtime).
// None of this changes what the projections write - only how those writes reach SQL Server, and only while the read
// model is incomplete anyway. Once caught up every projection is back on the ordinary EF path.
public sealed class ProjectionCatchUpOptions
{
    // Entity types with at least this many pending inserts in a single batch are written with SqlBulkCopy instead of
    // EF's row-by-row INSERT batching. Below the threshold the bulk-copy setup costs more than it saves, so the EF
    // path stays in use - which also keeps low-volume live operation on the well-trodden code path.
    public int BulkInsertThreshold { get; init; } = 200;

    // Disable the non-clustered indexes on the projection's own tables while catching up, and rebuild them once it is
    // caught up. A rebuild is almost entirely inserts, and every non-clustered index is a second write per row.
    // While disabled the indexes are not maintained and the optimizer ignores them, so queries against a
    // mid-rebuild read model fall back to scans - acceptable precisely because that read model is incomplete.
    public bool DisableIndexesWhileCatchingUp { get; init; } = true;

    // ...but only when at least this many events are still to come. Rebuilding the indexes afterwards is itself heavy
    // work on a table of millions of rows, so it has to be earned: a projection that is a few thousand events behind
    // after a deploy would spend far longer tearing its indexes down and putting them back than it saves. Only a
    // backlog on the scale of a real rebuild is worth it.
    public long MinimumBacklogForIndexDisabling { get; init; } = 100_000;

    // Emit a throughput line every N batches, so a rebuild can be measured rather than guessed at.
    public int MetricsLogIntervalInBatches { get; init; } = 25;

    // ...and at least this often, however few batches have gone by. How long N batches take is a property of the
    // rebuild being measured, not of the measurement: a projection that reads nothing per event gets through them in
    // minutes, one that scans its own tables takes hours - and that second case is exactly the one this line exists
    // to diagnose, so it is the one where it must not stay silent. Counting minutes as well puts a ceiling on how
    // late the first reading can arrive.
    public int MetricsLogIntervalInMinutes { get; init; } = 5;

    // Let a sub-projection read the whole batch before the handlers run, so rows the handlers would each fetch for
    // themselves are fetched once. Projections that do not offer to do this ignore it.
    //
    // Switchable because it changes where a projection reads from, not just how fast: a prefetched segment is served
    // out of the change tracker rather than queried per event. That is meant to be invisible, and the tests say it
    // is, but "meant to be invisible" is exactly the kind of claim worth being able to withdraw without a rollback -
    // and turning it off is also how the same range of events can be replayed both ways and the results compared.
    public bool PrefetchBatches { get; init; } = true;

    public static ProjectionCatchUpOptions Default { get; } = new();
}
