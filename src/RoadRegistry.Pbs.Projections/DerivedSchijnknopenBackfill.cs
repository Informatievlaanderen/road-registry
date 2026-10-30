namespace RoadRegistry.Pbs.Projections;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Schema;
using Schema.Records;

// One-time backfill for GAWR-7836: the projection only builds the AfgeleideWegsegmenten of a road segment again when
// an event for it comes in, so the segments projected before the afgeleide schijnknopen existed have none, and the
// begin and end node of every one of their rows is still the one of the whole segment. This derives every segment
// again from what the read model already holds - the segment row and its attribute rows - exactly as the projection
// would, without replaying a single event.
//
// It is to run while RoadNetworkChangesPbsProjection is stopped: it rewrites the same rows the projection writes, and
// the two must not interleave. The caller checks that before every chunk through canContinue, so a projection started
// halfway makes the backfill stop rather than race it.
//
// Segments are taken in WS_OIDN order, a chunk at a time, each chunk committed on its own. Running it again is
// harmless: a segment that already has its schijnknopen gets them back with the same ids. That also makes it
// resumable from the last WS_OIDN it logged.
public static class DerivedSchijnknopenBackfill
{
    public static async Task<int> RunAsync(
        IDbContextFactory<PbsContext> dbContextFactory,
        ILogger logger,
        Func<bool> canContinue,
        int chunkSize,
        int fromSegmentId,
        CancellationToken ct)
    {
        var lastSegmentId = fromSegmentId - 1;
        var segmentsDone = 0;

        while (true)
        {
            ct.ThrowIfCancellationRequested();
            if (!canContinue())
            {
                logger.LogWarning("Backfill of the afgeleide schijnknopen stopped after WS_OIDN {LastSegmentId} ({Count} segment(s)): RoadNetworkChangesPbsProjection is running again. Stop it and resume from WS_OIDN {Next}.",
                    lastSegmentId, segmentsDone, lastSegmentId + 1);
                return segmentsDone;
            }

            var chunk = await BackfillChunk(dbContextFactory, lastSegmentId, chunkSize, ct);
            if (chunk is null)
            {
                break;
            }

            lastSegmentId = chunk.Value.LastSegmentId;
            segmentsDone += chunk.Value.Count;
            logger.LogInformation("Backfill of the afgeleide schijnknopen reached WS_OIDN {LastSegmentId} ({Count} segment(s) so far).", lastSegmentId, segmentsDone);
        }

        logger.LogInformation("Backfill of the afgeleide schijnknopen completed: {Count} segment(s) derived again.", segmentsDone);
        return segmentsDone;
    }

    private static async Task<(int LastSegmentId, int Count)?> BackfillChunk(IDbContextFactory<PbsContext> dbContextFactory, int afterSegmentId, int chunkSize, CancellationToken ct)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync(ct);
        context.ChangeTracker.AutoDetectChangesEnabled = false;

        var segments = await context.RoadSegments.AsNoTracking()
            .Where(x => x.WS_OIDN > afterSegmentId)
            .OrderBy(x => x.WS_OIDN)
            .Take(chunkSize)
            .ToListAsync(ct);
        if (segments.Count == 0)
        {
            return null;
        }

        var from = segments[0].WS_OIDN;
        var to = segments[^1].WS_OIDN;

        // Every row of the chunk in one query per table, rather than the per-segment queries the projection does.
        var morphology = await Load(context.RoadSegmentMorphologyAttributes, x => x.WS_OIDN, from, to, ct);
        var category = await Load(context.RoadSegmentCategoryAttributes, x => x.WS_OIDN, from, to, ct);
        var access = await Load(context.RoadSegmentAccessRestrictionAttributes, x => x.WS_OIDN, from, to, ct);
        var surface = await Load(context.RoadSegmentSurfaceTypeAttributes, x => x.WS_OIDN, from, to, ct);
        var streetName = await Load(context.RoadSegmentStreetNameAttributes, x => x.WS_OIDN, from, to, ct);
        var maintainer = await Load(context.RoadSegmentMaintenanceAuthorityAttributes, x => x.WS_OIDN, from, to, ct);
        var car = await Load(context.RoadSegmentCarTrafficDirectionAttributes, x => x.WS_OIDN, from, to, ct);
        var bike = await Load(context.RoadSegmentBikeTrafficDirectionAttributes, x => x.WS_OIDN, from, to, ct);
        var pedestrian = await Load(context.RoadSegmentPedestrianTrafficDirectionAttributes, x => x.WS_OIDN, from, to, ct);

        context.DerivedRoadSegments.RemoveRange(await context.DerivedRoadSegments.Where(x => x.WS_OIDN >= from && x.WS_OIDN <= to).ToListAsync(ct));
        var schijnknopen = (await context.DerivedSchijnknopen.Where(x => x.WS_OIDN >= from && x.WS_OIDN <= to).ToListAsync(ct))
            .OrderBy(x => x.VOLGNR)
            .ToLookup(x => x.WS_OIDN);

        var derived = segments
            .Select(normal => (Normal: normal,
                Rows: RoadSegmentPbsProjection.FlattenDerived(normal,
                    morphology[normal.WS_OIDN].ToList(), category[normal.WS_OIDN].ToList(), access[normal.WS_OIDN].ToList(),
                    surface[normal.WS_OIDN].ToList(), streetName[normal.WS_OIDN].ToList(), maintainer[normal.WS_OIDN].ToList(),
                    car[normal.WS_OIDN].ToList(), bike[normal.WS_OIDN].ToList(), pedestrian[normal.WS_OIDN].ToList()),
                Existing: schijnknopen[normal.WS_OIDN].ToList()))
            .ToList();

        // The new ids of the whole chunk in one range.
        var newIds = new Queue<int>(await DerivedSchijnknoopIds.AllocateAsync(context,
            derived.Sum(x => Math.Max(RoadSegmentPbsProjection.SchijnknopenNeeded(x.Rows) - x.Existing.Count, 0)), highestStored: null, ct));

        foreach (var (normal, rows, existing) in derived)
        {
            var take = Math.Max(RoadSegmentPbsProjection.SchijnknopenNeeded(rows) - existing.Count, 0);
            var ids = Enumerable.Range(0, take).Select(_ => newIds.Dequeue()).ToList();

            RoadSegmentPbsProjection.LinkSchijnknopen(context, normal, rows, existing, ids);
            context.DerivedRoadSegments.AddRange(rows);
        }

        context.ChangeTracker.DetectChanges();
        await context.SaveChangesAsync(ct);

        return (to, segments.Count);
    }

    private static async Task<ILookup<int, T>> Load<T>(DbSet<T> set, Func<T, int> segmentId, int from, int to, CancellationToken ct)
        where T : class
    {
        // The attribute records all carry WS_OIDN, but through no shared interface; EF.Property reaches the column.
        var rows = await set.AsNoTracking()
            .Where(x => EF.Property<int>(x, "WS_OIDN") >= from && EF.Property<int>(x, "WS_OIDN") <= to)
            .ToListAsync(ct);
        return rows.ToLookup(segmentId);
    }
}
