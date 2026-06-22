namespace RoadRegistry.Read.Projections;

using Marten;
using Microsoft.Extensions.Logging;
using RoadRegistry.Infrastructure;
using RoadRegistry.Infrastructure.MartenDb.Projections;
using RoadRegistry.Read.Projections.Setup;
using RoadRegistry.StreetName;

public class RoadNetworkChangesReadProjection : RoadNetworkChangesProjection
{
    public RoadNetworkChangesReadProjection(int batchSize, IStreetNameCache streetNameCache, IStreetNameClient streetNameClient, ILoggerFactory loggerFactory)
        : base([
                new OrganizationReadProjection(),
                new RoadNodeReadProjection(),
                new RoadSegmentReadProjection(streetNameCache, streetNameClient),
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
