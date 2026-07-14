namespace RoadRegistry.Pbs.Projections;

using System.Threading;
using System.Threading.Tasks;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using JasperFx.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RoadRegistry.Extensions;
using RoadRegistry.Extracts.Infrastructure.Dbase;
using RoadRegistry.RoadNode.Events.V1;
using RoadRegistry.RoadNode.Events.V2;
using RoadRegistry.ValueObjects;
using RoadRegistry.Infrastructure.MartenDb.Projections;
using Schema;
using Schema.Records;

public class RoadNodePbsProjection : RunnerDbContextRoadNetworkChangesProjection<PbsContext>
{
    public RoadNodePbsProjection(IDbContextFactory<PbsContext> dbContextFactory, ILoggerFactory loggerFactory = null)
        : base(dbContextFactory, loggerFactory)
    {
        // V1
        When<IEvent<ImportedRoadNode>>((context, e, ct) =>
            WriteV1(context, e.Data.RoadNodeId, e.Data.Geometry, e.Data.Type, e.Data.Provenance, ct));

        When<IEvent<RoadNodeAdded>>((context, e, ct) =>
            WriteV1(context, e.Data.RoadNodeId, e.Data.Geometry, e.Data.Type, e.Data.Provenance, ct));

        When<IEvent<RoadNodeModified>>((context, e, ct) =>
            WriteV1(context, e.Data.RoadNodeId, e.Data.Geometry, e.Data.Type, e.Data.Provenance, ct));

        When<IEvent<RoadNodeRemoved>>(async (context, e, ct) =>
        {
            var record = await context.RoadNodes.FindAsync([e.Data.RoadNodeId], ct);
            if (record is not null)
            {
                context.RoadNodes.Remove(record);
            }
        });

        // V2
        When<IEvent<RoadNodeWasAdded>>(async (context, e, ct) =>
        {
            var record = await context.RoadNodes.FindAsync([e.Data.RoadNodeId.ToInt32()], ct);
            var isNew = record is null;
            record ??= new RoadNodeRecord { WK_OIDN = e.Data.RoadNodeId.ToInt32(), CREATIE = e.Data.Provenance.ToPbsDate() };
            record.GRENSKNOOP = e.Data.Grensknoop.ToDbaseShortValue();
            record.GEOMETRIE = e.Data.Geometry.EnsureLambert08().RoundToCm().Value;
            record.VERSIE = e.Data.Provenance.ToPbsDate();
            if (isNew)
            {
                context.RoadNodes.Add(record);
            }
        });

        When<IEvent<RoadNodeTypeWasChanged>>(async (context, e, ct) =>
        {
            var record = await context.RoadNodes.FindAsync([e.Data.RoadNodeId.ToInt32()], ct);
            if (record is null)
            {
                return;
            }
            record.TYPE = e.Data.Type.Translation.Identifier;
            record.LBLTYPE = e.Data.Type.Translation.Name;
            record.VERSIE = e.Data.Provenance.ToPbsDate();
        });

        When<IEvent<RoadNodeWasModified>>(async (context, e, ct) =>
        {
            var record = await context.RoadNodes.FindAsync([e.Data.RoadNodeId.ToInt32()], ct);
            if (record is null)
            {
                return;
            }
            if (e.Data.Geometry is not null)
            {
                record.GEOMETRIE = e.Data.Geometry.EnsureLambert08().RoundToCm().Value;
            }
            if (e.Data.Grensknoop is not null)
            {
                record.GRENSKNOOP = e.Data.Grensknoop.Value.ToDbaseShortValue();
            }
            record.VERSIE = e.Data.Provenance.ToPbsDate();
        });

        When<IEvent<RoadNodeWasMigrated>>(async (context, e, ct) =>
        {
            var record = await context.RoadNodes.FindAsync([e.Data.RoadNodeId.ToInt32()], ct);
            var isNew = record is null;
            record ??= new RoadNodeRecord { WK_OIDN = e.Data.RoadNodeId.ToInt32(), CREATIE = e.Data.Provenance.ToPbsDate() };
            record.GRENSKNOOP = e.Data.Grensknoop.ToDbaseShortValue();
            record.GEOMETRIE = e.Data.Geometry.EnsureLambert08().RoundToCm().Value;
            record.VERSIE = e.Data.Provenance.ToPbsDate();
            if (isNew)
            {
                context.RoadNodes.Add(record);
            }
        });

        When<IEvent<RoadNodeWasRemoved>>((context, e, ct) => Remove(context, e.Data.RoadNodeId, ct));
        When<IEvent<RoadNodeWasRemovedBecauseOfMigration>>((context, e, ct) => Remove(context, e.Data.RoadNodeId, ct));
    }

    private static async Task Remove(PbsContext context, RoadNodeId roadNodeId, CancellationToken ct)
    {
        var record = await context.RoadNodes.FindAsync([roadNodeId.ToInt32()], ct);
        if (record is not null)
        {
            context.RoadNodes.Remove(record);
        }
    }

    // V1 road node: upsert the geometry and the type (mapped V1 -> V2, null when unmapped). V1 nodes carry no grensknoop.
    private static async Task WriteV1(PbsContext context, int nodeId, RoadNodeGeometry geometry, string type, ProvenanceData provenance, CancellationToken ct)
    {
        var record = await context.RoadNodes.FindAsync([nodeId], ct);
        var isNew = record is null;
        record ??= new RoadNodeRecord { WK_OIDN = nodeId, CREATIE = provenance.ToPbsDate() };
        record.GRENSKNOOP = null;
        record.GEOMETRIE = geometry.EnsureLambert08().RoundToCm().Value;
        var v2Type = V1ToV2.RoadNodeType(type);
        record.TYPE = v2Type?.Translation.Identifier;
        record.LBLTYPE = v2Type?.Translation.Name;
        record.VERSIE = provenance.ToPbsDate();
        if (isNew)
        {
            context.RoadNodes.Add(record);
        }
    }
}
