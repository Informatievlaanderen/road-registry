namespace RoadRegistry.BackOffice.Api.Tests.Extracten;

using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using Api.Extracten;
using AutoFixture;
using BackOffice.Handlers.Sqs.Extracts;
using Be.Vlaanderen.Basisregisters.Sqs.Requests;
using FluentAssertions;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Moq;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using RoadRegistry.Extracts;

// The endpoints serving the renewed data model. What sets them apart from their v1 counterparts is the archive they
// ask for and the reference system the contour is stored in: contours become Lambert 2008 from here on, and the v1
// endpoints convert back to Lambert 72 when they go looking for their data.
public partial class ExtractsControllerTests
{
    private const string ContourInLambert72 = "MULTIPOLYGON (((155000 210000, 155000 211000, 156000 211000, 156000 210000, 155000 210000)))";

    [Fact]
    public async Task WhenRequestExtractByContourV2_ThenTheBijhoudingArchiveIsRequestedInLambert08()
    {
        var sentRequest = CaptureRequestExtract();

        var result = await Controller.ExtractDownloadaanvraagPerContourV2(
            new ExtractDownloadaanvraagPerContourBody(ContourInLambert72, Fixture.Create<string>(), true, null),
            new ExtractDownloadaanvraagPerContourBodyValidator());

        Assert.IsType<AcceptedResult>(result);
        sentRequest.Value!.ZipArchiveWriterVersion.Should().Be(WellKnownZipArchiveWriterVersions.DomainV2_Bijhouding);
        sentRequest.Value.Contour.SRID.Should().Be(WellknownSrids.Lambert08);
    }

    [Fact]
    public async Task WhenRequestExtractByContourV2_WithInvalidRequest_ThenValidationException()
    {
        var act = () => Controller.ExtractDownloadaanvraagPerContourV2(
            new ExtractDownloadaanvraagPerContourBody(default, default, default, default),
            new ExtractDownloadaanvraagPerContourBodyValidator());

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task WhenRequestExtractByFileV2_ThenTheBijhoudingArchiveIsRequestedInLambert08()
    {
        var sentRequest = CaptureRequestExtract();

        var result = await Controller.ExtractDownloadaanvraagPerBestandV2(
            new ExtractDownloadaanvraagPerBestandBody(Fixture.Create<string>(), await ContourFiles(), true),
            new ExtractDownloadaanvraagPerBestandValidator(Encoding.UTF8),
            ShapefileContourReaderReturning(ContourInLambert72));

        Assert.IsType<AcceptedResult>(result);
        sentRequest.Value!.ZipArchiveWriterVersion.Should().Be(WellKnownZipArchiveWriterVersions.DomainV2_Bijhouding);
        sentRequest.Value.Contour.SRID.Should().Be(WellknownSrids.Lambert08);
    }

    // The same check the v1 endpoint got: a contour that reads fine but does not hold up as a geometry must not be
    // stored, and it is checked before the reference system is changed so a contour that was already broken is not
    // reported as something the conversion did.
    [Fact]
    public async Task WhenRequestExtractByFileV2_WithSelfIntersectingContour_ThenValidationException()
    {
        const string selfIntersectingContour = "MULTIPOLYGON (((194477.00366637472 206971.83339355365, 194469.6805155593 206969.91542548305, 194452.85470475757 206975.14624749395, 193640.4208660975 206778.9904220851, 193345.57686541631 206711.33845741072, 193326.39718470967 206705.75891393243, 193324.3048559053 206719.18469042706, 193409.567254683 206737.66692819892, 193820.53550400632 206827.11398458539, 194105.96402506795 206891.80181678684, 194197.32904952508 206912.02766189564, 194470.37795849424 206988.65920435538, 194475.26005903777 206978.19756033356, 194477.00366637472 206971.83339355365)))";

        var bestanden = await ContourFiles();

        var act = () => Controller.ExtractDownloadaanvraagPerBestandV2(
            new ExtractDownloadaanvraagPerBestandBody(Fixture.Create<string>(), bestanden, true),
            new ExtractDownloadaanvraagPerBestandValidator(Encoding.UTF8),
            ShapefileContourReaderReturning(selfIntersectingContour));

        var ex = (await act.Should().ThrowAsync<ValidationException>()).Which;
        ex.Errors.Should().ContainSingle(e => e.ErrorCode == "ExtractContourInvalid");
        Mediator.Verify(x => x.Send(It.IsAny<RequestExtractSqsRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private StrongBox<RequestExtractSqsRequest> CaptureRequestExtract()
    {
        var sentRequest = new StrongBox<RequestExtractSqsRequest>();
        Mediator
            .Setup(x => x.Send(It.IsAny<RequestExtractSqsRequest>(), It.IsAny<CancellationToken>()))
            .Callback<IRequest<LocationResult>, CancellationToken>((request, _) => sentRequest.Value = (RequestExtractSqsRequest)request)
            .ReturnsAsync(Fixture.Create<LocationResult>());

        return sentRequest;
    }

    private static IExtractShapefileContourReader ShapefileContourReaderReturning(string contour)
    {
        var reader = new Mock<IExtractShapefileContourReader>();
        reader
            .Setup(x => x.Read(It.IsAny<Stream>(), It.IsAny<GeometryFactory>()))
            .Returns(new WKTReader().Read(contour));

        return reader.Object;
    }

    private static async Task<FormFileCollection> ContourFiles()
    {
        return
        [
            await EmbeddedResourceReader.ReadFormFileAsync("polygon.shp", "application/octet-stream", CancellationToken.None),
            await EmbeddedResourceReader.ReadFormFileAsync("polygon.prj", "application/octet-stream", CancellationToken.None)
        ];
    }
}
