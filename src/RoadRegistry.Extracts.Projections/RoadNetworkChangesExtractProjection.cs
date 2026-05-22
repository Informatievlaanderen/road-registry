namespace RoadRegistry.Extracts.Projections;

using Marten;
using Microsoft.Extensions.Logging;
using RoadRegistry.Infrastructure.MartenDb.Projections;
using Setup;

public class RoadNetworkChangesExtractProjection : RoadNetworkChangesProjection
{
    public RoadNetworkChangesExtractProjection(int batchSize, ILoggerFactory loggerFactory)
        : base([
                new RoadNodeExtractProjection(),
                new RoadSegmentExtractProjection(),
                new GradeSeparatedJunctionExtractProjection(),
                new GradeJunctionExtractProjection()
            ], loggerFactory,
            batchSize: batchSize)
    {
    }

    protected override void ConfigureSchema(StoreOptions options)
    {
        options.ConfigureExtractDocuments();
    }
}
