namespace RoadRegistry.BackOffice.Abstractions.Jobs
{
    using System;

    public record GetJobsResponse(JobResponse[] Jobs, Uri? NextPage);
}
