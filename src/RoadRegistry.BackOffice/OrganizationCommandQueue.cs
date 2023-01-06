namespace RoadRegistry.BackOffice;

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Be.Vlaanderen.Basisregisters.EventHandling;
using Framework;
using Messages;
using SqlStreamStore;

public class OrganizationCommandQueue : RoadRegistryCommandQueue, IOrganizationCommandQueue
{
    private static readonly EventMapping CommandMapping =
        new(OrganizationCommands.All.ToDictionary(command => command.Name));

    public OrganizationCommandQueue(IStreamStore store)
        : base(store, CommandMapping)
    {
    }

    public Task Write(OrganizationId organizationId, Command command, CancellationToken cancellationToken)
    {
        return AppendToStoreStream(command, OrganizationId.ToStreamName(organizationId), cancellationToken);
    }
}
