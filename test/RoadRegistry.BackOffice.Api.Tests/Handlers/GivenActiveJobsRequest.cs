namespace RoadRegistry.BackOffice.Api.Tests.Handlers
{
    using Api.Handlers;
    using AutoFixture;
    using FluentAssertions;
    using Jobs;
    using Jobs.Abstractions;
    using Moq;
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using TicketingService.Abstractions;
    using Xunit;

    public class GivenActiveJobsRequest
    {
        private readonly Fixture _fixture;
        private readonly FakeJobsContext _jobsContext;
        private readonly Mock<ITicketingUrl> _ticketingUrl;

        public GivenActiveJobsRequest()
        {
            _fixture = new Fixture();
            _jobsContext = new FakeJobsContextFactory().CreateDbContext();
            _ticketingUrl = new Mock<ITicketingUrl>();
            _ticketingUrl
                .Setup(x => x.For(It.IsAny<Guid>()))
                .Returns<Guid>(GetTicketUrl);
        }

        private static Uri GetTicketUrl(Guid ticketId)
        {
            return new Uri( $"https://api.basisregisters.vlaanderen.be/v2/tickets/{ticketId}");
        }

        [Fact]
        public async Task ThenReturnActiveJobs()
        {
            var jobInStatusCreated = new Job(DateTimeOffset.Now, JobStatus.Created, UploadType.Uploads, _fixture.Create<Guid>());
            var jobInStatusProcessing = new Job(DateTimeOffset.Now, JobStatus.Processing, UploadType.Uploads, _fixture.Create<Guid>());
            var jobInStatusError = new Job(DateTimeOffset.Now, JobStatus.Error, UploadType.Uploads, _fixture.Create<Guid>());
            var jobInStatusCancelled = new Job(DateTimeOffset.Now, JobStatus.Cancelled, UploadType.Uploads, _fixture.Create<Guid>());
            var jobInStatusCompleted = new Job(DateTimeOffset.Now, JobStatus.Completed, UploadType.Uploads, _fixture.Create<Guid>());
            _jobsContext.Jobs.Add(jobInStatusCreated);
            _jobsContext.Jobs.Add(jobInStatusProcessing);
            _jobsContext.Jobs.Add(jobInStatusError);
            _jobsContext.Jobs.Add(jobInStatusCancelled);
            _jobsContext.Jobs.Add(jobInStatusCompleted);
            await _jobsContext.SaveChangesAsync(CancellationToken.None);
            
            var handler = new GetActiveJobsRequestHandler(_jobsContext, _ticketingUrl.Object);
            var response = await handler.Handle(new GetActiveJobsRequest(), CancellationToken.None);

            response.Jobs.Should().HaveCount(2);
            response.Jobs.Should().ContainSingle(x =>
                x.Id == jobInStatusCreated.Id
                && x.Created == jobInStatusCreated.Created
                && x.Status == jobInStatusCreated.Status
                && x.TicketUrl == GetTicketUrl(jobInStatusCreated.TicketId));
            response.Jobs.Should().ContainSingle(x =>
                x.Id == jobInStatusProcessing.Id
                && x.Created == jobInStatusProcessing.Created
                && x.Status == jobInStatusProcessing.Status
                && x.TicketUrl == GetTicketUrl(jobInStatusProcessing.TicketId));
        }
    }
}
