namespace RoadRegistry.MartenDb.MigrationGenerator;

using Marten;
using RoadRegistry.Extracts.Projections.Setup;
using RoadRegistry.Infrastructure.MartenDb.Projections;
using RoadRegistry.Infrastructure.MartenDb.Setup;
using RoadRegistry.Read.Projections.Setup;

public static class MigrationModel
{
    // The complete Marten model - every document type any host uses - in one place. This is the single source of
    // truth the migration generator (db-dump / db-patch) diffs against. Runtime hosts register only what they run.
    public static StoreOptions ConfigureAllRoadDocuments(this StoreOptions options)
    {
        options.ConfigureReadDocuments();
        options.ConfigureExtractDocuments();
        options.AddRoadNetworkTopologyProjection();
        options.AddRoadAggregatesSnapshots();
        options.ConfigureRoadNetworkChangesProgression();
        return options;
    }
}
