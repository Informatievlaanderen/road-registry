namespace RoadRegistry
{
    using System.Collections.Generic;
    using Converters;
    using Newtonsoft.Json;

    public static class WellKnownJsonConverters
    {
        public static readonly IReadOnlyCollection<JsonConverter> Converters;

        static WellKnownJsonConverters()
        {
            Converters =
            [
                new GeoJSON.Net.Converters.GeoJsonConverter(),
                new OrganizationIdConverter(),
                new EuropeanRoadNumberConverter(),
                new NationalRoadNumberConverter(),
                new NumberedRoadNumberConverter(),
                new RoadNodeIdConverter(),
                new RoadNodeTypeConverter(),
                new RoadNodeTypeV2Converter(),
                new RoadSegmentIdConverter(),
                new RoadSegmentAccessRestrictionConverter(),
                new RoadSegmentAccessRestrictionV2Converter(),
                new RoadSegmentCategoryConverter(),
                new RoadSegmentCategoryV2Converter(),
                new RoadSegmentGeometryDrawMethodConverter(),
                new RoadSegmentLaneCountConverter(),
                new RoadSegmentLaneDirectionConverter(),
                new RoadSegmentMorphologyConverter(),
                new RoadSegmentMorphologyV2Converter(),
                new RoadSegmentPositionConverter(),
                new RoadSegmentStatusConverter(),
                new RoadSegmentStatusV2Converter(),
                new RoadSegmentSurfaceTypeConverter(),
                new RoadSegmentSurfaceTypeV2Converter(),
                new RoadSegmentWidthConverter(),
                new RoadSegmentNumberedRoadDirectionConverter(),
                new RoadSegmentNumberedRoadOrdinalConverter(),
                new RoadSegmentDynamicAttributeValuesJsonConverter(),
                new GradeSeparatedJunctionIdConverter(),
                new GradeSeparatedJunctionTypeConverter(),
                new StreetNameLocalIdConverter(),
                new DownloadIdConverter(),
                new UploadIdConverter(),
                new ExtractRequestIdConverter(),
                new RoadNetworkIdConverter()
            ];
        }
    }
}
