namespace RoadRegistry.Jobs.Abstractions
{
    using BackOffice.Abstractions;
    using Jobs;
    using MediatR;
    using System.Collections.Generic;

    public record GetJobsRequest(Pagination Pagination, List<JobStatus> JobStatuses) : IRequest<GetJobsResponse>;
}
