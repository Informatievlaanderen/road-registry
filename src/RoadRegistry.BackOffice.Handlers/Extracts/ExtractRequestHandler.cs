namespace RoadRegistry.BackOffice.Handlers.Extracts;

using Abstractions;
using Framework;
using Microsoft.Extensions.Logging;

public abstract class ExtractRequestHandler<TRequest, TResponse> : EndpointRequestHandler<TRequest, TResponse>
    where TRequest : EndpointRequest<TResponse>
    where TResponse : EndpointResponse
{
    protected ExtractRequestHandler(
        CommandHandlerDispatcher dispatcher,
        ILogger logger) : base(dispatcher, logger)
    {
    }

    protected override async Task<TResponse> InnerHandleAsync(TRequest request, CancellationToken cancellationToken)
    {
        var downloadId = new DownloadId(Guid.NewGuid());
        var randomExternalRequestId = Guid.NewGuid().ToString("N");

        return await HandleRequestAsync(request, downloadId, randomExternalRequestId, cancellationToken);
    }

    protected abstract Task<TResponse> HandleRequestAsync(TRequest request, DownloadId downloadId, string randomExternalRequestId, CancellationToken cancellationToken);
}
