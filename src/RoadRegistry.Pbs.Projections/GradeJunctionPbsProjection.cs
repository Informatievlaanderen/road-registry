namespace RoadRegistry.Pbs.Projections;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using JasperFx.Events;
using RoadRegistry.GradeJunction.Events.V2;
using RoadRegistry.Infrastructure.MartenDb.Projections;
using Schema;
using Schema.Records;

public class GradeJunctionPbsProjection : RunnerDbContextRoadNetworkChangesProjection<PbsContext>
{
    public GradeJunctionPbsProjection(IDbContextFactory<PbsContext> dbContextFactory, ILoggerFactory? loggerFactory = null)
        : base(dbContextFactory, loggerFactory)
    {
        When<IEvent<GradeJunctionWasAdded>>(async (context, e, ct) =>
        {
            var record = await context.GradeJunctions.FindAsync([e.Data.GradeJunctionId.ToInt32()], ct);
            var isNew = record is null;
            record ??= new GradeJunctionRecord { GK_OIDN = e.Data.GradeJunctionId.ToInt32(), CREATIE = e.Data.Provenance.ToPbsDate() };
            record.WS1_OIDN = e.Data.RoadSegmentId1.ToInt32();
            record.WS2_OIDN = e.Data.RoadSegmentId2.ToInt32();
            record.GEOMETRIE = e.Data.Geometry.Value;
            record.VERSIE = e.Data.Provenance.ToPbsDate();
            if (isNew)
            {
                context.GradeJunctions.Add(record);
            }
        });

        When<IEvent<GradeJunctionGeometryWasChanged>>(async (context, e, ct) =>
        {
            var record = await context.GradeJunctions.FindAsync([e.Data.GradeJunctionId.ToInt32()], ct);
            if (record is null)
            {
                return;
            }
            record.GEOMETRIE = e.Data.Geometry.Value;
            record.VERSIE = e.Data.Provenance.ToPbsDate();
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
