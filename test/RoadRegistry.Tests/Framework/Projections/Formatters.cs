namespace RoadRegistry.Framework.Projections
{
    using NetTopologySuite.IO.Converters;
    using Newtonsoft.Json;

    public static class Formatters
    {
        private static readonly JsonConverter[] Converters;

        static Formatters()
        {
            Converters = new JsonConverter[]
            {
                new GeometryConverter(),
                new CoordinateConverter(),
            };
        }

        public static string NamedJsonMessage<T>(T message)
        {
            return $"{message.GetType().Name} - {JsonConvert.SerializeObject(message, Formatting.Indented, Converters)}";
        }
    }
}
