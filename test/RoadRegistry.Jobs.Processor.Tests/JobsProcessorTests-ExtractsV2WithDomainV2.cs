namespace RoadRegistry.Jobs.Processor.Tests
{
    using AutoFixture;
    using BackOffice;
    using BackOffice.Abstractions.Jobs;
    using BackOffice.FeatureToggles;
    using BackOffice.Handlers.Sqs.Extracts;
    using BackOffice.Uploads;
    using Be.Vlaanderen.Basisregisters.BlobStore;
    using Extracts.Schema;
    using FluentAssertions;
    using Infrastructure.Options;
    using MediatR;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging.Abstractions;
    using Moq;
    using NetTopologySuite.Geometries;
    using RoadRegistry.Tests.BackOffice.Scenarios;
    using TicketingService.Abstractions;

    public partial class JobsProcessorTests
    {
        [Fact]
        public async Task FlowTest_ExtractsV2WithDomainV2()
        {
            var fixture = new RoadNetworkTestData().ObjectProvider;
            var mockTicketing = new Mock<ITicketing>();
            var blobClient = new Mock<IBlobClient>();
            var mockMediator = new Mock<IMediator>();
            var mockIHostApplicationLifeTime = new Mock<IHostApplicationLifetime>();
            var jobsContext = new FakeJobsContextFactory().CreateDbContext();

            var ticketId = Guid.NewGuid();
            var downloadId = Guid.NewGuid();

            var job = new Job(DateTimeOffset.Now, JobStatus.Created, UploadType.ExtractsV2, ticketId)
            {
                DownloadId = downloadId,
                OperatorName = fixture.Create<string>().Substring(0, 20)
            };
            jobsContext.Jobs.Add(job);
            await jobsContext.SaveChangesAsync(CancellationToken.None);

            var blobName = new BlobName(job.ReceivedBlobName);

            blobClient
                .Setup(x => x.BlobExistsAsync(blobName, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var blobFileName = "file.zip";
            blobClient
                .Setup(x => x.GetBlobAsync(blobName, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new BlobObject(
                    blobName,
                    Metadata.None.Add(new KeyValuePair<MetadataKey, string>(new MetadataKey("filename"), blobFileName)),
                    ContentType.Parse("X-multipart/abc"),
                    _ => Task.FromResult<Stream>(EmbeddedResourceReader.Read("valid.zip"))));

            var extractsDbContext = new FakeExtractsDbContextFactory().CreateDbContext();
            var extractRequestId = fixture.Create<ExtractRequestId>();
            extractsDbContext.ExtractRequests.Add(new ExtractRequest
            {
                ExtractRequestId = extractRequestId,
                Description = fixture.Create<string>()
            });
            extractsDbContext.ExtractDownloads.Add(new ExtractDownload
            {
                DownloadId = downloadId,
                Contour = Polygon.Empty,
                ExtractRequestId = extractRequestId,
                ZipArchiveWriterVersion = WellKnownZipArchiveWriterVersions.DomainV2
            });
            await extractsDbContext.SaveChangesAsync();

            var sut = new JobsProcessor(
                new JobsProcessorOptions
                {
                    MaxJobLifeTimeInMinutes = 65
                },
                jobsContext,
                mockTicketing.Object,
                new RoadNetworkJobsBlobClient(blobClient.Object),
                mockMediator.Object,
                Mock.Of<IExtractRequestCleaner>(),
                new RoadNetworkUploadsBlobClient(blobClient.Object),
                extractsDbContext,
                new NullLoggerFactory(),
                mockIHostApplicationLifeTime.Object);

            // Act
            await sut.StartAsync(CancellationToken.None);

            // Assert
            jobsContext.Jobs.First().Status.Should().Be(JobStatus.Completed);

            mockTicketing.Verify(x =>
                x.Pending(ticketId, It.IsAny<CancellationToken>()),
                Times.Once);

            var createBlobInvocation = blobClient.Invocations
                .Single(x => x.Method.Name == nameof(IBlobClient.CreateBlobAsync));
            var blobMetadataFilename = createBlobInvocation
                .Arguments.OfType<Metadata>()
                .Single()
                .Single();
            blobMetadataFilename.Key.ToString().Should().Be("filename");
            blobMetadataFilename.Value.Should().Be(blobFileName);

            var executedRequest = Assert.IsType<UploadExtractSqsRequestV2>(mockMediator.Invocations.Single().Arguments.First());
            executedRequest.TicketId.Should().Be(ticketId);
            executedRequest.DownloadId.ToGuid().Should().Be(downloadId);
            executedRequest.ProvenanceData!.Operator.Should().Be(job.OperatorName);
            createBlobInvocation.Arguments.OfType<BlobName>().Single().ToString().Should().Be(executedRequest.UploadId.ToString());

            mockIHostApplicationLifeTime.Verify(x => x.StopApplication(), Times.Once);
        }

        [Fact]
        public async Task FlowTest_ExtractsV2WithDomainV2_MissingDownloadId()
        {
            var mockTicketing = new Mock<ITicketing>();
            var blobClient = new Mock<IBlobClient>();
            var mockMediator = new Mock<IMediator>();
            var mockIHostApplicationLifeTime = new Mock<IHostApplicationLifetime>();
            var jobsContext = new FakeJobsContextFactory().CreateDbContext();

            var ticketId = Guid.NewGuid();

            var job = new Job(DateTimeOffset.Now, JobStatus.Created, UploadType.Extracts, ticketId)
            {
                DownloadId = null
            };
            jobsContext.Jobs.Add(job);
            await jobsContext.SaveChangesAsync(CancellationToken.None);

            var blobName = new BlobName(job.ReceivedBlobName);

            blobClient
                .Setup(x => x.BlobExistsAsync(blobName, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            blobClient
                .Setup(x => x.GetBlobAsync(blobName, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new BlobObject(
                    blobName,
                    null!,
                    ContentType.Parse("X-multipart/abc"),
                    _ => Task.FromResult<Stream>(EmbeddedResourceReader.Read("valid.zip"))));

            var sut = new JobsProcessor(
                new JobsProcessorOptions
                {
                    MaxJobLifeTimeInMinutes = 65
                },
                jobsContext,
                mockTicketing.Object,
                new RoadNetworkJobsBlobClient(blobClient.Object),
                mockMediator.Object,
                Mock.Of<IExtractRequestCleaner>(),
                new RoadNetworkUploadsBlobClient(Mock.Of<IBlobClient>()),
                new FakeExtractsDbContextFactory().CreateDbContext(),
                new NullLoggerFactory(),
                mockIHostApplicationLifeTime.Object);

            // Act
            await sut.StartAsync(CancellationToken.None);

            // Assert
            jobsContext.Jobs.First().Status.Should().Be(JobStatus.Error);

            mockTicketing.Verify(x => x.Error(
                ticketId,
                It.Is<TicketError>(ticketError =>
                    ticketError.Errors.First().ErrorCode == "DownloadIdIsRequired"
                    && ticketError.Errors.First().ErrorMessage == "Download id is verplicht."),
                It.IsAny<CancellationToken>()), Times.Once);

            mockIHostApplicationLifeTime.Verify(x => x.StopApplication(), Times.Once);
        }
    }
}
