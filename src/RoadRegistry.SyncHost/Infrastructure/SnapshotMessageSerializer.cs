namespace RoadRegistry.SyncHost.Infrastructure;

using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka;
using Newtonsoft.Json;
using JsonSerializerExtensions = Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple.Extensions.JsonSerializerExtensions;

public class SnapshotMessageSerializer<TMessage> : IMessageSerializer<string>
{
    private readonly JsonSerializer _serializer;

    public SnapshotMessageSerializer(JsonSerializerSettings jsonSerializerSettings)
    {
        _serializer = JsonSerializer.CreateDefault(jsonSerializerSettings);
    }

    public object Deserialize(string value, MessageContext context)
    {
        var deserializedValue = _serializer.Deserialize<TMessage>(value);
        return new SnapshotMessage
        {
            Value = deserializedValue,
            Key = context.Key,
            Offset = context.Offset
        };
    }

    public string Serialize(object message)
    {
        return JsonSerializerExtensions.Serialize(_serializer, message);
    }
}

public sealed record SnapshotMessage
{
    public object Value { get; init; }
    public string Key { get; init; }
    public long Offset { get; init; }
}
