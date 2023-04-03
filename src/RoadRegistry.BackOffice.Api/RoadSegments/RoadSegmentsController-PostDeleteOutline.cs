namespace RoadRegistry.BackOffice.Api.RoadSegments;

using Abstractions.RoadSegmentsOutline;
using Be.Vlaanderen.Basisregisters.AcmIdm;
using Be.Vlaanderen.Basisregisters.Api.Exceptions;
using Be.Vlaanderen.Basisregisters.Sqs.Exceptions;
using FeatureToggles;
using Handlers.Sqs.RoadSegments;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Swashbuckle.AspNetCore.Filters;
using System.Threading;
using System.Threading.Tasks;

public partial class RoadSegmentsController
{
    /// <summary>
    ///     Verwijder een ingeschetst wegsegment
    /// </summary>
    /// <param name="featureToggle"></param>
    /// <param name="id">Identificator van het ingeschetst wegsegment.</param>
    /// <param name="cancellationToken"></param>
    /// <response code="202">Als het ingeschetst wegsegment gevonden is.</response>
    /// <response code="400">Als uw verzoek foutieve data bevat.</response>
    /// <response code="404">Als het ingeschetst wegsegment niet gevonden kan worden.</response>
    /// <response code="500">Als er een interne fout is opgetreden.</response>
    [HttpPost("{id}/acties/verwijderen/schets")]
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
    [SwaggerOperation(Description = "Verwijder een ingeschetst wegsegment.")]
    public async Task<IActionResult> PostDeleteOutline(
        [FromServices] UseRoadSegmentOutlineDeleteFeatureToggle featureToggle,
        [FromRoute] int id,
        CancellationToken cancellationToken)
    {
        if (!featureToggle.FeatureEnabled)
        {
            return NotFound();
        }

        try
        {
            var result = await _mediator.Send(Enrich(
                new DeleteRoadSegmentOutlineSqsRequest
                {
                    Request = new DeleteRoadSegmentOutlineRequest(id)
                }
            ), cancellationToken);

            return Accepted(result);
        }
        catch (IdempotencyException)
        {
            return Accepted();
        }
    }
}
