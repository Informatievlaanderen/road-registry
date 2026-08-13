namespace RoadRegistry.Projections.Tests.Projections;

using FluentAssertions;
using RoadRegistry.Extracts.Projections;
using RoadRegistry.Extracts.Projections.Setup;
using RoadRegistry.Infrastructure.MartenDb.Projections;
using RoadRegistry.Read.Projections;
using RoadRegistry.Read.Projections.Setup;

// The rebuild endpoints derive the document types to wipe from the same Configure methods the projections run,
// so they can never miss a type. These tests pin that detection: if it silently broke, a rebuild would truncate
// nothing and replay onto stale documents.
public class MartenProjectionDocumentsTests
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
