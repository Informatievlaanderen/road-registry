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
                new CoordinateConverter(),
                new FeatureCollectionConverter(),
                new FeatureConverter(),
                new AttributesTableConverter(),
                new GeometryConverter(_factory, 2),
                new GeometryArrayConverter(_factory, 2),
                new CoordinateConverter(_factory.PrecisionModel, 2),
                new EnvelopeConverter(),

                new GeoJSON.Net.Converters.GeoJsonConverter()
            };
        }

        public static IReadOnlyCollection<JsonConverter> Converters => _converters.ToArray();
    }
}
