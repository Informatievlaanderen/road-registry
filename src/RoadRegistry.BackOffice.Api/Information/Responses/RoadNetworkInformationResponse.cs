namespace RoadRegistry.BackOffice.Api.Information.Responses
{
    using System;
    using Schema;

    public class RoadNetworkInformationResponse
    {
        public bool CompletedImport { get; set; }
        public int OrganizationCount { get; set; }
        public int RoadNodeCount { get; set; }
        public int RoadSegmentCount { get; set; }
        public int RoadSegmentEuropeanRoadAttributeCount { get; set; }
        public int RoadSegmentNumberedRoadAttributeCount { get; set; }
        public int RoadSegmentNationalRoadAttributeCount { get; set; }
        public int RoadSegmentLaneAttributeCount { get; set; }
        public int RoadSegmentWidthAttributeCount { get; set; }
        public int RoadSegmentSurfaceAttributeCount { get; set; }
        public int GradeSeparatedJunctionCount { get; set; }

        public static RoadNetworkInformationResponse From(RoadNetworkInfo info)
        {
            if (info == null) throw new ArgumentNullException(nameof(info));
            return new RoadNetworkInformationResponse
            {
                CompletedImport = info.CompletedImport,
                OrganizationCount = info.OrganizationCount,
                RoadNodeCount = info.RoadNodeCount,
                RoadSegmentCount = info.RoadSegmentCount,
                RoadSegmentEuropeanRoadAttributeCount = info.RoadSegmentEuropeanRoadAttributeCount,
                RoadSegmentNationalRoadAttributeCount = info.RoadSegmentNationalRoadAttributeCount,
                RoadSegmentNumberedRoadAttributeCount = info.RoadSegmentNumberedRoadAttributeCount,
                RoadSegmentLaneAttributeCount = info.RoadSegmentLaneAttributeCount,
                RoadSegmentSurfaceAttributeCount = info.RoadSegmentSurfaceAttributeCount,
                RoadSegmentWidthAttributeCount = info.RoadSegmentWidthAttributeCount,
                GradeSeparatedJunctionCount = info.GradeSeparatedJunctionCount
            };
        }
    }
}
