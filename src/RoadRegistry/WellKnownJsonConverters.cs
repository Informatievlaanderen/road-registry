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
                //common
                new GeoJSON.Net.Converters.GeoJsonConverter(),
                new OrganizationIdConverter(),
                new EuropeanRoadNumberConverter(),
                new NationalRoadNumberConverter(),
                new RoadNodeIdConverter(),
                new RoadSegmentIdConverter(),
                new RoadSegmentPositionConverter(),
                new RoadSegmentWidthConverter(),
                new GradeSeparatedJunctionIdConverter(),
                new StreetNameLocalIdConverter(),
                new DownloadIdConverter(),
                new UploadIdConverter(),
                new ExtractRequestIdConverter(),

                //v1
                new NumberedRoadNumberConverter(),
                new RoadNodeTypeConverter(),
                new RoadSegmentAccessRestrictionConverter(),
                new RoadSegmentCategoryConverter(),
                new RoadSegmentGeometryDrawMethodConverter(),
                new RoadSegmentLaneCountConverter(),
                new RoadSegmentLaneDirectionConverter(),
                new RoadSegmentMorphologyConverter(),
                new RoadSegmentStatusConverter(),
                new RoadSegmentSurfaceTypeConverter(),
                new RoadSegmentNumberedRoadDirectionConverter(),
                new RoadSegmentNumberedRoadOrdinalConverter(),
                new GradeSeparatedJunctionTypeConverter(),

                //v2
                new RoadNodeTypeV2Converter(),
                new RoadSegmentAccessRestrictionV2Converter(),
                new RoadSegmentCategoryV2Converter(),
                new RoadSegmentGeometryDrawMethodV2Converter(),
                new RoadSegmentMorphologyV2Converter(),
                new RoadSegmentStatusV2Converter(),
                new RoadSegmentSurfaceTypeV2Converter(),
                new RoadSegmentDynamicAttributeValuesJsonConverter(),
                new GradeSeparatedJunctionTypeV2Converter(),
                new RoadNetworkIdConverter()
            ];
        }
    }
}
