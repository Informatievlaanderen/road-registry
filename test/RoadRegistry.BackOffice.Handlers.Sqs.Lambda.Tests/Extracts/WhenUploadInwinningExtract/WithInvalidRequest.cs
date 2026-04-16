namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.Extracts.WhenUploadInwinningExtract;

using AutoFixture;
using Be.Vlaanderen.Basisregisters.BlobStore;
using FluentAssertions;
using Moq;
using RoadRegistry.BackOffice.Handlers.Sqs.Extracts;
using RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.UploadExtract;
using RoadRegistry.BackOffice.Uploads;
using RoadRegistry.Extensions;
using RoadRegistry.Tests.BackOffice.Extracts.DomainV2;
using TicketingService.Abstractions;
using Xunit.Abstractions;
using Polygon = NetTopologySuite.Geometries.Polygon;

public class WithInvalidRequest : WhenUploadInwinningExtractTestBase
{
    public WithInvalidRequest(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public async Task WhenTransactionZoneHasChanged_ThenError()
    {
        // Arrange
        var downloadId = Fixture.Create<DownloadId>();
        var extractRequestId = Fixture.Create<ExtractRequestId>();

        ExtractsDbContext.Inwinningszones.Add(new()
        {
            NisCode = ObjectProvider.Create<string>(),
            Contour = Polygon.Empty,
            Operator = ObjectProvider.Create<string>(),
            DownloadId = downloadId,
            Completed = false
        });

        ExtractsDbContext.ExtractRequests.Add(new()
        {
            ExtractRequestId = extractRequestId,
            CurrentDownloadId = downloadId,
            Description = Fixture.Create<string>()
        });
        ExtractsDbContext.ExtractDownloads.Add(new()
        {
            DownloadId = downloadId,
            ExtractRequestId = extractRequestId,
            Contour = Polygon.Empty,
            DownloadedOn = DateTimeOffset.Now
        });
        await ExtractsDbContext.SaveChangesAsync();

        var blobClientMock = new Mock<IBlobClient>();
        blobClientMock
            .Setup(x => x.GetBlobAsync(It.IsAny<BlobName>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BlobObject(new BlobName("archive.zip"),
                Metadata.None,
                ContentType.Parse("application/zip"),
                _ =>
                {
                    var archiveStream = new DomainV2ZipArchiveBuilder()
                        .WithChange((builder, _) =>
                        {
                            builder.TestData.TransactionZoneShapeRecord.Geometry = builder.TestData.TransactionZoneShapeRecord.Geometry.Buffer(-0.1).ToMultiPolygon();
                        })
                        .BuildArchiveStream();
                    return Task.FromResult<Stream>(archiveStream);
                }));

        // Act
        await HandleRequest(new UploadInwinningExtractSqsRequest
            {
                DownloadId = downloadId,
                UploadId = ObjectProvider.Create<UploadId>(),
                ExtractRequestId = extractRequestId
            },
            extractUploader: new ExtractUploader(
                ExtractsDbContext,
                new RoadNetworkUploadsBlobClient(blobClientMock.Object),
                new FakeZipArchiveFeatureCompareTranslator(),
                new FakeExtractUploadFailedEmailClient(),
                TicketingMock.Object
            ));

        // Assert
        TicketingMock.VerifyThatTicketHasError(code: "ErrorTransactionZoneHasChanged");
    }

    [Fact]
    public async Task EnsureAllActualSchijnknopenAreRemoved()
    {
        // Arrange
        var downloadId = Fixture.Create<DownloadId>();
        var extractRequestId = Fixture.Create<ExtractRequestId>();

        ExtractsDbContext.Inwinningszones.Add(new()
        {
            NisCode = ObjectProvider.Create<string>(),
            Contour = Polygon.Empty,
            Operator = ObjectProvider.Create<string>(),
            DownloadId = downloadId,
            Completed = false
        });

        ExtractsDbContext.ExtractRequests.Add(new()
        {
            ExtractRequestId = extractRequestId,
            CurrentDownloadId = downloadId,
            Description = Fixture.Create<string>()
        });
        var transactionZoneGeometry = Fixture.Create<Polygon>().ToMultiPolygon();
        ExtractsDbContext.ExtractDownloads.Add(new()
        {
            DownloadId = downloadId,
            ExtractRequestId = extractRequestId,
            Contour = transactionZoneGeometry,
            DownloadedOn = DateTimeOffset.Now
        });
        await ExtractsDbContext.SaveChangesAsync();

        var blobClientMock = new Mock<IBlobClient>();
        blobClientMock
            .Setup(x => x.GetBlobAsync(It.IsAny<BlobName>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BlobObject(new BlobName("archive.zip"),
                Metadata.None,
                ContentType.Parse("application/zip"),
                _ =>
                {
                    var archiveStream = new DomainV2ZipArchiveBuilder()
                        .WithExtract((builder, _) =>
                        {
                            builder.DataSet.RoadNodeDbaseRecords.Add(builder.CreateRoadNodeDbaseRecord(x =>
                            {
                                x.TYPE.Value = RoadNodeTypeV2.Schijnknoop;
                            }));
                            builder.DataSet.RoadNodeShapeRecords.Add(builder.CreateRoadNodeShapeRecord());
                        })
                        .WithChange((builder, _) =>
                        {
                            builder.TestData.TransactionZoneShapeRecord.Geometry = transactionZoneGeometry;
                        })
                        .BuildArchiveStream();
                    return Task.FromResult<Stream>(archiveStream);
                }));

        // Act
        var act = () => HandleRequest(new UploadInwinningExtractSqsRequest
            {
                DownloadId = downloadId,
                UploadId = ObjectProvider.Create<UploadId>(),
                ExtractRequestId = extractRequestId
            },
            extractUploader: new ExtractUploader(
                ExtractsDbContext,
                new RoadNetworkUploadsBlobClient(blobClientMock.Object),
                new FakeZipArchiveFeatureCompareTranslator(),
                new FakeExtractUploadFailedEmailClient(),
                TicketingMock.Object
            ));

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task GivenCompletedInwinningszone_ThenException()
    {
        // Arrange
        var downloadId = ObjectProvider.Create<DownloadId>();
        var request = new UploadInwinningExtractSqsRequest
        {
            DownloadId = downloadId,
            UploadId = ObjectProvider.Create<UploadId>(),
            ExtractRequestId = ObjectProvider.Create<ExtractRequestId>()
        };

        ExtractsDbContext.Inwinningszones.Add(new()
        {
            NisCode = ObjectProvider.Create<string>(),
            Contour = Polygon.Empty,
            Operator = ObjectProvider.Create<string>(),
            DownloadId = downloadId,
            Completed = true
        });

        await ExtractsDbContext.SaveChangesAsync();

        // Act
        await HandleRequest(request);

        // Assert
        TicketingMock.Verify(x =>
            x.Error(
                request.TicketId,
                It.Is<TicketError>(e => e.ErrorCode == "InwinningszoneIsGesloten"),
                CancellationToken.None
            )
        );
    }
}
