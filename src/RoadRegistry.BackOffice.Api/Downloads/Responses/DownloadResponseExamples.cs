namespace RoadRegistry.Api.Downloads.Responses
{
    using System.Threading;
    using Swashbuckle.AspNetCore.Filters;

    public class DownloadResponseExamples : IExamplesProvider
    {
        public object GetExamples()
        {
            return new RoadRegistryExtractArchive("wegenregister").CreateCallbackFileStreamResult(CancellationToken.None);
        }
    }
}
