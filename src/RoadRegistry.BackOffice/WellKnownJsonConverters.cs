namespace RoadRegistry.BackOffice
{
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using NetTopologySuite.Geometries;
    using NetTopologySuite.IO.Converters;
    using Newtonsoft.Json;

    public static class WellKnownJsonConverters
    {
        public static readonly IReadOnlyCollection<JsonConverter> Converters;

        static WellKnownJsonConverters()
        {
            var factory = new GeometryFactory(new PrecisionModel(), SpatialReferenceSystemIdentifier.BelgeLambert1972.ToInt32());

            Converters =
            [
                new GeometryConverter(),
                new FeatureCollectionConverter(),
                new FeatureConverter(),
                new AttributesTableConverter(),
                new GeometryConverter(factory, 2),
                new EnvelopeConverter(),

                new GeoJSON.Net.Converters.GeoJsonConverter()
            ];
        }
    }
}
