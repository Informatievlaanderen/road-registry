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
using RoadRegistry.BackOffice.Api.Tests.Abstractions;
using SqlStreamStore;

[Collection(nameof(SqlServerCollection))]
public class DownloadControllerTests : ControllerTests<DownloadController>
{
    private readonly SqlServer _fixture;
    private readonly CancellationTokenSource _tokenSource;

    public DownloadControllerTests(SqlServer fixture, IMediator mediator, IStreamStore streamStore, RoadNetworkUploadsBlobClient uploadClient, RoadNetworkExtractUploadsBlobClient extractUploadClient)
        : base(mediator, streamStore, uploadClient, extractUploadClient)
    {
        _fixture = fixture ?? throw new ArgumentNullException(nameof(fixture));
        _tokenSource = new CancellationTokenSource();
    }

    [Fact]
    public async Task When_downloading_editor_archive_before_an_import()
    {
        await using var context = await _fixture.CreateEmptyEditorContextAsync(await _fixture.CreateDatabaseAsync());
        var result = await Controller.Get(_tokenSource.Token);
        var statusCodeResult = Assert.IsType<StatusCodeResult>(result);
        Assert.Equal(StatusCodes.Status503ServiceUnavailable, statusCodeResult.StatusCode);
    }

    [Fact]
    public async Task When_downloading_editor_archive_during_an_import()
    {
        var database = await _fixture.CreateDatabaseAsync();
        await using (var context = await _fixture.CreateEmptyEditorContextAsync(database))
        {
            context.RoadNetworkInfo.Add(new RoadNetworkInfo
            {
                Id = 0,
                CompletedImport = false
            });
            await context.SaveChangesAsync();
        }

        await using (var context = await _fixture.CreateEditorContextAsync(database))
        {
            var result = await Controller.Get(_tokenSource.Token);
            var statusCodeResult = Assert.IsType<StatusCodeResult>(result);
            Assert.Equal(StatusCodes.Status503ServiceUnavailable, statusCodeResult.StatusCode);
        }
    }

    [Fact]
    public async Task When_downloading_editor_archive_after_an_import()
    {
        var database = await _fixture.CreateDatabaseAsync();
        await using (var context = await _fixture.CreateEmptyEditorContextAsync(database))
        {
            context.RoadNetworkInfo.Add(new RoadNetworkInfo
            {
                Id = 0,
                CompletedImport = true
            });
            await context.SaveChangesAsync();
        }

        await using (var context = await _fixture.CreateEditorContextAsync(database))
        {
            var result = await Controller.Get(_tokenSource.Token);
            var fileCallbackResult = Assert.IsType<FileCallbackResult>(result);
            Assert.Equal("wegenregister.zip", fileCallbackResult.FileDownloadName);
        }
    }

    [Fact]
    public async Task When_downloading_product_archive_before_an_import()
    {
        await using var context = await _fixture.CreateEmptyProductContextAsync(await _fixture.CreateDatabaseAsync());
        var version = DateTime.Today.ToString("yyyyMMdd");
        var result = await Controller.Get(_tokenSource.Token);

        var statusCodeResult = Assert.IsType<StatusCodeResult>(result);
        Assert.Equal(StatusCodes.Status503ServiceUnavailable, statusCodeResult.StatusCode);
    }

    [Fact]
    public async Task When_downloading_product_archive_during_an_import()
    {
        var database = await _fixture.CreateDatabaseAsync();
        await using (var context = await _fixture.CreateEmptyProductContextAsync(database))
        {
            context.RoadNetworkInfo.Add(new RoadNetworkInfo
            {
                Id = 0,
                CompletedImport = false
            });
            await context.SaveChangesAsync();
        }

        await using (var context = await _fixture.CreateProductContextAsync(database))
        {
            var version = DateTime.Today.ToString("yyyyMMdd");
            var result = await Controller.Get(_tokenSource.Token);
            var statusCodeResult = Assert.IsType<StatusCodeResult>(result);
            Assert.Equal(StatusCodes.Status503ServiceUnavailable, statusCodeResult.StatusCode);
        }
    }

    [Fact]
    public async Task When_downloading_product_archive_after_an_import()
    {
        var database = await _fixture.CreateDatabaseAsync();
        await using (var context = await _fixture.CreateEmptyProductContextAsync(database))
        {
            context.RoadNetworkInfo.Add(new RoadNetworkInfo
            {
                Id = 0,
                CompletedImport = true
            });
            await context.SaveChangesAsync();
        }

        await using (var context = await _fixture.CreateProductContextAsync(database))
        {
            var version = DateTime.Today.ToString("yyyyMMdd");
            var result = await Controller.Get(_tokenSource.Token);
            var fileCallbackResult = Assert.IsType<FileCallbackResult>(result);
            Assert.Equal($"wegenregister-{version}.zip", fileCallbackResult.FileDownloadName);
        }
    }
}
