namespace RoadRegistry.BackOffice.Api.Handlers
{
    using System;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Infrastructure;
    using Jobs;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions.Jobs;
    using TicketingService.Abstractions;

    public sealed class GetJobByIdRequestHandler : IRequestHandler<GetJobByIdRequest, JobResponse>
    {
        private readonly JobsContext _jobsContext;
        private readonly ITicketingUrl _ticketingUrl;
        private readonly IPagedUriGenerator _pagedUriGenerator;

        public GetJobByIdRequestHandler(
            JobsContext jobsContext,
            ITicketingUrl ticketingUrl,
            IPagedUriGenerator pagedUriGenerator)
        {
            _jobsContext = jobsContext;
            _ticketingUrl = ticketingUrl;
            _pagedUriGenerator = pagedUriGenerator;
        }

        public async Task<JobResponse> Handle(
            GetJobByIdRequest byIdRequest,
            CancellationToken cancellationToken)
        {
            var job = await _jobsContext.FindJob(byIdRequest.JobId, cancellationToken);

            if (job is null)
            {
                throw new ApiException("Onbestaande upload job.", StatusCodes.Status404NotFound);
            }

            return new JobResponse(
                job.Id,
                TicketUrl: job.TicketId != Guid.Empty ? _ticketingUrl.For(job.TicketId) : null,
                job.Status,
                job.Created,
                job.LastChanged);
        }
    }
}
