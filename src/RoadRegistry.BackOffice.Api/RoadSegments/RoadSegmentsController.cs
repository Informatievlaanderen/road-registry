namespace RoadRegistry.BackOffice.Api.RoadSegments;

using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Abstractions.Exceptions;
using Abstractions.RoadSegments;
using Abstractions.Validation;
using Be.Vlaanderen.Basisregisters.Api;
using Be.Vlaanderen.Basisregisters.Api.Exceptions;
using FeatureToggles;
using Infrastructure;
using Infrastructure.Controllers.Attributes;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

[ApiVersion(Version.Current)]
[AdvertiseApiVersions(Version.CurrentAdvertised)]
[ApiRoute("wegsegmenten")]
[ApiExplorerSettings(GroupName = "Wegsegmenten")]
[ApiKeyAuth(WellKnownAuthRoles.Road)]
public class RoadSegmentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public RoadSegmentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("{id}/acties/straatnaamkoppelen")]
    public async Task<IActionResult> PostLinkStreetNameToRoadSegment(
        [FromServices] UseLinkRoadSegmentToStreetNameFeatureToggle featureToggle,
        [FromBody] PostLinkStreetNameToRoadSegmentParameters parameters,
        CancellationToken cancellationToken)
    {
        if (!featureToggle.FeatureEnabled)
        {
            return NotFound();
        }

        try
        {
            var request = new LinkRoadSegmentToStreetNameRequest(parameters?.WegsegmentId ?? 0, parameters?.LinkerstraatnaamId, parameters?.RechterstraatnaamId);
            var response = await _mediator.Send(request, cancellationToken);

            return Ok(response);
        }
        catch (RoadSegmentNotFoundException)
        {
            throw new ApiException(ValidationErrors.RoadSegment.NotFound.Message, StatusCodes.Status404NotFound);
        }
    }
}

public sealed record PostLinkStreetNameToRoadSegmentParameters(int WegsegmentId, string? LinkerstraatnaamId, string? RechterstraatnaamId);
