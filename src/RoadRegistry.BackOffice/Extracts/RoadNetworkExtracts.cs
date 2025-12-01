namespace RoadRegistry.BackOffice.Extracts;

using System;
using System.Threading;
using System.Threading.Tasks;
using Be.Vlaanderen.Basisregisters.EventHandling;
using Core;
using Framework;
using Newtonsoft.Json;
using RoadRegistry.Extensions;
using SqlStreamStore;

public class RoadNetworkExtracts : EventSourcedEntityRepository<RoadNetworkExtract, ExtractRequestId>, IRoadNetworkExtracts
{
    private static readonly StreamName Prefix = new("extract-");

    public RoadNetworkExtracts(EventSourcedEntityMap map, IStreamStore store, JsonSerializerSettings settings, EventMapping mapping, EventEnricher eventEnricher)
        : base(map, store, settings, mapping,
            ToStreamName,
            () => RoadNetworkExtract.Factory(eventEnricher.ThrowIfNull())
        )
    {
    }

    public static StreamName ToStreamName(ExtractRequestId id)
    {
        return Prefix.WithSuffix(id.ToString());
    }

    public void Add(RoadNetworkExtract extract)
    {
        ArgumentNullException.ThrowIfNull(extract);

        Add(extract.Id, extract);
    }

    public Task<RoadNetworkExtract> Get(ExtractRequestId id, CancellationToken ct = default)
    {
        return FindAsync(id, ct);
    }
}
