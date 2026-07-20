namespace RoadRegistry.Pbs.Projections;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RoadRegistry.Infrastructure.MartenDb.Projections;
using Schema;

public class RoadNetworkChangesPbsProjection : DbContextBackedRoadNetworkChangesProjection<PbsContext>
{
    public RoadNetworkChangesPbsProjection(int batchSize, ILoggerFactory loggerFactory, IDbContextFactory<PbsContext> dbContextFactory)
        : base(dbContextFactory,
            [
                new OrganizationPbsProjection(),
                new StreetNamePbsProjection(),
                new RoadNodePbsProjection(),
                new RoadSegmentPbsProjection(),
                new GradeJunctionPbsProjection(),
                new GradeSeparatedJunctionPbsProjection()
            ], loggerFactory,
            batchSize: batchSize)
    {
    }
}
