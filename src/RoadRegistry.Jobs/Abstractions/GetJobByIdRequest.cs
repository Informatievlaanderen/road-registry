namespace RoadRegistry.Jobs.Abstractions
{
    using System;
    using MediatR;

    public sealed record GetJobByIdRequest(Guid JobId) : IRequest<JobResponse>;
}
