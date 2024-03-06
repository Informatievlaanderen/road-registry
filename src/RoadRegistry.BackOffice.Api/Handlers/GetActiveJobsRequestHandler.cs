namespace RoadRegistry.BackOffice.Api.Handlers
{
    using Infrastructure;
    using Jobs;
    using Jobs.Abstractions;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using TicketingService.Abstractions;

    public sealed class GetActiveJobsRequestHandler : IRequestHandler<GetActiveJobsRequest, GetActiveJobsResponse>
    {
        private readonly JobsContext _jobsContext;
        private readonly ITicketingUrl _ticketingUrl;
        private readonly IPagedUriGenerator _pagedUriGenerator;

        public GetActiveJobsRequestHandler(
            JobsContext jobsContext,
            ITicketingUrl ticketingUrl,
            IPagedUriGenerator pagedUriGenerator)
        {
            _jobsContext = jobsContext;
            _ticketingUrl = ticketingUrl;
            _pagedUriGenerator = pagedUriGenerator;
        }

        public async Task<GetActiveJobsResponse> Handle(
            GetActiveJobsRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _jobsContext
                .Jobs
                .Where(x => x.Status != JobStatus.Cancelled && x.Status != JobStatus.Completed)
                .ToListAsync(cancellationToken);

            return new GetActiveJobsResponse(
                result.Select(x =>
                    new JobResponse
                    (
                        Id: x.Id,
                        TicketUrl: x.TicketId.HasValue ? _ticketingUrl.For(x.TicketId.Value) : null,
                        Status: x.Status,
                        Created: x.Created,
                        LastChanged: x.LastChanged
                    )
                ).ToArray());
        }
    }
}
