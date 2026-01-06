namespace RoadRegistry.Jobs.Processor.Tests
{
    using AutoFixture;
    using BackOffice.Abstractions.Extracts;
    using BackOffice.Abstractions.Jobs;
    using BackOffice.FeatureToggles;
    using BackOffice.Uploads;
    using Be.Vlaanderen.Basisregisters.BlobStore;
    using Extracts.Schema;
    using FluentAssertions;
    using Infrastructure.Options;
    using MediatR;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging.Abstractions;
    using Moq;
    using TicketingService.Abstractions;

    public partial class JobsProcessorTests
    {
        [Fact]
        public async Task FlowTest_Extracts()
        {
            var fixture = FixtureFactory.Create();
            var mockTicketing = new Mock<ITicketing>();
            var mockIBlobClient = new Mock<IBlobClient>();
            var mockMediator = new Mock<IMediator>();
            var mockIHostApplicationLifeTime = new Mock<IHostApplicationLifetime>();
            var jobsContext = new FakeJobsContextFactory().CreateDbContext();

            var ticketId = Guid.NewGuid();
            var downloadId = Guid.NewGuid();

            var job = new Job(DateTimeOffset.Now, JobStatus.Created, UploadType.Extracts, ticketId)
            {
                DownloadId = downloadId,
                OperatorName = fixture.Create<string>().Substring(0, 20)
            };
            jobsContext.Jobs.Add(job);
            await jobsContext.SaveChangesAsync(CancellationToken.None);

            var blobName = new BlobName(job.ReceivedBlobName);

            mockIBlobClient
                .Setup(x => x.BlobExistsAsync(blobName, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            mockIBlobClient
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
                new RoadNetworkJobsBlobClient(mockIBlobClient.Object),
                mockMediator.Object,
                Mock.Of<IExtractRequestCleaner>(),
                new RoadNetworkUploadsBlobClient(Mock.Of<IBlobClient>()),
                new FakeExtractsDbContextFactory().CreateDbContext(),
                new NullLoggerFactory(),
                mockIHostApplicationLifeTime.Object);

            // Act
            await sut.StartAsync(CancellationToken.None);

            // Assert
            jobsContext.Jobs.First().Status.Should().Be(JobStatus.Completed);

            mockTicketing.Verify(x =>
                x.Pending(ticketId, It.IsAny<CancellationToken>()),
                Times.Once);

            var executedRequest = Assert.IsType<UploadExtractRequest>(mockMediator.Invocations.Single().Arguments.First());
            Assert.Equal(ticketId, executedRequest.TicketId);
            Assert.Equal(blobName, executedRequest.Archive.FileName);
            Assert.Equal(downloadId.ToString("N"), executedRequest.DownloadId);
            executedRequest.ProvenanceData.Operator.Should().Be(job.OperatorName);

            mockIHostApplicationLifeTime.Verify(x => x.StopApplication(), Times.Once);
        }

        [Fact]
        public async Task FlowTest_Extracts_MissingDownloadId()
        {
            var mockTicketing = new Mock<ITicketing>();
            var mockIBlobClient = new Mock<IBlobClient>();
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

            mockIBlobClient
                .Setup(x => x.BlobExistsAsync(blobName, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            mockIBlobClient
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
                new RoadNetworkJobsBlobClient(mockIBlobClient.Object),
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
