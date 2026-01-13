namespace RoadRegistry.BackOffice.Uploads;

using System.Collections.Generic;
using System.Reflection;
using Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple;
using Be.Vlaanderen.Basisregisters.MessageHandling.AwsSqs.Simple.Extensions;
using Newtonsoft.Json;

public class SqsJsonMessageSerializer
{
    private readonly JsonSerializer _serializer;
    private readonly IReadOnlyCollection<Assembly> _messagesAssemblies;

    public SqsJsonMessageSerializer(SqsOptions sqsOptions, IReadOnlyCollection<Assembly> messagesAssemblies)
    {
        _serializer = JsonSerializer.CreateDefault(sqsOptions.JsonSerializerSettings);
        _messagesAssemblies = messagesAssemblies;
    }

    public object? Deserialize(string message)
    {
        var sqsJsonMessage = _serializer.Deserialize<SqsJsonMessage>(message);
        return sqsJsonMessage?.Map(_serializer, _messagesAssemblies);
    }

    public string Serialize<T>(T message)
    {
        var sqsJsonMessage = SqsJsonMessage.Create(message, _serializer);
        return _serializer.Serialize(sqsJsonMessage);
    }
}
