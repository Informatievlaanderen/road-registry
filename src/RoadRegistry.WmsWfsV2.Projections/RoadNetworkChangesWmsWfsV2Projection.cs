namespace RoadRegistry.WmsWfsV2.Projections;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RoadRegistry.Infrastructure.MartenDb.Projections;
using Schema;

public class RoadNetworkChangesWmsWfsV2Projection : DbContextBackedRoadNetworkChangesProjection<WmsWfsV2Context>
{
    public RoadNetworkChangesWmsWfsV2Projection(int batchSize, ILoggerFactory loggerFactory, IDbContextFactory<WmsWfsV2Context> dbContextFactory)
        : base(dbContextFactory,
            [
                new OrganizationWmsWfsV2Projection(),
                new StreetNameWmsWfsV2Projection(),
                new RoadNodeWmsWfsV2Projection(),
                new RoadSegmentWmsWfsV2Projection(),
                new GradeJunctionWmsWfsV2Projection(),
                new GradeSeparatedJunctionWmsWfsV2Projection()
            ], loggerFactory,
            batchSize: batchSize)
    {
    }
}
