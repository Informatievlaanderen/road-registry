namespace RoadRegistry.WmsWfsV2.Projections;

using System;
using System.Threading;
using System.Threading.Tasks;
using NetTopologySuite.Geometries;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using JasperFx.Events;
using RoadRegistry.Extensions;
using RoadRegistry.GradeJunction.Events.V2;
using RoadRegistry.Infrastructure.MartenDb.Projections;
using Schema;
using Schema.Records;

public class GradeJunctionWmsWfsV2Projection : RunnerDbContextRoadNetworkChangesProjection<WmsWfsV2Context>
{
    public GradeJunctionWmsWfsV2Projection()
    {
        When<IEvent<GradeJunctionWasAdded>>((context, e, ct) =>
            Write(context, e.Data.GradeJunctionId.ToInt32(), e.Data.RoadSegmentId1.ToInt32(), e.Data.RoadSegmentId2.ToInt32(),
                e.Data.Geometry.Value.Force2D(), e.Data.Provenance.Timestamp.ToDateTimeOffset(), ct));

        // Not a newly observed crossing: it is the one the named grade separated junction used to record, now recorded
        // as a grade junction. For this projection it is an insert all the same.
        When<IEvent<GradeJunctionWasAddedBecauseOfGradeSeparatedJunctionChange>>((context, e, ct) =>
            Write(context, e.Data.GradeJunctionId.ToInt32(), e.Data.RoadSegmentId1.ToInt32(), e.Data.RoadSegmentId2.ToInt32(),
                e.Data.Geometry.Value.Force2D(), e.Data.Provenance.Timestamp.ToDateTimeOffset(), ct));

        When<IEvent<GradeJunctionWasModified>>(async (context, e, ct) =>
        {
            var record = await context.GradeJunctions.FindAsync([e.Data.GradeJunctionId.ToInt32()], ct);
            if (record is null)
            {
                return;
            }
            if (e.Data.RoadSegmentId1 is not null)
            {
                record.WS1_OIDN = e.Data.RoadSegmentId1.Value.ToInt32();
            }
            if (e.Data.RoadSegmentId2 is not null)
            {
                record.WS2_OIDN = e.Data.RoadSegmentId2.Value.ToInt32();
            }
            record.VERSIE = e.Data.Provenance.Timestamp.ToDateTimeOffset();
        });

        When<IEvent<GradeJunctionGeometryWasChanged>>(async (context, e, ct) =>
        {
            var record = await context.GradeJunctions.FindAsync([e.Data.GradeJunctionId.ToInt32()], ct);
            if (record is null)
            {
                return;
            }
            record.GEOMETRIE = e.Data.Geometry.Value.Force2D();
            record.VERSIE = e.Data.Provenance.Timestamp.ToDateTimeOffset();
        });

        When<IEvent<GradeJunctionWasRemoved>>(async (context, e, ct) =>
        {
            var record = await context.GradeJunctions.FindAsync([e.Data.GradeJunctionId.ToInt32()], ct);
            if (record is not null)
            {
                context.GradeJunctions.Remove(record);
            }
        });

        // The crossing did not disappear: it is a grade separated junction from here on, which the grade separated
        // projection inserts. For this projection the grade junction is gone all the same.
        When<IEvent<GradeJunctionWasChangedToGradeSeparatedJunction>>(async (context, e, ct) =>
        {
            var record = await context.GradeJunctions.FindAsync([e.Data.GradeJunctionId.ToInt32()], ct);
            if (record is not null)
            {
                context.GradeJunctions.Remove(record);
            }
        });
    }

    // Upsert, not insert. A created event is not guaranteed to arrive once per id: the projection-state position that
    // used to guard that is a position, and a replay from before it - a recovery, a rebuild - delivers the event
    // again. A bare Add then becomes a primary key violation that pauses the shard, which is how this projection fell
    // over on GelijkgrondseKruisingen during the 2026-10-08 recovery. FindAsync also sees what the current batch has
    // added, so a duplicate inside one SaveChanges resolves here too.
    //
    // CREATIE is set only when the row is new, so a re-applied event does not rewrite the moment the crossing first
    // appeared.
    private static async Task Write(WmsWfsV2Context context, int id, int roadSegmentId1, int roadSegmentId2, Geometry? geometry, DateTimeOffset timestamp, CancellationToken ct)
    {
        var record = await context.GradeJunctions.FindAsync([id], ct);
        if (record is null)
        {
            record = new GradeJunctionRecord { GK_OIDN = id, CREATIE = timestamp };
            context.GradeJunctions.Add(record);
        }

        record.WS1_OIDN = roadSegmentId1;
        record.WS2_OIDN = roadSegmentId2;
        record.GEOMETRIE = geometry;
        record.VERSIE = timestamp;
    }
}
