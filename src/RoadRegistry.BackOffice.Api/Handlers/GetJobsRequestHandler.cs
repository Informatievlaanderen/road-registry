namespace RoadRegistry.BackOffice.Api.Handlers
{
    using System;
    using Abstractions;
    using Infrastructure;
    using Jobs;
    using Jobs.Abstractions;
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using TicketingService.Abstractions;

    public class GetJobsRequestHandler : IRequestHandler<GetJobsRequest, GetJobsResponse>
    {
        private readonly JobsContext _jobsContext;
        private readonly ITicketingUrl _ticketingUrl;
        private readonly IPagedUriGenerator _pagedUriGenerator;

        public GetJobsRequestHandler(JobsContext jobsContext, ITicketingUrl ticketingUrl, IPagedUriGenerator pagedUriGenerator)
        {
            _jobsContext = jobsContext;
            _ticketingUrl = ticketingUrl;
            _pagedUriGenerator = pagedUriGenerator;
        }

        public async Task<GetJobsResponse> Handle(GetJobsRequest request, CancellationToken cancellationToken)
        {
            var query = _jobsContext.Jobs.AsQueryable();

            if (request.JobStatuses.Any())
            {
                query = query.Where(x => request.JobStatuses.Contains(x.Status));
            }

            var result = await query
                .OrderBy(x => x.Created)
                .Skip(request.Pagination.Offset ?? 0)
                .Take((request.Pagination.Limit ?? Pagination.MaxLimit) + 1)
                .ToListAsync(cancellationToken);

            return new GetJobsResponse(
                result
                    .Take(request.Pagination.Limit ?? Pagination.MaxLimit)
                    .Select(x =>
                    new JobResponse
                    (
                        Id : x.Id,
                        TicketUrl : x.TicketId != Guid.Empty ? _ticketingUrl.For(x.TicketId) : null,
                        Status : x.Status,
                        Created : x.Created,
                        LastChanged : x.LastChanged
                    )
                ).ToArray(),
                _pagedUriGenerator.NextPage(result, request.Pagination, "v1/upload/jobs"));
        }
    }
}

