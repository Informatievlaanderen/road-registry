namespace RoadRegistry.Jobs.Processor.Tests
{
    using BackOffice.Abstractions.Exceptions;
    using BackOffice.Abstractions.Jobs;
    using BackOffice.Exceptions;
    using BackOffice.FeatureToggles;
    using BackOffice.Uploads;
    using Be.Vlaanderen.Basisregisters.BlobStore;
    using Extracts.Schema;
    using Extracts.Uploads;
    using FluentAssertions;
    using Infrastructure.Options;
    using MediatR;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging.Abstractions;
    using Moq;
    using TicketingService.Abstractions;
    using ValueObjects.ProblemCodes;

    public partial class JobsProcessorTests
    {
        [Fact]
        public async Task EnsureExtractCleanupIsInvoked()
        {
            var mockExtractRequestCleaner = new Mock<IExtractRequestCleaner>();

            var sut = new JobsProcessor(
                new JobsProcessorOptions(),
                new FakeJobsContextFactory().CreateDbContext(),
                Mock.Of<ITicketing>(),
                new RoadNetworkJobsBlobClient(Mock.Of<IBlobClient>()),
                Mock.Of<IMediator>(),
                mockExtractRequestCleaner.Object,
                new RoadNetworkUploadsBlobClient(Mock.Of<IBlobClient>()),
                new FakeExtractsDbContextFactory().CreateDbContext(),
                new NullLoggerFactory(),
                Mock.Of<IHostApplicationLifetime>());

            // Act
            await sut.StartAsync(CancellationToken.None);

            // Assert
            mockExtractRequestCleaner.Verify(x => x.CloseOldExtracts(
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task WhenBlobNotFound()
        {
            var mockTicketing = new Mock<ITicketing>();
            var mockIBlobClient = new Mock<IBlobClient>();
            var mockMediator = new Mock<IMediator>();
            var mockIHostApplicationLifeTime = new Mock<IHostApplicationLifetime>();
            var jobsContext = new FakeJobsContextFactory().CreateDbContext();

            var ticketId = Guid.NewGuid();
            var job = new Job(DateTimeOffset.Now, JobStatus.Created, UploadType.Uploads, ticketId);

            jobsContext.Jobs.Add(job);
            await jobsContext.SaveChangesAsync(CancellationToken.None);

            var blobName = new BlobName(job.ReceivedBlobName);

            mockIBlobClient
                .Setup(x => x.BlobExistsAsync(blobName, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

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
            mockTicketing.Verify(x => x.Pending(ticketId, It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task WhenBlobObjectIsNull_ThenLogErrorAndContinue()
        {
            var ct = CancellationToken.None;
            var mockTicketing = new Mock<ITicketing>();
            var mockIBlobClient = new Mock<IBlobClient>();
            var mockMediator = new Mock<IMediator>();
            var mockIHostApplicationLifeTime = new Mock<IHostApplicationLifetime>();
            var jobsContext = new FakeJobsContextFactory().CreateDbContext();

            var ticketId = Guid.NewGuid();
            var job = new Job(DateTimeOffset.Now, JobStatus.Created, UploadType.Uploads, ticketId);

            jobsContext.Jobs.Add(job);
            await jobsContext.SaveChangesAsync(ct);

            var blobName = new BlobName(job.ReceivedBlobName);

            mockIBlobClient
                .Setup(x => x.BlobExistsAsync(blobName, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            mockIBlobClient
                .Setup(x => x.GetBlobAsync(blobName, It.IsAny<CancellationToken>()))
                .ReturnsAsync((BlobObject)null);

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
            await sut.StartAsync(ct);

            // Assert
            jobsContext.Jobs.First().Status.Should().Be(JobStatus.Created);
        }

        [Fact]
        public async Task WhenZipArchiveValidationProblems_ThenTicketError()
        {
            var ct = CancellationToken.None;
            var mockTicketing = new Mock<ITicketing>();
            var mockIBlobClient = new Mock<IBlobClient>();
            var mockMediator = new Mock<IMediator>();
            var mockIHostApplicationLifeTime = new Mock<IHostApplicationLifetime>();
            var jobsContext = new FakeJobsContextFactory().CreateDbContext();

            var ticketId = Guid.NewGuid();
            var job = new Job(DateTimeOffset.Now, JobStatus.Created, UploadType.Uploads, ticketId);

            jobsContext.Jobs.Add(job);
            await jobsContext.SaveChangesAsync(ct);

            var blobName = new BlobName(job.ReceivedBlobName);

            mockIBlobClient
                .Setup(x => x.BlobExistsAsync(blobName, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            mockIBlobClient
                .Setup(x => x.GetBlobAsync(blobName, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new BlobObject(blobName, null!, ContentType.Parse("X-multipart/abc"),
                    _ => Task.FromResult<Stream>(EmbeddedResourceReader.Read("empty.zip"))));

            mockMediator
                .Setup(x => x.Send(It.IsAny<object>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => throw new ZipArchiveValidationException(ZipArchiveProblems.Single(new FileError("file.dbf", ProblemCode.Common.NotFound))));

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
            await sut.StartAsync(ct);

            // Assert
            jobsContext.Jobs.First().Status.Should().Be(JobStatus.Error);

            mockTicketing.Verify(x => x.Error(
                ticketId,
                It.Is<TicketError>(ticketError =>
                    ticketError.Errors.First().ErrorCode == "file.dbf_ErrorNotFound"
                    && ticketError.Errors.First().ErrorMessage == "De waarde ontbreekt."),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task WhenUnsupportedMediaTypeException_ThenTicketError()
        {
            var ct = CancellationToken.None;
            var mockTicketing = new Mock<ITicketing>();
            var mockIBlobClient = new Mock<IBlobClient>();
            var mockMediator = new Mock<IMediator>();
            var mockIHostApplicationLifeTime = new Mock<IHostApplicationLifetime>();
            var jobsContext = new FakeJobsContextFactory().CreateDbContext();

            var ticketId = Guid.NewGuid();
            var job = new Job(DateTimeOffset.Now, JobStatus.Created, UploadType.Uploads, ticketId);

            jobsContext.Jobs.Add(job);
            await jobsContext.SaveChangesAsync(ct);

            var blobName = new BlobName(job.ReceivedBlobName);

            mockIBlobClient
                .Setup(x => x.BlobExistsAsync(blobName, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            mockIBlobClient
                .Setup(x => x.GetBlobAsync(blobName, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new BlobObject(blobName, null!, ContentType.Parse("X-multipart/abc"),
                    _ => Task.FromResult<Stream>(EmbeddedResourceReader.Read("empty.zip"))));

            mockMediator
                .Setup(x => x.Send(It.IsAny<object>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => throw new UnsupportedMediaTypeException(ContentType.Parse("X-multipart/abc")));

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
            await sut.StartAsync(ct);

            // Assert
            jobsContext.Jobs.First().Status.Should().Be(JobStatus.Error);

            mockTicketing.Verify(x => x.Error(
                ticketId,
                It.Is<TicketError>(ticketError =>
                    ticketError.Errors.First().ErrorCode == "UnsupportedMediaType"
                    && ticketError.Errors.First().ErrorMessage == "Bestandstype is foutief. 'X-multipart/abc' is geen geldige waarde."),
                It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
