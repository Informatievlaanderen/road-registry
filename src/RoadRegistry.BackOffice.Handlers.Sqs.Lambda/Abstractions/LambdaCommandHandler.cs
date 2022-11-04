namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Abstractions;

using Amazon.Lambda.Core;
using MediatR;

public abstract class LambdaCommandHandler<TCommand> : IRequestHandler<TCommand>
    where TCommand : LambdaCommand
{
    public async Task<Unit> Handle(TCommand request, CancellationToken cancellationToken)
    {
        await HandleAsync(request, request.GetContext(), cancellationToken);
        return Unit.Value;
    }

    public abstract Task HandleAsync(TCommand command, ILambdaContext context, CancellationToken cancellationToken);
}

public abstract class LambdaCommandHandler<TCommand, TResponse> : IRequestHandler<TCommand, TResponse>
    where TCommand : LambdaCommand<TResponse>
    where TResponse : LambdaCommandResponse
{
    public async Task<TResponse> Handle(TCommand request, CancellationToken cancellationToken)
    {
        var response = await HandleAsync(request, request.GetContext(), cancellationToken);
        return response;
    }

    public abstract Task<TResponse> HandleAsync(TCommand command, ILambdaContext context, CancellationToken cancellationToken);
}
