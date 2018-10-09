namespace RoadRegistry.Api.Extracts.Responses
{
    using System.Threading;
    using Swashbuckle.AspNetCore.Filters;

    public class RoadRegistryResponseExample : IExamplesProvider
    {
        public object GetExamples()
        {
            return new RoadRegistryExtractArchive("wegenregister").CreateCallbackFileStreamResult(CancellationToken.None);
        }
    }
}
