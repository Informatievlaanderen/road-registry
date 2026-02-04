namespace RoadRegistry.Extracts.Projections;

using Marten;
using Microsoft.Extensions.Logging;
using RoadRegistry.Infrastructure.MartenDb.Projections;
using Setup;

public class ExtractsRoadNetworkChangesProjection : RoadNetworkChangesProjection
{
    public ExtractsRoadNetworkChangesProjection(int batchSize, ILoggerFactory loggerFactory)
        : base([new RoadNodeProjection(), new RoadSegmentProjection(), new GradeSeparatedJunctionProjection()], loggerFactory,
            batchSize: batchSize)
    {
    }

    protected override void ConfigureSchema(StoreOptions options)
    {
        options.ConfigureExtractDocuments();
    }
}
