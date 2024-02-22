namespace RoadRegistry.SyncHost.Infrastructure;

using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka;
using Newtonsoft.Json;

public class GrarContractsMessageSerializer : JsonMessageSerializer
{
    public GrarContractsMessageSerializer(JsonSerializerSettings jsonSerializerSettings)
        : base(jsonSerializerSettings)
    {
        // force load of assembly to ensure .Map() works correctly
        var _ = typeof(Be.Vlaanderen.Basisregisters.GrAr.Contracts.IQueueMessage);
    }
}
