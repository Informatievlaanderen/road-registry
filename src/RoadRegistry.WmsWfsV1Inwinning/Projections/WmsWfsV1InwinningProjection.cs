namespace RoadRegistry.WmsWfsV1Inwinning.Projections;

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JasperFx.Events;
using Microsoft.EntityFrameworkCore;
using Records;
using RoadRegistry.Infrastructure.MartenDb.Projections;
using RoadRegistry.RoadNode.Events.V2;
using RoadRegistry.RoadSegment.Events.V2;

// Records which road segments and road nodes are ingewonnen, and marks their V1 WMS and WFS records IsV2, which the V1
// views leave out. A migrated entity is V2 by definition, and so ingewonnen; so is one removed as part of that migration.
//
// A V1 record that is not there is left alone: the flag is set on what the V1 read models hold when the event arrives.
public class WmsWfsV1InwinningProjection : RunnerDbContextRoadNetworkChangesProjection<WmsWfsV1InwinningContext>
{
    public WmsWfsV1InwinningProjection()
    {
        When<IEvent<RoadSegmentWasMigrated>>((context, e, ct) =>
            CompleteRoadSegment(context, e.Data.RoadSegmentId.ToInt32(), ct));

        When<IEvent<RoadSegmentWasRemovedBecauseOfMigration>>((context, e, ct) =>
            CompleteRoadSegment(context, e.Data.RoadSegmentId.ToInt32(), ct));

        When<IEvent<RoadNodeWasMigrated>>((context, e, ct) =>
            CompleteRoadNode(context, e.Data.RoadNodeId.ToInt32(), ct));

        When<IEvent<RoadNodeWasRemovedBecauseOfMigration>>((context, e, ct) =>
            CompleteRoadNode(context, e.Data.RoadNodeId.ToInt32(), ct));
    }

    // Looked up first: the same id can be completed by more than one event, and Find also sees what this batch added.
    private static async Task CompleteRoadSegment(WmsWfsV1InwinningContext context, int roadSegmentId, CancellationToken cancellationToken)
    {
        if (await context.CompletedRoadSegments.FindAsync([roadSegmentId], cancellationToken) is null)
        {
            context.CompletedRoadSegments.Add(new CompletedRoadSegmentRecord { WS_OIDN = roadSegmentId });
        }

        if (await context.V1WmsRoadSegments.FindAsync([roadSegmentId], cancellationToken) is { } wmsRoadSegment)
        {
            wmsRoadSegment.IsV2 = true;
        }

        foreach (var europeanRoad in await context.V1WmsEuropeanRoads.Where(x => x.WS_OIDN == roadSegmentId).ToListAsync(cancellationToken))
        {
            europeanRoad.IsV2 = true;
        }

        foreach (var nationalRoad in await context.V1WmsNationalRoads.Where(x => x.WS_OIDN == roadSegmentId).ToListAsync(cancellationToken))
        {
            nationalRoad.IsV2 = true;
        }

        if (await context.V1WfsRoadSegments.FindAsync([roadSegmentId], cancellationToken) is { } wfsRoadSegment)
        {
            wfsRoadSegment.IsV2 = true;
        }
    }

    // The V1 WMS has no road nodes.
    private static async Task CompleteRoadNode(WmsWfsV1InwinningContext context, int roadNodeId, CancellationToken cancellationToken)
    {
        if (await context.CompletedRoadNodes.FindAsync([roadNodeId], cancellationToken) is null)
        {
            context.CompletedRoadNodes.Add(new CompletedRoadNodeRecord { WK_OIDN = roadNodeId });
        }

        if (await context.V1WfsRoadNodes.FindAsync([roadNodeId], cancellationToken) is { } wfsRoadNode)
        {
            wfsRoadNode.IsV2 = true;
        }
    }
}
