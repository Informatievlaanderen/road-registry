namespace RoadRegistry.Pbs.Projections;

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using JasperFx.Events;
using RoadRegistry.GradeSeparatedJunction.Events.V1;
using RoadRegistry.GradeSeparatedJunction.Events.V2;
using RoadSegmentV1 = RoadRegistry.RoadSegment.Events.V1;
using RoadSegmentV2 = RoadRegistry.RoadSegment.Events.V2;
using RoadRegistry.Infrastructure.MartenDb.Projections;
using Schema;
using Schema.Records;

public class GradeSeparatedJunctionPbsProjection : RunnerDbContextRoadNetworkChangesProjection<PbsContext>
{
    public GradeSeparatedJunctionPbsProjection(IDbContextFactory<PbsContext> dbContextFactory, ILoggerFactory loggerFactory = null)
        : base(dbContextFactory, loggerFactory)
    {
        // V1
        When<IEvent<ImportedGradeSeparatedJunction>>((context, e, ct) =>
            WriteV1(context, e.Data.Id, e.Data.LowerRoadSegmentId, e.Data.UpperRoadSegmentId, e.Data.Type, e.Data.Provenance, ct));

        When<IEvent<GradeSeparatedJunctionAdded>>((context, e, ct) =>
            WriteV1(context, e.Data.Id, e.Data.LowerRoadSegmentId, e.Data.UpperRoadSegmentId, e.Data.Type, e.Data.Provenance, ct));

        When<IEvent<GradeSeparatedJunctionModified>>((context, e, ct) =>
            WriteV1(context, e.Data.Id, e.Data.LowerRoadSegmentId, e.Data.UpperRoadSegmentId, e.Data.Type, e.Data.Provenance, ct));

        When<IEvent<GradeSeparatedJunctionRemoved>>(async (context, e, ct) =>
        {
            var record = await context.GradeSeparatedJunctions.FindAsync([e.Data.Id], ct);
            if (record is not null)
            {
                context.GradeSeparatedJunctions.Remove(record);
            }
        });

        // V2
        When<IEvent<GradeSeparatedJunctionWasAdded>>(async (context, e, ct) =>
        {
            var record = await context.GradeSeparatedJunctions.FindAsync([e.Data.GradeSeparatedJunctionId.ToInt32()], ct);
            var isNew = record is null;
            record ??= new GradeSeparatedJunctionRecord { OK_OIDN = e.Data.GradeSeparatedJunctionId.ToInt32(), CREATIE = e.Data.Provenance.ToPbsDate() };
            record.ON_WS_OIDN = e.Data.LowerRoadSegmentId.ToInt32();
            record.BO_WS_OIDN = e.Data.UpperRoadSegmentId.ToInt32();
            record.TYPE = e.Data.Type.Translation.Identifier;
            record.LBLTYPE = e.Data.Type.Translation.Name;
            record.GEOMETRIE = await JunctionGeometry.FirstIntersection(context, record.ON_WS_OIDN, record.BO_WS_OIDN, ct);
            record.VERSIE = e.Data.Provenance.ToPbsDate();
            if (isNew)
            {
                context.GradeSeparatedJunctions.Add(record);
            }
        });

        When<IEvent<GradeSeparatedJunctionWasModified>>(async (context, e, ct) =>
        {
            var record = await context.GradeSeparatedJunctions.FindAsync([e.Data.GradeSeparatedJunctionId.ToInt32()], ct);
            if (record is null)
            {
                return;
            }
            if (e.Data.LowerRoadSegmentId is not null)
            {
                record.ON_WS_OIDN = e.Data.LowerRoadSegmentId.Value.ToInt32();
            }
            if (e.Data.UpperRoadSegmentId is not null)
            {
                record.BO_WS_OIDN = e.Data.UpperRoadSegmentId.Value.ToInt32();
            }
            if (e.Data.Type is not null)
            {
                record.TYPE = e.Data.Type.Translation.Identifier;
                record.LBLTYPE = e.Data.Type.Translation.Name;
            }
            // A modify may change either linked segment, so the intersection point is recomputed from the current pair.
            record.GEOMETRIE = await JunctionGeometry.FirstIntersection(context, record.ON_WS_OIDN, record.BO_WS_OIDN, ct);
            record.VERSIE = e.Data.Provenance.ToPbsDate();
        });

        When<IEvent<GradeSeparatedJunctionWasRemoved>>(async (context, e, ct) =>
        {
            var record = await context.GradeSeparatedJunctions.FindAsync([e.Data.GradeSeparatedJunctionId.ToInt32()], ct);
            if (record is not null)
            {
                context.GradeSeparatedJunctions.Remove(record);
            }
        });
        When<IEvent<GradeSeparatedJunctionWasRemovedBecauseOfMigration>>(async (context, e, ct) =>
        {
            var record = await context.GradeSeparatedJunctions.FindAsync([e.Data.GradeSeparatedJunctionId.ToInt32()], ct);
            if (record is not null)
            {
                context.GradeSeparatedJunctions.Remove(record);
            }
        });

        // The point geometry is the intersection of the two linked segments, so when a segment's geometry changes the
        // intersection moves. RoadSegmentPbsProjection has already stored the new segment geometry by the time these run,
        // so any junction referencing the changed segment is recomputed from the current pair.
        When<IEvent<RoadSegmentV1.RoadSegmentModified>>((context, e, ct) => RecalculateForSegment(context, e.Data.RoadSegmentId, ct));
        When<IEvent<RoadSegmentV1.RoadSegmentGeometryModified>>((context, e, ct) => RecalculateForSegment(context, e.Data.RoadSegmentId, ct));
        When<IEvent<RoadSegmentV2.RoadSegmentWasModified>>((context, e, ct) => RecalculateForSegment(context, e.Data.RoadSegmentId.ToInt32(), ct));
        When<IEvent<RoadSegmentV2.RoadSegmentGeometryWasModified>>((context, e, ct) => RecalculateForSegment(context, e.Data.RoadSegmentId.ToInt32(), ct));
        When<IEvent<RoadSegmentV2.RoadSegmentWasMerged>>((context, e, ct) => RecalculateForSegment(context, e.Data.RoadSegmentId.ToInt32(), ct));
        When<IEvent<RoadSegmentV2.RoadSegmentWasSplit>>((context, e, ct) => RecalculateForSegment(context, e.Data.RoadSegmentId.ToInt32(), ct));
        When<IEvent<RoadSegmentV2.RoadSegmentWasMigrated>>((context, e, ct) => RecalculateForSegment(context, e.Data.RoadSegmentId.ToInt32(), ct));
    }

