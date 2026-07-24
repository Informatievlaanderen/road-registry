namespace RoadRegistry.WmsWfsV2.Projections;

using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using JasperFx.Events;
using RoadRegistry.GradeJunction.Events.V2;
using RoadRegistry.Infrastructure.MartenDb.Projections;
using Schema;
using Schema.Records;

public class GradeJunctionWmsWfsV2Projection : RunnerDbContextRoadNetworkChangesProjection<WmsWfsV2Context>
{
    public GradeJunctionWmsWfsV2Projection()
    {
        // A created event means the entity does not exist yet, so insert directly without a lookup (re-delivery is
        // guarded by the projection-state position in the base projection).
        When<IEvent<GradeJunctionWasAdded>>((context, e, ct) =>
        {
            context.GradeJunctions.Add(new GradeJunctionRecord
            {
                GK_OIDN = e.Data.GradeJunctionId.ToInt32(),
                WS1_OIDN = e.Data.RoadSegmentId1.ToInt32(),
                WS2_OIDN = e.Data.RoadSegmentId2.ToInt32(),
                GEOMETRIE = e.Data.Geometry.Value,
                CREATIE = e.Data.Provenance.Timestamp.ToDateTimeOffset(),
                VERSIE = e.Data.Provenance.Timestamp.ToDateTimeOffset()
            });
            return Task.CompletedTask;
        });

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
            record.GEOMETRIE = e.Data.Geometry.Value;
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
    }
}
