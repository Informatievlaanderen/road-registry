namespace RoadRegistry.BackOffice.Api;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Abstractions;
using Dbase;
using Downloads;
using Framework;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RoadRegistry.Framework.Containers;
using Xunit;

[Collection(nameof(SqlServerCollection))]
public class DownloadControllerTests : ControllerTests<DownloadController>
{
    private readonly SqlServer _fixture;
    private readonly CancellationTokenSource _tokenSource;

    public DownloadControllerTests(SqlServer fixture)
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

public class StreetNameCacheStub : IStreetNameCache
{
    public Task<Dictionary<int, string>> GetStreetNamesById(IEnumerable<int> streetNameIds, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
