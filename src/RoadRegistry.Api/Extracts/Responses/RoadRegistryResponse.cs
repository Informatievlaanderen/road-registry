namespace RoadRegistry.Api.Extracts.Responses
{
    using Swashbuckle.AspNetCore.Filters;

    public class RoadRegistryResponseExample : IExamplesProvider
    {
        public object GetExamples()
        {
            return new RoadRegistryExtractArchive("wegenregister").CreateResponse();
        }
    }
}
