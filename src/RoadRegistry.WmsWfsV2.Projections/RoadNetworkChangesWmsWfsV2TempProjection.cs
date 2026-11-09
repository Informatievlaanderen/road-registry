namespace RoadRegistry.WmsWfsV2.Projections;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RoadRegistry.Infrastructure.MartenDb.Projections;
using Schema;

// The WmsWfsV2 projection a second time, filling the shadow copy of the read model in the 'roadTemp' schema while the
// live one keeps serving the current one.
//
// It is the same projection - same sub-projections, same handlers, same settings - pointed at a context scoped to the
// other schema. Everything that separates the two follows from the type name: Marten keys a shard and its progressions
// by the projection's name, and the read model keys its position row by it, so this one replays from the start of the
// event stream without touching anything the live projection owns.
//
// It exists to rebuild a read model without taking it down, and stays in the code for the next time that is needed:
// Projections:WmsWfsV2:Shadow:Enabled runs it, POST projections/{id}/promote swaps it in once it is switched off again (ShadowReadModelPromoter).
public sealed class RoadNetworkChangesWmsWfsV2TempProjection : RoadNetworkChangesWmsWfsV2Projection
{
    public RoadNetworkChangesWmsWfsV2TempProjection(
        int batchSize,
        ILoggerFactory loggerFactory,
        IDbContextFactory<WmsWfsV2Context> dbContextFactory,
        ProjectionCatchUpOptions? catchUpOptions = null)
        : base(batchSize, loggerFactory, dbContextFactory, catchUpOptions)
    {
    }
}
