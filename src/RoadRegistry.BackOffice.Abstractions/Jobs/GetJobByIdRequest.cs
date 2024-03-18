namespace RoadRegistry.BackOffice.Abstractions.Jobs
{
    using System;
    using MediatR;

    public sealed record GetJobByIdRequest(Guid JobId) : IRequest<JobResponse>;
}
