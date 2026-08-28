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

    public static ProjectionCatchUpOptions Default { get; } = new();
}
