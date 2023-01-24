namespace RoadRegistry.BackOffice.Api.RoadSegments;

using System.Threading;
using System.Threading.Tasks;
using Abstractions.RoadSegments;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Be.Vlaanderen.Basisregisters.Sqs.Exceptions;
using FeatureToggles;
using Handlers.Sqs.RoadSegments;
using Microsoft.AspNetCore.Mvc;

public partial class RoadSegmentsController
{
    [HttpPost("corrigeer/versies")]
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
            var result = await _mediator.Send(
                new CorrectRoadSegmentVersionsSqsRequest
                {
                    Request = new CorrectRoadSegmentVersionsRequest(),
                    Metadata = GetMetadata(),
                    ProvenanceData = new ProvenanceData(CreateFakeProvenance())
                }, cancellationToken);

            return Accepted(result);
        }
        catch (IdempotencyException)
        {
            return Accepted();
        }
    }
}
