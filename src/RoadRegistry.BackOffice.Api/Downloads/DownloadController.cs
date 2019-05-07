namespace RoadRegistry.Api.Downloads
{
    using System.IO.Compression;
    using System.Text;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Api;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using BackOffice.Schema;
    using Infrastructure;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Net.Http.Headers;
    using Newtonsoft.Json.Converters;
    using Responses;
    using Swashbuckle.AspNetCore.Filters;
    using ZipArchiveWriters;

    [ApiVersion("1.0")]
    [AdvertiseApiVersions("1.0")]
    [ApiRoute("download")]
    [ApiExplorerSettings(GroupName = "Downloads")]
    public class DownloadController : ControllerBase
    {
        /// <summary>
        /// Request an archive of the entire road registry for shape editing purposes.
        /// </summary>
        /// <param name="context">The database context to query data with.</param>
        /// <response code="200">Returned if the road registry can be downloaded.</response>
        /// <response code="500">Returned if the road registry can not be downloaded due to an unforeseen server error.</response>
        /// <response code="503">Returned if the road registry can not yet be downloaded (e.g. because the import has not yet completed).</response>
        [HttpGet("")]
        [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BasicApiProblem), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(DownloadResponseExamples), jsonConverter: typeof(StringEnumConverter))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples), jsonConverter: typeof(StringEnumConverter))]
        public async Task<IActionResult> Get(
            [FromServices] ShapeContext context)
        {
            var info = await context.RoadNetworkInfo.SingleOrDefaultAsync(HttpContext.RequestAborted);
            if (info == null || !info.CompletedImport)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable);
            }

            return new FileCallbackResult(
                new MediaTypeHeaderValue("application/zip"),
                async (stream, actionContext) =>
                {
                    var encoding = Encoding.ASCII; // TODO: Inject
                    var writer = new RoadNetworkForShapeEditingZipArchiveWriter(encoding);
                    using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, true, Encoding.UTF8))
                    {
                        await writer.WriteAsync(archive, context, HttpContext.RequestAborted);
                    }
                })
            {
                FileDownloadName = "wegenregister.zip"
            };
        }
    }
}
