namespace RoadRegistry.WmsWfsV1Inwinning.Projections;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RoadRegistry.Infrastructure.MartenDb.Projections;

public class RoadNetworkChangesWmsWfsV1InwinningProjection : DbContextBackedRoadNetworkChangesProjection<WmsWfsV1InwinningContext>
{
    public RoadNetworkChangesWmsWfsV1InwinningProjection(int batchSize, ILoggerFactory loggerFactory, IDbContextFactory<WmsWfsV1InwinningContext> dbContextFactory, ProjectionCatchUpOptions? catchUpOptions = null)
        : base(dbContextFactory,
            [
                new WmsWfsV1InwinningProjection()
            ], loggerFactory,
            batchSize: batchSize,
            catchUpOptions: catchUpOptions)
    {
    }
}
