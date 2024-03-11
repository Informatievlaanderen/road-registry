namespace RoadRegistry.BackOffice.Api.Handlers
{
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

        public GetActiveJobsRequestHandler(
            JobsContext jobsContext,
            ITicketingUrl ticketingUrl)
        {
            _jobsContext = jobsContext;
            _ticketingUrl = ticketingUrl;
        }

        public async Task<GetActiveJobsResponse> Handle(
            GetActiveJobsRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _jobsContext
                .Jobs
                .Where(x => x.Status == JobStatus.Created || x.Status == JobStatus.Preparing)
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
