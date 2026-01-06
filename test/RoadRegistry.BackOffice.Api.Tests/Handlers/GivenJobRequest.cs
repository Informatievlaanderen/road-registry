namespace RoadRegistry.BackOffice.Api.Tests.Handlers
{
    using Api.Infrastructure;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using FluentAssertions;
    using Jobs;
    using Microsoft.AspNetCore.Http;
    using Moq;
    using RoadRegistry.BackOffice.Api.Handlers;
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions.Jobs;
    using TicketingService.Abstractions;
    using Xunit;

    public class GivenJobRequest
    {
        private readonly Fixture _fixture;
        private readonly FakeJobsContext _jobsContext;
        private readonly Mock<ITicketingUrl> _ticketingUrl;

        public GivenJobRequest()
        {
            _fixture = FixtureFactory.Create();
            _jobsContext = new FakeJobsContextFactory().CreateDbContext();
            _ticketingUrl = new Mock<ITicketingUrl>();
            _ticketingUrl
                .Setup(x => x.For(It.IsAny<Guid>()))
                .Returns<Guid>(ticketId => new Uri(GetTicketUrl(ticketId)));
        }

        private static string GetTicketUrl(Guid ticketId)
        {
            return $"https://api.basisregisters.vlaanderen.be/v2/tickets/{ticketId}";
        }

        [Theory]
        [InlineData(JobStatus.Completed)]
        [InlineData(JobStatus.Error)]
        public async Task ThenReturnJobsRecordsForJob(JobStatus jobStatus)
        {
            var job = new Job(DateTimeOffset.Now, jobStatus, UploadType.Uploads, _fixture.Create<Guid>());
            var anotherJob = new Job(DateTimeOffset.Now, jobStatus, UploadType.Uploads, _fixture.Create<Guid>());
            _jobsContext.Jobs.Add(job);
            _jobsContext.Jobs.Add(anotherJob);
            await _jobsContext.SaveChangesAsync(CancellationToken.None);

            var mockPagedUriGenerator = new Mock<IPagedUriGenerator>();

            var handler = new GetJobByIdRequestHandler(_jobsContext, _ticketingUrl.Object, mockPagedUriGenerator.Object);
            var response = await handler.Handle(new GetJobByIdRequest(job.Id), CancellationToken.None);

            response.Id.Should().Be(job.Id);
            response.Created.Should().Be(job.Created);
            response.Status.Should().Be(job.Status);
            response.TicketUrl.Should().Be(GetTicketUrl(job.TicketId));
        }

        [Fact]
        public async Task WithUnexistingJob_ThenThrowsApiException()
        {
            var jobId = _fixture.Create<Guid>();

            var mockPagedUriGenerator = new Mock<IPagedUriGenerator>();
            var handler = new GetJobByIdRequestHandler(_jobsContext, _ticketingUrl.Object, mockPagedUriGenerator.Object);
            var act = async () => await handler.Handle(new GetJobByIdRequest(jobId), CancellationToken.None);

            await act.Should()
                .ThrowAsync<ApiException>()
                .Where(x =>
                    x.StatusCode == StatusCodes.Status404NotFound
                    && x.Message == "Onbestaande upload job.");
        }
    }
}
