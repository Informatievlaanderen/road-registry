namespace RoadRegistry.Api.Oslo.Road
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Aiv.Vbr.Api;
    using Aiv.Vbr.Api.Exceptions;
    using Aiv.Vbr.CommandHandling;
    using Infrastructure;
    using Infrastructure.ETag;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Net.Http.Headers;
    using Newtonsoft.Json.Converters;
    using Projections.Oslo;
    using Responses;
    using Swashbuckle.AspNetCore.Examples;

    [ApiVersion("1.0")]
    [AdvertiseApiVersions("1.0")]
    [ApiRoute("wegen")]
    [ApiExplorerSettings(GroupName = "Wegen")]
    public class RoadController : ApiBusController
    {
        public RoadController(ICommandHandlerResolver bus) : base(bus) { }

        /// <summary>
        /// Vraag een weg op.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="id">Identificator van de weg.</param>
        /// <param name="ifMatch">Optionele If-Match header met de vereiste minimum positie van de event store.</param>
        /// <param name="cancellationToken"></param>
        /// <response code="200">Als de weg gevonden is.</response>
        /// <response code="404">Als de weg niet gevonden kan worden.</response>
        /// <response code="412">Als de gevraagde minimum positie van de event store nog niet bereikt is.</response>
        /// <response code="500">Als er een interne fout is opgetreden.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(RoadResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BasicApiProblem), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(BasicApiProblem), StatusCodes.Status412PreconditionFailed)]
        [ProducesResponseType(typeof(BasicApiProblem), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(RoadResponseExamples), jsonConverter: typeof(StringEnumConverter))]
        [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(RoadNotFoundResponseExamples), jsonConverter: typeof(StringEnumConverter))]
        [SwaggerResponseExample(StatusCodes.Status412PreconditionFailed, typeof(PreconditionFailedResponseExamples), jsonConverter: typeof(StringEnumConverter))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples), jsonConverter: typeof(StringEnumConverter))]
        public async Task<IActionResult> Get(
            [FromServices] OsloContext context,
            [FromRoute] Guid id,
            [FromHeader(Name = HeaderNames.IfMatch)] string ifMatch,
            CancellationToken cancellationToken)
        {
            var projectionPosition = await context.GetProjectionPositionAsync(cancellationToken);

            if (IsInvalidETag(ifMatch, projectionPosition))
                throw new ApiException("De gevraagde minimum positie van de event store is nog niet bereikt.", StatusCodes.Status412PreconditionFailed);

            var road =
                await context
                    .Roads
                    .AsNoTracking()
                    .SingleOrDefaultAsync(item => item.RoadId == id, cancellationToken);

            if (road == null)
                throw new ApiException("Onbestaande weg.", StatusCodes.Status404NotFound);

            return new OkWithETagResult(
                new RoadResponse(road),
                projectionPosition.ToString());
        }

        /// <summary>
        /// Vraag een lijst met actieve wegen op.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="hostingEnvironment"></param>
        /// <param name="ifMatch">Optionele If-Match header met de vereiste minimum positie van de event store.</param>
        /// <param name="cancellationToken"></param>
        /// <param name="taal">Gewenste taal van de respons.</param>
        /// <response code="200">Als de opvraging van een lijst met wegen gelukt is.</response>
        /// <response code="412">Als de gevraagde minimum positie van de event store nog niet bereikt is.</response>
        /// <response code="500">Als er een interne fout is opgetreden.</response>
        [HttpGet]
        [ProducesResponseType(typeof(List<RoadListResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(BasicApiProblem), StatusCodes.Status412PreconditionFailed)]
        [ProducesResponseType(typeof(BasicApiProblem), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(RoadListResponseExamples), jsonConverter: typeof(StringEnumConverter))]
        [SwaggerResponseExample(StatusCodes.Status412PreconditionFailed, typeof(PreconditionFailedResponseExamples), jsonConverter: typeof(StringEnumConverter))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples), jsonConverter: typeof(StringEnumConverter))]
        public async Task<IActionResult> List(
            CancellationToken cancellationToken,
            [FromServices] OsloContext context,
            [FromServices] IHostingEnvironment hostingEnvironment,
            [FromHeader(Name = HeaderNames.IfMatch)] string ifMatch,
            Taal? taal)
        {
            var projectionPosition = await context.GetProjectionPositionAsync(cancellationToken);

            if (IsInvalidETag(ifMatch, projectionPosition))
                throw new ApiException("De gevraagde minimum positie van de event store is nog niet bereikt.", StatusCodes.Status412PreconditionFailed);

            return new OkWithETagResult(
                await context
                    .RoadList
                    .AsNoTracking()
                    .Select(road => new RoadListResponse(
                        road.DefaultName,
                        road.NameDutch,
                        road.NameFrench,
                        road.NameGerman,
                        road.NameEnglish,
                        taal))
                    .ToListAsync(cancellationToken),
                projectionPosition.ToString());
        }

        private static bool IsInvalidETag(string ifMatch, long projectionPosition)
        {
            if (string.IsNullOrWhiteSpace(ifMatch))
                return false;

            var etag = Convert.ToInt64(ifMatch);

            return etag > projectionPosition;
        }
    }
}
