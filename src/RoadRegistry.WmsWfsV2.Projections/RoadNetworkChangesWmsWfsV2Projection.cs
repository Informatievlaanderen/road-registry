namespace RoadRegistry.WmsWfsV2.Projections;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RoadRegistry.Infrastructure.MartenDb.Projections;
using Schema;

public class RoadNetworkChangesWmsWfsV2Projection : DbContextBackedRoadNetworkChangesProjection<WmsWfsV2Context>
{
    public RoadNetworkChangesWmsWfsV2Projection(int batchSize, ILoggerFactory loggerFactory, IDbContextFactory<WmsWfsV2Context> dbContextFactory, ProjectionCatchUpOptions? catchUpOptions = null)
        : this(batchSize, loggerFactory, dbContextFactory, new DerivedLabelCache(), catchUpOptions)
    {
    }

    // The street name and organization sub-projections own the writes to the two label tables; the road segment
    // sub-projection reads them back for every derived row. They therefore share one cache instance: the writers keep
    // it current, the reader gets its labels without a query per event.
    private RoadNetworkChangesWmsWfsV2Projection(int batchSize, ILoggerFactory loggerFactory, IDbContextFactory<WmsWfsV2Context> dbContextFactory, DerivedLabelCache labelCache, ProjectionCatchUpOptions? catchUpOptions)
        : base(dbContextFactory,
            [
                new OrganizationWmsWfsV2Projection(labelCache),
                new StreetNameWmsWfsV2Projection(labelCache),
                new RoadNodeWmsWfsV2Projection(),
                new RoadSegmentWmsWfsV2Projection(labelCache),
                new GradeJunctionWmsWfsV2Projection(),
                new GradeSeparatedJunctionWmsWfsV2Projection()
            ], loggerFactory,
            batchSize: batchSize,
            catchUpOptions: catchUpOptions)
    {
    }
}
