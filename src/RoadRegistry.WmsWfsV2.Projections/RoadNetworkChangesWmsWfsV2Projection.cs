namespace RoadRegistry.WmsWfsV2.Projections;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RoadRegistry.Infrastructure.MartenDb.Projections;
using Schema;

public class RoadNetworkChangesWmsWfsV2Projection : RoadNetworkChangesProjection
{
    public RoadNetworkChangesWmsWfsV2Projection(int batchSize, ILoggerFactory loggerFactory, IDbContextFactory<WmsWfsV2Context> dbContextFactory)
        : base([
                new OrganizationWmsWfsV2Projection(dbContextFactory, loggerFactory),
                new StreetNameWmsWfsV2Projection(dbContextFactory, loggerFactory),
                new RoadNodeWmsWfsV2Projection(dbContextFactory, loggerFactory),
                new RoadSegmentWmsWfsV2Projection(dbContextFactory, loggerFactory),
                new GradeJunctionWmsWfsV2Projection(dbContextFactory, loggerFactory),
                new GradeSeparatedJunctionWmsWfsV2Projection(dbContextFactory, loggerFactory)
            ], loggerFactory,
            batchSize: batchSize)
    {
    }
}
