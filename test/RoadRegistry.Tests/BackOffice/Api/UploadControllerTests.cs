namespace RoadRegistry.BackOffice.Api
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using BackOffice.Extracts;
    using BackOffice.Framework;
    using BackOffice.Uploads;
    using Be.Vlaanderen.Basisregisters.BlobStore;
    using Be.Vlaanderen.Basisregisters.BlobStore.Memory;
    using Editor.Schema;
    using Editor.Schema.Extracts;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Primitives;
    using Moq;
    using Newtonsoft.Json;
    using NodaTime;
    using SqlStreamStore;
    using SqlStreamStore.Streams;
    using Uploads;
    using Xunit;

    public class UploadControllerTests : ControllerTests<UploadController>
    {
        protected override Mock<EditorContext> ConfigureEditorContextMock()
        {
            var mock = base.ConfigureEditorContextMock();
            mock.Setup(context => context.ExtractDownloads.FindAsync(It.IsAny<object[]>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ExtractDownloadRecord()
                {
                    RequestId = "",
                    RequestedOn = DateTime.UtcNow.Ticks,
                    ArchiveId = "",
                    Available = true,
                    AvailableOn = DateTime.UtcNow.Ticks,
                    DownloadId = Guid.Empty,
                    ExternalRequestId = "",
                });
            return mock;
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task When_uploading_a_file_that_is_not_a_zip(bool featureCompare)
        {
            var formFile = new FormFile(new MemoryStream(), 0L, 0L, "name", "name")
            {
                Headers = new HeaderDictionary(new Dictionary<string, StringValues>
                {
                    { "Content-Type", StringValues.Concat(StringValues.Empty, "application/octet-stream")}
                })
            };

            var result = await Controller.PostUpload(formFile, featureCompare, CancellationToken.None);
            Assert.IsType<UnsupportedMediaTypeResult>(result);
        }

        [Theory]
        [InlineData("application/zip", false)]
        [InlineData("application/x-zip-compressed", false)]
        [InlineData("application/zip", true)]
        [InlineData("application/x-zip-compressed", true)]
        public async Task When_uploading_a_file_that_is_a_zip(string contentType, bool featureCompare)
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
                    using (var entryStream = entry.Open())
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
                        {"Content-Type", StringValues.Concat(StringValues.Empty, contentType)}
                    })
                };
                var result = await Controller.PostUpload(formFile, featureCompare, CancellationToken.None);

                Assert.IsType<OkResult>(result);

                var page = await store.ReadAllForwards(Position.Start, 1, true);
                var message = Assert.Single(page.Messages);
                Assert.Equal(nameof(Messages.RoadNetworkChangesArchiveUploaded), message.Type);
                var uploaded =
                    JsonConvert.DeserializeObject<Messages.RoadNetworkChangesArchiveUploaded>(
                        await message.GetJsonData());

                Assert.True(await client.BlobExistsAsync(new BlobName(uploaded.ArchiveId)));
                var blob = await client.GetBlobAsync(new BlobName(uploaded.ArchiveId));
                using (var openStream = await blob.OpenAsync())
                {
                    var resultStream = new MemoryStream();
                    openStream.CopyTo(resultStream);
                    resultStream.Position = 0;
                    sourceStream.Position = 0;
                    Assert.Equal(sourceStream.ToArray(), resultStream.ToArray());
                }
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task When_uploading_an_externally_created_file_that_is_a_zip(bool featureCompare)
        {
            using (var sourceStream = new MemoryStream())
            {
                using (var embeddedStream =
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
                        {"Content-Type", StringValues.Concat(StringValues.Empty, "application/zip")}
                    })
                };
                var result = await Controller.PostUpload(formFile, featureCompare, CancellationToken.None);

                Assert.IsType<OkResult>(result);

                var page = await StreamStore.ReadAllForwards(Position.Start, 1, true);
                var message = Assert.Single(page.Messages);
                Assert.Equal(nameof(Messages.RoadNetworkChangesArchiveUploaded), message.Type);
                var uploaded =
                    JsonConvert.DeserializeObject<Messages.RoadNetworkChangesArchiveUploaded>(
                        await message.GetJsonData());

                Assert.True(await UploadBlobClient.BlobExistsAsync(new BlobName(uploaded.ArchiveId)));
                var blob = await UploadBlobClient.GetBlobAsync(new BlobName(uploaded.ArchiveId));
                using (var openStream = await blob.OpenAsync())
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
}
