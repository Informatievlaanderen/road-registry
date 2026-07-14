namespace RoadRegistry.Pbs.Projections;

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using JasperFx.Events;
using RoadRegistry.GradeJunction.Events.V2;
using RoadSegmentV1 = RoadRegistry.RoadSegment.Events.V1;
using RoadSegmentV2 = RoadRegistry.RoadSegment.Events.V2;
using RoadRegistry.Infrastructure.MartenDb.Projections;
using Schema;
using Schema.Records;

public class GradeJunctionPbsProjection : RunnerDbContextRoadNetworkChangesProjection<PbsContext>
{
    public GradeJunctionPbsProjection(IDbContextFactory<PbsContext> dbContextFactory, ILoggerFactory loggerFactory = null)
        : base(dbContextFactory, loggerFactory)
    {
        When<IEvent<GradeJunctionWasAdded>>(async (context, e, ct) =>
        {
            var record = await context.GradeJunctions.FindAsync([e.Data.GradeJunctionId.ToInt32()], ct);
            var isNew = record is null;
            record ??= new GradeJunctionRecord { GK_OIDN = e.Data.GradeJunctionId.ToInt32(), CREATIE = e.Data.Provenance.ToPbsDate() };
            record.WS1_OIDN = e.Data.RoadSegmentId1.ToInt32();
            record.WS2_OIDN = e.Data.RoadSegmentId2.ToInt32();
            record.GEOMETRIE = await JunctionGeometry.FirstIntersection(context, record.WS1_OIDN, record.WS2_OIDN, ct);
            record.VERSIE = e.Data.Provenance.ToPbsDate();
            if (isNew)
            {
                context.GradeJunctions.Add(record);
            }
        });

        When<IEvent<GradeJunctionWasRemoved>>(async (context, e, ct) =>
        {
            var record = await context.GradeJunctions.FindAsync([e.Data.GradeJunctionId.ToInt32()], ct);
            if (record is not null)
            {
                context.GradeJunctions.Remove(record);
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
        var junctions = await context.GradeJunctions
            .Where(x => x.WS1_OIDN == roadSegmentId || x.WS2_OIDN == roadSegmentId)
            .ToListAsync(ct);
        foreach (var junction in junctions)
        {
            junction.GEOMETRIE = await JunctionGeometry.FirstIntersection(context, junction.WS1_OIDN, junction.WS2_OIDN, ct);
        }
    }
}
