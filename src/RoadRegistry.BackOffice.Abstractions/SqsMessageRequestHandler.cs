namespace RoadRegistry.BackOffice.Abstractions;

using Framework;
using MediatR;
using Microsoft.Extensions.Logging;

public abstract class SqsMessageRequestHandler<TRequest> : IRequestHandler<TRequest>
    where TRequest : IRequest
{
    public async Task<Unit> Handle(TRequest request, CancellationToken cancellationToken)
    {
        await HandleAsync(request, cancellationToken);
        return Unit.Value;
    }

    public abstract Task HandleAsync(TRequest request, CancellationToken cancellationToken);
}

public abstract class SqsMessageRequestHandler<TRequest, TResponse> : IRequestHandler<TRequest, TResponse>
    where TRequest : SqsMessageRequest<TResponse>
    where TResponse : SqsMessageResponse
{
    protected readonly ILogger _logger;

    protected SqsMessageRequestHandler(CommandHandlerDispatcher dispatcher, ILogger logger)
    {
        Dispatcher = dispatcher;
        _logger = logger;
    }

    protected CommandHandlerDispatcher Dispatcher { get; init; }

    public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken)
    {
        var response = await HandleAsync(request, cancellationToken);
        return response;
    }

    public abstract Task<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken);
}