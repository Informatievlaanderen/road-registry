namespace RoadRegistry.BackOffice.Api.Uploads;

using System.Threading;
using System.Threading.Tasks;
using Abstractions.RoadSegments;
using Be.Vlaanderen.Basisregisters.Api;
using Infrastructure;
using Infrastructure.Controllers.Attributes;
using MediatR;
using Microsoft.AspNetCore.Mvc;

[ApiVersion(Version.Current)]
[AdvertiseApiVersions(Version.CurrentAdvertised)]
[ApiRoute("roadsegments")]
[ApiExplorerSettings(GroupName = "Uploads")]
[ApiKeyAuth(WellKnownAuthRoles.Road)]
public class RoadSegmentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public RoadSegmentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("{id}/actions/linkstreetname")]
    [RequestFormLimits(MultipartBodyLengthLimit = int.MaxValue, ValueLengthLimit = int.MaxValue)]
    public async Task<IActionResult> PostLinkStreetNameToRoadSegment([FromServices] UseFeatureCompareFeatureToggle useFeatureCompareToggle, [FromBody] LinkStreetNameToRoadSegmentParameters parameters, CancellationToken cancellationToken)
    {
        if (!useFeatureCompareToggle.FeatureEnabled) //TODO-rik use own featuretoggle
        {
            return NotFound();
        }

        var request = new LinkRoadSegmentToStreetNameRequest(parameters?.RoadSegmentId ?? 0, parameters?.LeftStreetNameId ?? 0, parameters?.RightStreetNameId ?? 0);
        var response = await _mediator.Send(request, cancellationToken);

        return Ok();
    }
}

public sealed record LinkStreetNameToRoadSegmentParameters(int RoadSegmentId, int LeftStreetNameId, int RightStreetNameId);
