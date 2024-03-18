namespace RoadRegistry.BackOffice.Abstractions.Jobs
{
    using MediatR;

    public sealed record GetActiveJobsRequest() : IRequest<GetActiveJobsResponse>;
}
