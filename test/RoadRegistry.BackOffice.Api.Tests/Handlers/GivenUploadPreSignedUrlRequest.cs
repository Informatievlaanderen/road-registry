namespace RoadRegistry.BackOffice.Api.Tests.Handlers
{
    using System.Collections.Generic;
    using System.Linq;
    using Abstractions.Jobs;
    using Api.Handlers;
    using Api.Infrastructure;
    using Api.Infrastructure.Options;
    using AutoFixture;
    using FluentAssertions;
    using Hosts.Infrastructure.Modules;
    using Jobs;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Options;
    using Moq;
    using FluentValidation;
    using TicketingService.Abstractions;

    public class GivenUploadPreSignedUrlRequest
    {
        private readonly Fixture _fixture;

        private readonly Mock<ITicketingUrl> _ticketingUrl;
        private readonly Mock<ITicketing> _ticketing;
        private readonly Mock<IJobUploadUrlPresigner> _uploadUrlPresigner;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessor;

        public GivenUploadPreSignedUrlRequest()
        {
            _fixture = FixtureFactory.Create();

            _ticketingUrl = new Mock<ITicketingUrl>();
            _ticketing = new Mock<ITicketing>();
            _uploadUrlPresigner = new Mock<IJobUploadUrlPresigner>();
            _httpContextAccessor = new Mock<IHttpContextAccessor>();

            _httpContextAccessor.Setup(x => x.HttpContext).Returns(new DefaultHttpContext());
        }

        private GetPresignedUploadUrlRequestHandler CreatePreSignedUrlRequestHandler(JobsContext jobsContext)
        {
            return new GetPresignedUploadUrlRequestHandler(
                jobsContext,
                _ticketing.Object,
                new FakeTicketingOptions(),
                _ticketingUrl.Object,
                _uploadUrlPresigner.Object,
                _httpContextAccessor.Object,
                Options.Create(new JobsBucketOptions { PendingUploadTimeoutInMinutes = 5 })
            );
        }

        [Fact]
        public async Task ReturnsGetPreSignedUrlResponse_UploadType_Uploads()
        {
            var ticketId = Guid.NewGuid();
            var ticketUrl = $"https://api.ticketing.vlaanderen.be/{ticketId}";
            var preSignedUrl = new Uri("https://signedUrl");

            _ticketing
                .Setup(x => x.CreateTicket(It.IsAny<IDictionary<string, string>>(), CancellationToken.None))
                .ReturnsAsync(ticketId);
            _ticketingUrl
                .Setup(x => x.For(ticketId))
                .Returns(new Uri(ticketUrl));
            _uploadUrlPresigner
                .Setup(x => x.CreatePresignedUploadUrl(It.IsAny<Job>()))
                .Returns(new CreatePresignedPostResponse(preSignedUrl, _fixture.Create<Dictionary<string, string>>()));

            var jobsContext = new FakeJobsContextFactory().CreateDbContext();
            var handler = CreatePreSignedUrlRequestHandler(jobsContext);
            var response = await handler.Handle(GetPresignedUploadUrlRequest.ForUploads(), CancellationToken.None);

            var job = await jobsContext.Jobs.SingleOrDefaultAsync();

            job.Should().NotBeNull();
            job.TicketId.Should().Be(ticketId);
            job.Status.Should().Be(JobStatus.Created);
            job.UploadType.Should().Be(UploadType.Uploads);
            job.DownloadId.Should().Be(null);
            response.JobId.Should().Be(job.Id);
            response.TicketUrl.Should().Be(ticketUrl);
            response.UploadUrl.Should().Be(preSignedUrl.ToString());

            var transaction = (FakeDbContextTransaction)jobsContext.Database.CurrentTransaction;
            transaction!.Status.Should().Be(FakeDbContextTransaction.TransactionStatus.Committed);
        }

        [Fact]
        public async Task ReturnsGetPreSignedUrlResponse_UploadType_Extracts()
        {
            var ticketId = Guid.NewGuid();
            var ticketUrl = $"https://api.ticketing.vlaanderen.be/{ticketId}";
            var preSignedUrl = new Uri("https://signedUrl");
            var downloadId = Guid.NewGuid();

            _ticketing
                .Setup(x => x.CreateTicket(It.IsAny<IDictionary<string, string>>(), CancellationToken.None))
                .ReturnsAsync(ticketId);
            _ticketingUrl
                .Setup(x => x.For(ticketId))
                .Returns(new Uri(ticketUrl));
            _uploadUrlPresigner
                .Setup(x => x.CreatePresignedUploadUrl(It.IsAny<Job>()))
                .Returns(new CreatePresignedPostResponse(preSignedUrl, _fixture.Create<Dictionary<string, string>>()));

            var jobsContext = new FakeJobsContextFactory().CreateDbContext();
            var handler = CreatePreSignedUrlRequestHandler(jobsContext);
            var response = await handler.Handle(GetPresignedUploadUrlRequest.ForExtracts(new DownloadId(downloadId)), CancellationToken.None);

            var job = await jobsContext.Jobs.SingleOrDefaultAsync();

            job.Should().NotBeNull();
            job.TicketId.Should().Be(ticketId);
            job.Status.Should().Be(JobStatus.Created);
            job.UploadType.Should().Be(UploadType.Extracts);
            job.DownloadId.Should().Be(downloadId);
            response.JobId.Should().Be(job.Id);
            response.TicketUrl.Should().Be(ticketUrl);
            response.UploadUrl.Should().Be(preSignedUrl.ToString());

            var transaction = (FakeDbContextTransaction)jobsContext.Database.CurrentTransaction;
            transaction!.Status.Should().Be(FakeDbContextTransaction.TransactionStatus.Committed);
        }

        [Fact]
        public async Task ReturnsGetPreSignedUrlResponse_UploadType_ExtractsV2()
        {
            var ticketId = Guid.NewGuid();
            var ticketUrl = $"https://api.ticketing.vlaanderen.be/{ticketId}";
            var preSignedUrl = new Uri("https://signedUrl");
            var downloadId = Guid.NewGuid();

            _ticketing
                .Setup(x => x.CreateTicket(It.IsAny<IDictionary<string, string>>(), CancellationToken.None))
                .ReturnsAsync(ticketId);
            _ticketingUrl
                .Setup(x => x.For(ticketId))
                .Returns(new Uri(ticketUrl));
            _uploadUrlPresigner
                .Setup(x => x.CreatePresignedUploadUrl(It.IsAny<Job>()))
                .Returns(new CreatePresignedPostResponse(preSignedUrl, _fixture.Create<Dictionary<string, string>>()));

            var jobsContext = new FakeJobsContextFactory().CreateDbContext();
            var handler = CreatePreSignedUrlRequestHandler(jobsContext);
            var response = await handler.Handle(GetPresignedUploadUrlRequest.ForExtractsV2(new DownloadId(downloadId)), CancellationToken.None);

            var job = await jobsContext.Jobs.SingleOrDefaultAsync();

            job.Should().NotBeNull();
            job.TicketId.Should().Be(ticketId);
            job.Status.Should().Be(JobStatus.Created);
            job.UploadType.Should().Be(UploadType.ExtractsV2);
            job.DownloadId.Should().Be(downloadId);
            response.JobId.Should().Be(job.Id);
            response.TicketId.Should().Be(ticketId);
            response.TicketUrl.Should().Be(ticketUrl);
            response.UploadUrl.Should().Be(preSignedUrl.ToString());

            var transaction = (FakeDbContextTransaction)jobsContext.Database.CurrentTransaction;
            transaction!.Status.Should().Be(FakeDbContextTransaction.TransactionStatus.Committed);
        }

        [Theory]
        [InlineData(JobStatus.Created, 0)]
        [InlineData(JobStatus.Created, 4)]
        [InlineData(JobStatus.Processing, 0)]
        [InlineData(JobStatus.Processing, 30)]
        public async Task GivenAnUploadIsPendingForTheDownload_ThenNoJobIsCreated(JobStatus status, int minutesAgo)
        {
            var downloadId = Guid.NewGuid();

            var jobsContext = new FakeJobsContextFactory().CreateDbContext();
            jobsContext.Jobs.Add(new Job(DateTimeOffset.UtcNow.AddMinutes(-minutesAgo), status, UploadType.Inwinning, Guid.NewGuid())
            {
                DownloadId = downloadId
            });
            await jobsContext.SaveChangesAsync();

            var handler = CreatePreSignedUrlRequestHandler(jobsContext);
            var act = () => handler.Handle(GetPresignedUploadUrlRequest.ForInwinning(new DownloadId(downloadId)), CancellationToken.None);

            var ex = await act.Should().ThrowAsync<ValidationException>();
            ex.Which.Errors.Single().ErrorCode.Should().Be(UploadNotFinished.ErrorCode);

            jobsContext.Jobs.Should().ContainSingle();
            _ticketing.Verify(x => x.CreateTicket(It.IsAny<IDictionary<string, string>>(), It.IsAny<CancellationToken>()), Times.Never);

            var transaction = (FakeDbContextTransaction)jobsContext.Database.CurrentTransaction;
            transaction!.Status.Should().Be(FakeDbContextTransaction.TransactionStatus.Rolledback);
        }

        [Theory]
        [InlineData(JobStatus.Created, 6)]
        [InlineData(JobStatus.Completed, 0)]
        [InlineData(JobStatus.Cancelled, 0)]
        [InlineData(JobStatus.Error, 0)]
        public async Task GivenNoUploadIsPendingForTheDownload_ThenAJobIsCreated(JobStatus status, int minutesAgo)
        {
            var ticketId = Guid.NewGuid();
            var downloadId = Guid.NewGuid();

            _ticketing
                .Setup(x => x.CreateTicket(It.IsAny<IDictionary<string, string>>(), CancellationToken.None))
                .ReturnsAsync(ticketId);
            _ticketingUrl
                .Setup(x => x.For(ticketId))
                .Returns(new Uri($"https://api.ticketing.vlaanderen.be/{ticketId}"));
            _uploadUrlPresigner
                .Setup(x => x.CreatePresignedUploadUrl(It.IsAny<Job>()))
                .Returns(new CreatePresignedPostResponse(new Uri("https://signedUrl"), _fixture.Create<Dictionary<string, string>>()));

            var jobsContext = new FakeJobsContextFactory().CreateDbContext();
            jobsContext.Jobs.Add(new Job(DateTimeOffset.UtcNow.AddMinutes(-minutesAgo), status, UploadType.Inwinning, Guid.NewGuid())
            {
                DownloadId = downloadId
            });
            jobsContext.Jobs.Add(new Job(DateTimeOffset.UtcNow, JobStatus.Processing, UploadType.Inwinning, Guid.NewGuid())
            {
                DownloadId = Guid.NewGuid()
            });
            await jobsContext.SaveChangesAsync();

            var handler = CreatePreSignedUrlRequestHandler(jobsContext);
            var response = await handler.Handle(GetPresignedUploadUrlRequest.ForInwinning(new DownloadId(downloadId)), CancellationToken.None);

            var job = await jobsContext.Jobs.SingleAsync(x => x.Id == response.JobId);
            job.DownloadId.Should().Be(downloadId);
            job.TicketId.Should().Be(ticketId);
            job.Status.Should().Be(JobStatus.Created);
        }

        [Fact]
        public async Task WhenError_ThenNoJobIsCreated()
        {
            _ticketing
                .Setup(x => x.CreateTicket(It.IsAny<IDictionary<string, string>>(), CancellationToken.None))
                .ThrowsAsync(new Exception());

            var databaseName = nameof(WhenError_ThenNoJobIsCreated);
            await using var jobsContext = new FakeJobsContextFactory(databaseName).CreateDbContext();
            var handler = CreatePreSignedUrlRequestHandler(jobsContext);

            try
            {
                await handler.Handle(GetPresignedUploadUrlRequest.ForUploads(), CancellationToken.None);
            }
            catch
            {
                // ignored
            }

            var transaction = (FakeDbContextTransaction)jobsContext.Database.CurrentTransaction;
            transaction!.Status.Should().Be(FakeDbContextTransaction.TransactionStatus.Rolledback);
        }
    }
}
