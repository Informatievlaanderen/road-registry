namespace RoadRegistry.BackOffice.Api.RoadSegments;

using System.Threading;
using System.Threading.Tasks;
using Abstractions.RoadSegments;
using Be.Vlaanderen.Basisregisters.Api;
using FeatureToggles;
using Infrastructure;
using Infrastructure.Controllers.Attributes;
using MediatR;
using Microsoft.AspNetCore.Mvc;

[ApiVersion(Version.Current)]
[AdvertiseApiVersions(Version.CurrentAdvertised)]
[ApiRoute("roadsegments")]
[ApiExplorerSettings(GroupName = "RoadSegments")]
[ApiKeyAuth(WellKnownAuthRoles.Road)]
public class RoadSegmentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public RoadSegmentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("{id}/actions/linkstreetname")]
    public async Task<IActionResult> PostLinkStreetNameToRoadSegment(
        [FromServices] UseLinkRoadSegmentToStreetNameFeatureToggle featureToggle,
        [FromBody] LinkStreetNameToRoadSegmentParameters parameters,
        CancellationToken cancellationToken)
    {
        if (!featureToggle.FeatureEnabled)
        {
            return NotFound();
        }

        var request = new LinkRoadSegmentToStreetNameRequest(parameters?.RoadSegmentId ?? 0, parameters?.LeftStreetNameId ?? 0, parameters?.RightStreetNameId ?? 0);
        var response = await _mediator.Send(request, cancellationToken);

        return Ok(response);
    }
}

public sealed record LinkStreetNameToRoadSegmentParameters(int RoadSegmentId, int LeftStreetNameId, int RightStreetNameId);
