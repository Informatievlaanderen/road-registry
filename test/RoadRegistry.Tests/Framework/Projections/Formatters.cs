namespace RoadRegistry.Tests.Framework.Projections;

using NetTopologySuite.Geometries;
using NetTopologySuite.IO.Converters;
using Newtonsoft.Json;

public static class Formatters
{
    private static readonly GeometryFactory _factory = new(new PrecisionModel(), 31370);
    private static readonly JsonConverter[] Converters;

    static Formatters()
    {
        Converters = new JsonConverter[]
        {
            new GeometryConverter(),
            new CoordinateConverter(),
            new FeatureCollectionConverter(),
            new FeatureConverter(),
            new AttributesTableConverter(),
            new GeometryConverter(_factory, 2),
            new GeometryArrayConverter(_factory, 2),
            new CoordinateConverter(_factory.PrecisionModel, 2),
            new EnvelopeConverter()
        };
    }

    public static string NamedJsonMessage<T>(T message)
    {
        return $"{message.GetType().Name} - {JsonConvert.SerializeObject(message, Formatting.Indented, Converters)}";
    }
}