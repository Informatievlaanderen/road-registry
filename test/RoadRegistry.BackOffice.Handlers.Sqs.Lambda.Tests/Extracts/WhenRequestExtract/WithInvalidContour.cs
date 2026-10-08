namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.Extracts.WhenRequestExtract;

using Abstractions.Extracts.V2;
using AutoFixture;
using BackOffice.Extracts;
using FluentAssertions;
using Framework;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using RoadRegistry.Extracts.Schema;
using Xunit.Abstractions;

// The endpoints turn an invalid contour away, but a contour we derived ourselves - a municipality boundary, a
// reprojected one - never passed one of them, so ExtractRequester corrects it before it is stored. An invalid contour
// in [ExtractDownloads] does not only fail its own extract: it keeps failing the overlap check of every extract
// requested after it, because SQL Server refuses any spatial operation on an invalid instance.
public class WithInvalidContour : WhenRequestExtractTestBase
{
    // A sliver drawn along a road whose ring crosses itself around (194233.64, 206922.22). Correcting it leaves three
    // separate slivers, so the correction is visible in both the validity and the number of parts.
    private const string SelfIntersectingContour = "MULTIPOLYGON (((194477.00366637472 206971.83339355365, 194469.6805155593 206969.91542548305, 194452.85470475757 206975.14624749395, 193640.4208660975 206778.9904220851, 193345.57686541631 206711.33845741072, 193326.39718470967 206705.75891393243, 193324.3048559053 206719.18469042706, 193409.567254683 206737.66692819892, 193820.53550400632 206827.11398458539, 194105.96402506795 206891.80181678684, 194197.32904952508 206912.02766189564, 194470.37795849424 206988.65920435538, 194475.26005903777 206978.19756033356, 194477.00366637472 206971.83339355365)))";

    // A ring with no area at all: correcting it leaves nothing to store.
    private const string CollapsingContour = "MULTIPOLYGON (((0 0, 10 0, 0 0, 0 0)))";

    public WithInvalidContour(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public async Task WithCorrectableContour_ThenStoredContourIsValid()
    {
        // Arrange
        var downloadId = ObjectProvider.Create<DownloadId>();
        var contour = (MultiPolygon)new WKTReader().Read(SelfIntersectingContour);
        contour.IsValid.Should().BeFalse("otherwise this test no longer covers the correction");

        var request = new RequestExtractData(
            ObjectProvider.Create<ExtractRequestId>(),
            downloadId,
            contour,
            ObjectProvider.Create<string>(),
            false,
            ObjectProvider.Create<string>()
        );

        // Act
        await HandleRequest(request);

        // Assert
        VerifyThatTicketHasCompleted(new RequestExtractResponse(downloadId));

        var extractDownload = ExtractsDbContext.ExtractDownloads.Single(x => x.DownloadId == downloadId);
        extractDownload.Status.Should().Be(ExtractDownloadStatus.Available);
        extractDownload.Contour.IsValid.Should().BeTrue();
        extractDownload.Contour.Should().BeOfType<MultiPolygon>("every other code path stores the contour as one");
        extractDownload.Contour.NumGeometries.Should().Be(3);
    }

    [Fact]
    public async Task WithContourThatCorrectsToNothing_ThenError()
    {
        // Arrange
        var downloadId = ObjectProvider.Create<DownloadId>();
        var request = new RequestExtractData(
            ObjectProvider.Create<ExtractRequestId>(),
            downloadId,
            (MultiPolygon)new WKTReader().Read(CollapsingContour),
            ObjectProvider.Create<string>(),
            false,
            ObjectProvider.Create<string>()
        );

        // Act
        await HandleRequest(request);

        // Assert
        // The problem code is translated on its way to the ticket, so what lands there is the Dutch one.
        VerifyThatTicketHasError("ContourOngeldig", "Contour is ongeldig.");

        // Nothing was stored: the contour is turned away before the request and the download are written.
        ExtractsDbContext.ExtractDownloads.Should().BeEmpty();
        ExtractsDbContext.ExtractRequests.Should().BeEmpty();
    }
}
