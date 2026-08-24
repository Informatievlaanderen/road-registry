namespace RoadRegistry.Projections.Tests.Projections.ReadProjections;

using FluentAssertions;
using RoadRegistry.Infrastructure.MartenDb.Projections;
using RoadRegistry.Read.Projections;
using RoadRegistry.Read.Projections.Setup;

// The rebuild endpoint derives the document types to wipe from the same ConfigureReadDocuments the projection
// runs, so it can never miss a type. This test pins that detection: if it silently broke, a rebuild would truncate
// nothing and replay onto stale documents.
public class ReadRebuildDocumentCoverageTests
{
    [Fact]
    public void DetectsReadDocumentTypes()
    {
        var documentTypes = MartenProjectionDocuments.GetDocumentTypes(options => options.ConfigureReadDocuments());

        documentTypes.Should().BeEquivalentTo([
            typeof(OrganizationReadItem),
            typeof(StreetNameReadItem),
            typeof(RoadNodeReadItem),
            typeof(RoadSegmentReadItem),
            typeof(StreetNameRoadSegmentsLink),
            typeof(OrganizationRoadSegmentsLink),
            typeof(GradeSeparatedJunctionReadItem),
            typeof(GradeJunctionReadItem)
        ]);
    }
}
