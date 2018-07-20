namespace RoadRegistry.Api.Extracts.Responses
{
    using Swashbuckle.AspNetCore.Examples;

    public class RoadRegistryResponse
    {
        public string Name => "Dummy for registry name";
    }

    public class RoadRegistryResponseExample : IExamplesProvider
    {
        public object GetExamples()
        {
            return new RoadRegistryResponse();
        }
    }
}
