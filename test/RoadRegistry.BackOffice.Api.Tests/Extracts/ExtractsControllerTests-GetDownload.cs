namespace RoadRegistry.BackOffice.Api.Tests.Extracts;

using Abstractions;
using FluentValidation;
using Xunit.Sdk;

public partial class ExtractsControllerTests
{
    // TODO: Figure out how to use Geometry with InMemoryDatabase (or switch to integration testing)
    // [Fact]
    // public async Task When_downloading_an_extract_using_an_unknown_download_id()
    // {
    //     var wktReader = new WKTReader(new NtsGeometryServices(GeometryConfiguration.GeometryFactory.PrecisionModel, GeometryConfiguration.GeometryFactory.SRID));
    //     var validator =
    //         new DownloadExtractRequestBodyValidator(wktReader,
    //             new NullLogger<DownloadExtractRequestBodyValidator>());
    //     var controller = new ExtractsController(SystemClock.Instance,Dispatch.Using(_resolver), _downloadClient, _uploadClient, wktReader, validator)
    //     {
    //         ControllerContext = new ControllerContext {HttpContext = new DefaultHttpContext()}
    //     };
    //     var context = new EditorContext();
    //     var response = await controller.GetDownload(
    //         context,
    //         new ExtractDownloadsOptions(),
    //         "8393620921e14ad49813dacb59ba850d");
    //     Assert.IsType<NotFoundResult>(response);
    // }
    //
    // [Fact]
    // public async Task When_downloading_an_extract_that_is_not_yet_available(){}
    //
    // [Fact]
    // public async Task When_downloading_an_extract_that_is_available(){}

    [Fact]
    public async Task When_downloading_an_extract_using_an_malformed_download_id()
    {
        try
        {
            await Controller.GetDownload(
                "not_a_guid_without_dashes",
                new ExtractDownloadsOptions(),
                CancellationToken.None);
            throw new XunitException("Expected a validation exception but did not receive any");
        }
        catch (ValidationException)
        {
        }
    }
}