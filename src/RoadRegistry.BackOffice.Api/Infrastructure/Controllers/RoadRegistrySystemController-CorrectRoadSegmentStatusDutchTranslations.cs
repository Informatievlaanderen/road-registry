namespace RoadRegistry.BackOffice.Api.Infrastructure.Controllers;

using Microsoft.AspNetCore.Mvc;
using RoadRegistry.BackOffice.Abstractions.RoadSegments;
using RoadRegistry.BackOffice.Handlers.Sqs.RoadSegments;
using Swashbuckle.AspNetCore.Annotations;
using System.Threading;
using System.Threading.Tasks;

public partial class RoadRegistrySystemController
{
    private const string CorrectRoadSegmentStatusDutchTranslationsRoute = "correct/roadsegmentstatus/dutch-translations";

    /// <summary>
    ///     Corrects the road segment status dutch translations.
    /// </summary>
    /// <param name="cancellationToken">
    ///     The cancellation token that can be used by other objects or threads to receive notice
    ///     of cancellation.
    /// </param>
    /// <returns>IActionResult.</returns>
    [HttpPost(CorrectRoadSegmentStatusDutchTranslationsRoute, Name = nameof(CorrectRoadSegmentStatusDutchTranslations))]
    [SwaggerOperation(OperationId = nameof(CorrectRoadSegmentStatusDutchTranslations), Description = "")]
    public async Task<IActionResult> CorrectRoadSegmentStatusDutchTranslations(CancellationToken cancellationToken)
    {
        // For now I will leave this hardcoded
        await Mediator.Send(new CorrectRoadSegmentStatusDutchTranslationsSqsRequest
        {
            Request = new CorrectRoadSegmentStatusDutchTranslationsRequest
            {
                Identifier = RoadSegmentStatus.PermitGranted.Translation.Identifier,
                Name = RoadSegmentStatus.PermitGranted.Translation.Name
            }
        }, cancellationToken);
        return Accepted();
    }
}
