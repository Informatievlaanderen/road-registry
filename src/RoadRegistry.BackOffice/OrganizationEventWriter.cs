namespace RoadRegistry.BackOffice;

using Be.Vlaanderen.Basisregisters.EventHandling;
using Framework;
using Messages;
using SqlStreamStore;
using System;
using System.Threading;
using System.Threading.Tasks;

public interface IOrganizationEventWriter
{
    Task WriteAsync(OrganizationId id, Guid messageId, object @event, CancellationToken cancellationToken);
    //Task WriteAsync(StreamName streamName, IRoadRegistryMessage message, object @event, CancellationToken cancellationToken);
}

public class OrganizationEventWriter : RoadRegistryEventWriter, IOrganizationEventWriter
{
    private static readonly EventMapping EventMapping =
        new(EventMapping.DiscoverEventNamesInAssembly(typeof(RoadNetworkEvents).Assembly));

    public OrganizationEventWriter(IStreamStore store, EventEnricher enricher)
        : base(store, enricher, EventMapping)
    {
    }

    public Task WriteAsync(OrganizationId id, Guid messageId, object @event, CancellationToken cancellationToken)
    {
        return WriteAsync(OrganizationId.ToStreamName(id), Guid.NewGuid(), @event, cancellationToken);
    }
}
