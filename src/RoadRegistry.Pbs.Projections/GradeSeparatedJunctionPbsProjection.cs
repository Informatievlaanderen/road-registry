namespace RoadRegistry.Pbs.Projections;

using System.Threading;
using System.Threading.Tasks;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using JasperFx.Events;
using RoadRegistry.Extensions;
using RoadRegistry.GradeSeparatedJunction.Events.V1;
using RoadRegistry.GradeSeparatedJunction.Events.V2;
using RoadRegistry.Infrastructure.MartenDb.Projections;
using RoadRegistry.ValueObjects;
using Schema;
using Schema.Records;

public class GradeSeparatedJunctionPbsProjection : RunnerDbContextRoadNetworkChangesProjection<PbsContext>
{
    public GradeSeparatedJunctionPbsProjection(IDbContextFactory<PbsContext> dbContextFactory, ILoggerFactory? loggerFactory = null)
        : base(dbContextFactory, loggerFactory)
    {
        // V1
        When<IEvent<ImportedGradeSeparatedJunction>>((context, e, ct) =>
            WriteV1(context, e.Data.Id, e.Data.LowerRoadSegmentId, e.Data.UpperRoadSegmentId, e.Data.Type, e.Data.Geometry, e.Data.Provenance, ct));

        When<IEvent<GradeSeparatedJunctionAdded>>((context, e, ct) =>
            WriteV1(context, e.Data.Id, e.Data.LowerRoadSegmentId, e.Data.UpperRoadSegmentId, e.Data.Type, e.Data.Geometry, e.Data.Provenance, ct));

        When<IEvent<GradeSeparatedJunctionModified>>((context, e, ct) =>
            WriteV1(context, e.Data.Id, e.Data.LowerRoadSegmentId, e.Data.UpperRoadSegmentId, e.Data.Type, e.Data.Geometry, e.Data.Provenance, ct));

        // V1 (legacy) geometry change produced by the migration; the geometry is in Lambert72 and normalized to Lambert08.
        When<IEvent<GradeSeparatedJunctionGeometryModified>>(async (context, e, ct) =>
        {
            var record = await context.GradeSeparatedJunctions.FindAsync([e.Data.Id], ct);
            if (record is null)
            {
                return;
            }
            record.GEOMETRIE = e.Data.Geometry?.EnsureLambert08().Value;
            record.VERSIE = e.Data.Provenance.ToPbsDate();
        });

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
            record.GEOMETRIE = e.Data.Geometry.EnsureLambert08().Value;
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
            // Geometry is not carried on the modify event; it arrives via GradeSeparatedJunctionGeometryWasChanged.
            record.VERSIE = e.Data.Provenance.ToPbsDate();
        });

        When<IEvent<GradeSeparatedJunctionGeometryWasChanged>>(async (context, e, ct) =>
        {
            var record = await context.GradeSeparatedJunctions.FindAsync([e.Data.GradeSeparatedJunctionId.ToInt32()], ct);
            if (record is null)
            {
                return;
            }
            record.GEOMETRIE = e.Data.Geometry.EnsureLambert08().Value;
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
    }

    private static async Task WriteV1(PbsContext context, int id, int lowerRoadSegmentId, int upperRoadSegmentId, string type, JunctionGeometry? geometry, ProvenanceData provenance, CancellationToken ct)
    {
        var record = await context.GradeSeparatedJunctions.FindAsync([id], ct);
        var isNew = record is null;
        record ??= new GradeSeparatedJunctionRecord { OK_OIDN = id, CREATIE = provenance.ToPbsDate() };
        record.ON_WS_OIDN = lowerRoadSegmentId;
        record.BO_WS_OIDN = upperRoadSegmentId;
        var v2Type = V1ToV2.GradeSeparatedJunctionType(type);
        record.TYPE = v2Type?.Translation.Identifier;
        record.LBLTYPE = v2Type?.Translation.Name;
        record.GEOMETRIE = geometry?.EnsureLambert08().Value;
        record.VERSIE = provenance.ToPbsDate();
        if (isNew)
        {
            context.GradeSeparatedJunctions.Add(record);
        }
    }
}
