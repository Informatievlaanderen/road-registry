namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda;

using Marten;
using RoadRegistry.Extracts.Projections.Setup;
using RoadRegistry.Infrastructure.MartenDb.Setup;

// The Marten store of this function, defined once. The function builds its store from it, and so does
// RoadRegistry.BackOffice.Handlers.Sqs.Lambda.CodeGen when it pre-generates the Marten code (GAWR-7236). It has to be
// the same configuration: the generated type names carry a hash of the document mappings, so code generated from any
// other store would never be picked up.
public static class FunctionMartenConfiguration
{
    public static void Configure(StoreOptions options)
    {
        options
            .AddRoadAggregatesSnapshots()
            .ConfigureExtractDocuments();

        // Marten looks for pre-generated types in the entry assembly by default. In a Lambda that is the runtime's
        // bootstrap, not this function, so the code would be compiled in here and then never looked for.
        options.ApplicationAssembly = typeof(Function).Assembly;
    }
}
