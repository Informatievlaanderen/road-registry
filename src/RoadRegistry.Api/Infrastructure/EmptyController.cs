namespace RoadRegistry.Api.Infrastructure
{
    using System.IO;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Net.Http.Headers;

    [ApiVersionNeutral]
    [Route("")]
    public class EmptyController : ApiController
    {
        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> Get(
            [FromServices] IHostingEnvironment hostingEnvironment,
            CancellationToken cancellationToken)
        {
            if (Request.Headers[HeaderNames.Accept].ToString().Contains("text/html"))
                return new ContentResult
                {
                    Content = await System.IO.File.ReadAllTextAsync(Path.Combine(hostingEnvironment.WebRootPath, "api-documentation.html"), cancellationToken),
                    ContentType = "text/html",
                    StatusCode = StatusCodes.Status200OK
                };

            return new OkObjectResult($"Welcome to the RoadRegistry Api v{Assembly.GetEntryAssembly().GetName().Version}.");
        }
    }
}