    private static async Task RecalculateForSegment(PbsContext context, int roadSegmentId, CancellationToken ct)
    {
        var junctions = await context.GradeSeparatedJunctions
            .Where(x => x.ON_WS_OIDN == roadSegmentId || x.BO_WS_OIDN == roadSegmentId)
            .ToListAsync(ct);
        foreach (var junction in junctions)
        {
            junction.GEOMETRIE = await JunctionGeometry.FirstIntersection(context, junction.ON_WS_OIDN, junction.BO_WS_OIDN, ct);
        }
    }

    // V1 grade separated junction: upsert lower/upper segment, type (mapped V1 -> V2, null when unmapped) and the point
    // geometry (first intersection of the two segments).
    private static async Task WriteV1(PbsContext context, int id, int lowerRoadSegmentId, int upperRoadSegmentId, string type, ProvenanceData provenance, CancellationToken ct)
    {
        var record = await context.GradeSeparatedJunctions.FindAsync([id], ct);
        var isNew = record is null;
        record ??= new GradeSeparatedJunctionRecord { OK_OIDN = id, CREATIE = provenance.ToPbsDate() };
        record.ON_WS_OIDN = lowerRoadSegmentId;
        record.BO_WS_OIDN = upperRoadSegmentId;
        var v2Type = V1ToV2.GradeSeparatedJunctionType(type);
        record.TYPE = v2Type?.Translation.Identifier;
        record.LBLTYPE = v2Type?.Translation.Name;
        record.GEOMETRIE = await JunctionGeometry.FirstIntersection(context, record.ON_WS_OIDN, record.BO_WS_OIDN, ct);
        record.VERSIE = provenance.ToPbsDate();
        if (isNew)
        {
            context.GradeSeparatedJunctions.Add(record);
        }
    }
}
