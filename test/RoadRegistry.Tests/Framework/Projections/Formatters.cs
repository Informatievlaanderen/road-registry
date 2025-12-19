namespace RoadRegistry.Tests.Framework.Projections;

using Newtonsoft.Json;
using RoadRegistry.BackOffice;

public static class Formatters
{
    private static readonly JsonConverter[] Converters;

    static Formatters()
    {
        Converters = WellKnownJsonConverters.Converters.ToArray();
    }

    public static string NamedJsonMessage<T>(T message)
    {
        return message is not null ? $"{message.GetType().Name} - {JsonConvert.SerializeObject(message, Formatting.Indented, Converters)}" : null;
    }
}
