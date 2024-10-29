namespace RoadRegistry.BackOffice;

using System.Threading;
using System.Threading.Tasks;
using Framework;
using SqlStreamStore;
using SqlStreamStore.Streams;

public interface IOrganizationEventWriter
{
    Task WriteAsync(OrganizationId id, Event @event, CancellationToken cancellationToken);
}

public class OrganizationEventWriter : RoadRegistryEventWriter, IOrganizationEventWriter
{
    public OrganizationEventWriter(IStreamStore store, EventEnricher enricher)
        : base(store, enricher)
    {
    }

    public Task WriteAsync(OrganizationId id, Event @event, CancellationToken cancellationToken)
    {
        return AppendToStoreStream(OrganizationId.ToStreamName(id), @event, ExpectedVersion.Any, [@event.Body], cancellationToken);
    }
}
