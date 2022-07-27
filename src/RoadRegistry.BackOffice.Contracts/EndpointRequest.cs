namespace RoadRegistry.BackOffice.Contracts;

using MediatR;

public abstract record EndpointRequest : IRequest
{
}

public abstract record EndpointRequest<TResponse> : IRequest<TResponse> where TResponse : EndpointResponse
{
}
