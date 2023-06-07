namespace RoadRegistry.BackOffice.Api.Tests.Extracts;

using Abstractions.Exceptions;
using Be.Vlaanderen.Basisregisters.BlobStore;
using FluentValidation;
using Messages;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SqlStreamStore.Streams;

public partial class ExtractsControllerTests
{
    [Fact]
    public async Task When_uploading_an_extract_after_fc_that_is_an_empty_zip()
    {
        try
        {
            await using var sourceStream = await EmbeddedResourceReader.ReadAsync("empty.zip");
            var formFile = EmbeddedResourceReader.ReadFormFile(sourceStream, "name", "application/zip");

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
        var formFile = EmbeddedResourceReader.ReadFormFile(new MemoryStream(), "name", "application/octet-stream");
        var result = await Controller.Upload(
            "not_a_guid_without_dashes",
            formFile,
            CancellationToken.None);

        Assert.IsType<UnsupportedMediaTypeResult>(result);
    }
}