namespace RoadRegistry.BackOffice.Api.Tests.Downloads;

using System;
using System.Threading.Tasks;
using Api.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

public partial class DownloadControllerTests
{
    [Fact]
    public async Task When_downloading_product_archive_after_an_import()
    {
        _productContext.RoadNetworkInfo.Add(new RoadNetworkInfo
        {
            Id = 0,
            CompletedImport = true
        });
        await _productContext.SaveChangesAsync();

        var version = DateTime.Today.ToString("yyyyMMdd");
        var result = await Controller.GetForProduct(version, _tokenSource.Token);
        var fileCallbackResult = Assert.IsType<FileCallbackResult>(result);
        Assert.Equal($"wegenregister-{version}.zip", fileCallbackResult.FileDownloadName);
    }

    [Fact]
    public async Task When_downloading_product_archive_before_an_import()
    {
        var version = DateTime.Today.ToString("yyyyMMdd");
        var result = await Controller.GetForProduct(version, _tokenSource.Token);
        var notFoundResult = Assert.IsType<NotFoundResult>(result);
        Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
    }

    [Fact]
    public async Task When_downloading_product_archive_during_an_import()
    {
        _productContext.RoadNetworkInfo.Add(new RoadNetworkInfo
        {
            Id = 0,
            CompletedImport = false
        });
        await _productContext.SaveChangesAsync();

        var version = DateTime.Today.ToString("yyyyMMdd");
        var result = await Controller.GetForProduct(version, _tokenSource.Token);
        var notFoundResult = Assert.IsType<NotFoundResult>(result);
        Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
    }
}
