namespace RoadRegistry.BackOffice.Abstractions;

using MediatR;

public abstract record SqsMessageRequest : IRequest
{
}

public abstract record SqsMessageRequest<TResponse> : IRequest<TResponse> where TResponse : SqsMessageResponse
{
}