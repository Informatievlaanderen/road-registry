namespace RoadRegistry.Jobs.Abstractions
{
    using System;

    public record GetJobsResponse(JobResponse[] Jobs, Uri? NextPage);
}
