namespace RoadRegistry.Jobs.Processor.Upload.Tests
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using Abstractions;
    using BackOffice.Abstractions.Uploads;
    using BackOffice.Uploads;
    using Be.Vlaanderen.Basisregisters.BlobStore;
    using FluentAssertions;
    using Infrastructure.Options;
    using MediatR;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging.Abstractions;
    using Microsoft.Extensions.Options;
    using Moq;
    using TicketingService.Abstractions;
    using Xunit;
    //TODO-rik unit test for uploadprocessor
    public class UploadProcessorTests
    {
        private readonly FakeJobsContext _jobsContext;

        public UploadProcessorTests()
        {
            _jobsContext = new FakeJobsContextFactory().CreateDbContext();
        }

        [Fact]
        public async Task FlowTest_Uploads()
        {
            var mockTicketing = new Mock<ITicketing>();
            var mockIBlobClient = new Mock<IBlobClient>();
            var mockMediator = new Mock<IMediator>();
            var mockIHostApplicationLifeTime = new Mock<IHostApplicationLifetime>();

            var ticketId = Guid.NewGuid();

            var job = new Job(DateTimeOffset.Now, JobStatus.Created, UploadType.Uploads, ticketId);
            _jobsContext.Jobs.Add(job);
            await _jobsContext.SaveChangesAsync(CancellationToken.None);

            var blobName = new BlobName(job.ReceivedBlobName);

            mockIBlobClient
                .Setup(x => x.BlobExistsAsync(blobName, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            mockIBlobClient
                .Setup(x => x.GetBlobAsync(blobName, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new BlobObject(
                    blobName,
                    null,
                    ContentType.Parse("X-multipart/abc"),
                    _ => Task.FromResult((Stream)new FileStream(
                        $"{AppContext.BaseDirectory}/UploadProcessor/valid.zip", FileMode.Open,
                        FileAccess.Read))));
            
            var sut = new UploadProcessor(
                new UploadProcessorOptions
                {
                    MaxJobLifeTimeInMinutes = 65
                },
                _jobsContext,
                mockTicketing.Object,
                new RoadNetworkJobsBlobClient(mockIBlobClient.Object),
                mockMediator.Object,
                new NullLoggerFactory(),
                mockIHostApplicationLifeTime.Object);

            // Act
            await sut.StartAsync(CancellationToken.None);

            // Assert
            mockTicketing.Verify(x =>
                x.Pending(ticketId, It.IsAny<CancellationToken>()),
                Times.Once);

            _jobsContext.Jobs.First().Status.Should().Be(JobStatus.Completed);

            var executedRequest = Assert.IsType<UploadExtractRequest>(mockMediator.Invocations.Single().Arguments.First());
            Assert.Equal(ticketId, executedRequest.TicketId);
            Assert.Equal(blobName, executedRequest.Archive.FileName);

            mockIHostApplicationLifeTime.Verify(x => x.StopApplication(), Times.Once);
        }

        [Fact]
        public async Task FlowTest_Extracts()
        {
            var mockTicketing = new Mock<ITicketing>();
            var mockIBlobClient = new Mock<IBlobClient>();
            var mockMediator = new Mock<IMediator>();
            var mockIHostApplicationLifeTime = new Mock<IHostApplicationLifetime>();

            var ticketId = Guid.NewGuid();
            var downloadId = Guid.NewGuid();

            var job = new Job(DateTimeOffset.Now, JobStatus.Created, UploadType.Extracts, ticketId)
            {
                DownloadId = downloadId
            };
            _jobsContext.Jobs.Add(job);
            await _jobsContext.SaveChangesAsync(CancellationToken.None);

            var blobName = new BlobName(job.ReceivedBlobName);

            mockIBlobClient
                .Setup(x => x.BlobExistsAsync(blobName, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            mockIBlobClient
                .Setup(x => x.GetBlobAsync(blobName, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new BlobObject(
                    blobName,
                    null,
                    ContentType.Parse("X-multipart/abc"),
                    _ => Task.FromResult((Stream)new FileStream(
                        $"{AppContext.BaseDirectory}/UploadProcessor/valid.zip", FileMode.Open,
                        FileAccess.Read))));
            
            var sut = new UploadProcessor(
                new UploadProcessorOptions
                {
                    MaxJobLifeTimeInMinutes = 65
                },
                _jobsContext,
                mockTicketing.Object,
                new RoadNetworkJobsBlobClient(mockIBlobClient.Object),
                mockMediator.Object,
                new NullLoggerFactory(),
                mockIHostApplicationLifeTime.Object);

            // Act
            await sut.StartAsync(CancellationToken.None);

            // Assert
            mockTicketing.Verify(x =>
                x.Pending(ticketId, It.IsAny<CancellationToken>()),
                Times.Once);

            _jobsContext.Jobs.First().Status.Should().Be(JobStatus.Completed);

            var executedRequest = Assert.IsType<BackOffice.Abstractions.Extracts.UploadExtractRequest>(mockMediator.Invocations.Single().Arguments.First());
            Assert.Equal(ticketId, executedRequest.TicketId);
            Assert.Equal(blobName, executedRequest.Archive.FileName);
            Assert.Equal(downloadId.ToString("N"), executedRequest.DownloadId);

            mockIHostApplicationLifeTime.Verify(x => x.StopApplication(), Times.Once);
        }

        [Fact]
        public async Task FlowTest_Extracts_MissingDownloadId()
        {
            var mockTicketing = new Mock<ITicketing>();
            var mockIBlobClient = new Mock<IBlobClient>();
            var mockMediator = new Mock<IMediator>();
            var mockIHostApplicationLifeTime = new Mock<IHostApplicationLifetime>();

            var ticketId = Guid.NewGuid();

            var job = new Job(DateTimeOffset.Now, JobStatus.Created, UploadType.Extracts, ticketId)
            {
                DownloadId = null
            };
            _jobsContext.Jobs.Add(job);
            await _jobsContext.SaveChangesAsync(CancellationToken.None);

            var blobName = new BlobName(job.ReceivedBlobName);

            mockIBlobClient
                .Setup(x => x.BlobExistsAsync(blobName, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            mockIBlobClient
                .Setup(x => x.GetBlobAsync(blobName, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new BlobObject(
                    blobName,
                    null,
                    ContentType.Parse("X-multipart/abc"),
                    _ => Task.FromResult((Stream)new FileStream(
                        $"{AppContext.BaseDirectory}/UploadProcessor/valid.zip", FileMode.Open,
                        FileAccess.Read))));
            
            var sut = new UploadProcessor(
                new UploadProcessorOptions
                {
                    MaxJobLifeTimeInMinutes = 65
                },
                _jobsContext,
                mockTicketing.Object,
                new RoadNetworkJobsBlobClient(mockIBlobClient.Object),
                mockMediator.Object,
                new NullLoggerFactory(),
                mockIHostApplicationLifeTime.Object);

            // Act
            await sut.StartAsync(CancellationToken.None);

            // Assert
            mockTicketing.Verify(x =>
                x.Error(ticketId, It.IsAny<TicketError>(), It.IsAny<CancellationToken>()),
                Times.Once);

            _jobsContext.Jobs.First().Status.Should().Be(JobStatus.Error);

            //TODO-rik analyze TicketError from invocation
            mockTicketing.Verify(x => x.Error(
                ticketId,
                It.Is<TicketError>(ticketError =>
                    ticketError.Errors.First().ErrorCode == "RequiredFileMissing"
                    && ticketError.Errors.First().ErrorMessage == $"Er ontbreekt een verplichte file in de zip: GEBOUW_ALL.DBF."),
                It.IsAny<CancellationToken>()), Times.Once);
            //var executedRequest = Assert.IsType<BackOffice.Abstractions.Extracts.UploadExtractRequest>(mockMediator.Invocations.Single().Arguments.First());
            //Assert.Equal(ticketId, executedRequest.TicketId);
            //Assert.Equal(blobName, executedRequest.Archive.FileName);
            //Assert.Equal(downloadId.ToString("N"), executedRequest.DownloadId);

            mockIHostApplicationLifeTime.Verify(x => x.StopApplication(), Times.Once);
        }

        [Fact]
        public async Task WhenBlobNotFound()
        {
            var mockTicketing = new Mock<ITicketing>();
            var mockIBlobClient = new Mock<IBlobClient>();
            var mockMediator = new Mock<IMediator>();
            var mockIHostApplicationLifeTime = new Mock<IHostApplicationLifetime>();

            var ticketId = Guid.NewGuid();
            var job = new Job(DateTimeOffset.Now, JobStatus.Created, UploadType.Uploads, ticketId);

            _jobsContext.Jobs.Add(job);
            await _jobsContext.SaveChangesAsync(CancellationToken.None);

            var blobName = new BlobName(job.ReceivedBlobName);

            mockIBlobClient
                .Setup(x => x.BlobExistsAsync(blobName, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var sut = new UploadProcessor(
                new UploadProcessorOptions
                {
                    MaxJobLifeTimeInMinutes = 65
                },
                _jobsContext,
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

            var ticketId = Guid.NewGuid();
            var job = new Job(DateTimeOffset.Now, JobStatus.Created, UploadType.Uploads, ticketId);

            _jobsContext.Jobs.Add(job);
            await _jobsContext.SaveChangesAsync(ct);

            var blobName = new BlobName(job.ReceivedBlobName);

            mockIBlobClient
                .Setup(x => x.BlobExistsAsync(blobName, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            mockIBlobClient
                .Setup(x => x.GetBlobAsync(blobName, It.IsAny<CancellationToken>()))
                .ReturnsAsync((BlobObject?)null);

            var sut = new UploadProcessor(
                new UploadProcessorOptions
                {
                    MaxJobLifeTimeInMinutes = 65
                },
                _jobsContext,
                mockTicketing.Object,
                new RoadNetworkJobsBlobClient(mockIBlobClient.Object),
                mockMediator.Object,
                new NullLoggerFactory(),
                mockIHostApplicationLifeTime.Object);

            // Act
            await sut.StartAsync(ct);

            // Assert
            //var jobRecords = _jobsContext.JobRecords.Where(x => x.JobId == job.Id);
            //jobRecords.Should().BeEmpty();
        }

        [Fact]
        public async Task WhenZipArchiveValidationProblems_ThenTicketError()
        {
            var ct = CancellationToken.None;
            var mockTicketing = new Mock<ITicketing>();
            var mockIBlobClient = new Mock<IBlobClient>();
            var mockMediator = new Mock<IMediator>();
            var mockIHostApplicationLifeTime = new Mock<IHostApplicationLifetime>();

            var ticketId = Guid.NewGuid();
            var job = new Job(DateTimeOffset.Now, JobStatus.Created, UploadType.Uploads, ticketId);

            _jobsContext.Jobs.Add(job);
            await _jobsContext.SaveChangesAsync(ct);

            var blobName = new BlobName(job.ReceivedBlobName);

            mockIBlobClient
                .Setup(x => x.BlobExistsAsync(blobName, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var zipFileStream = new FileStream($"{AppContext.BaseDirectory}/UploadProcessor/empty.zip",
                FileMode.Open, FileAccess.Read);

            mockIBlobClient
                .Setup(x => x.GetBlobAsync(blobName, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new BlobObject(blobName, null, ContentType.Parse("X-multipart/abc"),
                    _ => Task.FromResult((Stream)zipFileStream)));

            var sut = new UploadProcessor(
                new UploadProcessorOptions
                {
                    MaxJobLifeTimeInMinutes = 65
                },
                _jobsContext,
                mockTicketing.Object,
                new RoadNetworkJobsBlobClient(mockIBlobClient.Object),
                mockMediator.Object,
                new NullLoggerFactory(),
                mockIHostApplicationLifeTime.Object);

            // Act
            await sut.StartAsync(ct);

            // Assert
            mockTicketing.Verify(x => x.Error(
                ticketId,
                It.Is<TicketError>(x =>
                    x.Errors.First().ErrorCode == "RequiredFileMissing"
                    && x.Errors.First().ErrorMessage == $"Er ontbreekt een verplichte file in de zip: GEBOUW_ALL.DBF."),
                It.IsAny<CancellationToken>()), Times.Once);
            
            _jobsContext.Jobs.First().Status.Should().Be(JobStatus.Error);
        }
    }
}
