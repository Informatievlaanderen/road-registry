namespace RoadRegistry.BackOffice.Abstractions;

using Microsoft.Extensions.Logging;

public abstract class SqsMessageRequestHandler<TRequest, TResponse> : RequestHandler<TRequest, TResponse>
    where TRequest : SqsMessageRequest<TResponse>
    where TResponse : SqsMessageResponse
{
    protected SqsMessageRequestHandler(ILogger logger) : base(logger)
    {
    }
}
