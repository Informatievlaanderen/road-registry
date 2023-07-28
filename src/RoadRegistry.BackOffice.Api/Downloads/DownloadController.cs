namespace RoadRegistry.BackOffice.Api.Downloads;

using Be.Vlaanderen.Basisregisters.Api;
using Infrastructure;
using Infrastructure.Authentication;
using Infrastructure.Controllers.Attributes;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiKey]
[ApiVersion(Version.Current)]
[AdvertiseApiVersions(Version.CurrentAdvertised)]
[ApiRoute("download")]
[ApiExplorerSettings(GroupName = "Download")]
[Authorize(AuthenticationSchemes = AuthenticationSchemes.AllBearerSchemes)]
public partial class DownloadController : ApiController
{
    private readonly IMediator _mediator;

    public DownloadController(IMediator mediator)
    {
        _mediator = mediator;
    }
}
