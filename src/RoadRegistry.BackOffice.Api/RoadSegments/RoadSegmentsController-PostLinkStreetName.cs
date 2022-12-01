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

public partial class RoadSegmentsController
{
    [HttpPost("{id}/acties/straatnaamkoppelen")]
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

            return Ok(response);
        }
        catch (RoadSegmentNotFoundException)
        {
            throw new ApiException(ValidationErrors.RoadSegment.NotFound.Message, StatusCodes.Status404NotFound);
        }
    }

    public sealed record PostLinkStreetNameParameters(string? LinkerstraatnaamId, string? RechterstraatnaamId);
}
