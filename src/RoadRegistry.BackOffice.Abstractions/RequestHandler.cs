namespace RoadRegistry.BackOffice.Abstractions;

using MediatR;
using Microsoft.Extensions.Logging;

public abstract class RequestHandler<TRequest, TResponse> : IRequestHandler<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    protected readonly ILogger Logger;

    protected RequestHandler(ILogger logger)
    {
        Logger = logger;
    }

    public virtual async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken)
    {
        var response = await InnerHandleAsync(request, cancellationToken);
        return response;
    }

    protected abstract Task<TResponse> InnerHandleAsync(TRequest request, CancellationToken cancellationToken);
}
