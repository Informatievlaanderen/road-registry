namespace RoadRegistry.BackOffice.Abstractions;

using Framework;
using Messages;
using Microsoft.Extensions.Logging;

public abstract class EndpointRequestHandler<TRequest, TResponse> : RequestHandler<TRequest, TResponse>
    where TRequest : EndpointRequest<TResponse>
    where TResponse : EndpointResponse
{
    private readonly RoadRegistryCommandSender _commandSender;

    protected delegate Task RoadRegistryCommandSender(Command command, CancellationToken cancellationToken);

    protected RoadRegistryCommandSender Dispatch { get; private set; }
    protected RoadRegistryCommandSender Queue { get; private set; }

    protected EndpointRequestHandler(IRoadNetworkCommandQueue commandQueue, ILogger logger) : base(logger)
    {
        _commandSender = async (command, cancellationToken) => await commandQueue.Write(command, cancellationToken);
    }

    protected EndpointRequestHandler(CommandHandlerDispatcher commandHandlerDispatcher, ILogger logger) : base(logger)
    {
        _commandSender = (command, cancellationToken) => commandHandlerDispatcher(command, cancellationToken);
    }

    public override async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken)
    {
        Dispatch = async (command, token) => await HandleInitializer(command, token);
        Queue = async (command, token) => await HandleInitializer(command, token);

        var response = await HandleAsync(request, cancellationToken);
        return response;

        async Task HandleInitializer(Command command, CancellationToken token)
        {
            if (command.Body is RoadRegistryCommandIdentity commandMessage)
            {
                commandMessage.IdentityData = new RoadRegistryIdentity
                {
                    Metadata = request.Metadata,
                    ProvenanceData = request.ProvenanceData
                };
            }
            await _commandSender(command, token);
        }
    }
}
