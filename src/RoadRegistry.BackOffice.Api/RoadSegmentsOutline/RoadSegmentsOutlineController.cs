namespace RoadRegistry.BackOffice.Api.RoadSegmentsOutline;

using Be.Vlaanderen.Basisregisters.Api;
using Infrastructure;
using Infrastructure.Controllers;
using Infrastructure.Controllers.Attributes;
using MediatR;
using Microsoft.AspNetCore.Mvc;

[ApiVersion(Version.Current)]
[AdvertiseApiVersions(Version.CurrentAdvertised)]
[ApiRoute("wegsegmenten")]
[ApiExplorerSettings(GroupName = "Wegsegmenten")]
[ApiKeyAuth(WellKnownAuthRoles.Road)]
public partial class RoadSegmentsOutlineController : BackofficeApiController
{
    private readonly IMediator _mediator;

    public RoadSegmentsOutlineController(IMediator mediator)
    {
        _mediator = mediator;
    }
}
