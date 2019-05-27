namespace RoadRegistry.Api.Information.Responses
{
    using BackOffice.Schema;
    using Microsoft.AspNetCore.Mvc;
    using Swashbuckle.AspNetCore.Filters;

    public class InformationResponseExamples : IExamplesProvider
    {
        public object GetExamples()
        {
            return new JsonResult(new RoadNetworkInformationResponse
            {
                CompletedImport = false,
                GradeSeparatedJunctionCount = 5,
                OrganizationCount = 100,
                RoadNodeCount = 300,
                RoadSegmentCount = 500,
                RoadSegmentLaneAttributeCount = 200,
                RoadSegmentSurfaceAttributeCount = 500,
                RoadSegmentWidthAttributeCount = 300,
                RoadSegmentEuropeanRoadAttributeCount = 10,
                RoadSegmentNationalRoadAttributeCount = 50,
                RoadSegmentNumberedRoadAttributeCount = 60
            });
        }
    }
}
