namespace RoadRegistry.Projections.Tests.Projections.ExtractProjections;

using FluentAssertions;
using RoadRegistry.Extracts.Projections;
using RoadRegistry.Extracts.Projections.Setup;
using RoadRegistry.Infrastructure.MartenDb.Projections;

// The rebuild endpoint derives the document types to wipe from the same ConfigureExtractDocuments the projection
// runs, so it can never miss a type. This test pins that detection: if it silently broke, a rebuild would truncate
// nothing and replay onto stale documents.
public class ExtractRebuildDocumentCoverageTests
{
    [Fact]
    public void DetectsExtractDocumentTypes()
    {
        var documentTypes = MartenProjectionDocuments.GetDocumentTypes(options => options.ConfigureExtractDocuments());

        documentTypes.Should().BeEquivalentTo([
            typeof(RoadNodeExtractItem),
            typeof(RoadSegmentExtractItem),
            typeof(GradeSeparatedJunctionExtractItem),
            typeof(GradeJunctionExtractItem)
        ]);
    }
}
