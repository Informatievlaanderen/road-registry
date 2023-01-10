namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda;

using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Handlers;
using Framework;

public class RoadRegistryIdempotentCommandHandler : IIdempotentCommandHandler
{
    private readonly CommandHandlerDispatcher _dispatcher;

    public RoadRegistryIdempotentCommandHandler(CommandHandlerDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    public async Task<long> Dispatch(Guid? commandId, object command, IDictionary<string, object> metadata, CancellationToken cancellationToken)
    {
        await _dispatcher(new Command(command), cancellationToken);
        return 0;
    }
}
