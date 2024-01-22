namespace RoadRegistry.BackOffice.Api.Tests.Downloads;

using Api.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

public partial class DownloadControllerTests
{
    [Fact]
    public async Task When_downloading_editor_archive_after_an_import()
    {
        _editorContext.RoadNetworkInfo.Add(new RoadNetworkInfo
        {
            Id = 0,
            CompletedImport = true
        });
        await _editorContext.SaveChangesAsync();

        var result = await Controller.GetForEditor(_tokenSource.Token);
        var fileCallbackResult = Assert.IsType<FileCallbackResult>(result);

        var filename = $"wegenregister-{DateTime.Today:yyyyMMdd}.zip";
        Assert.Equal(filename, fileCallbackResult.FileDownloadName);
    }

    [Fact]
    public async Task When_downloading_editor_archive_before_an_import()
    {
        var result = await Controller.GetForEditor(_tokenSource.Token);
        var statusCodeResult = Assert.IsType<StatusCodeResult>(result);
        Assert.Equal(StatusCodes.Status503ServiceUnavailable, statusCodeResult.StatusCode);
    }

    [Fact]
    public async Task When_downloading_editor_archive_during_an_import()
    {
        _editorContext.RoadNetworkInfo.Add(new RoadNetworkInfo
        {
            Id = 0,
            CompletedImport = false
        });
        await _editorContext.SaveChangesAsync();

        var result = await Controller.GetForEditor(_tokenSource.Token);
        var statusCodeResult = Assert.IsType<StatusCodeResult>(result);
        Assert.Equal(StatusCodes.Status503ServiceUnavailable, statusCodeResult.StatusCode);
    }
}
