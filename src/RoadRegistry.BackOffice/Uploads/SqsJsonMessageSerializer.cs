namespace RoadRegistry.BackOffice.Uploads;

using Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple;
using Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple.Extensions;
using Newtonsoft.Json;

public class SqsJsonMessageSerializer
{
    private readonly JsonSerializer _serializer;

    public SqsJsonMessageSerializer(SqsOptions sqsOptions)
    {
        _serializer = JsonSerializer.CreateDefault(sqsOptions.JsonSerializerSettings);
    }

    public object Deserialize(string message)
    {
        var sqsJsonMessage = _serializer.Deserialize<SqsJsonMessage>(message);
        return sqsJsonMessage?.Map(_serializer);
    }

    public string Serialize<T>(T message)
    {
        var sqsJsonMessage = SqsJsonMessage.Create(message, _serializer);
        return _serializer.Serialize(sqsJsonMessage);
    }
}
