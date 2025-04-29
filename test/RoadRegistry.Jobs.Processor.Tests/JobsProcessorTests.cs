namespace RoadRegistry.Jobs.Processor.Tests
{
    using BackOffice.Abstractions.Uploads;
    using BackOffice.Core.ProblemCodes;
    using BackOffice.Exceptions;
    using BackOffice.Uploads;
    using Be.Vlaanderen.Basisregisters.BlobStore;
    using FluentAssertions;
    using Infrastructure.Options;
    using MediatR;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging.Abstractions;
    using Moq;
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using AutoFixture;
    using BackOffice.Abstractions.Exceptions;
    using BackOffice.Abstractions.Jobs;
    using TicketingService.Abstractions;
    using Xunit;

    public class JobsProcessorTests
    {
        [Fact]
        public async Task FlowTest_Uploads()
        {
            var fixture = new Fixture();
            var mockTicketing = new Mock<ITicketing>();
            var mockIBlobClient = new Mock<IBlobClient>();
            var mockMediator = new Mock<IMediator>();
            var mockIHostApplicationLifeTime = new Mock<IHostApplicationLifetime>();
            var jobsContext = new FakeJobsContextFactory().CreateDbContext();

            var ticketId = Guid.NewGuid();

            var job = new Job(DateTimeOffset.Now, JobStatus.Created, UploadType.Uploads, ticketId)
            {
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
            executedRequest.ProvenanceData.Operator.Should().Be(job.OperatorName);

            mockIHostApplicationLifeTime.Verify(x => x.StopApplication(), Times.Once);
        }

        [Fact]
        public async Task FlowTest_Extracts()
        {
            var fixture = new Fixture();
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
                new NullLoggerFactory(),
                mockIHostApplicationLifeTime.Object);

            // Act
            await sut.StartAsync(CancellationToken.None);

            // Assert
            jobsContext.Jobs.First().Status.Should().Be(JobStatus.Completed);

            mockTicketing.Verify(x =>
                x.Pending(ticketId, It.IsAny<CancellationToken>()),
                Times.Once);

            var executedRequest = Assert.IsType<BackOffice.Abstractions.Extracts.UploadExtractRequest>(mockMediator.Invocations.Single().Arguments.First());
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
