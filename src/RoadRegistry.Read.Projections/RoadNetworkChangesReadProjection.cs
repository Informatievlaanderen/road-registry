namespace RoadRegistry.Read.Projections;

using Marten;
using Microsoft.Extensions.Logging;
using RoadRegistry.Infrastructure.MartenDb.Projections;
using RoadRegistry.Read.Projections.Setup;

public class RoadNetworkChangesReadProjection : RoadNetworkChangesProjection
{
    public RoadNetworkChangesReadProjection(int batchSize, ILoggerFactory loggerFactory)
        : base([
                new OrganizationReadProjection(),
                new StreetNameReadProjection(),
                new RoadNodeReadProjection(),
                new RoadSegmentReadProjection(),
                new GradeSeparatedJunctionReadProjection(),
                new GradeJunctionReadProjection()
            ], loggerFactory,
            batchSize: batchSize)
    {
    }

    protected override void ConfigureSchema(StoreOptions options)
    {
        options.ConfigureReadDocuments();
    }
}
