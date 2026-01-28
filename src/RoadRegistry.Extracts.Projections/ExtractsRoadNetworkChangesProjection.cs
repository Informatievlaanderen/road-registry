namespace RoadRegistry.Extracts.Projections;

using Marten;
using Microsoft.Extensions.Logging;
using RoadRegistry.Infrastructure.MartenDb.Projections;

public class ExtractsRoadNetworkChangesProjection : RoadNetworkChangesProjection
{
    public const int BatchSize = DefaultBatchSize;

    public ExtractsRoadNetworkChangesProjection(ILoggerFactory loggerFactory)
        : base([new RoadNodeProjection(), new RoadSegmentProjection(), new GradeSeparatedJunctionProjection()], loggerFactory,
            batchSize: BatchSize,
            catchUpThreshold: BatchSize * 4)
    {
    }

    protected override void ConfigureSchema(StoreOptions options)
    {
        RoadNodeProjection.Configure(options);
        RoadSegmentProjection.Configure(options);
        GradeSeparatedJunctionProjection.Configure(options);
    }
}
