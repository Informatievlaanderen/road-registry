namespace RoadRegistry.Hosts;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BackOffice.Framework;
using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;

public class RoadRegistryIdempotentCommandHandler : IIdempotentCommandHandler
{
    private readonly CommandHandlerDispatcher _dispatcher;

    public RoadRegistryIdempotentCommandHandler(CommandHandlerDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    public async Task<long> Dispatch(Guid? commandId, object command, IDictionary<string, object> metadata, CancellationToken cancellationToken)
    {
        if (command is Command c)
        {
            await _dispatcher(c, cancellationToken);
            return 0;
        }

        await _dispatcher(new Command(command), cancellationToken);
        return 0;
    }
}
