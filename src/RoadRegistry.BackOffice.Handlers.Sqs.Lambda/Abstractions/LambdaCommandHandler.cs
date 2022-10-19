namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Abstractions;

using Amazon.Lambda.Core;
using MediatR;

public abstract class LambdaCommandHandler<TRequest> : IRequestHandler<TRequest>
    where TRequest : LambdaCommand
{
    public async Task<Unit> Handle(TRequest request, CancellationToken cancellationToken)
    {
        await HandleAsync(request, request.GetContext(), cancellationToken);
        return Unit.Value;
    }

    public abstract Task HandleAsync(TRequest request, ILambdaContext context, CancellationToken cancellationToken);
}

public abstract class LambdaCommandHandler<TRequest, TResponse> : IRequestHandler<TRequest, TResponse>
    where TRequest : LambdaCommand<TResponse>
    where TResponse : LambdaCommandResponse
{
    public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken)
    {
        var response = await HandleAsync(request, request.GetContext(), cancellationToken);
        return response;
    }

    public abstract Task<TResponse> HandleAsync(TRequest request, ILambdaContext context, CancellationToken cancellationToken);
}
