namespace RoadRegistry.Jobs.Abstractions
{
    using System;
    using MediatR;

    public sealed record CancelJobRequest(Guid JobId) : IRequest;
}
