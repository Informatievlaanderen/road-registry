namespace RoadRegistry.BackOffice.Api.Tests.Handlers
{
    using Api.Handlers;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using FluentAssertions;
    using Jobs;
    using Microsoft.AspNetCore.Http;
    using Moq;
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions.Jobs;
    using TicketingService.Abstractions;
    using Xunit;

    public class GivenCancelJobRequest
    {
        private readonly Fixture _fixture;
        private readonly FakeJobsContext _jobsContext;

        public GivenCancelJobRequest()
        {
            _fixture = FixtureFactory.Create();
            _jobsContext = new FakeJobsContextFactory().CreateDbContext();
        }

        [Fact]
        public async Task WithNotExistingJobId_ThenReturnsNotFound()
        {
            var jobId = _fixture.Create<Guid>();
            var request = new CancelJobRequest(jobId);
            var handler = new CancelJobRequestHandler(_jobsContext, Mock.Of<ITicketing>());

            var act = () => handler.Handle(request, CancellationToken.None);

            await act.Should()
                .ThrowAsync<ApiException>()
                .Where(x =>
                    x.StatusCode == StatusCodes.Status404NotFound
                    && x.Message == "Onbestaande upload job.");
        }

        [Theory]
        [InlineData(JobStatus.Processing)]
        [InlineData(JobStatus.Completed)]
        [InlineData(JobStatus.Error)]
        [InlineData(JobStatus.Cancelled)]
        public async Task WithJobBeingNotCreated_ThenReturnsBadRequest(JobStatus jobStatus)
        {
            // Arrange
            var job = _fixture.Create<Job>();
            job.UpdateStatus(jobStatus);
            _jobsContext.Jobs.Add(job);

            var request = new CancelJobRequest(job.Id);
            var handler = new CancelJobRequestHandler(_jobsContext, Mock.Of<ITicketing>());

            // Act
            var act = () => handler.Handle(request, CancellationToken.None);

            // Assert
            await act.Should()
                .ThrowAsync<ApiException>()
                .Where(x =>
                    x.StatusCode == StatusCodes.Status400BadRequest
                    && x.Message == $"De status van de upload job '{job.Id}' is {job.Status.ToString().ToLower()}, hierdoor kan deze job niet geannuleerd worden.");
        }

        [Fact]
        public async Task WithJobInStatusCreated_ThenJobIsCancelled()
        {
            // Arrange
            var job = _fixture.Create<Job>();
            job.UpdateStatus(JobStatus.Created);
            _jobsContext.Jobs.Add(job);

            var ticketing = new Mock<ITicketing>();
            ticketing
                .Setup(x => x.Get(job.TicketId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(
                    new Ticket(
                        job.TicketId,
                        TicketStatus.Created,
                        new Dictionary<string, string>()
                        ));

            var request = new CancelJobRequest(job.Id);
            var handler = new CancelJobRequestHandler(_jobsContext, ticketing.Object);

            // Act
            await handler.Handle(request, CancellationToken.None);

            // Assert
            job.Status.Should().Be(JobStatus.Cancelled);
            ticketing.Verify(x => x.Complete(
                job.TicketId,
                new TicketResult(new { JobStatus = "Cancelled" }),
                It.IsAny<CancellationToken>()));
        }
    }
}
