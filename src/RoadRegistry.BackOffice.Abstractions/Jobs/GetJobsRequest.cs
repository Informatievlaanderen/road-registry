namespace RoadRegistry.BackOffice.Abstractions.Jobs
{
    using Abstractions;
    using MediatR;
    using System.Collections.Generic;

    public record GetJobsRequest(Pagination Pagination, List<JobStatus> JobStatuses) : IRequest<GetJobsResponse>;
}
