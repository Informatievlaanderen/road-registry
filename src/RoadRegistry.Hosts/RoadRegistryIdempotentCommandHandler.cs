namespace RoadRegistry.Hosts;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BackOffice.Framework;
using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Handlers;

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
