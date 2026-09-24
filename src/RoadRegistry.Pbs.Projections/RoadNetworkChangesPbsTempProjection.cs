namespace RoadRegistry.Pbs.Projections;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RoadRegistry.Infrastructure.MartenDb.Projections;
using Schema;

// The PBS projection a second time, filling the shadow copy of the read model in the 'RoadRegistryPbsTemp' schema while
// the live one keeps serving the current one: same projection, same settings, a context scoped to the other schema,
// and a name of its own - which is what gives it its own Marten shard, its own progressions and its own position row.
//
// It exists to rebuild a read model without taking it down. Once it has caught up the two schemas are swapped, and
// this class, its registration and its schema go with the change that swaps them - as they did for WmsWfsV2.
public sealed class RoadNetworkChangesPbsTempProjection : RoadNetworkChangesPbsProjection
{
    public RoadNetworkChangesPbsTempProjection(
        int batchSize,
        ILoggerFactory loggerFactory,
        IDbContextFactory<PbsContext> dbContextFactory,
        ProjectionCatchUpOptions? catchUpOptions = null)
        : base(batchSize, loggerFactory, dbContextFactory, catchUpOptions)
    {
    }
}
