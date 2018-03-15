namespace Wegenregister.Api.Oslo.ProjectionState
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Aiv.Vbr.Api;
    using Aiv.Vbr.Api.Exceptions;
    using Infrastructure;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Newtonsoft.Json.Converters;
    using Projections.Oslo;
    using Responses;
    using Swashbuckle.AspNetCore.Examples;

    [ApiVersion("1.0")]
    [AdvertiseApiVersions("1.0")]
    [ApiRoute("projectionstates")]
    [ApiExplorerSettings(GroupName = "Projectie status")]
    public class ProjectionStateController : ApiController
    {
        /// <summary>
        /// Vraag de status op van de projecties.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="cancellationToken"></param>
        /// <response code="200">Als de opvraging van de status van de projecties gelukt is.</response>
        /// <response code="500">Als er een interne fout is opgetreden.</response>
        [HttpGet]
        //[Authorize]
        [ProducesResponseType(typeof(List<ProjectionStateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BasicApiProblem), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(ProjectionStateResponseExamples), jsonConverter: typeof(StringEnumConverter))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples), jsonConverter: typeof(StringEnumConverter))]
        public async Task<IActionResult> List(
            [FromServices] OsloContext context,
            CancellationToken cancellationToken)
        {
            return new OkObjectResult(await context.ProjectionStates.AsNoTracking().Select(x => new ProjectionStateResponse(x)).ToListAsync(cancellationToken));
        }
    }
}
