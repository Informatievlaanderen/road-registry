namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Abstractions;

using Amazon.Lambda.Core;
using MediatR;

public abstract record LambdaCommand : IRequest
{
    private readonly ILambdaContext _context;

    protected LambdaCommand(ILambdaContext context)
    {
        _context = context;
    }

    public ILambdaContext GetContext()
    {
        return _context;
    }
}

public abstract record LambdaCommand<TResponse> : IRequest<TResponse> where TResponse : LambdaCommandResponse
{
    private readonly ILambdaContext _context;

    protected LambdaCommand(ILambdaContext context)
    {
        _context = context;
    }

    public ILambdaContext GetContext()
    {
        return _context;
    }
}