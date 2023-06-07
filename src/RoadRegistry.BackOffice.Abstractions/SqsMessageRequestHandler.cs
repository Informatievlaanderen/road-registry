namespace RoadRegistry.BackOffice.Abstractions;

using MediatR;
using Microsoft.Extensions.Logging;

public abstract class SqsMessageRequestHandler<TRequest> : IRequestHandler<TRequest>
    where TRequest : SqsMessageRequest
{
    public async Task Handle(TRequest request, CancellationToken cancellationToken)
    {
        await HandleAsync(request, cancellationToken);
    }

    public abstract Task HandleAsync(TRequest request, CancellationToken cancellationToken);
}

public abstract class SqsMessageRequestHandler<TRequest, TResponse> : IRequestHandler<TRequest, TResponse>
    where TRequest : SqsMessageRequest<TResponse>
    where TResponse : SqsMessageResponse
{
    protected readonly ILogger Logger;

    protected SqsMessageRequestHandler(ILogger logger)
    {
        Logger = logger;
    }
    
    public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken)
    {
        var response = await HandleAsync(request, cancellationToken);
        return response;
    }

    public abstract Task<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken);
}
