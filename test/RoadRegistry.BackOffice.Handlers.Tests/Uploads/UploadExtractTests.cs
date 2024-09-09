namespace RoadRegistry.BackOffice.Handlers.Tests.Uploads
{
    using Abstractions.Exceptions;
    using Abstractions.Uploads;
    using Amazon.Runtime.Internal.Util;
    using BackOffice.Uploads;
    using Be.Vlaanderen.Basisregisters.BlobStore;
    using Editor.Schema;
    using FeatureCompare.Readers;
    using Handlers.Uploads;
    using Microsoft.Extensions.Logging;
    using Moq;
    using ZipArchiveWriters.Cleaning;

    public class UploadExtractTests
    {
        [Fact]
        public async Task GivenNotZipFile_ThenThrowsUnsupportedMediaTypeException()
        {
            // Arrange
            var request = new UploadExtractRequest(new UploadExtractArchiveRequest("test.zip", EmbeddedResourceReader.Read("test.txt"), ContentType.Parse("binary/octet-stream")), null);

            // Act
            var sut = new UploadExtractRequestHandler(
                new RoadNetworkUploadsBlobClient(Mock.Of<IBlobClient>()),
                new TransactionZoneFeatureCompareFeatureReader(FileEncoding.UTF8),
                new FakeEditorContext(),
                Mock.Of<IZipArchiveBeforeFeatureCompareValidator>(),
                Mock.Of<IRoadNetworkCommandQueue>(),
                Mock.Of<IBeforeFeatureCompareZipArchiveCleaner>(),
                Mock.Of<ILogger<UploadExtractRequestHandler>>()
            );

            // Assert
            await Assert.ThrowsAsync<UnsupportedMediaTypeException>(() => sut.HandleAsync(request, CancellationToken.None));
        }
    }
}
