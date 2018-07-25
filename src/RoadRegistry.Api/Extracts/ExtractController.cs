namespace RoadRegistry.Api.Extracts
{
    using System.Threading;
    using System.Threading.Tasks;
    using Aiv.Vbr.Api;
    using Aiv.Vbr.Api.Exceptions;
    using Aiv.Vbr.CommandHandling;
    using Infrastructure;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json.Converters;
    using Projections;
    using Responses;
    using Swashbuckle.AspNetCore.Examples;

    [ApiVersion("1.0")]
    [AdvertiseApiVersions("1.0")]
    [ApiRoute("extracten")]
    [ApiExplorerSettings(GroupName = "Extracten")]
    public class ExtractController : ApiBusController
    {
        public ExtractController(ICommandHandlerResolver bus)
            : base(bus)
        { }

        /// <summary>
        /// Vraag een dump van het volledige register op.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="cancellationToken"></param>
        /// <response code="200">Als wegenregister kan gedownload worden.</response>
        /// <response code="500">Als er een interne fout is opgetreden.</response>
        [HttpGet("")]
        [ProducesResponseType(typeof(RoadRegistryResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BasicApiProblem), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(RoadRegistryResponseExample), jsonConverter: typeof(StringEnumConverter))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples), jsonConverter: typeof(StringEnumConverter))]
        public async Task<IActionResult> Get(
            [FromServices] ShapeContext context,
            CancellationToken cancellationToken)

        {

//            var roadNodes =
//                await context
//                    .RoadNodes
//                    .AsNoTracking()
//                    .ToListAsync(cancellationToken);
//
//            if (roadNodes == null)
//                throw new ApiException("Er ging iets fout bij ophalen van de wegknooppunten.", StatusCodes.Status500InternalServerError);

            var projectionPosition = await context.GetProjectionPositionAsync(cancellationToken);
            return new OkWithETagResult(
                new RoadRegistryResponse(),
                projectionPosition.ToString());
        }
    }
}
