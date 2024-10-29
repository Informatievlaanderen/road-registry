namespace RoadRegistry.BackOffice;

using System;
using System.Threading;
using System.Threading.Tasks;
using Framework;
using SqlStreamStore;

public interface IMunicipalityEventWriter
{
    Task WriteAsync(StreamName streamName, int expectedVersion, Guid messageId, object[] events, CancellationToken cancellationToken);
}

public class MunicipalityEventWriter : RoadRegistryEventWriter, IMunicipalityEventWriter
{
    public MunicipalityEventWriter(IStreamStore store, EventEnricher enricher)
        : base(store, enricher)
    {
    }

    public Task WriteAsync(StreamName streamName, int expectedVersion, Guid messageId, object[] events, CancellationToken cancellationToken)
    {
        return AppendToStoreStream(streamName, messageId, expectedVersion, events, null, null, cancellationToken);
    }
}
