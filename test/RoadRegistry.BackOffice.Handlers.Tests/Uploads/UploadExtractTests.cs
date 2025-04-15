namespace RoadRegistry.BackOffice.Handlers.Tests.Uploads
{
    using Abstractions.Exceptions;
    using Abstractions.Uploads;
    using BackOffice.Uploads;
    using Be.Vlaanderen.Basisregisters.BlobStore;
    using Editor.Schema;
    using FeatureCompare;
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
                new TransactionZoneZipArchiveReader(new(FileEncoding.UTF8), new (FileEncoding.UTF8)),
                new FakeEditorContext(),
                Mock.Of<IZipArchiveBeforeFeatureCompareValidatorFactory>(),
                Mock.Of<IRoadNetworkCommandQueue>(),
                Mock.Of<IBeforeFeatureCompareZipArchiveCleanerFactory>(),
                Mock.Of<ILogger<UploadExtractRequestHandler>>()
            );

            // Assert
            await Assert.ThrowsAsync<UnsupportedMediaTypeException>(() => sut.Handle(request, CancellationToken.None));
        }
    }
}
