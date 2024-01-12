namespace RoadRegistry.BackOffice;

using Be.Vlaanderen.Basisregisters.EventHandling;
using Framework;
using Messages;
using SqlStreamStore;
using SqlStreamStore.Streams;
using System.Threading;
using System.Threading.Tasks;

public interface IOrganizationEventWriter
{
    Task WriteAsync(OrganizationId id, IRoadRegistryMessage message, object @event, CancellationToken cancellationToken);
}

public class OrganizationEventWriter : RoadRegistryEventWriter, IOrganizationEventWriter
{
    private static readonly EventMapping EventMapping =
        new(EventMapping.DiscoverEventNamesInAssembly(typeof(RoadNetworkEvents).Assembly));

    public OrganizationEventWriter(IStreamStore store, EventEnricher enricher)
        : base(store, enricher, EventMapping)
    {
    }

    public Task WriteAsync(OrganizationId id, IRoadRegistryMessage message, object @event, CancellationToken cancellationToken)
    {
        return AppendToStoreStream(OrganizationId.ToStreamName(id), message, ExpectedVersion.Any, new[] { @event }, cancellationToken);
    }
}
