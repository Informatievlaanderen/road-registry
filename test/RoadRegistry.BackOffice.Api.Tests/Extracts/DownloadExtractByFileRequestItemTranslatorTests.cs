namespace RoadRegistry.BackOffice.Api.Tests.Extracts;

using System.Text;
using BackOffice.Abstractions.Extracts;
using Be.Vlaanderen.Basisregisters.BlobStore;
using FluentAssertions;
using FluentValidation;
using Handlers.Extracts;
using Microsoft.Extensions.Logging;

public class DownloadExtractByFileRequestItemTranslatorTests : IAsyncLifetime
{
    private const int ValidBuffer = 50;
    private readonly Encoding _encoding = Encoding.UTF8;
    private readonly DownloadExtractByFileRequestItemTranslator _translator;
    private DownloadExtractByFileRequestItem _prjFilePoint;
    private DownloadExtractByFileRequestItem _prjFilePolygon;
    private DownloadExtractByFileRequestItem _shpFilePoint;
    private DownloadExtractByFileRequestItem _shpFilePolygon;

    public DownloadExtractByFileRequestItemTranslatorTests(ILogger<DownloadExtractByFileRequestItemTranslator> logger)
    {
        _translator = new DownloadExtractByFileRequestItemTranslator(_encoding, logger);
    }

    public async Task DisposeAsync()
    {
        await _prjFilePolygon.ReadStream.DisposeAsync();
        await _shpFilePolygon.ReadStream.DisposeAsync();
        await _prjFilePoint.ReadStream.DisposeAsync();
        await _shpFilePoint.ReadStream.DisposeAsync();
    }

    public async Task InitializeAsync()
    {
        _prjFilePolygon = await GetDownloadExtractByFileRequestItemFromResource("polygon.prj");
        _shpFilePolygon = await GetDownloadExtractByFileRequestItemFromResource("polygon.shp");
        _prjFilePoint = await GetDownloadExtractByFileRequestItemFromResource("point.prj");
        _shpFilePoint = await GetDownloadExtractByFileRequestItemFromResource("point.shp");
    }

    private async Task<DownloadExtractByFileRequestItem> GetDownloadExtractByFileRequestItemFromResource(string name)
    {
        return new DownloadExtractByFileRequestItem(name, await GetEmbeddedResourceStream(name), ContentType.Parse("application/octet-stream"));
    }

    private async Task<MemoryStream> GetEmbeddedResourceStream(string name)
    {
        var sourceStream = new MemoryStream();

        await using (var embeddedStream = typeof(ExtractControllerTests).Assembly.GetManifestResourceStream(typeof(ExtractControllerTests), name))
        {
            embeddedStream.CopyTo(sourceStream);
        }

        sourceStream.Position = 0;

        return sourceStream;
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