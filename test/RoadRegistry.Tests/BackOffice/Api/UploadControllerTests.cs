namespace RoadRegistry.BackOffice.Api
{
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using BackOffice.Framework;
    using BackOffice.Uploads;
    using Be.Vlaanderen.Basisregisters.BlobStore;
    using Be.Vlaanderen.Basisregisters.BlobStore.Memory;
    using Core;
    using Messages;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Primitives;
    using Newtonsoft.Json;
    using NodaTime;
    using SqlStreamStore;
    using SqlStreamStore.Streams;
    using Uploads;
    using Xunit;

    public class UploadControllerTests
    {
        private class FakeRoadNetworkSnapshotReader : IRoadNetworkSnapshotReader
        {
            public Task<(RoadNetworkSnapshot snapshot, int version)> ReadSnapshot(CancellationToken cancellationToken)
            {
                return Task.FromResult<(RoadNetworkSnapshot snapshot, int version)>((null, ExpectedVersion.NoStream));
            }
        }

        [Fact]
        public async Task When_uploading_a_file_that_is_not_a_zip()
        {
            var client = new MemoryBlobClient();
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
            var controller = new UploadController(
                Dispatch.Using(resolver),
                client)
            {
                ControllerContext = new ControllerContext {HttpContext = new DefaultHttpContext()}
            };
            var formFile = new FormFile(new MemoryStream(), 0L, 0L, "name", "name")
            {
                Headers = new HeaderDictionary(new Dictionary<string, StringValues>
                {
                    { "Content-Type", StringValues.Concat(StringValues.Empty, "application/octet-stream")}
                })
            };
            var result = await controller.Post(formFile);

            Assert.IsType<UnsupportedMediaTypeResult>(result);
        }

        [Theory]
        [InlineData("application/zip")]
        [InlineData("application/x-zip-compressed")]
        public async Task When_uploading_a_file_that_is_a_zip(string contentType)
        {
            var client = new MemoryBlobClient();
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
            var controller = new UploadController(Dispatch.Using(resolver),
                client)
            {
                ControllerContext = new ControllerContext {HttpContext = new DefaultHttpContext()}
            };

            using (var sourceStream = new MemoryStream())
            {
                using (var archive = new ZipArchive(sourceStream, ZipArchiveMode.Create, true, Encoding.UTF8))
                {
                    var entry = archive.CreateEntry("entry");
                    using (var entryStream = entry.Open())
                    {
                        entryStream.Write(new byte[] {1, 2, 3, 4});
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
                var result = await controller.Post(formFile);

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

        [Fact]
        public async Task When_uploading_an_externally_created_file_that_is_a_zip()
        {
            var client = new MemoryBlobClient();
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
            var controller = new UploadController(
                Dispatch.Using(resolver),
                client)
            {
                ControllerContext = new ControllerContext {HttpContext = new DefaultHttpContext()}
            };

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
                var result = await controller.Post(formFile);

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
    }
}
