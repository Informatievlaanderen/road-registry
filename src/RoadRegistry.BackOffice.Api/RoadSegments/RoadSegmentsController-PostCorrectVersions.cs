namespace RoadRegistry.BackOffice.Api.RoadSegments;

using Be.Vlaanderen.Basisregisters.Sqs.Exceptions;
using FeatureToggles;
using Handlers.Sqs.RoadSegments;
using Infrastructure.Controllers.Attributes;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;

public partial class RoadSegmentsController
{
    [HttpPost("corrigeer/versies")]
    [ApiKeyAuth(WellKnownAuthRoles.Road)]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> PostCorrectVersions(
        [FromServices] UseRoadSegmentCorrectVersionsFeatureToggle featureToggle,
        CancellationToken cancellationToken = default)
    {
        if (!featureToggle.FeatureEnabled)
        {
            return NotFound();
        }

        try
        {
            var result = await _mediator.Send(Enrich(
                    new CorrectRoadSegmentVersionsSqsRequest()
                ), cancellationToken);

            return Accepted(result);
        }
        catch (IdempotencyException)
        {
            return Accepted();
        }
    }
}