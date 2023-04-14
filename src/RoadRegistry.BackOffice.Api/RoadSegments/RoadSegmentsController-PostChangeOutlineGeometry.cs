namespace RoadRegistry.BackOffice.Api.RoadSegments;

using Abstractions.RoadSegmentsOutline;
using Be.Vlaanderen.Basisregisters.AcmIdm;
using Be.Vlaanderen.Basisregisters.Api.Exceptions;
using Be.Vlaanderen.Basisregisters.Sqs.Exceptions;
using FluentValidation;
using Handlers.Sqs.RoadSegments;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Parameters;
using RoadRegistry.BackOffice.Abstractions.Extensions;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;
using System.Threading;
using System.Threading.Tasks;

public partial class RoadSegmentsController
{
    /// <summary>
    ///     Wijzig de geometrie van een ingeschetst wegsegment.
    /// </summary>
    /// <param name="idValidator"></param>
    /// <param name="validator"></param>
    /// <param name="parameters"></param>
    /// <param name="id"></param>
    /// <param name="cancellationToken"></param>
    /// <response code="202">Als het wegsegment gevonden is.</response>
    /// <response code="400">Als uw verzoek foutieve data bevat.</response>
    /// <response code="404">Als het wegsegment niet gevonden kan worden.</response>
    /// <response code="500">Als er een interne fout is opgetreden.</response>
    [HttpPost("{id}/acties/wijzigen/schetsgeometrie")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = PolicyNames.GeschetsteWeg.Beheerder)]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [SwaggerResponseHeader(StatusCodes.Status202Accepted, "ETag", "string", "De ETag van de response.")]
    [SwaggerResponseHeader(StatusCodes.Status202Accepted, "x-correlation-id", "string", "Correlatie identificator van de response.")]
    [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(BadRequestResponseExamples))]
    [SwaggerResponseExample(StatusCodes.Status404NotFound, typeof(RoadSegmentNotFoundResponseExamples))]
    [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
    [SwaggerRequestExample(typeof(PostChangeOutlineGeometryParameters), typeof(PostChangeOutlineGeometryParametersExamples))]
    [SwaggerOperation(Description = "Wijzig de geometrie van een wegsegment met geometriemethode <ingeschetst>.")]
    public async Task<IActionResult> PostChangeOutlineGeometry(
        [FromServices] RoadSegmentOutlinedIdValidator idValidator,
        [FromServices] PostChangeOutlineGeometryParametersValidator validator,
        [FromBody] PostChangeOutlineGeometryParameters parameters,
        [FromRoute] int id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await idValidator.ValidateRoadSegmentIdAndThrowAsync(id, cancellationToken);
            await validator.ValidateAndThrowAsync(parameters, cancellationToken);

            var sqsRequest = new ChangeRoadSegmentOutlineGeometrySqsRequest
            {
                Request = new ChangeRoadSegmentOutlineGeometryRequest(
                    new RoadSegmentId(id),
                    GeometryTranslator.Translate(GeometryTranslator.ParseGmlLineString(parameters.MiddellijnGeometrie))
                )
            };
            var result = await _mediator.Send(Enrich(sqsRequest), cancellationToken);

            return Accepted(result);
        }
        catch (IdempotencyException)
        {
            return Accepted();
        }
    }
}
