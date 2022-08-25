namespace RoadRegistry.BackOffice.Api.Tests;

using BackOffice.Framework;
using BackOffice.Uploads;
using Be.Vlaanderen.Basisregisters.BlobStore;
using Be.Vlaanderen.Basisregisters.BlobStore.Memory;
using MediatR;
using Messages;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using NodaTime;
using RoadRegistry.BackOffice.Api.Tests.Abstractions;
using RoadRegistry.BackOffice.Extracts;
using SqlStreamStore;
using SqlStreamStore.Streams;
using System.IO.Compression;
using System.Text;
using Uploads;

public class UploadControllerTests : ControllerTests<UploadController>
{
    public UploadControllerTests(IMediator mediator, IStreamStore streamStore, RoadNetworkUploadsBlobClient uploadClient, RoadNetworkExtractUploadsBlobClient extractUploadClient)
        : base(mediator, streamStore, uploadClient, extractUploadClient)
    {
    }

    [Fact]
    public async Task When_uploading_a_file_that_is_not_a_zip()
    {
        var formFile = new FormFile(new MemoryStream(), 0L, 0L, "name", "name")
        {
            Headers = new HeaderDictionary(new Dictionary<string, StringValues>
            {
                { "Content-Type", StringValues.Concat(StringValues.Empty, "application/octet-stream") }
            })
        };

        var result = await Controller.PostUpload(formFile, CancellationToken.None);
        Assert.IsType<UnsupportedMediaTypeResult>(result);
    }

    [Theory]
    [InlineData("application/zip")]
    [InlineData("application/x-zip-compressed")]
    public async Task When_uploading_a_file_that_is_a_zip(string contentType)
    {
        var client = new RoadNetworkUploadsBlobClient(new MemoryBlobClient());
        var store = new InMemoryStreamStore();
        var validator = new ZipArchiveValidator(Encoding.UTF8);
        var resolver = Resolve.WhenEqualToMessage(
            new RoadNetworkChangesArchiveCommandModule(
                client,
                store,
                new FakeRoadNetworkSnapshotReader(),
                validator,
                SystemClock.Instance
            )
        );

        using (var sourceStream = new MemoryStream())
        {
            using (var archive = new ZipArchive(sourceStream, ZipArchiveMode.Create, true, Encoding.UTF8))
            {
                var entry = archive.CreateEntry("entry");
                await using (var entryStream = entry.Open())
                {
                    entryStream.Write(new byte[] { 1, 2, 3, 4 });
                    entryStream.Flush();
                }
            }

            sourceStream.Position = 0;

            var formFile = new FormFile(sourceStream, 0L, sourceStream.Length, "name", "name")
            {
                Headers = new HeaderDictionary(new Dictionary<string, StringValues>
                {
                    { "Content-Type", StringValues.Concat(StringValues.Empty, contentType) }
                })
            };
            var result = await Controller.PostUpload(formFile, CancellationToken.None);

            Assert.IsType<OkResult>(result);

            var page = await store.ReadAllForwards(Position.Start, 1, true);
            var message = Assert.Single(page.Messages);
            Assert.Equal(nameof(RoadNetworkChangesArchiveUploaded), message.Type);
            var uploaded =
                JsonConvert.DeserializeObject<RoadNetworkChangesArchiveUploaded>(
                    await message.GetJsonData());

            Assert.True(await client.BlobExistsAsync(new BlobName(uploaded.ArchiveId)));
            var blob = await client.GetBlobAsync(new BlobName(uploaded.ArchiveId));
            await using (var openStream = await blob.OpenAsync())
            {
                var resultStream = new MemoryStream();
                openStream.CopyTo(resultStream);
                resultStream.Position = 0;
                sourceStream.Position = 0;
                Assert.Equal(sourceStream.ToArray(), resultStream.ToArray());
            }
        }
    }

    [Fact]
    public async Task When_uploading_an_externally_created_file_that_is_a_zip()
    {
        using (var sourceStream = new MemoryStream())
        {
            await using (var embeddedStream =
                         typeof(UploadControllerTests).Assembly.GetManifestResourceStream(typeof(UploadControllerTests),
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
            var result = await Controller.PostUpload(formFile, CancellationToken.None);

            Assert.IsType<OkObjectResult>(result);

            var page = await StreamStore.ReadAllForwards(Position.Start, 1);
            var message = Assert.Single(page.Messages);
            Assert.Equal(nameof(RoadNetworkChangesArchiveUploaded), message.Type);
            var uploaded =
                JsonConvert.DeserializeObject<RoadNetworkChangesArchiveUploaded>(
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
    }
}
