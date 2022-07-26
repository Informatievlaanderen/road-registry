namespace RoadRegistry.Api.BackOffice.Abstractions;

using MediatR;

public record EndpointRequest : IRequest { }
public record EndpointRequest<TResponse> : IRequest<TResponse> where TResponse : EndpointResponse { }
public abstract record EndpointResponse() { }
