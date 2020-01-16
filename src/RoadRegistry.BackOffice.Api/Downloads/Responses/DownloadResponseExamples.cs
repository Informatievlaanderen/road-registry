namespace RoadRegistry.Api.Downloads.Responses
{
    using System.Threading;
    using System.Threading.Tasks;
    using Infrastructure;
    using Microsoft.Net.Http.Headers;
    using Swashbuckle.AspNetCore.Filters;

    public class DownloadResponseExamples : IExamplesProvider
    {
        public object GetExamples()
        {
            return new FileCallbackResult(
                new MediaTypeHeaderValue("application/octet-stream"),
                (stream, actionContext) => Task.CompletedTask
            )
            {
                FileDownloadName = "Wegenregister.zip"
            };
        }
    }
}
