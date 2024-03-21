using System.Collections.Generic;
using System.Linq;

namespace RoadRegistry.BackOffice
{
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using NetTopologySuite.Geometries;
    using NetTopologySuite.IO.Converters;
    using Newtonsoft.Json;

    public static class WellKnownJsonConverters
    {
        private static JsonConverter[] _converters;

        static WellKnownJsonConverters()
        {
            var _factory = new GeometryFactory(new PrecisionModel(), SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32());

            _converters = new JsonConverter[]
            {
                new GeometryConverter(),
                // new CoordinateConverter(), removed https://github.com/NetTopologySuite/NetTopologySuite.IO.GeoJSON/commit/52f33001e3f2536a3811abed6400dcfa2954dd7d
                new FeatureCollectionConverter(),
                new FeatureConverter(),
                new AttributesTableConverter(),
                new GeometryConverter(_factory, 2),
                // new GeometryArrayConverter(_factory, 2), todo-rik
                // new CoordinateConverter(_factory.PrecisionModel, 2),
                new EnvelopeConverter(),

                new GeoJSON.Net.Converters.GeoJsonConverter()
            };
        }

        public static IReadOnlyCollection<JsonConverter> Converters => _converters.ToArray();
    }
}
