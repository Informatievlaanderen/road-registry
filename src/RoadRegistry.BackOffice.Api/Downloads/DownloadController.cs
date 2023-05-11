namespace RoadRegistry.BackOffice.Api.Downloads;

using Be.Vlaanderen.Basisregisters.Api;
using Infrastructure;
using Infrastructure.Controllers.Attributes;
using MediatR;
using Microsoft.AspNetCore.Mvc;

[ApiVersion(Version.Current)]
[AdvertiseApiVersions(Version.CurrentAdvertised)]
[ApiRoute("download")]
[ApiExplorerSettings(GroupName = "Download")]
[ApiKeyAuth(WellKnownAuthRoles.Road)]
public partial class DownloadController : ApiController
{
    private readonly IMediator _mediator;

    public DownloadController(IMediator mediator)
    {
        _mediator = mediator;
    }
}
