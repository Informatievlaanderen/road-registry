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
        public async Task FlowTest()
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
                new UploadProcessorOptions(),
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

            mockMediator.Verify(x =>
                x.Send(It.IsAny<UploadExtractRequest>(), It.IsAny<CancellationToken>()),
                Times.Once);
            
            mockIHostApplicationLifeTime.Verify(x => x.StopApplication(), Times.Once);
        }

        //[Fact]
        //public async Task WhenBlobNotFound()
        //{
        //    var mockTicketing = new Mock<ITicketing>();
        //    var mockIBlobClient = new Mock<IBlobClient>();
        //    var mockAmazonClient = new Mock<IAmazonECS>();
        //    var mockIHostApplicationLifeTime = new Mock<IHostApplicationLifetime>();

        //    var ticketId = Guid.NewGuid();
        //    var job = new Job(DateTimeOffset.Now, JobStatus.Created, ticketId);

        //    _jobsContext.Jobs.Add(job);
        //    await _jobsContext.SaveChangesAsync(CancellationToken.None);

        //    var blobName = new BlobName(job.ReceivedBlobName);

        //    mockIBlobClient
        //        .Setup(x => x.BlobExistsAsync(blobName, It.IsAny<CancellationToken>()))
        //        .ReturnsAsync(false);

        //    var notificationsService = new Mock<INotificationService>();

        //    var sut = new UploadProcessor(
        //        _jobsContext,
        //        mockTicketing.Object,
        //        mockIBlobClient.Object,
        //        mockAmazonClient.Object,
        //        new NullLoggerFactory(),
        //        mockIHostApplicationLifeTime.Object,
        //        notificationsService.Object,
        //        Options.Create(new EcsTaskOptions()));

        //    // Act
        //    await sut.StartAsync(CancellationToken.None);

        //    // Assert
        //    mockTicketing.Verify(x => x.Pending(ticketId, It.IsAny<CancellationToken>()), Times.Never);
        //    notificationsService.Verify(x => x.PublishToTopicAsync(It.IsAny<NotificationMessage>()), Times.Never);
        //}

        //[Fact]
        //public async Task WhenBlobObjectIsNull_ThenLogErrorAndContinue()
        //{
        //    var ct = CancellationToken.None;
        //    var mockTicketing = new Mock<ITicketing>();
        //    var mockIBlobClient = new Mock<IBlobClient>();
        //    var mockAmazonClient = new Mock<IAmazonECS>();
        //    var mockIHostApplicationLifeTime = new Mock<IHostApplicationLifetime>();

        //    var ticketId = Guid.NewGuid();
        //    var job = new Job(DateTimeOffset.Now, JobStatus.Created, ticketId);

        //    _jobsContext.Jobs.Add(job);
        //    await _jobsContext.SaveChangesAsync(ct);

        //    var blobName = new BlobName(job.ReceivedBlobName);

        //    mockIBlobClient
        //        .Setup(x => x.BlobExistsAsync(blobName, It.IsAny<CancellationToken>()))
        //        .ReturnsAsync(true);

        //    mockIBlobClient
        //        .Setup(x => x.GetBlobAsync(blobName, It.IsAny<CancellationToken>()))
        //        .ReturnsAsync((BlobObject?)null);

        //    var notificationsService = new Mock<INotificationService>();

        //    var sut = new UploadProcessor(
        //        _jobsContext,
        //        mockTicketing.Object,
        //        mockIBlobClient.Object,
        //        mockAmazonClient.Object,
        //        new NullLoggerFactory(),
        //        mockIHostApplicationLifeTime.Object,
        //        notificationsService.Object,
        //        Options.Create(new EcsTaskOptions()));

        //    // Act
        //    await sut.StartAsync(ct);

        //    // Assert
        //    var jobRecords = _jobsContext.JobRecords.Where(x => x.JobId == job.Id);
        //    jobRecords.Should().BeEmpty();
        //    notificationsService.Verify(x => x.PublishToTopicAsync(It.IsAny<NotificationMessage>()), Times.Never);
        //}

        //[Fact]
        //public async Task WhenZipArchiveValidationProblems_ThenTicketError()
        //{
        //    var ct = CancellationToken.None;
        //    var mockTicketing = new Mock<ITicketing>();
        //    var mockIBlobClient = new Mock<IBlobClient>();
        //    var mockAmazonClient = new Mock<IAmazonECS>();
        //    var mockIHostApplicationLifeTime = new Mock<IHostApplicationLifetime>();

        //    var ticketId = Guid.NewGuid();
        //    var job = new Job(DateTimeOffset.Now, JobStatus.Created, ticketId);

        //    _jobsContext.Jobs.Add(job);
        //    await _jobsContext.SaveChangesAsync(ct);

        //    var blobName = new BlobName(job.ReceivedBlobName);

        //    mockIBlobClient
        //        .Setup(x => x.BlobExistsAsync(blobName, It.IsAny<CancellationToken>()))
        //        .ReturnsAsync(true);

        //    var zipFileStream = new FileStream($"{AppContext.BaseDirectory}/UploadProcessor/gebouw_dbf_missing.zip",
        //        FileMode.Open, FileAccess.Read);

        //    mockIBlobClient
        //        .Setup(x => x.GetBlobAsync(blobName, It.IsAny<CancellationToken>()))
        //        .ReturnsAsync(new BlobObject(blobName, null, ContentType.Parse("X-multipart/abc"),
        //            _ => Task.FromResult((Stream)zipFileStream)));

        //    var notificationsService = new Mock<INotificationService>();

        //    var sut = new UploadProcessor(
        //        _jobsContext,
        //        mockTicketing.Object,
        //        mockIBlobClient.Object,
        //        mockAmazonClient.Object,
        //        new NullLoggerFactory(),
        //        mockIHostApplicationLifeTime.Object,
        //        notificationsService.Object,
        //        Options.Create(new EcsTaskOptions()));

        //    // Act
        //    await sut.StartAsync(ct);

        //    // Assert
        //    mockTicketing.Verify(x => x.Error(
        //        ticketId,
        //        It.Is<TicketError>(x =>
        //            x.Errors.First().ErrorCode == "RequiredFileMissing"
        //            && x.Errors.First().ErrorMessage == $"Er ontbreekt een verplichte file in de zip: GEBOUW_ALL.DBF."),
        //        It.IsAny<CancellationToken>()), Times.Once);

        //    var jobRecords = _jobsContext.JobRecords.Where(x => x.JobId == job.Id);
        //    jobRecords.Should().BeEmpty();

        //    _jobsContext.Jobs.First().Status.Should().Be(JobStatus.Error);

        //    notificationsService.Verify(x => x.PublishToTopicAsync(It.IsAny<NotificationMessage>()), Times.Once);
        //}

        //[Fact]
        //public async Task WhenDbaseRecordHasMissingShapeRecord_ThenTicketError()
        //{
        //    var ct = CancellationToken.None;
        //    var mockTicketing = new Mock<ITicketing>();
        //    var mockIBlobClient = new Mock<IBlobClient>();
        //    var mockAmazonClient = new Mock<IAmazonECS>();
        //    var mockIHostApplicationLifeTime = new Mock<IHostApplicationLifetime>();

        //    var ticketId = Guid.NewGuid();
        //    var job = new Job(DateTimeOffset.Now, JobStatus.Created, ticketId);

        //    _jobsContext.Jobs.Add(job);
        //    await _jobsContext.SaveChangesAsync(ct);

        //    var blobName = new BlobName(job.ReceivedBlobName);

        //    mockIBlobClient
        //        .Setup(x => x.BlobExistsAsync(blobName, It.IsAny<CancellationToken>()))
        //        .ReturnsAsync(true);

        //    var zipFileStream = new FileStream($"{AppContext.BaseDirectory}/UploadProcessor/gebouw_shape_missing.zip",
        //        FileMode.Open, FileAccess.Read);

        //    mockIBlobClient
        //        .Setup(x => x.GetBlobAsync(blobName, It.IsAny<CancellationToken>()))
        //        .ReturnsAsync(new BlobObject(blobName, null, ContentType.Parse("X-multipart/abc"),
        //            _ => Task.FromResult((Stream)zipFileStream)));

        //    var notificationsService = new Mock<INotificationService>();

        //    var sut = new UploadProcessor(
        //        _jobsContext,
        //        mockTicketing.Object,
        //        mockIBlobClient.Object,
        //        mockAmazonClient.Object,
        //        new NullLoggerFactory(),
        //        mockIHostApplicationLifeTime.Object,
        //        notificationsService.Object,
        //        Options.Create(new EcsTaskOptions()));

        //    // Act
        //    await sut.StartAsync(ct);

        //    // Assert
        //    mockTicketing.Verify(x => x.Error(
        //        ticketId,
        //        It.Is<TicketError>(x =>
        //            x.ErrorCode == "OntbrekendeGeometriePolygoonShapeFile"
        //            && x.ErrorMessage ==
        //            $"In de meegegeven shape file hebben niet alle gebouwen een geometriePolygoon. Record nummers: 2"),
        //        It.IsAny<CancellationToken>()), Times.Once);

        //    var jobRecords = _jobsContext.JobRecords.Where(x => x.JobId == job.Id);
        //    jobRecords.Should().BeEmpty();

        //    _jobsContext.Jobs.First().Status.Should().Be(JobStatus.Error);

        //    notificationsService.Verify(x => x.PublishToTopicAsync(It.IsAny<NotificationMessage>()), Times.Once);
        //}

        //[Fact]
        //public async Task WhenDbaseRecordHasInvalidGrId_ThenTicketError()
        //{
        //    var ct = CancellationToken.None;
        //    var mockTicketing = new Mock<ITicketing>();
        //    var mockIBlobClient = new Mock<IBlobClient>();
        //    var mockAmazonClient = new Mock<IAmazonECS>();
        //    var mockIHostApplicationLifeTime = new Mock<IHostApplicationLifetime>();

        //    var ticketId = Guid.NewGuid();
        //    var job = new Job(DateTimeOffset.Now, JobStatus.Created, ticketId);

        //    _jobsContext.Jobs.Add(job);
        //    await _jobsContext.SaveChangesAsync(ct);

        //    var blobName = new BlobName(job.ReceivedBlobName);

        //    mockIBlobClient
        //        .Setup(x => x.BlobExistsAsync(blobName, It.IsAny<CancellationToken>()))
        //        .ReturnsAsync(true);

        //    var zipFileStream = new FileStream($"{AppContext.BaseDirectory}/UploadProcessor/gebouw_grid_invalid.zip",
        //        FileMode.Open, FileAccess.Read);

        //    mockIBlobClient
        //        .Setup(x => x.GetBlobAsync(blobName, It.IsAny<CancellationToken>()))
        //        .ReturnsAsync(new BlobObject(blobName, null, ContentType.Parse("X-multipart/abc"),
        //            _ => Task.FromResult((Stream)zipFileStream)));

        //    var notificationsService = new Mock<INotificationService>();

        //    var sut = new UploadProcessor(
        //        _jobsContext,
        //        mockTicketing.Object,
        //        mockIBlobClient.Object,
        //        mockAmazonClient.Object,
        //        new NullLoggerFactory(),
        //        mockIHostApplicationLifeTime.Object,
        //        notificationsService.Object,
        //        Options.Create(new EcsTaskOptions()));

        //    // Act
        //    await sut.StartAsync(ct);

        //    // Assert
        //    mockTicketing.Verify(x => x.Error(
        //        ticketId,
        //        It.Is<TicketError>(x =>
        //            x.Errors.Any(y =>
        //                y.ErrorCode == "GebouwIdOngeldig" &&
        //                y.ErrorMessage == "De meegegeven waarde in de kolom 'GRID' is ongeldig. Record nummer: 1, GRID: invalid puri")),
        //        It.IsAny<CancellationToken>()), Times.Once);

        //    var jobRecords = _jobsContext.JobRecords.Where(x => x.JobId == job.Id);
        //    jobRecords.Should().BeEmpty();

        //    _jobsContext.Jobs.First().Status.Should().Be(JobStatus.Error);

        //    notificationsService.Verify(x => x.PublishToTopicAsync(It.IsAny<NotificationMessage>()), Times.Once);
        //}

        //[Fact]
        //public async Task WhenDbaseRecordHasInvalidVersionDate_ThenTicketError()
        //{
        //    var ct = CancellationToken.None;
        //    var mockTicketing = new Mock<ITicketing>();
        //    var mockIBlobClient = new Mock<IBlobClient>();
        //    var mockAmazonClient = new Mock<IAmazonECS>();
        //    var mockIHostApplicationLifeTime = new Mock<IHostApplicationLifetime>();

        //    var ticketId = Guid.NewGuid();
        //    var job = new Job(DateTimeOffset.Now, JobStatus.Created, ticketId);

        //    _jobsContext.Jobs.Add(job);
        //    await _jobsContext.SaveChangesAsync(ct);

        //    var blobName = new BlobName(job.ReceivedBlobName);

        //    mockIBlobClient
        //        .Setup(x => x.BlobExistsAsync(blobName, It.IsAny<CancellationToken>()))
        //        .ReturnsAsync(true);

        //    var zipFileStream = new FileStream($"{AppContext.BaseDirectory}/UploadProcessor/gebouw_versiondate_invalid.zip",
        //        FileMode.Open, FileAccess.Read);

        //    mockIBlobClient
        //        .Setup(x => x.GetBlobAsync(blobName, It.IsAny<CancellationToken>()))
        //        .ReturnsAsync(new BlobObject(blobName, null, ContentType.Parse("X-multipart/abc"),
        //            _ => Task.FromResult((Stream)zipFileStream)));

        //    var notificationsService = new Mock<INotificationService>();

        //    var sut = new UploadProcessor(
        //        _jobsContext,
        //        mockTicketing.Object,
        //        mockIBlobClient.Object,
        //        mockAmazonClient.Object,
        //        new NullLoggerFactory(),
        //        mockIHostApplicationLifeTime.Object,
        //        notificationsService.Object,
        //        Options.Create(new EcsTaskOptions()));

        //    // Act
        //    await sut.StartAsync(ct);

        //    // Assert
        //    mockTicketing.Verify(x => x.Error(
        //        ticketId,
        //        It.Is<TicketError>(ticketError =>
        //            ticketError.Errors.Any(y =>
        //                y.ErrorCode == "InvalidVersionDate" &&
        //                y.ErrorMessage == "Record number(s):1,2")),
        //        It.IsAny<CancellationToken>()), Times.Once);

        //    var jobRecords = _jobsContext.JobRecords.Where(x => x.JobId == job.Id);
        //    jobRecords.Should().BeEmpty();

        //    _jobsContext.Jobs.First().Status.Should().Be(JobStatus.Error);

        //    notificationsService.Verify(x => x.PublishToTopicAsync(It.IsAny<NotificationMessage>()), Times.Once);
        //}

        //[Fact]
        //public async Task WhenDbaseRecordHasInvalidEndDate_ThenTicketError()
        //{
        //    var ct = CancellationToken.None;
        //    var mockTicketing = new Mock<ITicketing>();
        //    var mockIBlobClient = new Mock<IBlobClient>();
        //    var mockAmazonClient = new Mock<IAmazonECS>();
        //    var mockIHostApplicationLifeTime = new Mock<IHostApplicationLifetime>();

        //    var ticketId = Guid.NewGuid();
        //    var job = new Job(DateTimeOffset.Now, JobStatus.Created, ticketId);

        //    _jobsContext.Jobs.Add(job);
        //    await _jobsContext.SaveChangesAsync(ct);

        //    var blobName = new BlobName(job.ReceivedBlobName);

        //    mockIBlobClient
        //        .Setup(x => x.BlobExistsAsync(blobName, It.IsAny<CancellationToken>()))
        //        .ReturnsAsync(true);

        //    var zipFileStream = new FileStream($"{AppContext.BaseDirectory}/UploadProcessor/gebouw_enddate_invalid.zip",
        //        FileMode.Open, FileAccess.Read);

        //    mockIBlobClient
        //        .Setup(x => x.GetBlobAsync(blobName, It.IsAny<CancellationToken>()))
        //        .ReturnsAsync(new BlobObject(blobName, null, ContentType.Parse("X-multipart/abc"),
        //            _ => Task.FromResult((Stream)zipFileStream)));

        //    var notificationsService = new Mock<INotificationService>();

        //    var sut = new UploadProcessor(
        //        _jobsContext,
        //        mockTicketing.Object,
        //        mockIBlobClient.Object,
        //        mockAmazonClient.Object,
        //        new NullLoggerFactory(),
        //        mockIHostApplicationLifeTime.Object,
        //        notificationsService.Object,
        //        Options.Create(new EcsTaskOptions()));

        //    // Act
        //    await sut.StartAsync(ct);

        //    // Assert
        //    mockTicketing.Verify(x => x.Error(
        //        ticketId,
        //        It.Is<TicketError>(ticketError =>
        //            ticketError.Errors.Any(y =>
        //                y.ErrorCode == "InvalidEndDate" &&
        //                y.ErrorMessage == "Record number(s):1,2")),
        //        It.IsAny<CancellationToken>()), Times.Once);

        //    var jobRecords = _jobsContext.JobRecords.Where(x => x.JobId == job.Id);
        //    jobRecords.Should().BeEmpty();

        //    _jobsContext.Jobs.First().Status.Should().Be(JobStatus.Error);

        //    notificationsService.Verify(x => x.PublishToTopicAsync(It.IsAny<NotificationMessage>()), Times.Once);

        //}

        //[Fact]
        //public async Task WhenBlobNotFoundException_ThenLogAndContinue()
        //{
        //    var ct = CancellationToken.None;
        //    var mockTicketing = new Mock<ITicketing>();
        //    var mockIBlobClient = new Mock<IBlobClient>();
        //    var mockAmazonClient = new Mock<IAmazonECS>();
        //    var mockIHostApplicationLifeTime = new Mock<IHostApplicationLifetime>();

        //    var ticketId = Guid.NewGuid();
        //    var job = new Job(DateTimeOffset.Now, JobStatus.Created, ticketId);

        //    _jobsContext.Jobs.Add(job);
        //    await _jobsContext.SaveChangesAsync(ct);

        //    var blobName = new BlobName(job.ReceivedBlobName);

        //    mockIBlobClient
        //        .Setup(x => x.BlobExistsAsync(blobName, It.IsAny<CancellationToken>()))
        //        .ReturnsAsync(true);

        //    mockIBlobClient
        //        .Setup(x => x.GetBlobAsync(blobName, It.IsAny<CancellationToken>()))
        //        .ReturnsAsync(new BlobObject(blobName, null, ContentType.Parse("X-multipart/abc"),
        //            _ => throw new BlobNotFoundException(blobName)));

        //    var notificationsService = new Mock<INotificationService>();

        //    var sut = new UploadProcessor(
        //        _jobsContext,
        //        mockTicketing.Object,
        //        mockIBlobClient.Object,
        //        mockAmazonClient.Object,
        //        new NullLoggerFactory(),
        //        mockIHostApplicationLifeTime.Object,
        //        notificationsService.Object,
        //        Options.Create(new EcsTaskOptions()));

        //    // Act
        //    await sut.StartAsync(ct);

        //    // Assert
        //    var jobRecords = _jobsContext.JobRecords.Where(x => x.JobId == job.Id);
        //    jobRecords.Should().BeEmpty();

        //    notificationsService.Verify(x => x.PublishToTopicAsync(It.IsAny<NotificationMessage>()), Times.Never);
        //}
    }
}
