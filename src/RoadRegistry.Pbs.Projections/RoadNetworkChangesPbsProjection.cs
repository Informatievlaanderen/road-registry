namespace RoadRegistry.Pbs.Projections;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RoadRegistry.Infrastructure.MartenDb.Projections;
using Schema;

public class RoadNetworkChangesPbsProjection : RoadNetworkChangesProjection
{
    public RoadNetworkChangesPbsProjection(int batchSize, ILoggerFactory loggerFactory, IDbContextFactory<PbsContext> dbContextFactory)
        : base([
                new OrganizationPbsProjection(dbContextFactory, loggerFactory),
                new StreetNamePbsProjection(dbContextFactory, loggerFactory),
                new RoadNodePbsProjection(dbContextFactory, loggerFactory),
                new RoadSegmentPbsProjection(dbContextFactory, loggerFactory),
                new GradeJunctionPbsProjection(dbContextFactory, loggerFactory),
                new GradeSeparatedJunctionPbsProjection(dbContextFactory, loggerFactory)
            ], loggerFactory,
            batchSize: batchSize)
    {
    }
}
