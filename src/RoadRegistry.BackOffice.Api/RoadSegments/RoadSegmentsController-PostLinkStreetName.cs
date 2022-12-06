namespace RoadRegistry.BackOffice.Api.RoadSegments;

using System.Threading;
using System.Threading.Tasks;
using Abstractions.Exceptions;
using Abstractions.RoadSegments;
using Abstractions.Validation;
using Be.Vlaanderen.Basisregisters.Api.Exceptions;
using FeatureToggles;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Filters;

public partial class RoadSegmentsController
{
    [HttpPost("{id}/acties/straatnaamkoppelen")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(BadRequestResponseExamples))]
    [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
    public async Task<IActionResult> PostLinkStreetName(
        [FromServices] UseRoadSegmentLinkStreetNameFeatureToggle featureToggle,
        [FromBody] PostLinkStreetNameParameters parameters,
        [FromRoute] int id,
        CancellationToken cancellationToken)
    {
        if (!featureToggle.FeatureEnabled)
        {
            return NotFound();
        }

        try
        {
            var request = new LinkStreetNameToRoadSegmentRequest(id, parameters?.LinkerstraatnaamId, parameters?.RechterstraatnaamId);
            var response = await _mediator.Send(request, cancellationToken);

            return Accepted(response);
        }
        catch (RoadSegmentNotFoundException)
        {
            throw new ApiException(ValidationErrors.RoadSegment.NotFound.Message, StatusCodes.Status404NotFound);
        }
    }
}

public sealed record PostLinkStreetNameParameters(string? LinkerstraatnaamId, string? RechterstraatnaamId);
