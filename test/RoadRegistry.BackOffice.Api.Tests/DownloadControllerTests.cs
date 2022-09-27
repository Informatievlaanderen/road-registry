namespace RoadRegistry.BackOffice.Api.Tests;

using Api.Downloads;
using Api.Framework;
using Autofac;
using Autofac.Core;
using BackOffice.Extracts;
using BackOffice.Uploads;
using Dbase;
using Editor.Schema;
using Framework.Containers;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Product.Schema;
using RoadRegistry.BackOffice.Api.Tests.Abstractions;
using SqlStreamStore;

[Collection(nameof(SqlServerCollection))]
public class DownloadControllerTests : ControllerTests<DownloadController>
{
    private readonly SqlServer _fixture;
    private readonly EditorContext _editorContext;
    private readonly ProductContext _productContext;
    private readonly CancellationTokenSource _tokenSource;

    public DownloadControllerTests(
        SqlServer fixture,
        EditorContext editorContext,
        ProductContext productContext,
        IMediator mediator,
        IStreamStore streamStore,
        RoadNetworkUploadsBlobClient uploadClient,
        RoadNetworkExtractUploadsBlobClient extractUploadClient)
        : base(mediator, streamStore, uploadClient, extractUploadClient)
    {
        _fixture = fixture ?? throw new ArgumentNullException(nameof(fixture));
        _tokenSource = new CancellationTokenSource();
        _editorContext = editorContext ?? throw new ArgumentNullException(nameof(editorContext));
        _productContext = productContext ?? throw new ArgumentNullException(nameof(productContext));
    }

    [Fact]
    public async Task When_downloading_editor_archive_before_an_import()
    {
        var result = await Controller.Get(_tokenSource.Token);
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

        var result = await Controller.Get(_tokenSource.Token);
        var statusCodeResult = Assert.IsType<StatusCodeResult>(result);
        Assert.Equal(StatusCodes.Status503ServiceUnavailable, statusCodeResult.StatusCode);
    }

    [Fact]
    public async Task When_downloading_editor_archive_after_an_import()
    {
        _editorContext.RoadNetworkInfo.Add(new RoadNetworkInfo
        {
            Id = 0,
            CompletedImport = true
        });
        await _editorContext.SaveChangesAsync();

        var result = await Controller.Get(_tokenSource.Token);
        var fileCallbackResult = Assert.IsType<FileCallbackResult>(result);

        var filename = $"wegenregister-{DateTime.Today.ToString("yyyyMMdd")}.zip";
        Assert.Equal(filename, fileCallbackResult.FileDownloadName);
    }

    [Fact]
    public async Task When_downloading_product_archive_before_an_import()
    {
        var result = await Controller.Get(_tokenSource.Token);
        var statusCodeResult = Assert.IsType<StatusCodeResult>(result);
        Assert.Equal(StatusCodes.Status503ServiceUnavailable, statusCodeResult.StatusCode);
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

        var result = await Controller.Get(_tokenSource.Token);
        var statusCodeResult = Assert.IsType<StatusCodeResult>(result);
        Assert.Equal(StatusCodes.Status503ServiceUnavailable, statusCodeResult.StatusCode);
    }

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
        var result = await Controller.Get(version, _tokenSource.Token);
        var fileCallbackResult = Assert.IsType<FileCallbackResult>(result);
        Assert.Equal($"wegenregister-{version}.zip", fileCallbackResult.FileDownloadName);
    }
}
