namespace RoadRegistry.BackOffice.Api.Information
{
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Api;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Newtonsoft.Json.Converters;
    using Responses;
    using Schema;
    using Swashbuckle.AspNetCore.Filters;

    [ApiVersion("1.0")]
    [AdvertiseApiVersions("1.0")]
    [ApiRoute("information")]
    [ApiExplorerSettings(GroupName = "Information")]
    public class DownloadController : ControllerBase
    {
        /// <summary>
        /// Request information about the entire road registry for shape editing purposes.
        /// </summary>
        /// <param name="context">The database context to query data with.</param>
        /// <response code="200">Returned if the road registry information can be downloaded.</response>
        /// <response code="500">Returned if the road registry information can not be downloaded due to an unforeseen server error.</response>
        [HttpGet("")]
        [ProducesResponseType(typeof(JsonResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(InformationResponseExamples), jsonConverter: typeof(StringEnumConverter))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples), jsonConverter: typeof(StringEnumConverter))]
        public async Task<IActionResult> Get(
            [FromServices] BackOfficeContext context)
        {
            var info = await context.RoadNetworkInfo.SingleOrDefaultAsync(HttpContext.RequestAborted);
            return new JsonResult(RoadNetworkInformationResponse.From(info));
        }
    }
}
