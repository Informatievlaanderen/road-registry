namespace RoadRegistry.Tests.Framework.Projections;

using NetTopologySuite.Geometries;
using NetTopologySuite.IO.Converters;
using Newtonsoft.Json;

public static class Formatters
{
    private static readonly GeometryFactory Factory = new(new PrecisionModel(), 31370);
    private static readonly JsonConverter[] Converters;

    static Formatters()
    {
        Converters =
        [
            new GeometryConverter(),
            new FeatureCollectionConverter(),
            new FeatureConverter(),
            new AttributesTableConverter(),
            new GeometryConverter(Factory, 2),
            new EnvelopeConverter()
        ];
    }

    public static string NamedJsonMessage<T>(T message)
    {
        return $"{message.GetType().Name} - {JsonConvert.SerializeObject(message, Formatting.Indented, Converters)}";
    }
}
