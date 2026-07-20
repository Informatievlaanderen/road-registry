namespace RoadRegistry.Read.Projections;

using Marten;
using Microsoft.Extensions.Logging;
using RoadRegistry.Infrastructure.MartenDb.Projections;
using RoadRegistry.Read.Projections.Setup;
using RoadRegistry.StreetName;

public class RoadNetworkChangesReadProjection : MartenBackedRoadNetworkChangesProjection
{
    public RoadNetworkChangesReadProjection(int batchSize, ILoggerFactory loggerFactory, IStreetNameClient streetNameClient)
        : base([
                new OrganizationReadProjection(),
                new StreetNameReadProjection(),
                new RoadNodeReadProjection(),
                new RoadSegmentReadProjection(streetNameClient, loggerFactory.CreateLogger<RoadSegmentReadProjection>()),
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
