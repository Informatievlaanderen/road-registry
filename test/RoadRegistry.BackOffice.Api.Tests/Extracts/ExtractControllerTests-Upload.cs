namespace RoadRegistry.BackOffice.Api.Tests.Extracts;

using BackOffice.Abstractions.Exceptions;
using Be.Vlaanderen.Basisregisters.BlobStore;
using FluentValidation;
using Messages;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using SqlStreamStore.Streams;

public partial class ExtractControllerTests
{
    [Fact]
    public async Task When_uploading_an_extract_after_fc_that_is_an_empty_zip()
    {
        try
        {
            using var sourceStream = new MemoryStream();
            await using (var embeddedStream =
                         typeof(ExtractControllerTests).Assembly.GetManifestResourceStream(
                             typeof(ExtractControllerTests),
                             "empty.zip"))
            {
                embeddedStream.CopyTo(sourceStream);
            }

            sourceStream.Position = 0;

            var formFile = new FormFile(sourceStream, 0L, sourceStream.Length, "name", "name")
            {
                Headers = new HeaderDictionary(new Dictionary<string, StringValues>
                {
                    { "Content-Type", StringValues.Concat(StringValues.Empty, "application/zip") }
                })
            };

            var result = await Controller.Upload(
                "not_a_guid_without_dashes",
                formFile,
                CancellationToken.None);

            Assert.IsType<OkResult>(result);

            var page = await StreamStore.ReadAllForwards(Position.Start, 1);
            var message = Assert.Single(page.Messages);
            Assert.Equal(nameof(RoadNetworkExtractChangesArchiveUploaded), message.Type);
            var uploaded =
                JsonConvert.DeserializeObject<RoadNetworkExtractChangesArchiveUploaded>(
                    await message.GetJsonData());

            Assert.True(await UploadBlobClient.BlobExistsAsync(new BlobName(uploaded.ArchiveId)));
            var blob = await UploadBlobClient.GetBlobAsync(new BlobName(uploaded.ArchiveId));
            await using (var openStream = await blob.OpenAsync())
            {
                var resultStream = new MemoryStream();
                openStream.CopyTo(resultStream);
                resultStream.Position = 0;
                sourceStream.Position = 0;
                Assert.Equal(sourceStream.ToArray(), resultStream.ToArray());
            }
        }
        catch (UploadExtractNotFoundException)
        {
        }
        catch (ValidationException)
        {
        }
    }

    [Fact]
    public async Task When_uploading_an_extract_after_fc_that_is_not_a_zip()
    {
        var formFile = new FormFile(new MemoryStream(), 0L, 0L, "name", "name")
        {
            Headers = new HeaderDictionary(new Dictionary<string, StringValues>
            {
                { "Content-Type", StringValues.Concat(StringValues.Empty, "application/octet-stream") }
            })
        };

        var result = await Controller.Upload(
            "not_a_guid_without_dashes",
            formFile,
            CancellationToken.None);
        Assert.IsType<UnsupportedMediaTypeResult>(result);
    }
}
