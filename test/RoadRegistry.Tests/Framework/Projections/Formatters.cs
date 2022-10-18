namespace RoadRegistry.Tests.Framework.Projections;

using NetTopologySuite.IO.Converters;
using Newtonsoft.Json;

public static class Formatters
{
    static Formatters()
    {
        Converters = new JsonConverter[]
        {
            new GeometryConverter(),
            new CoordinateConverter()
        };
    }

    private static readonly JsonConverter[] Converters;

    public static string NamedJsonMessage<T>(T message)
    {
        return $"{message.GetType().Name} - {JsonConvert.SerializeObject(message, Formatting.Indented, Converters)}";
    }
}