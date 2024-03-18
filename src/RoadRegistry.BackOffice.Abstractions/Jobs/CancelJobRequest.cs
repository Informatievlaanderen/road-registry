namespace RoadRegistry.BackOffice.Abstractions.Jobs
{
    using System;
    using MediatR;

    public sealed record CancelJobRequest(Guid JobId) : IRequest<CancelJobResponse>;
}
