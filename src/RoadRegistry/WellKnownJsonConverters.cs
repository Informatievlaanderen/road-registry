namespace RoadRegistry
{
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Converters;
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
                new GeoJSON.Net.Converters.GeoJsonConverter(),
                new OrganizationIdConverter(),
                new EuropeanRoadNumberConverter(),
                new NationalRoadNumberConverter(),
                new NumberedRoadNumberConverter(),
                new RoadNodeIdConverter(),
                new RoadNodeTypeConverter(),
                new RoadSegmentIdConverter(),
                new RoadSegmentAccessRestrictionConverter(),
                new RoadSegmentCategoryConverter(),
                new RoadSegmentGeometryDrawMethodConverter(),
                new RoadSegmentLaneCountConverter(),
                new RoadSegmentLaneDirectionConverter(),
                new RoadSegmentMorphologyConverter(),
                new RoadSegmentPositionConverter(),
                new RoadSegmentStatusConverter(),
                new RoadSegmentSurfaceTypeConverter(),
                new RoadSegmentWidthConverter(),
                new RoadSegmentNumberedRoadDirectionConverter(),
                new RoadSegmentNumberedRoadOrdinalConverter(),
                new RoadSegmentDynamicAttributeValuesJsonConverter(),
                new GradeSeparatedJunctionIdConverter(),
                new GradeSeparatedJunctionTypeConverter(),
                new StreetNameLocalIdConverter(),
                new DownloadIdConverter()
            ];
        }
    }
}
