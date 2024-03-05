namespace RoadRegistry.BackOffice.Api.Tests.Extracts;

using Abstractions.Extracts;
using FluentAssertions;
using FluentValidation;
using Handlers.Extracts;
using ContentType = Be.Vlaanderen.Basisregisters.BlobStore.ContentType;

public class DownloadExtractByFileRequestItemTranslatorTests : IAsyncLifetime
{
    private const int ValidBuffer = 50;
    
    private readonly IDownloadExtractByFileRequestItemTranslator _translator = new DownloadExtractByFileRequestItemTranslator();
    private DownloadExtractByFileRequestItem _prjFilePoint;
    private DownloadExtractByFileRequestItem _prjFilePolygon;
    private DownloadExtractByFileRequestItem _shpFilePoint;
    private DownloadExtractByFileRequestItem _shpFilePolygon;
    private DownloadExtractByFileRequestItem _shpFilePolygonCounterClockWise;
    private DownloadExtractByFileRequestItem _shpFilePolygonNetTopologySuite;

    public async Task DisposeAsync()
    {
        await _prjFilePolygon.ReadStream.DisposeAsync();
        await _shpFilePolygon.ReadStream.DisposeAsync();
        await _prjFilePoint.ReadStream.DisposeAsync();
        await _shpFilePoint.ReadStream.DisposeAsync();
        await _shpFilePolygonCounterClockWise.ReadStream.DisposeAsync();
        await _shpFilePolygonNetTopologySuite.ReadStream.DisposeAsync();
    }

    public async Task InitializeAsync()
    {
        _prjFilePolygon = await GetDownloadExtractByFileRequestItemFromResource("polygon.prj");
        _shpFilePolygon = await GetDownloadExtractByFileRequestItemFromResource("polygon.shp");
        _shpFilePolygonCounterClockWise = await GetDownloadExtractByFileRequestItemFromResource("polygon-ccw.shp");
        _shpFilePolygonNetTopologySuite = await GetDownloadExtractByFileRequestItemFromResource("polygon-nettopologysuite.shp");
        _prjFilePoint = await GetDownloadExtractByFileRequestItemFromResource("point.prj");
        _shpFilePoint = await GetDownloadExtractByFileRequestItemFromResource("point.shp");
    }

    private async Task<DownloadExtractByFileRequestItem> GetDownloadExtractByFileRequestItemFromResource(string name)
    {
        return new DownloadExtractByFileRequestItem(name, await EmbeddedResourceReader.ReadAsync(name), ContentType.Parse("application/octet-stream"));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(99)]
    [InlineData(100)]
    public async Task Translate_will_allow_valid_buffer(int givenBuffer)
    {
        var act = () => Task.FromResult(_translator.Translate(_shpFilePolygon, givenBuffer));
        await act.Should().NotThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Translate_will_allow_valid_geometry_type()
    {
        var act = () => Task.FromResult(_translator.Translate(_shpFilePolygon, ValidBuffer));
        await act.Should().NotThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Translate_will_allow_valid_counterclockwise_polygon()
    {
        var act = () => Task.FromResult(_translator.Translate(_shpFilePolygonCounterClockWise, ValidBuffer));
        await act.Should().NotThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Translate_will_allow_valid_nettopologysuite_polygon()
    {
        var act = () => Task.FromResult(_translator.Translate(_shpFilePolygonNetTopologySuite, ValidBuffer));
        await act.Should().NotThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Translate_will_not_allow_empty_file()
    {
        var shpFileEmpty = await GetDownloadExtractByFileRequestItemFromResource("empty-polygon.shp");
        using (shpFileEmpty.ReadStream)
        {
            var act = () => Task.FromResult(_translator.Translate(shpFileEmpty, ValidBuffer));
            await act.Should().ThrowAsync<ValidationException>();
        }
    }

    [Fact]
    public async Task Translate_will_not_allow_invalid_file()
    {
        var zipFile = await GetDownloadExtractByFileRequestItemFromResource("empty.zip");
        using (zipFile.ReadStream)
        {
            var act = () => Task.FromResult(_translator.Translate(zipFile, ValidBuffer));
            await act.Should().ThrowAsync<ValidationException>();
        }
    }

    [Fact]
    public async Task Translate_will_not_allow_invalid_geometry_type()
    {
        var act = () => Task.FromResult(_translator.Translate(_shpFilePoint, ValidBuffer));
        await act.Should().ThrowAsync<ValidationException>();
    }
}
