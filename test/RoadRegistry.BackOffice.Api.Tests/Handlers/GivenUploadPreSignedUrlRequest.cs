namespace RoadRegistry.BackOffice.Api.Tests.Handlers
{
    using System.Collections.Generic;
    using Abstractions.Jobs;
    using Api.Handlers;
    using Api.Infrastructure;
    using AutoFixture;
    using FluentAssertions;
    using Hosts.Infrastructure.Modules;
    using Jobs;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Moq;
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
            _fixture = new Fixture();

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
                _httpContextAccessor.Object
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
