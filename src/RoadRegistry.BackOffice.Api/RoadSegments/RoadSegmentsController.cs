namespace RoadRegistry.BackOffice.Api.RoadSegments;

using Be.Vlaanderen.Basisregisters.Api;
using Infrastructure;
using Infrastructure.Controllers.Attributes;
using MediatR;
using Microsoft.AspNetCore.Mvc;

[ApiVersion(Version.Current)]
[AdvertiseApiVersions(Version.CurrentAdvertised)]
[ApiRoute("wegsegmenten")]
[ApiExplorerSettings(GroupName = "Wegsegmenten")]
[ApiKeyAuth(WellKnownAuthRoles.Road)]
public partial class RoadSegmentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public RoadSegmentsController(IMediator mediator)
    {
        _mediator = mediator;
    }
}
