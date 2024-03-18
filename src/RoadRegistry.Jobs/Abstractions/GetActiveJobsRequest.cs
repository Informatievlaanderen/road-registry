namespace RoadRegistry.Jobs.Abstractions
{
    using MediatR;

    public sealed record GetActiveJobsRequest() : IRequest<GetActiveJobsResponse>;
}
